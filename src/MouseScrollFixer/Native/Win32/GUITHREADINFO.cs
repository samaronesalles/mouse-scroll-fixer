using System.Runtime.InteropServices;

namespace MouseScrollFixer.Native.Win32;

/// <summary>
/// Informações sobre o estado GUI de um thread — usada para obter o HWND
/// com foco teclado dentro de um processo externo via <c>GetGUIThreadInfo</c>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct GUITHREADINFO
{
    public uint cbSize;
    public uint flags;
    public nint hwndActive;
    public nint hwndFocus;
    public nint hwndCapture;
    public nint hwndMenuOwner;
    public nint hwndMoveSize;
    public nint hwndCaret;
    public RECT rcCaret;
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int left;
    public int top;
    public int right;
    public int bottom;
}
