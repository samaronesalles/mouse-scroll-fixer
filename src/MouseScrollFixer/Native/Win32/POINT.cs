using System.Runtime.InteropServices;

namespace MouseScrollFixer.Native.Win32;

[StructLayout(LayoutKind.Sequential)]
internal struct POINT
{
    public int X;
    public int Y;
}
