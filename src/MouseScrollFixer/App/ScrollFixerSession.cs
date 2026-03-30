using System.Runtime.InteropServices;
using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.Core.ScrollNormalization;
using MouseScrollFixer.Hooks;
using MouseScrollFixer.Native.Win32;

namespace MouseScrollFixer.App;

/// <summary>
/// Coordena hook + lista de inclusão + preferência de ativação.
/// Intercepta <c>WM_MOUSEWHEEL</c>, identifica o controle-filho sob o cursor,
/// verifica a whitelist e redireciona a mensagem via <c>PostMessage</c>.
/// A mensagem original é consumida (não repassada ao sistema) para evitar duplicação.
/// </summary>
internal sealed class ScrollFixerSession : IDisposable
{
    private readonly LowLevelMouseHook _mouseHook;
    private AppConfig _config = null!;
    private HashSet<string> _paths = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    public ScrollFixerSession(AppConfig config)
    {
        _mouseHook = new LowLevelMouseHook();
        _mouseHook.MessageInterceptor = OnMouseMessage;
        ApplyConfig(config);
    }

    /// <summary>
    /// Indica se o hook de baixo nível está instalado (fix ligado e instalação bem-sucedida).
    /// </summary>
    public bool HookInstalled => _mouseHook.IsInstalled;

    public void ApplyConfig(AppConfig config)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _config = config;
        RebuildPaths();

        if (_config.Activation.Enabled)
            _mouseHook.TryInstall();
        else
            _mouseHook.Suspend();
    }

    private void RebuildPaths()
    {
        _paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in _config.InclusionList)
        {
            if (string.IsNullOrWhiteSpace(e.ExecutablePath))
                continue;

            _paths.Add(AppConfigValidator.NormalizeExecutablePath(e.ExecutablePath));
        }
    }

    /// <returns><c>true</c> se a mensagem foi tratada e deve ser consumida pelo hook.</returns>
    private bool OnMouseMessage(int nCode, nint wParam, nint lParam)
    {
        if (_disposed || nCode != 0)
            return false;

        if (!_config.Activation.Enabled)
            return false;

        var msg = (uint)(nint)wParam;
        if (msg == Win32Constants.WM_MOUSEHWHEEL)
            return false;

        if (msg != Win32Constants.WM_MOUSEWHEEL)
            return false;

        var st = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);

        if (!WindowTargetResolver.TryResolveFromPoint(st.pt, out var target))
            return false;

        var normalizedPath = AppConfigValidator.NormalizeExecutablePath(target.ExecutablePath);
        if (!_paths.Contains(normalizedPath))
            return false;

        var behavior = _config.Behavior ?? BehaviorProfile.CreateDefault();

        var rawDelta = ScrollNormalizer.GetVerticalWheelDelta(st.mouseData);
        var delta = behavior.TouchpadSameAsWheel
            ? ScrollNormalizer.NormalizeVerticalWheelDelta(rawDelta, behavior)
            : rawDelta;

        if (delta == 0)
            return false;

        // Prefere o controle com foco teclado dentro do processo-alvo.
        // WindowFromPoint pode retornar um painel decorativo (gutter, borda) que não processa
        // scroll, enquanto o controle interativo real está em hwndFocus do thread.
        var effectiveHwnd = GetFocusedHwndInSameProcess(target.ChildHwnd);

        if (behavior.UseVScrollFallback)
        {
            var scrollTarget = FindScrollableAncestor(effectiveHwnd);
            PostVScrollMessages(scrollTarget, delta, behavior);
        }
        else
        {
            PostWheelMessage(effectiveHwnd, st.pt, delta);
        }

        return true;
    }

    /// <summary>
    /// Envia <c>WM_MOUSEWHEEL</c> via <c>PostMessage</c> diretamente ao controle-filho.
    /// O controle recebe a mensagem como se o sistema a tivesse entregue a ele.
    /// </summary>
    private static void PostWheelMessage(nint hwnd, POINT pt, int delta)
    {
        var keyState = GetCurrentKeyState();
        var wParam = (nint)(int)(((uint)(ushort)(short)delta << 16) | keyState);
        var lParam = (nint)(int)(((ushort)pt.Y << 16) | (ushort)pt.X);

        User32.PostMessageW(hwnd, Win32Constants.WM_MOUSEWHEEL, wParam, lParam);
    }

    /// <summary>
    /// Obtém o controle com foco teclado (<c>hwndFocus</c>) dentro do mesmo processo que
    /// <paramref name="hwndInProcess"/>. Se não for possível determinar ou se o resultado
    /// pertencer a um processo diferente, devolve <paramref name="hwndInProcess"/> como fallback.
    /// </summary>
    private static nint GetFocusedHwndInSameProcess(nint hwndInProcess)
    {
        if (hwndInProcess == 0)
            return hwndInProcess;

        var root = User32.GetAncestor(hwndInProcess, Win32Constants.GA_ROOT);
        if (root == 0)
            return hwndInProcess;

        var threadId = User32.GetWindowThreadProcessId(root, out var pid);
        if (threadId == 0)
            return hwndInProcess;

        var gti = new GUITHREADINFO { cbSize = (uint)Marshal.SizeOf<GUITHREADINFO>() };
        if (!User32.GetGUIThreadInfo(threadId, ref gti) || gti.hwndFocus == 0)
            return hwndInProcess;

        // Verifica que o hwndFocus pertence ao mesmo processo, evitando redirecionamentos cruzados.
        User32.GetWindowThreadProcessId(gti.hwndFocus, out var focusPid);
        return focusPid == pid ? gti.hwndFocus : hwndInProcess;
    }

    /// <summary>
    /// Sobe a hierarquia de janelas a partir de <paramref name="hwnd"/> procurando o primeiro
    /// ancestral com o estilo <c>WS_VSCROLL</c>. Isso é necessário porque <c>WindowFromPoint</c>
    /// retorna o filho mais profundo, mas o <c>WM_VSCROLL</c> deve ir para a janela que
    /// efetivamente possui a scrollbar (ex.: container do editor no Delphi 3).
    /// Para ao atingir uma janela top-level (sem <c>WS_CHILD</c>) e retorna <paramref name="hwnd"/>
    /// como fallback caso nenhum ancestral elegível seja encontrado.
    /// </summary>
    private static nint FindScrollableAncestor(nint hwnd)
    {
        const int maxLevels = 16;
        var candidate = hwnd;

        for (var i = 0; i < maxLevels && candidate != 0; i++)
        {
            var style = (uint)User32.GetWindowLong(candidate, Win32Constants.GWL_STYLE);
            if ((style & Win32Constants.WS_VSCROLL) != 0)
                return candidate;

            if ((style & Win32Constants.WS_CHILD) == 0)
                break;

            candidate = User32.GetParent(candidate);
        }

        return hwnd;
    }

    /// <summary>
    /// Converte o delta da roda em múltiplas <c>WM_VSCROLL</c> (<c>SB_LINEUP</c>/<c>SB_LINEDOWN</c>).
    /// Necessário para aplicações legadas que não processam <c>WM_MOUSEWHEEL</c>.
    /// </summary>
    private static void PostVScrollMessages(nint hwnd, int delta, BehaviorProfile behavior)
    {
        var lines = behavior.LinesPerNotchApprox is > 0 ? behavior.LinesPerNotchApprox.Value : 3.0;
        var notches = Math.Max(1, (int)Math.Round(Math.Abs(delta) / (double)ScrollNormalizer.WheelDeltaUnit));
        var totalLines = (int)Math.Max(1, Math.Round(notches * lines));

        var scrollCmd = delta > 0
            ? Win32Constants.SB_LINEUP
            : Win32Constants.SB_LINEDOWN;

        for (var i = 0; i < totalLines; i++)
            User32.PostMessageW(hwnd, Win32Constants.WM_VSCROLL, (nint)scrollCmd, 0);

        User32.PostMessageW(hwnd, Win32Constants.WM_VSCROLL, Win32Constants.SB_ENDSCROLL, 0);
    }

    private static ushort GetCurrentKeyState()
    {
        ushort state = 0;
        if ((User32.GetKeyState(Win32Constants.VK_CONTROL) & 0x8000) != 0) state |= Win32Constants.MK_CONTROL;
        if ((User32.GetKeyState(Win32Constants.VK_SHIFT) & 0x8000) != 0) state |= Win32Constants.MK_SHIFT;
        if ((User32.GetKeyState(Win32Constants.VK_LBUTTON) & 0x8000) != 0) state |= Win32Constants.MK_LBUTTON;
        if ((User32.GetKeyState(Win32Constants.VK_RBUTTON) & 0x8000) != 0) state |= Win32Constants.MK_RBUTTON;
        if ((User32.GetKeyState(Win32Constants.VK_MBUTTON) & 0x8000) != 0) state |= Win32Constants.MK_MBUTTON;
        return state;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _mouseHook.MessageInterceptor = null;
        _mouseHook.Dispose();
    }
}
