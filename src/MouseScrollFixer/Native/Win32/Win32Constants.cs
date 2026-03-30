namespace MouseScrollFixer.Native.Win32;

internal static class Win32Constants
{
    public const int WH_MOUSE_LL = 14;

    public const uint GA_ROOT = 2;

    public const uint WM_MOUSEWHEEL = 0x020A;
    public const uint WM_MOUSEHWHEEL = 0x020E;
    public const uint WM_VSCROLL = 0x0115;

    public const uint WHEEL_DELTA = 120;

    public const int SB_LINEUP = 0;
    public const int SB_LINEDOWN = 1;
    public const int SB_ENDSCROLL = 8;

    public const ushort MK_LBUTTON = 0x0001;
    public const ushort MK_RBUTTON = 0x0002;
    public const ushort MK_SHIFT = 0x0004;
    public const ushort MK_CONTROL = 0x0008;
    public const ushort MK_MBUTTON = 0x0010;

    public const int VK_LBUTTON = 0x01;
    public const int VK_RBUTTON = 0x02;
    public const int VK_MBUTTON = 0x04;
    public const int VK_SHIFT = 0x10;
    public const int VK_CONTROL = 0x11;

    public const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;

    // GetWindowLong index
    public const int GWL_STYLE = -16;

    // Window style flags relevantes para scroll
    public const uint WS_CHILD  = 0x40000000;
    public const uint WS_VSCROLL = 0x00200000;
}
