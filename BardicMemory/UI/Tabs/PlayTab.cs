using System;
using System.Collections.Generic;
using System.Numerics;
using BardicMemory.Config;
using BardicMemory.GameData;
using BardicMemory.Services;
using BardicMemory.State;
using BardicMemory.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;

namespace BardicMemory.UI.Tabs;

/// <summary>
/// The play tab: the category strip, the guess rows and the search box.
/// </summary>
internal enum ResultAction
{
    None,
    PlayWhole,
    Next,
    EndGame,
}

public sealed class PlayTab(
    Configuration configuration,
    SongRepository repository,
    RoundService rounds,
    SongSearchService search)
{
    private const float RowGap = 10f;
    private const float ToggleGap = 6f;
    private static readonly LocalisedString[] ActionLabels =
        [LocalisedText.Submit, LocalisedText.IncreaseDuration, LocalisedText.GiveUp];

    private readonly SongSearchCombo combo = new();

    private bool revealPending;

    public float Height { get; private set; }

    public void Draw()
    {
        if (repository.IsEmpty)
        {
            ImGui.TextColored(Theme.Wrong, LocalisedText.NoSongs.Text);
            return;
        }

        ImGui.Spacing();
        Theme.CentredText("Bardic Memory", ImGui.GetContentRegionAvail().X, Theme.Text);
        ImGui.Spacing();

        DrawCategoryStrip();
        ImGui.Spacing();

        GuessRows.Draw(rounds.Round);
        ImGuiHelpers.ScaledDummy(RowGap);

        DurationBar.Draw(rounds.Round.Tier, rounds.ClipProgressSeconds);
        ImGuiHelpers.ScaledDummy(RowGap);

        DrawPlayControl();
        ImGuiHelpers.ScaledDummy(RowGap);

        DrawActionRow();

        var style = ImGui.GetStyle();
        Height = ImGui.GetCursorPosY() - style.ItemSpacing.Y + style.WindowPadding.Y;
        DrawReveal();
    }

    private void DrawCategoryStrip()
    {
        var chosen = CategoryTabs.Draw(repository.Categories, configuration.SelectedCategoryId);
        if (chosen is not { } categoryId)
        {
            return;
        }

        rounds.EndGame();
        combo.Clear();
        configuration.SelectedCategoryId = categoryId;
        configuration.Save();
    }

    private void DrawPlayControl()
    {
        var round = rounds.Round;
        var playing = rounds.IsClipPlaying;
        var clicked = PlayButton.Draw(round.Song is not null, playing);

        ImGui.SameLine();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + Theme.Scaled(16f));

        if (VolumeButton.Draw(configuration.ClipVolume) is { } volume)
        {
            configuration.ClipVolume = volume;
            configuration.Save();
            rounds.ApplyClipVolume();
        }

        if (!clicked)
        {
            return;
        }

        if (playing)
        {
            rounds.StopPlayback();
        }
        else if (round.IsFinished)
        {
            rounds.PlayWholeTrack();
        }
        else
        {
            rounds.PlayCurrentClip();
        }
    }

    private void DrawActionRow()
    {
        DrawSearchRow(live: rounds.IsGameActive && !rounds.Round.IsFinished);
        ImGuiHelpers.ScaledDummy(ToggleGap);
        DrawGameButton(ending: rounds.IsGameActive);

        if (rounds.LastError.Length > 0)
        {
            ImGui.TextColored(Theme.Wrong, rounds.LastError);
        }
    }

    private void DrawGameButton(bool ending)
    {
        using var colour = ImRaii.PushColor(ImGuiCol.Button, ending ? Theme.Wrong : Theme.Accent);
        using var hovered = ImRaii.PushColor(ImGuiCol.ButtonHovered, ending ? Theme.WrongHovered : Theme.AccentHovered);

        if (!DrawWideButton(ending ? LocalisedText.EndGame.Text : LocalisedText.StartGame.Text))
        {
            return;
        }

        combo.Clear();

        if (ending)
        {
            rounds.EndGame();
        }
        else
        {
            rounds.StartRound();
        }
    }

    private static bool DrawWideButton(string label)
    {
        return ImGui.Button(label, new Vector2(ImGui.GetContentRegionAvail().X, Theme.Scaled(38f)));
    }

    private void DrawSearchRow(bool live)
    {
        var available = ImGui.GetContentRegionAvail().X;
        var spacing = ImGui.GetStyle().ItemSpacing.X;
        var buttonWidth = Math.Min(ActionButtonWidth(), available * 0.5f);
        var fieldWidth = Math.Max(Theme.Scaled(80f), available - buttonWidth - spacing);

        var guess = live ? combo.Selected ?? search.ExactMatch(combo.Query) : null;
        var isGiveUp = live && guess is null && !rounds.CanRaiseTier;
        string label;
        if (guess is not null)
        {
            label = LocalisedText.Submit.Text;
        }
        else if (isGiveUp)
        {
            label = LocalisedText.GiveUp.Text;
        }
        else
        {
            label = LocalisedText.IncreaseDuration.Text;
        }

        bool clicked;
        using (ImRaii.Disabled(!live))
        {
            combo.DrawField(fieldWidth);
            ImGui.SameLine();

            using (ImRaii.PushColor(ImGuiCol.Button, ButtonFill(live, isGiveUp, false)))
            using (ImRaii.PushColor(ImGuiCol.ButtonHovered, ButtonFill(live, isGiveUp, true)))
            {
                clicked = ImGui.Button($"{label}###bardicMemoryAction", new Vector2(buttonWidth, Theme.Scaled(38f)));
            }
        }

        if (!live)
        {
            return;
        }

        combo.DrawResults(search.Search(combo.Query), fieldWidth);

        if (!clicked)
        {
            return;
        }

        if (guess is { } chosen)
        {
            revealPending = rounds.SubmitGuess(chosen);
            combo.Clear();
        }
        else if (rounds.CanRaiseTier)
        {
            rounds.RaiseTier();
        }
        else
        {
            rounds.GiveUp();
            revealPending = rounds.Round.IsFinished;
        }
    }

    private static Vector4 ButtonFill(bool live, bool isGiveUp, bool hovered)
    {
        if (!live)
        {
            return Theme.Panel;
        }

        if (isGiveUp)
        {
            return hovered ? Theme.WrongHovered : Theme.Wrong;
        }

        return hovered ? Theme.AccentHovered : Theme.Accent;
    }

    private static float ActionButtonWidth()
    {
        var widest = 0f;
        foreach (var action in ActionLabels)
        {
            widest = Math.Max(widest, ImGui.CalcTextSize(action.Text).X);
        }

        return widest + (ImGui.GetStyle().FramePadding.X * 2f) + Theme.Scaled(8f);
    }

    private void DrawReveal()
    {
        if (!rounds.Round.IsFinished)
        {
            revealPending = false;
            return;
        }

        if (revealPending)
        {
            ResultPopup.Open();
            revealPending = false;
        }

        switch (ResultPopup.Draw(rounds.Round))
        {
            case ResultAction.PlayWhole:
                rounds.PlayWholeTrack();
                break;
            case ResultAction.Next:
                combo.Clear();
                rounds.StartRound();
                break;
            case ResultAction.EndGame:
                combo.Clear();
                rounds.EndGame();
                break;
        }
    }
}

internal static class CategoryTabs
{
    private const float Gap = 1f;

    public static uint? Draw(IReadOnlyList<SongCategory> categories, uint selectedId)
    {
        if (categories.Count == 0)
        {
            return null;
        }

        using var spacing = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, new Vector2(Gap, Gap));
        using var rounding = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, Theme.Scaled(4f));

        var available = ImGui.GetContentRegionAvail().X;
        var tabWidth = WidestLabel(categories);
        var perRow = Math.Max(1, (int)((available + Gap) / (tabWidth + Gap)));

        uint? clicked = null;

        for (var index = 0; index < categories.Count; index++)
        {
            if (index % perRow == 0)
            {
                var onThisRow = Math.Min(perRow, categories.Count - index);
                var rowWidth = (tabWidth * onThisRow) + (Gap * (onThisRow - 1));
                var indent = (available - rowWidth) * 0.5f;

                if (indent > 0f)
                {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + indent);
                }
            }
            else
            {
                ImGui.SameLine();
            }

            if (DrawTab(categories[index], selectedId, tabWidth))
            {
                clicked = categories[index].Id;
            }
        }

        return clicked == selectedId ? null : clicked;
    }

    private static float WidestLabel(IReadOnlyList<SongCategory> categories)
    {
        var widest = 0f;
        foreach (var category in categories)
        {
            widest = Math.Max(widest, ImGui.CalcTextSize(category.Name).X);
        }

        return widest + (ImGui.GetStyle().FramePadding.X * 2f) + Theme.Scaled(12f);
    }

    private static bool DrawTab(SongCategory category, uint selectedId, float width)
    {
        var isSelected = category.Id == selectedId;
        using var background = ImRaii.PushColor(ImGuiCol.Button, isSelected ? Theme.Accent : Theme.Panel);
        using var hovered = ImRaii.PushColor(ImGuiCol.ButtonHovered, isSelected ? Theme.AccentHovered : Theme.PanelHovered);
        using var text = ImRaii.PushColor(ImGuiCol.Text, isSelected ? Theme.Text : Theme.TextDim);

        var clicked = ImGui.Button($"{category.Name}##category{category.Id}", new Vector2(width, Theme.Scaled(28f)));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(LocalisedText.SongCount.Format(category.SongCount));
        }

        return clicked;
    }
}

internal static class GuessRows
{
    public static void Draw(RoundState round)
    {
        var width = ImGui.GetContentRegionAvail().X;
        for (var tier = 0; tier < DurationTiers.Count; tier++)
        {
            DrawRow(round, tier, width);
        }
    }

    private static void DrawRow(RoundState round, int tier, float width)
    {
        var status = round.StatusOf(tier);
        var size = new Vector2(width, Theme.Scaled(38f));
        var origin = ImGui.GetCursorScreenPos();
        ImGui.Dummy(size);

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(origin, origin + size, Theme.Packed(BackgroundOf(status)), Theme.Scaled(4f));

        if (status == TierStatus.Active)
        {
            drawList.AddRect(
                origin,
                origin + size,
                Theme.Packed(Theme.PanelHovered),
                Theme.Scaled(4f),
                ImDrawFlags.None,
                Theme.Scaled(1.5f));
        }

        var label = LabelOf(round, tier, status);
        var position = origin + ((size - ImGui.CalcTextSize(label)) * 0.5f);
        drawList.AddText(position, Theme.Packed(ColourOf(round, status)), label);
    }

    private static string LabelOf(RoundState round, int tier, TierStatus status) => status switch
    {
        TierStatus.Skipped => LocalisedText.Skipped.Text,
        TierStatus.Solved or TierStatus.Revealed => round.Song?.Name ?? string.Empty,
        TierStatus.Active when round.LastWrongGuess.Length > 0 => round.LastWrongGuess,
        _ => TierLabel.Points(tier),
    };

    private static Vector4 BackgroundOf(TierStatus status) => status switch
    {
        TierStatus.Skipped or TierStatus.Revealed => Theme.PanelSkipped,
        TierStatus.Solved => Theme.Accent,
        _ => Theme.Panel,
    };

    private static Vector4 ColourOf(RoundState round, TierStatus status) => status switch
    {
        TierStatus.Solved or TierStatus.Skipped => Theme.Text,
        TierStatus.Revealed => Theme.Wrong,
        TierStatus.Active when round.LastWrongGuess.Length > 0 => Theme.Wrong,
        _ => Theme.TextDim,
    };
}

internal static class DurationBar
{
    public static void Draw(int currentTier, float progressSeconds)
    {
        var width = ImGui.GetContentRegionAvail().X;
        var stopSeconds = DurationTiers.SecondsFor(currentTier);
        var drawList = ImGui.GetWindowDrawList();

        var labelOrigin = ImGui.GetCursorScreenPos();
        ImGui.Dummy(new Vector2(width, Theme.Scaled(24f)));

        var label = TierLabel.Duration(currentTier);
        var labelWidth = ImGui.CalcTextSize(label).X;
        var labelX = Math.Clamp(
            labelOrigin.X + PositionOf(stopSeconds, width) - (labelWidth * 0.5f),
            labelOrigin.X,
            labelOrigin.X + width - labelWidth);

        drawList.AddText(new Vector2(labelX, labelOrigin.Y), Theme.Packed(Theme.Text), label);

        var origin = ImGui.GetCursorScreenPos();
        var height = Theme.Scaled(22f);
        ImGui.Dummy(new Vector2(width, height));

        FillTo(drawList, origin, width, height, MaxSeconds(), Theme.Panel);
        FillTo(drawList, origin, width, height, stopSeconds, Theme.PanelSkipped);
        FillTo(drawList, origin, width, height, Math.Min(progressSeconds, stopSeconds), Theme.Accent);

        DrawTicks(drawList, origin, width, height, currentTier);
        DrawStopLine(drawList, origin, width, height, stopSeconds);
    }

    private static void FillTo(ImDrawListPtr drawList, Vector2 origin, float width, float height, float seconds, Vector4 colour)
    {
        var right = origin.X + PositionOf(seconds, width);
        if (right <= origin.X)
        {
            return;
        }

        drawList.AddRectFilled(origin, new Vector2(right, origin.Y + height), Theme.Packed(colour), Theme.Scaled(4f));
    }

    private static void DrawTicks(ImDrawListPtr drawList, Vector2 origin, float width, float height, int currentTier)
    {
        for (var tier = 0; tier < DurationTiers.LastIndex; tier++)
        {
            if (tier == currentTier)
            {
                continue;
            }

            var x = origin.X + PositionOf(DurationTiers.SecondsFor(tier), width);
            drawList.AddLine(
                new Vector2(x, origin.Y),
                new Vector2(x, origin.Y + height),
                Theme.Packed(Theme.Divider),
                Theme.Scaled(1f));
        }
    }

    private static void DrawStopLine(ImDrawListPtr drawList, Vector2 origin, float width, float height, float seconds)
    {
        var thickness = Theme.Scaled(2f);
        var overhang = Theme.Scaled(3f);
        var x = Math.Clamp(
            origin.X + PositionOf(seconds, width),
            origin.X + (thickness * 0.5f),
            origin.X + width - (thickness * 0.5f));

        drawList.AddLine(
            new Vector2(x, origin.Y - overhang),
            new Vector2(x, origin.Y + height + overhang),
            Theme.Packed(Theme.Text),
            thickness);
    }

    private static float PositionOf(float seconds, float width)
    {
        return Math.Clamp(seconds / MaxSeconds(), 0f, 1f) * width;
    }

    private static float MaxSeconds()
    {
        return DurationTiers.SecondsFor(DurationTiers.LastIndex);
    }
}

internal static class PlayButton
{
    public static bool Draw(bool enabled, bool playing)
    {
        var diameter = Theme.Scaled(62f);
        Theme.CentreForWidth(diameter);

        var origin = ImGui.GetCursorScreenPos();
        ImGui.InvisibleButton("##play", new Vector2(diameter, diameter));

        var clicked = enabled && ImGui.IsItemClicked();
        var hovered = enabled && ImGui.IsItemHovered();

        var radius = diameter * 0.5f;
        var centre = origin + new Vector2(radius, radius);
        var fill = enabled ? (hovered ? Theme.AccentHovered : Theme.Accent) : Theme.Panel;
        var glyph = Theme.Packed(enabled ? Theme.Text : Theme.TextDim);

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddCircleFilled(centre, radius, Theme.Packed(fill), 48);

        if (playing)
        {
            DrawStopGlyph(drawList, centre, diameter, glyph);
        }
        else
        {
            DrawPlayGlyph(drawList, centre, diameter, glyph);
        }

        return clicked;
    }

    private static void DrawPlayGlyph(ImDrawListPtr drawList, Vector2 centre, float diameter, uint colour)
    {
        var height = diameter * 0.26f;
        var halfWidth = height * 0.55f;
        var nudge = height * 0.08f;

        drawList.AddTriangleFilled(
            new Vector2(centre.X - halfWidth + nudge, centre.Y - height),
            new Vector2(centre.X - halfWidth + nudge, centre.Y + height),
            new Vector2(centre.X + halfWidth + nudge, centre.Y),
            colour);
    }

    private static void DrawStopGlyph(ImDrawListPtr drawList, Vector2 centre, float diameter, uint colour)
    {
        var half = diameter * 0.19f;

        drawList.AddRectFilled(
            new Vector2(centre.X - half, centre.Y - half),
            new Vector2(centre.X + half, centre.Y + half),
            colour,
            diameter * 0.03f);
    }
}

internal static class VolumeButton
{
    private const string PopupId = "##bardicMemoryVolume";

    public static float? Draw(float volume)
    {
        var size = Theme.Scaled(30f);
        var origin = ImGui.GetCursorScreenPos();

        ImGui.InvisibleButton("##volume", new Vector2(size, size));

        if (ImGui.IsItemClicked())
        {
            ImGui.OpenPopup(PopupId);
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(LocalisedText.ClipVolumeReading.Format((volume * 100f).ToString("0", LocalisedString.Culture)));
        }

        DrawSpeaker(origin, size, volume, ImGui.IsItemHovered());

        using var popup = ImRaii.Popup(PopupId);
        if (!popup.Success)
        {
            return null;
        }

        ImGui.TextColored(Theme.TextDim, LocalisedText.ClipVolume.Text);

        var percent = Math.Clamp(volume, 0f, 1f) * 100f;
        ImGui.SetNextItemWidth(Theme.Scaled(200f));
        var changed = ImGui.SliderFloat("##volumeslider", ref percent, 0f, 100f, "%.0f%%", ImGuiSliderFlags.None);

        if (ImGui.Button($"{LocalisedText.ResetVolume.Text}###bardicMemoryResetVolume"))
        {
            return 1f;
        }

        return changed ? Math.Clamp(percent / 100f, 0f, 1f) : null;
    }

    private static void DrawSpeaker(Vector2 origin, float size, float volume, bool hovered)
    {
        var drawList = ImGui.GetWindowDrawList();
        var colour = Theme.Packed(hovered ? Theme.Text : Theme.TextDim);
        var centre = origin + new Vector2(size * 0.5f, size * 0.5f);
        var unit = size * 0.12f;

        DrawSpeakerBody(drawList, centre, unit, colour);

        if (volume <= 0.001f)
        {
            var start = new Vector2(centre.X + (unit * 0.9f), centre.Y - unit);
            var end = new Vector2(centre.X + (unit * 2.6f), centre.Y + unit);

            drawList.AddLine(start, end, colour, Theme.Scaled(1.6f));
            drawList.AddLine(new Vector2(end.X, start.Y), new Vector2(start.X, end.Y), colour, Theme.Scaled(1.6f));
            return;
        }

        var pivot = new Vector2(centre.X - (unit * 1.4f), centre.Y);
        DrawWave(drawList, pivot, unit * 2.4f, colour);

        if (volume > 0.5f)
        {
            DrawWave(drawList, pivot, unit * 3.4f, colour);
        }
    }

    private static void DrawSpeakerBody(ImDrawListPtr drawList, Vector2 centre, float unit, uint colour)
    {
        var left = centre.X - (unit * 2.4f);

        drawList.AddRectFilled(
            new Vector2(left, centre.Y - (unit * 0.7f)),
            new Vector2(left + unit, centre.Y + (unit * 0.7f)),
            colour);

        drawList.AddTriangleFilled(
            new Vector2(left + unit, centre.Y - (unit * 0.7f)),
            new Vector2(left + unit, centre.Y + (unit * 0.7f)),
            new Vector2(left + (unit * 2.6f), centre.Y),
            colour);

        drawList.AddTriangleFilled(
            new Vector2(left + (unit * 2.6f), centre.Y),
            new Vector2(left + (unit * 2.6f), centre.Y - (unit * 1.6f)),
            new Vector2(left + unit, centre.Y - (unit * 0.7f)),
            colour);

        drawList.AddTriangleFilled(
            new Vector2(left + (unit * 2.6f), centre.Y),
            new Vector2(left + (unit * 2.6f), centre.Y + (unit * 1.6f)),
            new Vector2(left + unit, centre.Y + (unit * 0.7f)),
            colour);
    }

    private static void DrawWave(ImDrawListPtr drawList, Vector2 pivot, float radius, uint colour)
    {
        drawList.PathArcTo(pivot, radius, -0.85f, 0.85f, 12);
        drawList.PathStroke(colour, ImDrawFlags.None, Theme.Scaled(1.6f));
    }
}

internal sealed class SongSearchCombo
{
    private const int MaxLength = 128;
    private const int VisibleRows = 6;

    private string query = string.Empty;
    private Song? selected;
    private Vector2 fieldOrigin;

    public Song? Selected => selected;

    public string Query => query;

    public void Clear()
    {
        query = string.Empty;
        selected = null;
    }

    public void DrawField(float width)
    {
        using var background = ImRaii.PushColor(ImGuiCol.FrameBg, Theme.Panel);
        using var rounding = ImRaii.PushStyle(ImGuiStyleVar.FrameRounding, Theme.Scaled(4f));
        using var padding = ImRaii.PushStyle(ImGuiStyleVar.FramePadding, Theme.Scaled(10f, 10f));

        fieldOrigin = ImGui.GetCursorScreenPos();
        ImGui.SetNextItemWidth(width);

        if (ImGui.InputTextWithHint("##songsearch", LocalisedText.SearchSong.Text, ref query, MaxLength))
        {
            selected = null;
        }
    }

    public void DrawResults(IReadOnlyList<SongMatch> matches, float width)
    {
        if (selected is not null || query.Length == 0 || matches.Count == 0)
        {
            return;
        }

        var style = ImGui.GetStyle();
        var rowHeight = (ImGui.GetTextLineHeight() * 2f) + Theme.Scaled(6f);
        var pitch = rowHeight + style.ItemSpacing.Y;
        var height = (pitch * Math.Min(matches.Count, VisibleRows)) + (style.WindowPadding.Y * 2f);

        var cursor = ImGui.GetCursorScreenPos();
        ImGui.SetCursorScreenPos(fieldOrigin - new Vector2(0f, height + Theme.Scaled(4f)));

        using (ImRaii.PushColor(ImGuiCol.ChildBg, Theme.PanelSkipped))
        using (var list = ImRaii.Child("##songresults", new Vector2(width, height), true))
        {
            if (list.Success)
            {
                foreach (var match in matches)
                {
                    DrawResult(match, rowHeight);
                }
            }
        }

        ImGui.SetCursorScreenPos(cursor);
    }

    private void DrawResult(SongMatch match, float rowHeight)
    {
        var origin = ImGui.GetCursorScreenPos();
        var textWidth = ImGui.GetContentRegionAvail().X;

        var clicked = ImGui.Selectable(
            $"##result{match.Song.RollId}",
            false,
            ImGuiSelectableFlags.None,
            new Vector2(0f, rowHeight));

        var drawList = ImGui.GetWindowDrawList();
        var padding = Theme.Scaled(3f);
        drawList.AddText(origin + new Vector2(0f, padding), Theme.Packed(Theme.Text), match.Song.Name);

        if (match.Hint.Length > 0)
        {
            var below = origin + new Vector2(0f, padding + ImGui.GetTextLineHeight());
            drawList.AddText(below, Theme.Packed(Theme.TextDim), Theme.Truncate(match.Hint, textWidth));
        }

        if (clicked)
        {
            selected = match.Song;
            query = match.Song.Name;
        }
    }
}

internal static class ResultPopup
{
    private const string PopupId = "###bardicMemoryResult";

    private const ImGuiWindowFlags Flags =
        ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoSavedSettings;

    public static void Open()
    {
        if (!ImGui.IsPopupOpen(PopupId))
        {
            ImGui.OpenPopup(PopupId);
        }
    }

    public static ResultAction Draw(RoundState round)
    {
        var centre = ImGui.GetWindowPos() + (ImGui.GetWindowSize() * 0.5f);
        ImGui.SetNextWindowPos(centre, ImGuiCond.Always, new Vector2(0.5f, 0.5f));
        ImGui.SetNextWindowSize(new Vector2(Theme.Scaled(360f), 0f));

        using var popup = ImRaii.PopupModal(PopupId, Flags);
        if (!popup.Success)
        {
            return ResultAction.None;
        }

        var solved = round.Outcome == RoundOutcome.Solved;
        DrawBanner(TierLabel.Score(round.Score), solved ? Theme.Accent : Theme.PanelSkipped);
        DrawSummary(round, solved);

        return DrawActions();
    }

    private static void DrawBanner(string title, Vector4 colour)
    {
        var origin = ImGui.GetCursorScreenPos();
        var size = new Vector2(ImGui.GetContentRegionAvail().X, Theme.Scaled(34f));
        ImGui.Dummy(size);

        var drawList = ImGui.GetWindowDrawList();
        drawList.AddRectFilled(origin, origin + size, Theme.Packed(colour), Theme.Scaled(4f));
        drawList.AddText(origin + ((size - ImGui.CalcTextSize(title)) * 0.5f), Theme.Packed(Theme.Text), title);
    }

    private static void DrawSummary(RoundState round, bool solved)
    {
        var width = ImGui.GetContentRegionAvail().X;

        ImGui.Spacing();
        Theme.CentredWrapped(round.Song?.Name ?? string.Empty, width, Theme.Text, 2);

        if (solved)
        {
            Theme.CentredText(LocalisedText.SolvedAt.Format(TierLabel.Duration(round.Tier)), width, Theme.TextDim);
        }

        if (round.Song is not { } song)
        {
            ImGui.Spacing();
            return;
        }

        ImGui.Spacing();
        var lines = 0;

        if (song.Description.Length > 0)
        {
            lines += Theme.CentredWrapped(song.Description, width, Theme.TextDim, 3);
        }

        if (song.Locations.Length > 0)
        {
            lines += Theme.CentredWrapped(LocalisedText.PlaysIn.Format(song.Locations), width, Theme.TextDim, 2);
        }

        if (lines == 0)
        {
            Theme.CentredText(song.CategoryName, width, Theme.TextDim);
        }

        ImGui.Spacing();
    }

    private static ResultAction DrawActions()
    {
        var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) * 0.5f;
        var size = new Vector2(width, Theme.Scaled(34f));

        bool playWhole;
        using (ImRaii.PushColor(ImGuiCol.Button, Theme.Panel))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Theme.PanelHovered))
        {
            playWhole = ImGui.Button($"{LocalisedText.PlayFullTrack.Text}###bardicMemoryPlayFull", size);
        }

        if (playWhole)
        {
            return ResultAction.PlayWhole;
        }

        ImGui.SameLine();

        bool next;
        using (ImRaii.PushColor(ImGuiCol.Button, Theme.Accent))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Theme.AccentHovered))
        {
            next = ImGui.Button($"{LocalisedText.NextSong.Text}###bardicMemoryNextSong", size);
        }

        if (next)
        {
            ImGui.CloseCurrentPopup();
            return ResultAction.Next;
        }

        ImGui.Spacing();

        bool endGame;
        using (ImRaii.PushColor(ImGuiCol.Button, Theme.Wrong))
        using (ImRaii.PushColor(ImGuiCol.ButtonHovered, Theme.WrongHovered))
        {
            endGame = ImGui.Button(
                $"{LocalisedText.EndGame.Text}###bardicMemoryPopupEndGame",
                new Vector2(ImGui.GetContentRegionAvail().X, size.Y));
        }

        if (!endGame)
        {
            return ResultAction.None;
        }

        ImGui.CloseCurrentPopup();
        return ResultAction.EndGame;
    }
}
