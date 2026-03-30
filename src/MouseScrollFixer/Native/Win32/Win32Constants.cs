namespace MouseScrollFixer.Native.Win32;

internal static class Win32Constants
{
    public const int WH_MOUSE_LL = 14;

    public const uint GA_ROOT = 2;

    public const uint WM_MOUSEWHEEL = 0x020A;
    public const uint WM_MOUSEHWHEEL = 0x020E;

    public const uint WHEEL_DELTA = 120;

    public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
}
