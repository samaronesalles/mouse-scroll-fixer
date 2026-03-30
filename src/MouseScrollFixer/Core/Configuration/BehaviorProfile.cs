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

    public static BehaviorProfile CreateDefault() =>
        new() { TouchpadSameAsWheel = true };
}
