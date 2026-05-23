using System.Text.Json.Serialization;

namespace MouseScrollFixer.Core.Configuration;

internal sealed class StartupPreferences
{
    [JsonPropertyName("autoStartWithWindows")]
    public bool AutoStartWithWindows { get; set; }

    [JsonPropertyName("runAsAdmin")]
    public bool RunAsAdmin { get; set; }

    public static StartupPreferences CreateDefault() =>
        new()
        {
            AutoStartWithWindows = false,
            RunAsAdmin = false
        };
}
