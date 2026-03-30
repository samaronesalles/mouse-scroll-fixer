using System.IO.Pipes;
using System.Text;

namespace MouseScrollFixer.SingleInstance;

/// <summary>
/// Mutex nomeado <c>Local\</c> + identificador fixo do produto; segundo processo sinaliza o primeiro por pipe nomeado.
/// </summary>
internal static class SingleInstanceCoordinator
{
    /// <summary>Identificador estável do produto (não alterar sem nota de migração).</summary>
    private const string ProductId = "7B2E9F1A-4C3D-4E8B-9A1F-0D5E6F7A8B9C";

    internal static string MutexName => $@"Local\MouseScrollFixer_{ProductId}";

    internal static string PipeName => $"MouseScrollFixer_IPC_{ProductId}";

    internal const string ShowSettingsCommand = "SHOW_SETTINGS";

    /// <summary>
    /// Tenta obter exclusão mútua da sessão. Se <c>false</c>, outra instância já está ativa.
    /// </summary>
    public static bool TryAcquireSingleton([System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Mutex? mutex)
    {
        mutex = new Mutex(true, MutexName, out var createdNew);
        if (!createdNew)
        {
            mutex.Dispose();
            mutex = null;
            return false;
        }

        return true;
    }

    /// <summary>
    /// Notifica a instância existente para restaurar a janela de definições e o separador de configurações.
    /// </summary>
    public static void NotifyExistingInstance()
    {
        for (var i = 0; i < 120; i++)
        {
            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                client.Connect(400);
                using var writer = new StreamWriter(client, Encoding.UTF8) { AutoFlush = true };
                writer.WriteLine(ShowSettingsCommand);
                return;
            }
            catch
            {
                Thread.Sleep(75);
            }
        }
    }
}
