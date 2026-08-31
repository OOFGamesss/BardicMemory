using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace BardicMemory.GameData;

/// <summary>
/// Builds the song pool and its categories from the orchestrion mapping data.
/// </summary>
public readonly record struct Song(
    uint RollId,
    ushort BgmId,
    string Name,
    string FilePath,
    uint CategoryId,
    string Description,
    string CategoryName,
    string Locations)
{
    public string Hint
    {
        get
        {
            if (Description.Length > 0)
            {
                return Description;
            }

            return Locations.Length > 0 ? Locations : CategoryName;
        }
    }
}

public readonly record struct SongCategory(uint Id, string Name, int SongCount)
{
    public const uint AllId = uint.MaxValue;
}

public sealed class SongRepository
{
    private static readonly string[] PlaceholderTitles = ["Null BGM", "test"];

    private readonly List<Song> songs = [];
    private readonly List<SongCategory> categories = [];

    private int withoutBgmId;

    public SongRepository()
    {
        var categoryNames = ReadCategoryNames();

        LoadSongs(categoryNames, new SongLocationIndex(), new BgmIdIndex());
        LoadCategories(categoryNames);

        Plugin.Log.Information(
            $"Song pool built: {songs.Count} tracks across {categories.Count - 1} categories.");

        if (withoutBgmId > 0)
        {
            Plugin.Log.Information(
                $"{withoutBgmId} rolls have no background music id and were left out. "
                + "Regenerate Data/OrchestrionBgmMap.csv if a patch added songs.");
        }
    }

    public IReadOnlyList<Song> Songs => songs;

    public IReadOnlyList<SongCategory> Categories => categories;

    public bool IsEmpty => songs.Count == 0;

    public IReadOnlyList<Song> InCategory(uint categoryId)
    {
        if (categoryId == SongCategory.AllId)
        {
            return songs;
        }

        return songs.Where(song => song.CategoryId == categoryId).ToList();
    }

    private static Dictionary<uint, string> ReadCategoryNames()
    {
        var names = new Dictionary<uint, string>();

        foreach (var row in Plugin.DataManager.GetExcelSheet<OrchestrionCategory>())
        {
            var name = row.Name.ExtractText().Trim();
            if (!string.IsNullOrWhiteSpace(name))
            {
                names[row.RowId] = name;
            }
        }

        return names;
    }

    private void LoadSongs(
        IReadOnlyDictionary<uint, string> categoryNames,
        SongLocationIndex locations,
        BgmIdIndex bgmIds)
    {
        var paths = Plugin.DataManager.GetExcelSheet<OrchestrionPath>();
        var uiParams = Plugin.DataManager.GetExcelSheet<OrchestrionUiparam>();

        foreach (var roll in Plugin.DataManager.GetExcelSheet<Orchestrion>())
        {
            var name = roll.Name.ExtractText().Trim();
            if (string.IsNullOrWhiteSpace(name) || PlaceholderTitles.Contains(name, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!paths.TryGetRow(roll.RowId, out var path))
            {
                continue;
            }

            var file = path.File.ExtractText();
            if (file.Length == 0 || !Plugin.DataManager.FileExists(file))
            {
                continue;
            }

            var bgmId = bgmIds.For(roll.RowId);
            if (bgmId == 0)
            {
                withoutBgmId++;
                continue;
            }

            var categoryId = uiParams.TryGetRow(roll.RowId, out var uiParam)
                ? uiParam.OrchestrionCategory.RowId
                : 0u;

            var words = roll.Description.ExtractText().Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            songs.Add(new Song(
                roll.RowId,
                bgmId,
                name,
                file,
                categoryId,
                string.Join(' ', words),
                categoryNames.GetValueOrDefault(categoryId, string.Empty),
                locations.For(file)));
        }

        songs.Sort((left, right) => string.CompareOrdinal(left.Name, right.Name));
    }

    private void LoadCategories(IReadOnlyDictionary<uint, string> categoryNames)
    {
        categories.Add(new SongCategory(SongCategory.AllId, "All", songs.Count));

        var counts = songs.GroupBy(song => song.CategoryId)
                          .ToDictionary(group => group.Key, group => group.Count());

        foreach (var row in Plugin.DataManager.GetExcelSheet<OrchestrionCategory>().OrderBy(row => row.Order))
        {
            if (!counts.TryGetValue(row.RowId, out var count) || count == 0)
            {
                continue;
            }

            if (categoryNames.TryGetValue(row.RowId, out var name))
            {
                categories.Add(new SongCategory(row.RowId, name, count));
            }
        }
    }
}
