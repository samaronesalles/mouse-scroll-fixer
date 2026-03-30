using System.Text.Json.Serialization;

namespace MouseScrollFixer.Core.Configuration;

internal sealed class ActivationPreference
{
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("lastModifiedUtc")]
    public string? LastModifiedUtc { get; set; }
}
