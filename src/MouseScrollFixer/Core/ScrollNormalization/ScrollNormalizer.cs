using MouseScrollFixer.Core.Configuration;
using MouseScrollFixer.Native.Win32;

namespace MouseScrollFixer.Core.ScrollNormalization;

/// <summary>
/// Normalização do scroll vertical (<see cref="Win32Constants.WM_MOUSEWHEEL"/>).
/// O scroll horizontal (<see cref="Win32Constants.WM_MOUSEHWHEEL"/>) fica fora do MVP.
/// </summary>
internal static class ScrollNormalizer
{
    /// <summary>
    /// Unidade documentada pelo Win32 para um “clique” de roda (<c>WHEEL_DELTA</c>).
    /// </summary>
    public const int WheelDeltaUnit = (int)Win32Constants.WHEEL_DELTA;

    /// <summary>
    /// Extrai o delta vertical assinado (high word de <paramref name="mouseData"/>).
    /// </summary>
    public static int GetVerticalWheelDelta(uint mouseData)
    {
        return (short)((mouseData >> 16) & 0xFFFF);
    }

    /// <summary>
    /// Substitui o high word de <paramref name="mouseData"/> pelo delta vertical assinado.
    /// </summary>
    public static uint SetVerticalWheelDelta(uint mouseData, int delta)
    {
        return (mouseData & 0xFFFF) | (uint)(ushort)(short)delta << 16;
    }

    /// <summary>
    /// Converte o delta bruto num passo previsível em múltiplos de <see cref="WheelDeltaUnit"/>,
    /// respeitando <see cref="BehaviorProfile.LinesPerNotchApprox"/> e <see cref="BehaviorProfile.InvertVertical"/>.
    /// </summary>
    public static int NormalizeVerticalWheelDelta(int rawDelta, BehaviorProfile? behavior)
    {
        if (rawDelta == 0)
            return 0;

        var d = rawDelta;
        if (behavior?.InvertVertical == true)
            d = -d;

        var lines = behavior?.LinesPerNotchApprox is > 0 ? behavior.LinesPerNotchApprox!.Value : 1.0;

        var sign = Math.Sign(d);
        var magnitude = Math.Abs(d);
        var notches = Math.Max(1, (int)Math.Round(magnitude / (double)WheelDeltaUnit));
        var scaled = (int)Math.Round(notches * lines * WheelDeltaUnit);
        return sign * Math.Max(WheelDeltaUnit, scaled);
    }
}
