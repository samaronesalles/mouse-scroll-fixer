using MouseScrollFixer.Core.Configuration;

namespace MouseScrollFixer.App;

/// <summary>
/// Grava a configuração atual no encerramento sem bloquear o processo se o disco falhar.
/// </summary>
internal static class ApplicationExitPersistence
{
    public static void TrySave(AppConfigStore store, AppConfig config)
    {
        try
        {
            AppConfigStore.MergeDefaults(config);
            if (!AppConfigValidator.Validate(config).IsValid)
                return;

            store.Save(config);
        }
        catch
        {
            // Não impedir o encerramento nem mostrar UI nesta fase.
        }
    }
}
