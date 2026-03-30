using System.Runtime.InteropServices;

namespace MouseScrollFixer.Native.Win32;

internal static class Ntdll
{
    [DllImport("ntdll.dll")]
    public static extern int RtlGetVersion(ref OSVersionInfoEx versionInformation);
}

/// <summary>
/// Estrutura <c>OSVERSIONINFOEXW</c> para <see cref="Ntdll.RtlGetVersion"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
internal struct OSVersionInfoEx
{
    public int OSVersionInfoSize;
    public int MajorVersion;
    public int MinorVersion;
    public int BuildNumber;
    public int PlatformId;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string CSDVersion;
    public short ServicePackMajor;
    public short ServicePackMinor;
    public short SuiteMask;
    public byte ProductType;
    public byte Reserved;
}
