namespace MouseScrollFixer.Core.Configuration;

internal static class AppConfigValidator
{
    public const int MaxInclusionEntries = 64;

    public static ValidationResult Validate(AppConfig? config)
    {
        var errors = new List<string>();

        if (config is null)
        {
            errors.Add("A configuração não pode ser nula.");
            return ValidationResult.Failure(errors);
        }

        if (config.SchemaVersion < 1)
            errors.Add("O campo schemaVersion deve ser maior ou igual a 1.");

        if (config.Activation is null)
            errors.Add("O objeto activation é obrigatório.");

        if (config.InclusionList is null)
            errors.Add("A lista inclusionList é obrigatória.");
        else
        {
            if (config.InclusionList.Count > MaxInclusionEntries)
                errors.Add($"A lista de inclusão não pode ter mais de {MaxInclusionEntries} entradas.");

            ValidateInclusionEntries(config.InclusionList, errors);
        }

        if (config.Behavior is not null)
            ValidateBehavior(config.Behavior, errors);

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure(errors);
    }

    private static void ValidateBehavior(BehaviorProfile behavior, List<string> errors)
    {
        if (behavior.LinesPerNotchApprox is < 0)
            errors.Add("O campo behavior.linesPerNotchApprox não pode ser negativo.");
    }

    private static void ValidateInclusionEntries(IReadOnlyList<InclusionEntry> entries, List<string> errors)
    {
        var seen = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            var prefix = $"inclusionList[{i}]";

            if (entry is null)
            {
                errors.Add($"{prefix}: a entrada não pode ser nula.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(entry.Id))
                errors.Add($"{prefix}: o campo id é obrigatório.");
            else if (!Guid.TryParse(entry.Id, out _))
                errors.Add($"{prefix}: o campo id deve ser um UUID válido.");

            if (string.IsNullOrWhiteSpace(entry.ExecutablePath))
                errors.Add($"{prefix}: o campo executablePath é obrigatório.");

            if (!Enum.IsDefined(typeof(MatchKind), entry.MatchKind))
                errors.Add($"{prefix}: o campo matchKind tem um valor inválido.");

            if (string.IsNullOrWhiteSpace(entry.ExecutablePath) || !Enum.IsDefined(typeof(MatchKind), entry.MatchKind))
                continue;

            var key = BuildDuplicateKey(entry.ExecutablePath, entry.MatchKind);
            if (seen.TryGetValue(key, out var firstIndex))
                errors.Add($"{prefix}: caminho de executável duplicado (mesmo matchKind) em relação a inclusionList[{firstIndex}].");
            else
                seen[key] = i;
        }
    }

    private static string BuildDuplicateKey(string executablePath, MatchKind matchKind)
    {
        var normalized = NormalizeExecutablePath(executablePath);
        return $"{(int)matchKind}:{normalized}";
    }

    internal static string NormalizeExecutablePath(string executablePath)
    {
        var trimmed = executablePath.Trim();
        try
        {
            return Path.GetFullPath(trimmed);
        }
        catch
        {
            return trimmed;
        }
    }
}
