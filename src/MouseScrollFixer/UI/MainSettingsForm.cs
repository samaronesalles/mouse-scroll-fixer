using System.Globalization;
using MouseScrollFixer.App;
using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.UI.Resources;

namespace MouseScrollFixer.UI;

internal sealed partial class MainSettingsForm : Form
{
    private readonly AppConfigStore _store;
    private readonly AppConfig _config;
    private readonly ScrollFixerSession _session;
    private readonly Action? _onConfigChanged;

    private TabControl _tabControl = null!;
    private CheckBox _activationCheckBox = null!;
    private Label _helpLabel = null!;
    private Label _statusLabel = null!;
    private ListView _listView = null!;
    private Button _addButton = null!;
    private Button _removeButton = null!;
    private bool _suppressActivationPersist;

    public MainSettingsForm(AppConfigStore store, AppConfig config, ScrollFixerSession session, Action? onConfigChanged = null)
    {
        _store = store;
        _config = config;
        _session = session;
        _onConfigChanged = onConfigChanged;
        InitializeComponent();
        BuildUi();
        LoadFromConfig();
    }

    /// <summary>
    /// Sincroniza a caixa de ativação e o rótulo de estado quando o alterador é a bandeja.
    /// </summary>
    internal void SyncActivationFromConfig()
    {
        _suppressActivationPersist = true;
        try
        {
            _activationCheckBox.Checked = _config.Activation.Enabled;
            UpdateActivationUi();
        }
        finally
        {
            _suppressActivationPersist = false;
        }
    }

    /// <summary>RF-011: segundo arranque — separador de configurações visível.</summary>
    internal void SelectConfigurationTab()
    {
        if (_tabControl.TabCount > 0)
            _tabControl.SelectedIndex = 0;
    }

    private void BuildUi()
    {
        Text = UiStrings.Get("MainSettings_Title");

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        var settingsPage = new TabPage(UiStrings.Get("MainSettings_TabSettings"));
        _tabControl.TabPages.Add(settingsPage);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _helpLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            Text = UiStrings.Get("MainSettings_Help")
        };

        _activationCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = UiStrings.Get("MainSettings_Activation")
        };
        _activationCheckBox.CheckedChanged += (_, _) => OnActivationChanged();

        _statusLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            ForeColor = SystemColors.ControlDarkDark
        };

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = true
        };
        _listView.Columns.Add(UiStrings.Get("MainSettings_ColumnDisplayName"), 180);
        _listView.Columns.Add(UiStrings.Get("MainSettings_ColumnPath"), 460);
        _listView.SelectedIndexChanged += (_, _) => UpdateRemoveState();

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 0)
        };

        _addButton = new Button
        {
            Text = UiStrings.Get("MainSettings_Add"),
            AutoSize = true
        };
        _addButton.Click += (_, _) => AddExecutable();

        _removeButton = new Button
        {
            Text = UiStrings.Get("MainSettings_Remove"),
            AutoSize = true,
            Enabled = false
        };
        _removeButton.Click += (_, _) => RemoveSelected();

        buttons.Controls.Add(_addButton);
        buttons.Controls.Add(_removeButton);

        layout.Controls.Add(_helpLabel, 0, 0);
        layout.Controls.Add(_activationCheckBox, 0, 1);
        layout.Controls.Add(_statusLabel, 0, 2);
        layout.Controls.Add(_listView, 0, 3);
        layout.Controls.Add(buttons, 0, 4);

        settingsPage.Controls.Add(layout);
        Controls.Add(_tabControl);
    }

    private void LoadFromConfig()
    {
        _activationCheckBox.Checked = _config.Activation.Enabled;
        RefreshList();
        UpdateRemoveState();
        UpdateActivationUi();
    }

    private void UpdateActivationUi()
    {
        _statusLabel.Text = _config.Activation.Enabled
            ? UiStrings.Get("MainSettings_StatusFixOn")
            : UiStrings.Get("MainSettings_StatusFixOff");
    }

    private void RefreshList()
    {
        _listView.Items.Clear();
        foreach (var e in _config.InclusionList)
        {
            var display = string.IsNullOrWhiteSpace(e.DisplayName) ? Path.GetFileName(e.ExecutablePath) : e.DisplayName!;
            var item = new ListViewItem(display);
            item.SubItems.Add(e.ExecutablePath);
            item.Tag = e.Id;
            _listView.Items.Add(item);
        }
    }

    private void UpdateRemoveState()
    {
        _removeButton.Enabled = _listView.SelectedItems.Count > 0;
    }

    private void OnActivationChanged()
    {
        if (_suppressActivationPersist)
            return;

        _config.Activation.Enabled = _activationCheckBox.Checked;
        _config.Activation.LastModifiedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        PersistAndApply();
    }

    private void AddExecutable()
    {
        using var dlg = new OpenFileDialog
        {
            Title = UiStrings.Get("MainSettings_Add"),
            Filter = UiStrings.Get("MainSettings_AddExeFilter"),
            CheckFileExists = true,
            Multiselect = false
        };

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        var path = dlg.FileName;
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (!path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorNotExe"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!File.Exists(path))
        {
            MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorNotExe"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_config.InclusionList.Count >= AppConfigValidator.MaxInclusionEntries)
        {
            MessageBox.Show(this, UiStrings.Format("MainSettings_ErrorLimit", AppConfigValidator.MaxInclusionEntries), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var normalized = AppConfigValidator.NormalizeExecutablePath(path);
        foreach (var e in _config.InclusionList)
        {
            if (string.Equals(AppConfigValidator.NormalizeExecutablePath(e.ExecutablePath), normalized, StringComparison.OrdinalIgnoreCase) && e.MatchKind == MatchKind.ExecutablePath)
            {
                MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorDuplicate"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        _config.InclusionList.Add(new InclusionEntry
        {
            Id = Guid.NewGuid().ToString("D"),
            ExecutablePath = normalized,
            DisplayName = Path.GetFileName(normalized),
            MatchKind = MatchKind.ExecutablePath
        });

        RefreshList();
        PersistAndApply();
    }

    private void RemoveSelected()
    {
        if (_listView.SelectedItems.Count == 0)
            return;

        var id = (string)_listView.SelectedItems[0].Tag!;
        _config.InclusionList.RemoveAll(x => x.Id == id);
        RefreshList();
        PersistAndApply();
    }

    private void PersistAndApply()
    {
        AppConfigStore.MergeDefaults(_config);
        var vr = AppConfigValidator.Validate(_config);
        if (!vr.IsValid)
        {
            var msg = UiStrings.Get("MainSettings_ErrorInvalid") + Environment.NewLine + string.Join(Environment.NewLine, vr.Errors);
            MessageBox.Show(this, msg, UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            _store.Save(_config);
            _session.ApplyConfig(_config);
            if (_config.Activation.Enabled && !_session.HookInstalled)
            {
                MessageBox.Show(this, UiStrings.Get("Program_HookFailed"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            UpdateActivationUi();
            _onConfigChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorSave") + Environment.NewLine + ex.Message, UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
