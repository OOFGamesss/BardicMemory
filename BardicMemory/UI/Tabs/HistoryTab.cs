using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using BardicMemory.Models;
using BardicMemory.Services;
using BardicMemory.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BardicMemory.UI.Tabs;

/// <summary>
/// The history tab: the running totals and the table of finished rounds.
/// </summary>
public sealed class HistoryTab(HistoryService history)
{
    public void Draw()
    {
        ImGui.Spacing();
        StatsHeader.Draw(history.Stats);
        ImGui.Spacing();
        HistoryTable.Draw(history.Newest);
    }
}

internal static class StatsHeader
{
    public static void Draw(GameStats stats)
    {
        var culture = LocalisedString.Culture;

        (LocalisedString Label, string Value, string Id)[] tiles =
        [
            (LocalisedText.StatRounds, stats.RoundsPlayed.ToString(culture), "rounds"),
            (LocalisedText.StatSolved, (stats.SolveRate * 100f).ToString("0", culture) + "%", "solved"),
            (LocalisedText.StatPoints, stats.TotalPoints.ToString(culture), "points"),
            (LocalisedText.StatAverage, stats.AverageScore.ToString("0.0", culture), "average"),
            (LocalisedText.StatStreak, $"{stats.CurrentStreak} / {stats.BestStreak}", "streak"),
        ];

        var gaps = ImGui.GetStyle().ItemSpacing.X * (tiles.Length - 1);
        var width = (ImGui.GetContentRegionAvail().X - gaps) / tiles.Length;

        for (var index = 0; index < tiles.Length; index++)
        {
            if (index > 0)
            {
                ImGui.SameLine();
            }

            DrawTile(tiles[index].Label, tiles[index].Value, tiles[index].Id, width);
        }
    }

    private static void DrawTile(LocalisedString label, string value, string id, float width)
    {
        using var background = ImRaii.PushColor(ImGuiCol.ChildBg, Theme.Panel);
        using var rounding = ImRaii.PushStyle(ImGuiStyleVar.ChildRounding, Theme.Scaled(4f));
        using var tile = ImRaii.Child($"##stat{id}", new Vector2(width, Theme.Scaled(52f)), false);
        if (!tile.Success)
        {
            return;
        }

        ImGui.Spacing();
        Theme.CentredText(value, width, Theme.Text);
        Theme.CentredText(label.Text, width, Theme.TextDim);
    }
}

internal static class HistoryTable
{
    private const ImGuiTableFlags Flags =
        ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerH | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

    public static void Draw(IReadOnlyList<HistoryEntry> entries)
    {
        if (entries.Count == 0)
        {
            ImGui.TextColored(Theme.TextDim, LocalisedText.NoRoundsYet.Text);
            return;
        }

        using var table = ImRaii.Table("##history", 3, Flags, ImGui.GetContentRegionAvail());
        if (!table.Success)
        {
            return;
        }

        ImGui.TableSetupColumn(LocalisedText.ColumnSong.Text, ImGuiTableColumnFlags.WidthStretch, 3f);
        ImGui.TableSetupColumn(LocalisedText.ColumnScore.Text, ImGuiTableColumnFlags.WidthStretch, 1f);
        ImGui.TableSetupColumn(LocalisedText.ColumnTime.Text, ImGuiTableColumnFlags.WidthStretch, 2f);
        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableHeadersRow();

        foreach (var entry in entries)
        {
            DrawRow(entry);
        }
    }

    private static void DrawRow(HistoryEntry entry)
    {
        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.TextColored(entry.Solved ? Theme.Text : Theme.TextDim, entry.SongName);
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(LocalisedText.EndedAt.Format(TierLabel.Duration(entry.Tier)));
        }

        ImGui.TableNextColumn();
        ImGui.TextColored(entry.Score > 0 ? Theme.Accent : Theme.TextDim, entry.Score.ToString(LocalisedString.Culture));

        ImGui.TableNextColumn();
        var completed = entry.CompletedUtc.ToUniversalTime();
        ImGui.TextColored(
            Theme.TextDim,
            completed.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) + " ST");
    }
}
