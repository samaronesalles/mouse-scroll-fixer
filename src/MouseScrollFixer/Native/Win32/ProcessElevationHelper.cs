using System.ComponentModel;
using System.Diagnostics;
using System.Security.Principal;

namespace MouseScrollFixer.Native.Win32;

internal enum ElevationAttemptResult
{
    NotRequested,
    AlreadyElevated,
    ElevatedInstanceStarted,
    UserCancelled,
    Failed
}

internal static class ProcessElevationHelper
{
    private const int ErrorCancelled = 1223;

    public static bool IsProcessElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static ElevationAttemptResult TryStartElevatedInstance(string executablePath, IEnumerable<string>? arguments = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            var args = arguments?.ToArray() ?? [];
            if (args.Length > 0)
                psi.Arguments = string.Join(" ", args.Select(QuoteArgument));

            Process.Start(psi);
            return ElevationAttemptResult.ElevatedInstanceStarted;
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == ErrorCancelled)
        {
            return ElevationAttemptResult.UserCancelled;
        }
        catch (Win32Exception)
        {
            return ElevationAttemptResult.Failed;
        }
        catch (InvalidOperationException)
        {
            return ElevationAttemptResult.Failed;
        }
    }

    private static string QuoteArgument(string arg) =>
        arg.Contains(' ') || arg.Contains('"') ? $"\"{arg.Replace("\"", "\\\"", StringComparison.Ordinal)}\"" : arg;
}
