using System.Text.Json;
using System.Text.Json.Serialization;

namespace MouseScrollFixer.Core.Configuration;

internal static class AppConfigJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var o = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };
        o.Converters.Add(new JsonStringEnumConverter());
        return o;
    }
}
