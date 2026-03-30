using System.Diagnostics;

namespace MouseScrollFixer.Core.ConflictDetection;

/// <summary>
/// Heurística documentada: nomes de processo frequentemente associados a remapeamento de rato ou scroll.
/// Falsos positivos/negativos são possíveis; o utilizador decide (RF-007).
/// </summary>
internal static class ConflictDetector
{
    /// <summary>
    /// Nomes de processo (sem .exe) para <see cref="Process.GetProcessesByName(string)"/>.
    /// </summary>
    private static readonly string[] KnownProcessNames =
    [
        "XMouseButtonControl",
        "AutoHotkey",
        "AutoHotkeyU64",
        "AutoHotkeyA32",
        "PowerToys",
        "LGHUB",
        "LogiOptions",
        "logioptionsplus",
    ];

    /// <summary>
    /// Deteta se algum processo conhecido está em execução.
    /// </summary>
    public static ConflictDetectionResult Detect()
    {
        var matched = new List<string>();

        foreach (var name in KnownProcessNames)
        {
            Process[]? processes = null;
            try
            {
                processes = Process.GetProcessesByName(name);
                if (processes.Length > 0)
                    matched.Add(name);
            }
            catch
            {
                // ignorar processos inacessíveis
            }
            finally
            {
                if (processes is not null)
                {
                    foreach (var p in processes)
                    {
                        try
                        {
                            p.Dispose();
                        }
                        catch
                        {
                            // ignorar
                        }
                    }
                }
            }
        }

        return new ConflictDetectionResult(matched.Count > 0, matched);
    }
}
