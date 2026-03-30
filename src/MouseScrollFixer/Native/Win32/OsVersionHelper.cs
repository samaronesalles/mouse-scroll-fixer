using System.Runtime.InteropServices;

namespace MouseScrollFixer.Native.Win32;

/// <summary>
/// Windows 11 corresponde tipicamente a build &gt;= 22000 (NT 10.0).
/// </summary>
internal static class OsVersionHelper
{
    public static bool IsWindows11OrGreater()
    {
        return GetOsBuildNumber() >= 22000;
    }

    public static int GetOsBuildNumber()
    {
        var info = new OSVersionInfoEx
        {
            OSVersionInfoSize = Marshal.SizeOf(typeof(OSVersionInfoEx)),
            CSDVersion = string.Empty
        };

        if (Ntdll.RtlGetVersion(ref info) == 0)
            return info.BuildNumber;

        return Environment.OSVersion.Version.Build;
    }
}
