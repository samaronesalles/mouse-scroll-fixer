using Microsoft.Win32;

namespace MouseScrollFixer.Core.Startup;

internal static class WindowsStartupRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "MouseScrollFixer";

    public static void Register(string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

        var normalized = NormalizePath(executablePath);
        var value = QuotePath(normalized);

        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        key?.SetValue(ValueName, value);
    }

    public static void Unregister()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(ValueName, throwOnMissingValue: false);
    }

    public static bool IsRegistered(string executablePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

        var registered = TryGetRegisteredPath();
        return registered is not null && PathsEqual(registered, executablePath);
    }

    public static string? TryGetRegisteredPath() =>
        Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false)?.GetValue(ValueName) as string;

    internal static string NormalizePath(string path)
    {
        var trimmed = path.Trim().Trim('"');
        return Path.GetFullPath(trimmed);
    }

    internal static bool PathsEqual(string registeredValue, string executablePath) =>
        string.Equals(
            NormalizePath(registeredValue),
            NormalizePath(executablePath),
            StringComparison.OrdinalIgnoreCase);

    internal static string QuotePath(string normalizedPath) =>
        normalizedPath.Contains(' ') ? $"\"{normalizedPath}\"" : normalizedPath;
}
