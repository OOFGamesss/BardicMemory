using System;
using System.Collections.Generic;
using BardicMemory.GameData;
using BardicMemory.Models;
using Dalamud.Configuration;

namespace BardicMemory.Config;

/// <summary>
/// Persisted plugin settings, finished rounds and running totals.
/// </summary>
[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 1;

    public List<HistoryEntry> History { get; set; } = [];

    public GameStats Stats { get; set; } = new();

    public uint SelectedCategoryId { get; set; } = SongCategory.AllId;

    public int HistoryLimit { get; set; } = 500;

    public bool ExcludeSolvedSongs { get; set; }

    public bool SilenceZoneMusic { get; set; } = true;

    public float ClipVolume { get; set; } = 1f;

    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
