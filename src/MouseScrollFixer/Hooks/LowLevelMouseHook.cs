using MouseScrollFixer.Native.Win32;

namespace MouseScrollFixer.Hooks;

/// <summary>
/// Instala <c>WH_MOUSE_LL</c>. O callback deve ser rápido; a lógica pesada fica fora ou com cache.
/// </summary>
internal sealed class LowLevelMouseHook : IDisposable
{
    private readonly User32.LowLevelMouseProc _proc;
    private nint _hook;

    public LowLevelMouseHook()
    {
        _proc = HookCallback;
    }

    public event Action<int, nint, nint>? MouseMessage;

    /// <summary>
    /// Indica se o hook está instalado (não suspenso).
    /// </summary>
    public bool IsInstalled => _hook != 0;

    /// <summary>
    /// Instala o hook se estiver suspenso.
    /// </summary>
    public bool TryInstall()
    {
        if (_hook != 0)
            return true;

        var module = Kernel32.GetModuleHandle(null);
        _hook = User32.SetWindowsHookEx(Win32Constants.WH_MOUSE_LL, _proc, module, 0);
        return _hook != 0;
    }

    /// <summary>
    /// Remove o hook sem libertar o objeto (fix desligado — sem callback, repasse implícito pelo sistema).
    /// </summary>
    public void Suspend()
    {
        if (_hook == 0)
            return;

        User32.UnhookWindowsHookEx(_hook);
        _hook = 0;
    }

    private nint HookCallback(int nCode, nint wParam, nint lParam)
    {
        if (nCode < 0)
            return User32.CallNextHookEx(_hook, nCode, wParam, lParam);

        MouseMessage?.Invoke(nCode, wParam, lParam);
        return User32.CallNextHookEx(_hook, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hook == 0)
            return;

        User32.UnhookWindowsHookEx(_hook);
        _hook = 0;
    }
}
