using System;
using System.Collections.Generic;
using System.Linq;
using BardicMemory.Config;
using BardicMemory.GameData;
using BardicMemory.State;
using BardicMemory.Utility;

namespace BardicMemory.Services;

/// <summary>
/// The rules of the game. Picks songs, judges guesses and closes rounds and games out.
/// </summary>
public sealed class RoundService(
    Configuration configuration,
    SongRepository repository,
    PlaybackService playback,
    HistoryService history)
{
    private readonly Random random = new();

    private string explicitError = string.Empty;

    public RoundState Round { get; } = new();

    public bool IsGameActive { get; private set; }

    public string LastError => explicitError.Length > 0 ? explicitError : playback.BlockedReason;

    public bool CanRaiseTier => Round.IsPlaying && Round.Tier < DurationTiers.LastIndex;

    public bool IsClipPlaying => playback.IsPlaying;

    public float ClipProgressSeconds => playback.ProgressSeconds;

    public void StartRound()
    {
        var candidates = CandidateSongs();
        if (candidates.Count == 0)
        {
            explicitError = "No songs available in this category.";
            return;
        }

        explicitError = string.Empty;

        if (!playback.BeginRound())
        {
            return;
        }

        Round.Reset();
        Round.Song = candidates[random.Next(candidates.Count)];
        Round.Outcome = RoundOutcome.InProgress;
        IsGameActive = true;

        PlayCurrentClip();
    }

    private IReadOnlyList<Song> CandidateSongs()
    {
        var pool = repository.InCategory(configuration.SelectedCategoryId);
        if (!configuration.ExcludeSolvedSongs)
        {
            return pool;
        }

        var solved = history.SolvedRollIds();
        var unsolved = pool.Where(song => !solved.Contains(song.RollId)).ToList();

        return unsolved.Count > 0 ? unsolved : pool;
    }

    public void PlayCurrentClip()
    {
        if (Round.Song is not { } song)
        {
            return;
        }

        playback.PlayClip(song.BgmId, Round.ClipSeconds);
    }

    public void PlayWholeTrack()
    {
        if (Round.Song is not { } song)
        {
            return;
        }

        playback.PlayWhole(song.BgmId);
    }

    public void StopPlayback() => playback.StopAndRewind();

    public void ApplyClipVolume() => playback.ApplyClipVolume();

    public void RaiseTier()
    {
        if (!CanRaiseTier)
        {
            return;
        }

        Round.Tier++;
        Round.LastWrongGuess = string.Empty;

        if (!playback.TryExtendClip(Round.ClipSeconds))
        {
            PlayCurrentClip();
        }
    }

    public bool SubmitGuess(Song guess)
    {
        if (Round.Song is not { } song || !Round.IsPlaying)
        {
            return false;
        }

        var correct = guess.RollId == song.RollId || SongNameMatcher.Matches(guess.Name, song.Name);
        if (!correct)
        {
            Round.LastWrongGuess = guess.Name;
            return false;
        }

        Round.Score = DurationTiers.PointsFor(Round.Tier);
        Finish(RoundOutcome.Solved, song);
        return true;
    }

    public void GiveUp()
    {
        if (Round.Song is not { } song || !Round.IsPlaying)
        {
            return;
        }

        Round.Score = 0;
        Finish(RoundOutcome.GaveUp, song);
    }

    public void EndGame()
    {
        IsGameActive = false;
        explicitError = string.Empty;
        Round.Reset();
        playback.EndRound();
    }

    private void Finish(RoundOutcome outcome, Song song)
    {
        Round.Outcome = outcome;
        Round.LastWrongGuess = string.Empty;
        playback.StopClip();
        history.Record(song, Round);
    }
}
