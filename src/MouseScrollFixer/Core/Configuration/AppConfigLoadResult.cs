namespace MouseScrollFixer.Core.Configuration;

internal sealed class AppConfigLoadResult
{
    private AppConfigLoadResult(AppConfig config, AppConfigLoadKind kind, bool showCorruptionWarning)
    {
        Config = config;
        Kind = kind;
        ShowCorruptionWarning = showCorruptionWarning;
    }

    public AppConfig Config { get; }

    public AppConfigLoadKind Kind { get; }

    public bool ShowCorruptionWarning { get; }

    public static AppConfigLoadResult NewDefaults(AppConfig config) =>
        new(config, AppConfigLoadKind.DefaultNew, showCorruptionWarning: false);

    public static AppConfigLoadResult Ok(AppConfig config) =>
        new(config, AppConfigLoadKind.Loaded, showCorruptionWarning: false);

    public static AppConfigLoadResult Recovered(AppConfig config) =>
        new(config, AppConfigLoadKind.RecoveredFromBackup, showCorruptionWarning: true);

    public static AppConfigLoadResult SafeDefaults(AppConfig config) =>
        new(config, AppConfigLoadKind.CorruptedUsedSafeDefaults, showCorruptionWarning: true);
}
