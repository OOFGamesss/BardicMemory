using System;
using BardicMemory.Config;
using BardicMemory.Services;
using BardicMemory.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BardicMemory.UI.Tabs;

/// <summary>
/// The settings tab: the round toggles and the history controls.
/// </summary>
public sealed class SettingsTab(Configuration configuration, HistoryService history)
{
    private bool confirmingClear;

    public void Draw()
    {
        using var region = ImRaii.Child(
            "##bardicsettings",
            ImGui.GetContentRegionAvail(),
            false,
            ImGuiWindowFlags.NoScrollbar);
        if (!region.Success)
        {
            return;
        }

        ImGui.Spacing();

        DrawToggle(
            LocalisedText.SilenceMusicToggle,
            LocalisedText.SilenceMusicHelp,
            "bardicMemorySilenceMusic",
            configuration.SilenceZoneMusic,
            value => configuration.SilenceZoneMusic = value);

        DrawToggle(
            LocalisedText.SkipSolvedToggle,
            LocalisedText.SkipSolvedHelp,
            "bardicMemorySkipSolved",
            configuration.ExcludeSolvedSongs,
            value => configuration.ExcludeSolvedSongs = value);

        ImGui.Separator();
        DrawHistoryLimit();
        ImGui.Separator();
        DrawClearHistory();
    }

    private void DrawToggle(
        LocalisedString label,
        LocalisedString tooltip,
        string id,
        bool current,
        Action<bool> apply)
    {
        var value = current;
        if (ImGui.Checkbox($"{label.Text}###{id}", ref value))
        {
            apply(value);
            configuration.Save();
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(tooltip.Text);
        }
    }

    private void DrawHistoryLimit()
    {
        var limit = configuration.HistoryLimit;
        ImGui.SetNextItemWidth(Theme.Scaled(120f));

        if (!ImGui.InputInt($"{LocalisedText.HistoryLimit.Text}###bardicMemoryHistoryLimit", ref limit))
        {
            return;
        }

        configuration.HistoryLimit = Math.Clamp(limit, 10, 5000);
        configuration.Save();
    }

    private void DrawClearHistory()
    {
        if (!confirmingClear)
        {
            if (ImGui.Button($"{LocalisedText.ClearHistory.Text}###bardicMemoryClearHistory"))
            {
                confirmingClear = true;
            }

            return;
        }

        ImGui.TextColored(Theme.Wrong, LocalisedText.ClearHistoryWarning.Text);

        if (ImGui.Button($"{LocalisedText.ClearConfirm.Text}###bardicMemoryClearConfirm"))
        {
            history.Clear();
            confirmingClear = false;
        }

        ImGui.SameLine();

        if (ImGui.Button($"{LocalisedText.Cancel.Text}###bardicMemoryClearCancel"))
        {
            confirmingClear = false;
        }
    }
}
