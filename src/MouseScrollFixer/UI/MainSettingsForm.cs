using System.Drawing;
using System.Globalization;
using System.Linq;
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
    private Label _activationLastModifiedLabel = null!;
    private Label _statusLabel = null!;
    private GroupBox _behaviorGroup = null!;
    private CheckBox _touchpadSameAsWheelCheck = null!;
    private CheckBox _invertVerticalCheck = null!;
    private Label _linesPerNotchLabel = null!;
    private NumericUpDown _linesPerNotchNumeric = null!;
    private Label _linesPerNotchHintLabel = null!;
    private CheckBox _useVScrollFallbackCheck = null!;
    private Label _schemaVersionLabel = null!;
    private Label _appVersionLabel = null!;
    private ListView _listView = null!;
    private Button _addButton = null!;
    private Button _removeButton = null!;
    private Button _editButton = null!;
    private bool _suppressActivationPersist;
    private bool _suppressBehaviorPersist;

    public MainSettingsForm(AppConfigStore store, AppConfig config, ScrollFixerSession session, Action? onConfigChanged = null)
    {
        _store = store;
        _config = config;
        _session = session;
        _onConfigChanged = onConfigChanged;
        InitializeComponent();
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        BuildUi();
        LoadFromConfig();
    }

    /// <summary>
    /// Sincroniza a caixa de ativação e o rótulo de estado quando o alterador é a bandeja.
    /// </summary>
    internal void SyncActivationFromConfig()
    {
        _suppressActivationPersist = true;
        _suppressBehaviorPersist = true;
        try
        {
            _activationCheckBox.Checked = _config.Activation.Enabled;
            LoadBehaviorFromConfig();
            UpdateActivationUi();
            UpdateActivationMetaUi();
        }
        finally
        {
            _suppressActivationPersist = false;
            _suppressBehaviorPersist = false;
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
        Text = UiStrings.Format("MainSettings_Title", AppVersion.Informational);

        _tabControl = new TabControl { Dock = DockStyle.Fill };
        var settingsPage = new TabPage(UiStrings.Get("MainSettings_TabSettings"));
        _tabControl.TabPages.Add(settingsPage);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
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

        _activationLastModifiedLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            ForeColor = SystemColors.ControlDarkDark,
            Margin = new Padding(18, 0, 0, 0)
        };

        _statusLabel = new Label
        {
            AutoSize = true,
            MaximumSize = new Size(680, 0),
            ForeColor = SystemColors.ControlDarkDark
        };

        _behaviorGroup = new GroupBox
        {
            Text = UiStrings.Get("MainSettings_GroupBehavior"),
            AutoSize = true,
            Dock = DockStyle.Top,
            Padding = new Padding(10, 8, 10, 8)
        };

        var behaviorLayout = new TableLayoutPanel
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            ColumnCount = 1,
            Padding = new Padding(0)
        };

        _touchpadSameAsWheelCheck = new CheckBox
        {
            AutoSize = true,
            Text = UiStrings.Get("MainSettings_TouchpadSameAsWheel"),
            Margin = new Padding(0, 4, 0, 0)
        };
        _touchpadSameAsWheelCheck.CheckedChanged += (_, _) => OnBehaviorChanged();

        _invertVerticalCheck = new CheckBox
        {
            AutoSize = true,
            Text = UiStrings.Get("MainSettings_InvertVertical"),
            Margin = new Padding(0, 4, 0, 0)
        };
        _invertVerticalCheck.CheckedChanged += (_, _) => OnBehaviorChanged();

        var linesRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 4, 0, 0)
        };
        _linesPerNotchLabel = new Label
        {
            AutoSize = true,
            Text = UiStrings.Get("MainSettings_LinesPerNotch"),
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Padding = new Padding(0, 6, 8, 0)
        };
        _linesPerNotchNumeric = new NumericUpDown
        {
            DecimalPlaces = 2,
            Increment = 0.25m,
            Minimum = 0,
            Maximum = 100,
            Width = 90
        };
        _linesPerNotchNumeric.ValueChanged += (_, _) => OnBehaviorChanged();
        _linesPerNotchHintLabel = new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.ControlDarkDark,
            Text = UiStrings.Get("MainSettings_LinesPerNotchHint"),
            Padding = new Padding(8, 6, 0, 0)
        };
        linesRow.Controls.Add(_linesPerNotchLabel);
        linesRow.Controls.Add(_linesPerNotchNumeric);
        linesRow.Controls.Add(_linesPerNotchHintLabel);

        _useVScrollFallbackCheck = new CheckBox
        {
            AutoSize = true,
            Text = UiStrings.Get("MainSettings_UseVScrollFallback"),
            Margin = new Padding(0, 8, 0, 0)
        };
        _useVScrollFallbackCheck.CheckedChanged += (_, _) => OnBehaviorChanged();

        _schemaVersionLabel = new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.ControlDarkDark,
            Margin = new Padding(0, 10, 0, 0)
        };

        behaviorLayout.Controls.Add(_touchpadSameAsWheelCheck, 0, 0);
        behaviorLayout.Controls.Add(_invertVerticalCheck, 0, 1);
        behaviorLayout.Controls.Add(linesRow, 0, 2);
        behaviorLayout.Controls.Add(_useVScrollFallbackCheck, 0, 3);
        behaviorLayout.Controls.Add(_schemaVersionLabel, 0, 4);
        _behaviorGroup.Controls.Add(behaviorLayout);

        _listView = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            HideSelection = false,
            GridLines = true
        };
        _listView.Columns.Add(UiStrings.Get("MainSettings_ColumnDisplayName"), 120);
        _listView.Columns.Add(UiStrings.Get("MainSettings_ColumnPath"), 260);
        _listView.Columns.Add(UiStrings.Get("MainSettings_ColumnMatchKind"), 130);
        _listView.Columns.Add(UiStrings.Get("MainSettings_ColumnNotes"), 160);
        _listView.SelectedIndexChanged += (_, _) => UpdateListButtonsState();

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

        _editButton = new Button
        {
            Text = UiStrings.Get("MainSettings_EditEntry"),
            AutoSize = true,
            Enabled = false
        };
        _editButton.Click += (_, _) => EditSelected();

        buttons.Controls.Add(_addButton);
        buttons.Controls.Add(_removeButton);
        buttons.Controls.Add(_editButton);

        _appVersionLabel = new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.ControlDarkDark,
            Margin = new Padding(0, 10, 0, 0),
            Text = UiStrings.Format("MainSettings_AppVersion", AppVersion.Informational)
        };

        layout.Controls.Add(_helpLabel, 0, 0);
        layout.Controls.Add(_activationCheckBox, 0, 1);
        layout.Controls.Add(_activationLastModifiedLabel, 0, 2);
        layout.Controls.Add(_statusLabel, 0, 3);
        layout.Controls.Add(_behaviorGroup, 0, 4);
        layout.Controls.Add(_listView, 0, 5);
        layout.Controls.Add(buttons, 0, 6);
        layout.Controls.Add(_appVersionLabel, 0, 7);

        settingsPage.Controls.Add(layout);
        Controls.Add(_tabControl);
    }

    private void LoadFromConfig()
    {
        // Evitar PersistAndApply durante o preenchimento: alterar Checked dispara
        // CheckedChanged; sem suprimir activation, OnActivationChanged gravaria o objeto
        // behavior a partir dos controlos ainda nos valores por defeito (apagando a config).
        _suppressActivationPersist = true;
        _suppressBehaviorPersist = true;
        try
        {
            _activationCheckBox.Checked = _config.Activation.Enabled;
            LoadBehaviorFromConfig();
            RefreshList();
            UpdateListButtonsState();
            UpdateActivationUi();
            UpdateActivationMetaUi();
        }
        finally
        {
            _suppressActivationPersist = false;
            _suppressBehaviorPersist = false;
        }
    }

    private void LoadBehaviorFromConfig()
    {
        AppConfigStore.MergeDefaults(_config);
        var b = _config.Behavior ?? BehaviorProfile.CreateDefault();
        _touchpadSameAsWheelCheck.Checked = b.TouchpadSameAsWheel;
        _invertVerticalCheck.Checked = b.InvertVertical == true;
        _linesPerNotchNumeric.Value = b.LinesPerNotchApprox is > 0 ? (decimal)b.LinesPerNotchApprox.Value : 0;
        _useVScrollFallbackCheck.Checked = b.UseVScrollFallback;
        _schemaVersionLabel.Text = UiStrings.Format("MainSettings_SchemaVersion", _config.SchemaVersion);
    }

    private void SyncBehaviorToConfig()
    {
        _config.Behavior ??= BehaviorProfile.CreateDefault();
        var b = _config.Behavior;
        b.TouchpadSameAsWheel = _touchpadSameAsWheelCheck.Checked;
        b.InvertVertical = _invertVerticalCheck.Checked ? true : false;
        var ln = (double)_linesPerNotchNumeric.Value;
        b.LinesPerNotchApprox = ln > 0 ? ln : null;
        b.UseVScrollFallback = _useVScrollFallbackCheck.Checked;
    }

    private void OnBehaviorChanged()
    {
        if (_suppressBehaviorPersist)
            return;

        SyncBehaviorToConfig();
        PersistAndApply();
    }

    private void UpdateActivationUi()
    {
        _statusLabel.Text = _config.Activation.Enabled
            ? UiStrings.Get("MainSettings_StatusFixOn")
            : UiStrings.Get("MainSettings_StatusFixOff");
    }

    private void UpdateActivationMetaUi()
    {
        var s = _config.Activation.LastModifiedUtc;
        if (string.IsNullOrWhiteSpace(s))
        {
            _activationLastModifiedLabel.Text = UiStrings.Get("MainSettings_ActivationLastModifiedUnknown");
            return;
        }

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt))
        {
            var local = dt.Kind == DateTimeKind.Utc ? dt.ToLocalTime() : dt;
            _activationLastModifiedLabel.Text = UiStrings.Format(
                "MainSettings_ActivationLastModified",
                local.ToString("g", CultureInfo.GetCultureInfo("pt-BR")));
        }
        else
        {
            _activationLastModifiedLabel.Text = s;
        }
    }

    private static string GetMatchKindDisplay(MatchKind kind) =>
        kind switch
        {
            MatchKind.ExecutablePath => UiStrings.Get("MatchKind_ExecutablePath"),
            _ => kind.ToString()
        };

    private void RefreshList()
    {
        _listView.Items.Clear();
        foreach (var e in _config.InclusionList)
        {
            var display = string.IsNullOrWhiteSpace(e.DisplayName) ? Path.GetFileName(e.ExecutablePath) : e.DisplayName!;
            var item = new ListViewItem(display);
            item.SubItems.Add(e.ExecutablePath);
            item.SubItems.Add(GetMatchKindDisplay(e.MatchKind));
            var notes = e.Notes ?? string.Empty;
            if (notes.Length > 80)
                notes = notes[..77] + "…";
            item.SubItems.Add(notes);
            item.Tag = e.Id;
            _listView.Items.Add(item);
        }
    }

    private void UpdateListButtonsState()
    {
        var sel = _listView.SelectedItems.Count > 0;
        _removeButton.Enabled = sel;
        _editButton.Enabled = sel;
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

    private void EditSelected()
    {
        if (_listView.SelectedItems.Count == 0)
            return;

        var id = (string)_listView.SelectedItems[0].Tag!;
        var entry = _config.InclusionList.FirstOrDefault(x => x.Id == id);
        if (entry is null)
            return;

        using var dlg = new InclusionEntryEditForm(entry);
        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        dlg.ApplyTo(entry);
        if (string.IsNullOrWhiteSpace(entry.ExecutablePath))
        {
            MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorPathRequired"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            entry.ExecutablePath = AppConfigValidator.NormalizeExecutablePath(entry.ExecutablePath);
        }
        catch
        {
            MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorPathInvalid"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var normalized = AppConfigValidator.NormalizeExecutablePath(entry.ExecutablePath);
        foreach (var other in _config.InclusionList)
        {
            if (other.Id == entry.Id)
                continue;
            if (string.Equals(AppConfigValidator.NormalizeExecutablePath(other.ExecutablePath), normalized, StringComparison.OrdinalIgnoreCase)
                && other.MatchKind == entry.MatchKind)
            {
                MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorDuplicate"), UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

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
        SyncBehaviorToConfig();
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
            UpdateActivationMetaUi();
            _schemaVersionLabel.Text = UiStrings.Format("MainSettings_SchemaVersion", _config.SchemaVersion);
            _onConfigChanged?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, UiStrings.Get("MainSettings_ErrorSave") + Environment.NewLine + ex.Message, UiStrings.Get("MainSettings_ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
