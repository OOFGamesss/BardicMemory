using BardicMemory.GameData;
using BardicMemory.Utility;

namespace BardicMemory.State;

/// <summary>
/// The state of the round in play and the enums describing where it stands.
/// </summary>
public enum RoundOutcome
{
    Idle,
    InProgress,
    Solved,
    GaveUp,
}

public enum TierStatus
{
    Locked,
    Active,
    Skipped,
    Solved,
    Revealed,
}

public sealed class RoundState
{
    public Song? Song { get; set; }

    public int Tier { get; set; }

    public RoundOutcome Outcome { get; set; } = RoundOutcome.Idle;

    public string LastWrongGuess { get; set; } = string.Empty;

    public int Score { get; set; }

    public bool IsPlaying => Outcome == RoundOutcome.InProgress;

    public bool IsFinished => Outcome is RoundOutcome.Solved or RoundOutcome.GaveUp;

    public float ClipSeconds => DurationTiers.SecondsFor(Tier);

    public TierStatus StatusOf(int tier)
    {
        if (tier < Tier)
        {
            return TierStatus.Skipped;
        }

        if (tier > Tier)
        {
            return TierStatus.Locked;
        }

        return Outcome switch
        {
            RoundOutcome.Solved => TierStatus.Solved,
            RoundOutcome.GaveUp => TierStatus.Revealed,
            RoundOutcome.InProgress => TierStatus.Active,
            _ => TierStatus.Locked,
        };
    }

    public void Reset()
    {
        Song = null;
        Tier = 0;
        Outcome = RoundOutcome.Idle;
        LastWrongGuess = string.Empty;
        Score = 0;
    }
}
