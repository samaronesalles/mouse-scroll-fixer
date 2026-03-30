using System.Globalization;
using System.Resources;

namespace MouseScrollFixer.UI.Resources;

internal static class UiStrings
{
    private static readonly ResourceManager Manager = new("MouseScrollFixer.UI.Resources.Strings", typeof(UiStrings).Assembly);
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public static string Get(string name) => Manager.GetString(name, PtBr) ?? name;

    public static string Format(string name, params object[] args) =>
        string.Format(PtBr, Get(name), args);
}
