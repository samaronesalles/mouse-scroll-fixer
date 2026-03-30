using System.Text.Json.Serialization;

namespace MouseScrollFixer.Core.Configuration;

internal sealed class BehaviorProfile
{
    [JsonPropertyName("invertVertical")]
    public bool? InvertVertical { get; set; }

    [JsonPropertyName("linesPerNotchApprox")]
    public double? LinesPerNotchApprox { get; set; }

    [JsonPropertyName("touchpadSameAsWheel")]
    public bool TouchpadSameAsWheel { get; set; } = true;

    /// <summary>
    /// Quando <c>true</c>, converte <c>WM_MOUSEWHEEL</c> em múltiplas mensagens
    /// <c>WM_VSCROLL</c> (<c>SB_LINEUP</c>/<c>SB_LINEDOWN</c>) para aplicações legadas
    /// que não processam <c>WM_MOUSEWHEEL</c> (ex.: Delphi 3, VB6).
    /// </summary>
    [JsonPropertyName("useVScrollFallback")]
    public bool UseVScrollFallback { get; set; }

    public static BehaviorProfile CreateDefault() =>
        new() { TouchpadSameAsWheel = true };
}
