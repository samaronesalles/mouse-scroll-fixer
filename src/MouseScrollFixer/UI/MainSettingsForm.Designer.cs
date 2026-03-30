namespace MouseScrollFixer.UI;

internal sealed partial class MainSettingsForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        SuspendLayout();
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(720, 520);
        MinimumSize = new Size(560, 400);
        Name = "MainSettingsForm";
        StartPosition = FormStartPosition.CenterScreen;
        ResumeLayout(false);
    }
}
