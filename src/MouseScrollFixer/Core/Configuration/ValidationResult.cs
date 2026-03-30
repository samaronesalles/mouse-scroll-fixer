namespace MouseScrollFixer.Core.Configuration;

internal sealed class ValidationResult
{
    public ValidationResult(bool isValid, IReadOnlyList<string> errors)
    {
        IsValid = isValid;
        Errors = errors;
    }

    public bool IsValid { get; }

    public IReadOnlyList<string> Errors { get; }

    public static ValidationResult Success() => new(true, Array.Empty<string>());

    public static ValidationResult Failure(IReadOnlyList<string> errors) => new(false, errors);
}
