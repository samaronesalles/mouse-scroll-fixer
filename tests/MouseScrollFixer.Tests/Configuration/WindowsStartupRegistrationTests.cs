using MouseScrollFixer.Core.Startup;
using Xunit;

namespace MouseScrollFixer.Tests.Configuration;

public class WindowsStartupRegistrationTests
{
    [Fact]
    public void NormalizePath_TrimsQuotesAndReturnsFullPath()
    {
        var temp = Path.Combine(Path.GetTempPath(), "msf_startup_test.exe");
        var normalized = WindowsStartupRegistration.NormalizePath($" \"{temp}\" ");
        Assert.Equal(Path.GetFullPath(temp), normalized, StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void PathsEqual_IgnoresCaseAndQuotes()
    {
        var path = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "MouseScrollFixer.exe"));
        var quoted = $"\"{path}\"";
        Assert.True(WindowsStartupRegistration.PathsEqual(quoted, path));
        Assert.True(WindowsStartupRegistration.PathsEqual(path.ToUpperInvariant(), path.ToLowerInvariant()));
    }

    [Fact]
    public void PathsEqual_DifferentPaths_ReturnsFalse()
    {
        var a = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "a.exe"));
        var b = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "b.exe"));
        Assert.False(WindowsStartupRegistration.PathsEqual(a, b));
    }

    [Fact]
    public void QuotePath_QuotesWhenSpacesPresent()
    {
        var quoted = WindowsStartupRegistration.QuotePath(@"C:\Program Files\App.exe");
        Assert.Equal("\"C:\\Program Files\\App.exe\"", quoted);
    }

    [Fact]
    public void QuotePath_NoSpaces_Unquoted()
    {
        var quoted = WindowsStartupRegistration.QuotePath(@"C:\Apps\App.exe");
        Assert.Equal(@"C:\Apps\App.exe", quoted);
    }
}
