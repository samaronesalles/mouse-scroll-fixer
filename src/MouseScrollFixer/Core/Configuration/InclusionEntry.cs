using System.Text.Json.Serialization;

namespace MouseScrollFixer.Core.Configuration;

internal sealed class InclusionEntry
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("executablePath")]
    public string ExecutablePath { get; set; } = string.Empty;

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("matchKind")]
    public MatchKind MatchKind { get; set; }

    [JsonPropertyName("notes")]
    public string? Notes { get; set; }
}
