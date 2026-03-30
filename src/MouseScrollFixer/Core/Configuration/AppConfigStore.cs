using System.Text;
using System.Text.Json;

namespace MouseScrollFixer.Core.Configuration;

internal sealed class AppConfigStore
{
    private const string AppFolderName = "MouseScrollFixer";
    private const string PrimaryFileName = "app-config.json";
    private const string BackupFileName = "app-config.bak.json";
    private const string TempFileName = "app-config.json.tmp";

    private readonly string _directory;
    private readonly string _primaryPath;
    private readonly string _backupPath;

    public AppConfigStore()
    {
        _directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppFolderName);
        _primaryPath = Path.Combine(_directory, PrimaryFileName);
        _backupPath = Path.Combine(_directory, BackupFileName);
    }

    public string PrimaryConfigPath => _primaryPath;

    public AppConfigLoadResult Load()
    {
        Directory.CreateDirectory(_directory);

        if (!File.Exists(_primaryPath))
        {
            if (TryDeserializeFile(_backupPath, out var fromBackup) && fromBackup is not null && AppConfigValidator.Validate(fromBackup).IsValid)
            {
                MergeDefaults(fromBackup);
                return AppConfigLoadResult.Recovered(fromBackup);
            }

            var fresh = AppConfig.CreateDefault();
            MergeDefaults(fresh);
            return AppConfigLoadResult.NewDefaults(fresh);
        }

        if (TryDeserializeFile(_primaryPath, out var primary) && primary is not null)
        {
            MergeDefaults(primary);
            if (AppConfigValidator.Validate(primary).IsValid)
                return AppConfigLoadResult.Ok(primary);
        }

        if (TryDeserializeFile(_backupPath, out var backup) && backup is not null)
        {
            MergeDefaults(backup);
            if (AppConfigValidator.Validate(backup).IsValid)
                return AppConfigLoadResult.Recovered(backup);
        }

        var safe = AppConfig.CreateDefault();
        MergeDefaults(safe);
        return AppConfigLoadResult.SafeDefaults(safe);
    }

    public void Save(AppConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        MergeDefaults(config);

        var validation = AppConfigValidator.Validate(config);
        if (!validation.IsValid)
            throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Errors));

        Directory.CreateDirectory(_directory);

        var json = JsonSerializer.Serialize(config, AppConfigJson.Options);
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        var tempPath = Path.Combine(_directory, TempFileName);
        File.WriteAllText(tempPath, json, encoding);

        try
        {
            if (File.Exists(_primaryPath))
                File.Copy(_primaryPath, _backupPath, overwrite: true);

            File.Copy(tempPath, _primaryPath, overwrite: true);
        }
        finally
        {
            TryDelete(tempPath);
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // ignorar limpeza de ficheiro temporário
        }
    }

    private static bool TryDeserializeFile(string path, out AppConfig? config)
    {
        config = null;
        if (!File.Exists(path))
            return false;

        try
        {
            var json = File.ReadAllText(path);
            config = JsonSerializer.Deserialize<AppConfig>(json, AppConfigJson.Options);
            return config is not null;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    internal static void MergeDefaults(AppConfig config)
    {
        config.Behavior ??= BehaviorProfile.CreateDefault();
    }
}
