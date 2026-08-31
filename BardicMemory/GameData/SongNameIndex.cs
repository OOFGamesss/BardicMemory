using System;
using System.Collections.Generic;
using BardicMemory.Utility;
using Dalamud.Game;
using Lumina.Excel.Sheets;

namespace BardicMemory.GameData;

/// <summary>
/// Searches by any of the client languages for song titles.
/// </summary>
public sealed class SongNameIndex
{
    private static readonly ClientLanguage[] Languages =
        [ClientLanguage.English, ClientLanguage.Japanese, ClientLanguage.German, ClientLanguage.French];

    private static readonly string[] NoNames = [];

    private readonly Dictionary<uint, string[]> namesByRoll = [];

    public SongNameIndex()
    {
        var gathered = new Dictionary<uint, List<string>>();

        foreach (var language in Languages)
        {
            try
            {
                Gather(language, gathered);
            }
            catch (Exception exception)
            {
                Plugin.Log.Warning(exception, $"Could not read the song titles in {language}.");
            }
        }

        foreach (var (rollId, names) in gathered)
        {
            namesByRoll[rollId] = names.ToArray();
        }
    }

    public string[] For(uint rollId)
    {
        return namesByRoll.GetValueOrDefault(rollId, NoNames);
    }

    private static void Gather(ClientLanguage language, Dictionary<uint, List<string>> gathered)
    {
        foreach (var roll in Plugin.DataManager.GetExcelSheet<Orchestrion>(language))
        {
            var normalised = SongNameMatcher.Normalise(roll.Name.ExtractText());
            if (normalised.Length == 0)
            {
                continue;
            }

            if (!gathered.TryGetValue(roll.RowId, out var names))
            {
                names = [];
                gathered[roll.RowId] = names;
            }

            if (!names.Contains(normalised))
            {
                names.Add(normalised);
            }
        }
    }
}
