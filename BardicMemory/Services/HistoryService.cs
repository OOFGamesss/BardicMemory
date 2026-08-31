using System;
using System.Collections.Generic;
using System.Linq;
using BardicMemory.Config;
using BardicMemory.GameData;
using BardicMemory.Models;
using BardicMemory.State;

namespace BardicMemory.Services;

/// <summary>
/// Records finished rounds and keeps the running totals in step.
/// </summary>
public sealed class HistoryService(Configuration configuration)
{
    private IReadOnlyList<HistoryEntry>? newestCache;

    public GameStats Stats => configuration.Stats;

    public IReadOnlyList<HistoryEntry> Newest =>
        newestCache ??= Enumerable.Reverse(configuration.History).ToList();

    public HashSet<uint> SolvedRollIds()
    {
        var solved = new HashSet<uint>();

        foreach (var entry in configuration.History)
        {
            if (entry.Solved)
            {
                solved.Add(entry.RollId);
            }
        }

        return solved;
    }

    public void Record(Song song, RoundState round)
    {
        var entry = new HistoryEntry
        {
            RollId = song.RollId,
            SongName = song.Name,
            Score = round.Score,
            Tier = round.Tier,
            Solved = round.Outcome == RoundOutcome.Solved,
            CompletedUtc = DateTime.UtcNow,
        };

        configuration.History.Add(entry);

        var excess = configuration.History.Count - configuration.HistoryLimit;
        if (excess > 0)
        {
            configuration.History.RemoveRange(0, excess);
        }

        FoldIntoStats(entry);
        newestCache = null;
        configuration.Save();
    }

    private void FoldIntoStats(HistoryEntry entry)
    {
        var stats = configuration.Stats;
        stats.RoundsPlayed++;
        stats.TotalPoints += entry.Score;

        if (!entry.Solved)
        {
            stats.CurrentStreak = 0;
            return;
        }

        stats.RoundsSolved++;
        stats.CurrentStreak++;

        if (stats.CurrentStreak > stats.BestStreak)
        {
            stats.BestStreak = stats.CurrentStreak;
        }
    }

    public void Clear()
    {
        configuration.History.Clear();
        configuration.Stats = new GameStats();
        newestCache = null;
        configuration.Save();
    }
}
