using System.Reflection;

namespace MouseScrollFixer.App;

/// <summary>
/// Versão SemVer embutida no assembly (definida em <c>Version.props</c> na raiz do repositório).
/// </summary>
internal static class AppVersion
{
    public static string Informational =>
        typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? typeof(AppVersion).Assembly.GetName().Version?.ToString()
        ?? "?";
}
