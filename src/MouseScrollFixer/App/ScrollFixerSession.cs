using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.Core.ScrollNormalization;
using MouseScrollFixer.Hooks;
using MouseScrollFixer.Native.Win32;

namespace MouseScrollFixer.App;

/// <summary>
/// Coordena hook + lista de inclusão + preferência de ativação.
/// Com o fix desligado, o hook é suspenso (desinstalado); com fix ligado, reinstala-se.
/// No arranque, <see cref="ApplyConfig"/> aplica o <c>activation.enabled</c> já persistido (CS-005: último estado «ligado» sem reativação manual).
/// </summary>
internal sealed class ScrollFixerSession : IDisposable
{
    private readonly LowLevelMouseHook _mouseHook;
    private AppConfig _config;
    private HashSet<string> _paths = new(StringComparer.OrdinalIgnoreCase);
    private nint _cachedForegroundHwnd;
    private string? _cachedNormalizedPath;
    private bool _disposed;

    public ScrollFixerSession(AppConfig config)
    {
        _mouseHook = new LowLevelMouseHook();
        _mouseHook.MouseMessage += OnMouseMessage;
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
        InvalidateForegroundCache();

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

    private void InvalidateForegroundCache()
    {
        _cachedForegroundHwnd = 0;
        _cachedNormalizedPath = null;
    }

    private void OnMouseMessage(int nCode, nint wParam, nint lParam)
    {
        if (_disposed || nCode != 0)
            return;

        if (!_config.Activation.Enabled)
            return;

        var msg = (uint)(nint)wParam;
        if (msg == Win32Constants.WM_MOUSEHWHEEL)
            return;

        if (msg != Win32Constants.WM_MOUSEWHEEL)
            return;

        if (!TryResolveForegroundExeNormalized(out var normalizedPath))
            return;

        if (!_paths.Contains(normalizedPath))
            return;

        var behavior = _config.Behavior ?? BehaviorProfile.CreateDefault();
        if (!behavior.TouchpadSameAsWheel)
            return;

        var st = Marshal.PtrToStructure<MSLLHOOKSTRUCT>(lParam);
        var rawDelta = ScrollNormalizer.GetVerticalWheelDelta(st.mouseData);
        var newDelta = ScrollNormalizer.NormalizeVerticalWheelDelta(rawDelta, behavior);
        if (newDelta == rawDelta)
            return;

        st.mouseData = ScrollNormalizer.SetVerticalWheelDelta(st.mouseData, newDelta);
        Marshal.StructureToPtr(st, lParam, false);
    }

    private bool TryResolveForegroundExeNormalized([NotNullWhen(true)] out string? normalizedPath)
    {
        normalizedPath = null;
        var hwnd = User32.GetForegroundWindow();
        if (hwnd == 0)
            return false;

        if (hwnd == _cachedForegroundHwnd && _cachedNormalizedPath is not null)
        {
            normalizedPath = _cachedNormalizedPath;
            return true;
        }

        if (!WindowTargetResolver.TryGetExecutablePathForForegroundWindow(out var path))
        {
            InvalidateForegroundCache();
            return false;
        }

        _cachedForegroundHwnd = hwnd;
        _cachedNormalizedPath = AppConfigValidator.NormalizeExecutablePath(path);
        normalizedPath = _cachedNormalizedPath;
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _mouseHook.MouseMessage -= OnMouseMessage;
        _mouseHook.Dispose();
    }
}
