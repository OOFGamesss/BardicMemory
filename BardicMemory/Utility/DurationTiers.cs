using System;

namespace BardicMemory.Utility;

/// <summary>
/// The ladder of clip lengths, their points and the labels shown for them.
/// </summary>
public static class DurationTiers
{
    private static readonly float[] SecondsPerTier = [0.5f, 1f, 2f, 4f, 8f, 15f];
    private static readonly int[] PointsPerTier = [6, 5, 4, 3, 2, 1];

    public static int Count => SecondsPerTier.Length;

    public static int LastIndex => SecondsPerTier.Length - 1;

    public static float SecondsFor(int tier)
    {
        return SecondsPerTier[Math.Clamp(tier, 0, LastIndex)];
    }

    public static int PointsFor(int tier)
    {
        return PointsPerTier[Math.Clamp(tier, 0, LastIndex)];
    }
}

public static class TierLabel
{
    public static string Duration(int tier)
    {
        var seconds = DurationTiers.SecondsFor(tier);
        var text = seconds.ToString("0.##", LocalisedString.Culture);

        return LocalisedText.DurationSeconds.Format(text);
    }

    public static string Points(int tier)
    {
        return Score(DurationTiers.PointsFor(tier));
    }

    public static string Score(int points)
    {
        return points <= 0
            ? LocalisedText.NoPoints.Text
            : LocalisedText.Points.Format(points);
    }
}
