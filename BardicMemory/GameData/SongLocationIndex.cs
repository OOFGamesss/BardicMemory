using System;
using System.Collections.Generic;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace BardicMemory.GameData;

/// <summary>
/// Maps a track's audio file to the zones and duties it plays in.
/// </summary>
public sealed class SongLocationIndex
{
    private readonly Dictionary<string, List<string>> byFile = new(StringComparer.OrdinalIgnoreCase);

    public SongLocationIndex()
    {
        ReadTerritories();
        ReadDuties();
    }

    public string For(string filePath)
    {
        return byFile.TryGetValue(filePath, out var names) ? string.Join(", ", names) : string.Empty;
    }

    private void ReadTerritories()
    {
        foreach (var territory in Plugin.DataManager.GetExcelSheet<TerritoryType>())
        {
            var names = new List<string>(2);
            Append(names, territory.PlaceName.ValueNullable?.Name);
            Append(names, territory.ContentFinderCondition.ValueNullable?.Name);

            if (names.Count == 0)
            {
                continue;
            }

            foreach (var file in FilesOf(territory.BGM))
            {
                foreach (var name in names)
                {
                    Add(file, name);
                }
            }
        }
    }

    private static void Append(List<string> names, ReadOnlySeString? value)
    {
        var text = value?.ExtractText().Trim() ?? string.Empty;
        if (text.Length > 0)
        {
            names.Add(text);
        }
    }

    private static IEnumerable<string> FilesOf(RowRef reference)
    {
        if (reference.TryGetValue<BGM>(out var bgm))
        {
            return [bgm.File.ExtractText()];
        }

        if (!reference.TryGetValue<BGMSituation>(out var situation))
        {
            return [];
        }

        return
        [
            FileOf(situation.DaytimeID),
            FileOf(situation.NightID),
            FileOf(situation.BattleID),
            FileOf(situation.DaybreakID),
            FileOf(situation.TwilightID),
        ];
    }

    private void ReadDuties()
    {
        foreach (var duty in Plugin.DataManager.GetExcelSheet<ContentFinderCondition>())
        {
            var name = duty.Name.ExtractText().Trim();
            if (name.Length == 0)
            {
                continue;
            }

            if (!duty.Content.TryGetValue<InstanceContent>(out var content))
            {
                continue;
            }

            Add(FileOf(content.BGM), name);
            Add(FileOf(content.WinBGM), name);
        }
    }

    private static string FileOf(RowRef<BGM> reference)
    {
        return reference.ValueNullable is { } bgm ? bgm.File.ExtractText() : string.Empty;
    }

    private void Add(string file, string name)
    {
        if (string.IsNullOrEmpty(file))
        {
            return;
        }

        if (!byFile.TryGetValue(file, out var names))
        {
            byFile[file] = names = [];
        }

        if (!names.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            names.Add(name);
        }
    }
}
