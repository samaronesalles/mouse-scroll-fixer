namespace MouseScrollFixer.Core.ConflictDetection;

/// <summary>
/// Resultado de heurística leve (RF-007): não implica precedência automática nem desativação de software.
/// </summary>
internal readonly record struct ConflictDetectionResult(bool HasConflict, IReadOnlyList<string> MatchedProcessNames);
