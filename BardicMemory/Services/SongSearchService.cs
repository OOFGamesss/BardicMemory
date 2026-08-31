using System;
using System.Collections.Generic;
using System.Linq;
using BardicMemory.GameData;
using BardicMemory.Utility;

namespace BardicMemory.Services;

/// <summary>
/// The ranked lookup behind the search box and one row of its list.
/// </summary>
public enum MatchRank
{
    NameStartsWith,
    LooseNameStartsWith,
    NameContains,
    LooseNameContains,
    HintStartsWith,
    HintContains,
    LocationStartsWith,
    LocationContains,
    NoMatch,
}

public readonly record struct SongMatch(Song Song, string Hint);

public sealed class SongSearchService
{
    private const int MaxResults = 40;
    private const int MinimumHintLength = 3;

    private readonly SongRepository repository;
    private readonly SongNameIndex names;
    private readonly Dictionary<uint, string> normalisedHints;
    private readonly Dictionary<uint, string> normalisedLocations;

    private string cachedQuery = string.Empty;
    private IReadOnlyList<SongMatch> cachedResults = [];

    public SongSearchService(SongRepository repository)
    {
        this.repository = repository;

        names = new SongNameIndex();
        normalisedHints = Index(repository, song => $"{song.Description} {song.CategoryName}");
        normalisedLocations = Index(repository, song => song.Locations);
    }

    private static Dictionary<uint, string> Index(SongRepository repository, Func<Song, string> field)
    {
        return repository.Songs.ToDictionary(song => song.RollId, song => SongNameMatcher.Normalise(field(song)));
    }

    public IReadOnlyList<SongMatch> Search(string query)
    {
        if (query == cachedQuery)
        {
            return cachedResults;
        }

        cachedQuery = query;
        cachedResults = Rank(query);

        return cachedResults;
    }

    public Song? ExactMatch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        foreach (var song in repository.Songs)
        {
            if (SongNameMatcher.Matches(song.Name, query))
            {
                return song;
            }
        }

        return null;
    }

    private IReadOnlyList<SongMatch> Rank(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var normalised = SongNameMatcher.Normalise(query);
        if (normalised.Length == 0)
        {
            return [];
        }

        var ranked = new List<(Song Song, MatchRank Rank)>();

        foreach (var song in repository.Songs)
        {
            var rank = RankOf(song, query, normalised);
            if (rank != MatchRank.NoMatch)
            {
                ranked.Add((song, rank));
            }
        }

        ranked.Sort(ByRankThenName);

        if (ranked.Count > MaxResults)
        {
            ranked.RemoveRange(MaxResults, ranked.Count - MaxResults);
        }

        var results = new List<SongMatch>(ranked.Count);

        foreach (var (song, rank) in ranked)
        {
            results.Add(new SongMatch(song, HintFor(song, rank)));
        }

        return results;
    }

    private static int ByRankThenName((Song Song, MatchRank Rank) left, (Song Song, MatchRank Rank) right)
    {
        if (left.Rank != right.Rank)
        {
            return left.Rank.CompareTo(right.Rank);
        }

        return string.Compare(left.Song.Name, right.Song.Name, StringComparison.OrdinalIgnoreCase);
    }

    private static string HintFor(Song song, MatchRank rank)
    {
        if (rank == MatchRank.LocationStartsWith || rank == MatchRank.LocationContains)
        {
            return song.Locations;
        }

        return song.Hint;
    }

    private MatchRank RankOf(Song song, string rawQuery, string normalisedQuery)
    {
        if (song.Name.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
        {
            return MatchRank.NameStartsWith;
        }

        var titles = names.For(song.RollId);

        if (AnyTitleMatches(titles, normalisedQuery, fromStart: true))
        {
            return MatchRank.LooseNameStartsWith;
        }

        if (song.Name.Contains(rawQuery, StringComparison.OrdinalIgnoreCase))
        {
            return MatchRank.NameContains;
        }

        if (AnyTitleMatches(titles, normalisedQuery, fromStart: false))
        {
            return MatchRank.LooseNameContains;
        }

        if (normalisedQuery.Length < MinimumHintLength)
        {
            return MatchRank.NoMatch;
        }

        return HintRankOf(song, normalisedQuery);
    }

    private static bool AnyTitleMatches(string[] titles, string normalisedQuery, bool fromStart)
    {
        foreach (var title in titles)
        {
            var matched = fromStart
                ? title.StartsWith(normalisedQuery, StringComparison.Ordinal)
                : title.Contains(normalisedQuery, StringComparison.Ordinal);

            if (matched)
            {
                return true;
            }
        }

        return false;
    }

    private MatchRank HintRankOf(Song song, string normalisedQuery)
    {
        var hint = normalisedHints[song.RollId];

        if (hint.StartsWith(normalisedQuery, StringComparison.Ordinal))
        {
            return MatchRank.HintStartsWith;
        }

        if (hint.Contains(normalisedQuery, StringComparison.Ordinal))
        {
            return MatchRank.HintContains;
        }

        var places = normalisedLocations[song.RollId];

        if (places.StartsWith(normalisedQuery, StringComparison.Ordinal))
        {
            return MatchRank.LocationStartsWith;
        }

        return places.Contains(normalisedQuery, StringComparison.Ordinal)
            ? MatchRank.LocationContains
            : MatchRank.NoMatch;
    }
}
