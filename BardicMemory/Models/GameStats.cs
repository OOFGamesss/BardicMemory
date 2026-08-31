using System;

namespace BardicMemory.Models;

/// <summary>
/// The running totals across every round played.
/// </summary>
[Serializable]
public class GameStats
{
    public int RoundsPlayed { get; set; }

    public int RoundsSolved { get; set; }

    public int TotalPoints { get; set; }

    public int CurrentStreak { get; set; }

    public int BestStreak { get; set; }

    public float AverageScore => RoundsPlayed == 0 ? 0f : (float)TotalPoints / RoundsPlayed;

    public float SolveRate => RoundsPlayed == 0 ? 0f : (float)RoundsSolved / RoundsPlayed;
}
