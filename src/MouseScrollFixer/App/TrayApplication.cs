using System.Globalization;
using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.Core.ConflictDetection;
using MouseScrollFixer.UI;
using MouseScrollFixer.UI.Resources;

namespace MouseScrollFixer.App;

internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly TrayApplication _tray;

    public TrayApplicationContext(AppConfigStore store, AppConfig config, ScrollFixerSession session)
    {
        _tray = new TrayApplication(store, config, session);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _tray.Dispose();

        base.Dispose(disposing);
    }
}

internal sealed class TrayApplication : IDisposable
{
    private readonly AppConfigStore _store;
    private readonly AppConfig _config;
    private readonly ScrollFixerSession _session;
    private readonly NotifyIcon _notifyIcon;
    private readonly EventHandler _applicationExitHandler;
    private readonly ContextMenuStrip _contextMenu;
    private readonly ToolStripMenuItem _toggleFixItem;
    private MainSettingsForm? _settingsForm;
    private bool _disposed;

    public TrayApplication(AppConfigStore store, AppConfig config, ScrollFixerSession session)
    {
        _store = store;
        _config = config;
        _session = session;

        _applicationExitHandler = (_, _) => Dispose();
        Application.ApplicationExit += _applicationExitHandler;

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true
        };

        _contextMenu = new ContextMenuStrip();
        _contextMenu.Items.Add(UiStrings.Get("Tray_OpenSettings"), null, (_, _) => OpenSettings());
        _toggleFixItem = new ToolStripMenuItem(UiStrings.Get("Tray_MenuActivateFix"), null, (_, _) => ToggleFixFromTray());
        _contextMenu.Items.Add(_toggleFixItem);
        _contextMenu.Items.Add(UiStrings.Get("Tray_Exit"), null, (_, _) => Application.Exit());

        _contextMenu.Opening += (_, _) => UpdateTrayUi();

        _notifyIcon.ContextMenuStrip = _contextMenu;
        _notifyIcon.DoubleClick += (_, _) => OpenSettings();

        UpdateTrayUi();
        DetectAndNotifyConflictIfNeeded();
    }

    /// <summary>
    /// RF-007: aviso; sem precedência automática nem desativação de terceiros.
    /// </summary>
    private void DetectAndNotifyConflictIfNeeded()
    {
        var result = ConflictDetector.Detect();
        if (!result.HasConflict)
            return;

        var names = string.Join(", ", result.MatchedProcessNames);
        _notifyIcon.BalloonTipTitle = UiStrings.Get("Conflict_BalloonTitle");
        _notifyIcon.BalloonTipText = UiStrings.Format("Conflict_BalloonText", names);
        _notifyIcon.BalloonTipIcon = ToolTipIcon.Warning;
        _notifyIcon.ShowBalloonTip(8000);
    }

    private void UpdateTrayUi()
    {
        var enabled = _config.Activation.Enabled;
        _notifyIcon.Text = enabled ? UiStrings.Get("Tray_StatusFixOn") : UiStrings.Get("Tray_StatusFixOff");
        _toggleFixItem.Text = enabled ? UiStrings.Get("Tray_MenuDeactivateFix") : UiStrings.Get("Tray_MenuActivateFix");
    }

    private void ToggleFixFromTray()
    {
        _config.Activation.Enabled = !_config.Activation.Enabled;
        _config.Activation.LastModifiedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        AppConfigStore.MergeDefaults(_config);

        try
        {
            var vr = AppConfigValidator.Validate(_config);
            if (!vr.IsValid)
            {
                _config.Activation.Enabled = !_config.Activation.Enabled;
                MessageBox.Show(
                    UiStrings.Get("MainSettings_ErrorInvalid") + Environment.NewLine + string.Join(Environment.NewLine, vr.Errors),
                    UiStrings.Get("MainSettings_ErrorTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            _store.Save(_config);
            _session.ApplyConfig(_config);
            if (_config.Activation.Enabled && !_session.HookInstalled)
            {
                MessageBox.Show(
                    UiStrings.Get("Program_HookFailed"),
                    UiStrings.Get("MainSettings_ErrorTitle"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            _config.Activation.Enabled = !_config.Activation.Enabled;
            MessageBox.Show(
                UiStrings.Get("MainSettings_ErrorSave") + Environment.NewLine + ex.Message,
                UiStrings.Get("MainSettings_ErrorTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UpdateTrayUi();

        if (_settingsForm is { IsDisposed: false })
            _settingsForm.SyncActivationFromConfig();
    }

    private void OpenSettings()
    {
        if (_settingsForm is null || _settingsForm.IsDisposed)
            _settingsForm = new MainSettingsForm(_store, _config, _session, () => UpdateTrayUi());

        _settingsForm.ShowInTaskbar = true;
        _settingsForm.Show();
        _settingsForm.WindowState = FormWindowState.Normal;
        _settingsForm.Activate();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // T020: gravar preferência e lista antes de libertar o hook e o ícone da bandeja.
        ApplicationExitPersistence.TrySave(_store, _config);

        Application.ApplicationExit -= _applicationExitHandler;
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _contextMenu.Dispose();
        _settingsForm?.Dispose();

        // UnhookWindowsHookEx via LowLevelMouseHook.Dispose.
        _session.Dispose();
    }
}
