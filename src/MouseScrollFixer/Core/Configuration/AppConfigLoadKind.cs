namespace MouseScrollFixer.Core.Configuration;

internal enum AppConfigLoadKind
{
    /// <summary>
    /// Não existia ficheiro principal; foram usados valores padrão.
    /// </summary>
    DefaultNew,

    /// <summary>
    /// Leitura e validação do ficheiro principal com sucesso.
    /// </summary>
    Loaded,

    /// <summary>
    /// Ficheiro principal inválido; recuperado a partir da cópia de segurança.
    /// </summary>
    RecoveredFromBackup,

    /// <summary>
    /// Configuração inválida ou corrompida; aplicados valores seguros (fix desligado, lista vazia).
    /// </summary>
    CorruptedUsedSafeDefaults
}
