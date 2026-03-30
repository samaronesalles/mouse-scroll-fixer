using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.UI.Resources;

namespace MouseScrollFixer.UI;

/// <summary>
/// Edição de uma entrada da lista de inclusão (campos persistidos em JSON).
/// </summary>
internal sealed class InclusionEntryEditForm : Form
{
    private readonly TextBox _displayNameBox;
    private readonly TextBox _pathBox;
    private readonly ComboBox _matchKindCombo;
    private readonly TextBox _notesBox;

    public InclusionEntryEditForm(InclusionEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        Text = UiStrings.Get("InclusionEntryEdit_Title");
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(520, 380);
        Padding = new Padding(12);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var fields = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            AutoSize = true
        };
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        fields.Controls.Add(new Label { Text = UiStrings.Get("InclusionEntryEdit_DisplayName"), AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
        _displayNameBox = new TextBox { Dock = DockStyle.Fill, Text = entry.DisplayName ?? string.Empty };
        fields.Controls.Add(_displayNameBox, 1, 0);

        fields.Controls.Add(new Label { Text = UiStrings.Get("InclusionEntryEdit_ExecutablePath"), AutoSize = true, Anchor = AnchorStyles.Left }, 0, 1);
        _pathBox = new TextBox { Dock = DockStyle.Fill, Text = entry.ExecutablePath ?? string.Empty };
        fields.Controls.Add(_pathBox, 1, 1);

        fields.Controls.Add(new Label { Text = UiStrings.Get("InclusionEntryEdit_MatchKind"), AutoSize = true, Anchor = AnchorStyles.Left }, 0, 2);
        _matchKindCombo = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
        foreach (MatchKind mk in Enum.GetValues<MatchKind>())
            _matchKindCombo.Items.Add(GetMatchKindDisplay(mk));
        var idx = (int)entry.MatchKind;
        if (idx >= 0 && idx < _matchKindCombo.Items.Count)
            _matchKindCombo.SelectedIndex = idx;
        fields.Controls.Add(_matchKindCombo, 1, 2);

        fields.Controls.Add(new Label { Text = UiStrings.Get("InclusionEntryEdit_Notes"), AutoSize = true, Anchor = AnchorStyles.Left | AnchorStyles.Top }, 0, 3);
        _notesBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Text = entry.Notes ?? string.Empty,
            MinimumSize = new Size(0, 100)
        };
        fields.Controls.Add(_notesBox, 1, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Padding = new Padding(0, 12, 0, 0)
        };
        var ok = new Button { Text = UiStrings.Get("InclusionEntryEdit_OK"), DialogResult = DialogResult.OK };
        var cancel = new Button { Text = UiStrings.Get("InclusionEntryEdit_Cancel"), DialogResult = DialogResult.Cancel };
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        AcceptButton = ok;
        CancelButton = cancel;

        root.Controls.Add(fields, 0, 0);
        root.Controls.Add(buttons, 0, 1);
        Controls.Add(root);
    }

    private static string GetMatchKindDisplay(MatchKind kind) =>
        kind switch
        {
            MatchKind.ExecutablePath => UiStrings.Get("MatchKind_ExecutablePath"),
            _ => kind.ToString()
        };

    public void ApplyTo(InclusionEntry entry)
    {
        entry.DisplayName = string.IsNullOrWhiteSpace(_displayNameBox.Text) ? null : _displayNameBox.Text.Trim();
        entry.ExecutablePath = _pathBox.Text.Trim();
        entry.MatchKind = (MatchKind)Math.Max(0, _matchKindCombo.SelectedIndex);
        entry.Notes = string.IsNullOrWhiteSpace(_notesBox.Text) ? null : _notesBox.Text.Trim();
    }
}
