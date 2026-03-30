using System.Diagnostics.CodeAnalysis;
using System.Text;
using MouseScrollFixer.Native.Win32;

namespace MouseScrollFixer.Core.ScrollNormalization;

/// <summary>
/// Resolve o caminho do executável do processo dono da janela (HWND → raiz → PID → imagem),
/// alinhado a ADR-004/006 e ao modelo de inclusão por caminho.
/// </summary>
internal static class WindowTargetResolver
{
    /// <summary>
    /// Resolução por janela em primeiro plano (utilitário; o fix de scroll usa <see cref="TryGetExecutablePathFromPoint"/>).
    /// </summary>
    public static bool TryGetExecutablePathForForegroundWindow([NotNullWhen(true)] out string? path)
    {
        var hwnd = User32.GetForegroundWindow();
        return TryGetExecutablePathForWindow(hwnd, out path);
    }

    public static bool TryGetExecutablePathFromPoint(POINT screenPoint, [NotNullWhen(true)] out string? path)
    {
        var hwnd = User32.WindowFromPoint(screenPoint);
        return TryGetExecutablePathForWindow(hwnd, out path);
    }

    public static bool TryGetExecutablePathForWindow(nint hwnd, [NotNullWhen(true)] out string? path)
    {
        path = null;
        if (hwnd == 0)
            return false;

        var root = User32.GetAncestor(hwnd, Win32Constants.GA_ROOT);
        if (root == 0)
            return false;

        User32.GetWindowThreadProcessId(root, out var pid);
        if (pid == 0)
            return false;

        var hProcess = Kernel32.OpenProcess(Win32Constants.PROCESS_QUERY_LIMITED_INFORMATION, false, pid);
        if (hProcess == 0)
            return false;

        try
        {
            var buffer = new StringBuilder(4096);
            var size = buffer.Capacity;
            if (!Kernel32.QueryFullProcessImageNameW(hProcess, 0, buffer, ref size))
                return false;

            var result = buffer.ToString();
            if (string.IsNullOrWhiteSpace(result))
                return false;

            path = result;
            return true;
        }
        finally
        {
            Kernel32.CloseHandle(hProcess);
        }
    }
}
