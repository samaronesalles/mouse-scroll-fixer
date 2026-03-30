using System.Globalization;
using System.Threading;
using MouseScrollFixer.App;
using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.Native.Win32;
using MouseScrollFixer.SingleInstance;
using MouseScrollFixer.UI.Resources;

namespace MouseScrollFixer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

        if (!SingleInstanceCoordinator.TryAcquireSingleton(out var singletonMutex))
        {
            SingleInstanceCoordinator.NotifyExistingInstance();
            return;
        }

        try
        {
            RunApplication();
        }
        finally
        {
            singletonMutex.Dispose();
        }
    }

    private static void RunApplication()
    {
        // RF-010: sem telemetria nem envio remoto — apenas leitura/gravação local de configuração.
        ApplicationConfiguration.Initialize();

        if (!OsVersionHelper.IsWindows11OrGreater())
        {
            MessageBox.Show(
                "O MouseScrollFixer requer o Windows 11. Este sistema operacional não é suportado.",
                "MouseScrollFixer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var store = new AppConfigStore();
        var loadResult = store.Load();
        if (loadResult.ShowCorruptionWarning)
        {
            MessageBox.Show(
                "O ficheiro de configuração está inválido ou corrompido. Foram aplicados valores seguros (fix desligado, lista vazia) ou, quando possível, foi restaurada uma cópia anterior.",
                "MouseScrollFixer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        var config = loadResult.Config;
        AppConfigStore.MergeDefaults(config);

        // CS-005 / T021: o estado activation.enabled lido do disco é aplicado na sessão (hook se ligado).
        var session = new ScrollFixerSession(config);
        if (config.Activation.Enabled && !session.HookInstalled)
        {
            MessageBox.Show(
                UiStrings.Get("Program_HookFailed"),
                UiStrings.Get("MainSettings_ErrorTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        // T020: última gravação defensiva se o processo terminar após Application.Exit.
        Application.ApplicationExit += (_, _) => ApplicationExitPersistence.TrySave(store, config);

        Application.Run(new TrayApplicationContext(store, config, session));
    }
}
