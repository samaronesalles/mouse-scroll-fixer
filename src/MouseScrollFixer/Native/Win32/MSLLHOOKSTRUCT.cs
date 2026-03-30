using System.Runtime.InteropServices;

namespace MouseScrollFixer.Native.Win32;

/// <summary>
/// Estrutura para o hook de baixo nível do rato (<c>WH_MOUSE_LL</c>).
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct MSLLHOOKSTRUCT
{
    public POINT pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public nuint dwExtraInfo;
}
