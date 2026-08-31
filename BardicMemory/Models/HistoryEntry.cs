using System;

namespace BardicMemory.Models;

/// <summary>
/// Finished rounds get saved and listed on the history tab.
/// </summary>
[Serializable]
public class HistoryEntry
{
    public uint RollId { get; set; }

    public string SongName { get; set; } = string.Empty;

    public int Score { get; set; }

    public int Tier { get; set; }

    public bool Solved { get; set; }

    public DateTime CompletedUtc { get; set; } = DateTime.UtcNow;
}
