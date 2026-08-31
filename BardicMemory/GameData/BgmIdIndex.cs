using System;
using System.Collections.Generic;
using System.IO;

namespace BardicMemory.GameData;

/// <summary>
/// Maps an orchestrion roll to the background music row the game plays it from.
/// Credit to the orchestration plugin for the id map.
/// </summary>
public sealed class BgmIdIndex
{
    private const string ResourceName = "BardicMemory.Data.OrchestrionBgmMap.csv";

    private readonly Dictionary<uint, ushort> idsByRoll = [];

    public BgmIdIndex()
    {
        using var stream = typeof(BgmIdIndex).Assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            Plugin.Log.Error($"The song id map {ResourceName} is missing, so no song can be played.");
            return;
        }

        using var reader = new StreamReader(stream);
        while (reader.ReadLine() is { } line)
        {
            if (TryReadPair(line, out var rollId, out var bgmId))
            {
                idsByRoll[rollId] = bgmId;
            }
        }
    }

    public ushort For(uint rollId)
    {
        return idsByRoll.GetValueOrDefault(rollId, (ushort)0);
    }

    private static bool TryReadPair(string line, out uint rollId, out ushort bgmId)
    {
        rollId = 0;
        bgmId = 0;

        if (line.Length == 0 || line[0] == '#')
        {
            return false;
        }

        var comma = line.IndexOf(',');

        return comma > 0
               && uint.TryParse(line.AsSpan(0, comma), out rollId)
               && ushort.TryParse(line.AsSpan(comma + 1), out bgmId)
               && bgmId > 1;
    }
}
