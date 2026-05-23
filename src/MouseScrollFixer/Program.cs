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
    private static void Main(string[] args)
    {
        Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pt-BR");
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");

        if (!TryEnsureElevatedIfConfigured(args))
            return;

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

    /// <summary>
    /// RF-006/RF-007: gate UAC antes do mutex; negação continua sem elevação.
    /// </summary>
    private static bool TryEnsureElevatedIfConfigured(string[] args)
    {
        var store = new AppConfigStore();
        var loadResult = store.Load();
        var config = loadResult.Config;
        AppConfigStore.MergeDefaults(config);

        if (!config.Startup!.RunAsAdmin)
            return true;

        if (ProcessElevationHelper.IsProcessElevated())
            return true;

        var executablePath = Environment.ProcessPath ?? Application.ExecutablePath;
        var elevationArgs = args.Length > 1 ? args.Skip(1) : null;
        var result = ProcessElevationHelper.TryStartElevatedInstance(executablePath, elevationArgs);

        return result switch
        {
            ElevationAttemptResult.ElevatedInstanceStarted => false,
            ElevationAttemptResult.UserCancelled => ShowUacDeniedAndContinue(),
            ElevationAttemptResult.Failed => ShowUacDeniedAndContinue(),
            _ => true
        };
    }

    private static bool ShowUacDeniedAndContinue()
    {
        MessageBox.Show(
            UiStrings.Get("Startup_UacDeniedMessage"),
            UiStrings.Get("Startup_UacDeniedTitle"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);
        return true;
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

        // CS-005: o estado activation.enabled lido do disco é aplicado na sessão (hook se ligado).
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
