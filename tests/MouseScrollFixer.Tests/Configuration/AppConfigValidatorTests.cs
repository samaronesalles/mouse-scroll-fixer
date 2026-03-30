using MouseScrollFixer.Core.Configuration;
using Xunit;

namespace MouseScrollFixer.Tests.Configuration;

public class AppConfigValidatorTests
{
    [Fact]
    public void Validate_NullConfig_IsInvalid()
    {
        var r = AppConfigValidator.Validate(null);
        Assert.False(r.IsValid);
        Assert.Contains("nula", r.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DefaultMergedConfig_IsValid()
    {
        var c = AppConfig.CreateDefault();
        AppConfigStore.MergeDefaults(c);
        var r = AppConfigValidator.Validate(c);
        Assert.True(r.IsValid);
    }

    [Fact]
    public void Validate_SchemaVersionZero_IsInvalid()
    {
        var c = AppConfig.CreateDefault();
        c.SchemaVersion = 0;
        AppConfigStore.MergeDefaults(c);
        var r = AppConfigValidator.Validate(c);
        Assert.False(r.IsValid);
        Assert.Contains("schemaVersion", r.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_TooManyInclusionEntries_IsInvalid()
    {
        var c = AppConfig.CreateDefault();
        c.InclusionList.Clear();
        for (var i = 0; i < AppConfigValidator.MaxInclusionEntries + 1; i++)
        {
            c.InclusionList.Add(new InclusionEntry
            {
                Id = Guid.NewGuid().ToString("D"),
                ExecutablePath = $"C:\\Apps\\App{i}.exe",
                MatchKind = MatchKind.ExecutablePath
            });
        }

        AppConfigStore.MergeDefaults(c);
        var r = AppConfigValidator.Validate(c);
        Assert.False(r.IsValid);
        Assert.Contains("64", string.Join(" ", r.Errors), StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_DuplicateExecutablePath_IsInvalid()
    {
        var c = AppConfig.CreateDefault();
        c.InclusionList =
        [
            new InclusionEntry
            {
                Id = Guid.NewGuid().ToString("D"),
                ExecutablePath = "C:\\Test\\dup.exe",
                MatchKind = MatchKind.ExecutablePath
            },
            new InclusionEntry
            {
                Id = Guid.NewGuid().ToString("D"),
                ExecutablePath = "C:\\Test\\dup.exe",
                MatchKind = MatchKind.ExecutablePath
            }
        ];
        AppConfigStore.MergeDefaults(c);
        var r = AppConfigValidator.Validate(c);
        Assert.False(r.IsValid);
        Assert.Contains("duplicado", string.Join(" ", r.Errors), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_InvalidGuidId_IsInvalid()
    {
        var c = AppConfig.CreateDefault();
        c.InclusionList =
        [
            new InclusionEntry
            {
                Id = "not-a-uuid",
                ExecutablePath = "C:\\Test\\a.exe",
                MatchKind = MatchKind.ExecutablePath
            }
        ];
        AppConfigStore.MergeDefaults(c);
        var r = AppConfigValidator.Validate(c);
        Assert.False(r.IsValid);
        Assert.Contains("UUID", string.Join(" ", r.Errors), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_NegativeLinesPerNotch_IsInvalid()
    {
        var c = AppConfig.CreateDefault();
        c.Behavior = new BehaviorProfile { TouchpadSameAsWheel = true, LinesPerNotchApprox = -1 };
        AppConfigStore.MergeDefaults(c);
        var r = AppConfigValidator.Validate(c);
        Assert.False(r.IsValid);
        Assert.Contains("linesPerNotchApprox", string.Join(" ", r.Errors), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void NormalizeExecutablePath_TrimsAndFullPath()
    {
        var temp = Path.Combine(Path.GetTempPath(), "msf_norm_test.exe");
        var normalized = AppConfigValidator.NormalizeExecutablePath(" " + temp + " ");
        Assert.Equal(Path.GetFullPath(temp), normalized, StringComparer.OrdinalIgnoreCase);
    }
}
