using System.Text.Json.Serialization;

namespace MouseScrollFixer.Core.Configuration;

internal sealed class AppConfig
{
    public const int CurrentSchemaVersion = 1;

    [JsonPropertyName("schemaVersion")]
    public int SchemaVersion { get; set; } = CurrentSchemaVersion;

    [JsonPropertyName("activation")]
    public ActivationPreference Activation { get; set; } = new();

    [JsonPropertyName("inclusionList")]
    public List<InclusionEntry> InclusionList { get; set; } = new();

    [JsonPropertyName("behavior")]
    public BehaviorProfile? Behavior { get; set; }

    public static AppConfig CreateDefault() =>
        new()
        {
            SchemaVersion = CurrentSchemaVersion,
            Activation = new ActivationPreference { Enabled = false },
            InclusionList = new List<InclusionEntry>(),
            Behavior = BehaviorProfile.CreateDefault()
        };
}
