using System;
using System.Numerics;
using BardicMemory.Config;
using BardicMemory.GameData;
using BardicMemory.Services;
using BardicMemory.UI.Tabs;
using BardicMemory.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;

namespace BardicMemory.UI;

/// <summary>
/// The game window and its play, history, settings and support tabs.
/// </summary>
public sealed class MainWindow : Window, IDisposable
{
    private const float MinimumWidth = 520f;
    private const float LooseHeight = 420f;

    private readonly RoundService rounds;
    private readonly PlayTab play;
    private readonly HistoryTab historyTab;
    private readonly SettingsTab settingsTab;

    private bool playTabActive = true;

    public MainWindow(
        Configuration configuration,
        SongRepository repository,
        RoundService rounds,
        SongSearchService search,
        HistoryService history)
        : base("Bardic Memory###BardicMemoryMain", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.rounds = rounds;
        play = new PlayTab(configuration, repository, rounds, search);
        historyTab = new HistoryTab(history);
        settingsTab = new SettingsTab(configuration, history);

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(MinimumWidth, LooseHeight),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue),
        };
    }

    public override void OnClose()
    {
        rounds.EndGame();
    }

    public override void Draw()
    {
        DrawTabs();

        var pinned = playTabActive && play.Height > 0f;
        var minimumHeight = pinned ? Theme.Unscaled(play.Height) : LooseHeight;

        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(MinimumWidth, minimumHeight),
            MaximumSize = new Vector2(float.MaxValue, pinned ? minimumHeight : float.MaxValue),
        };
    }

    private void DrawTabs()
    {
        using var tabs = ImRaii.TabBar("##bardictabs");
        if (!tabs.Success)
        {
            return;
        }

        playTabActive = DrawTab(LocalisedText.PlayTab, "bardicMemoryPlay", play.Draw);
        DrawTab(LocalisedText.HistoryTab, "bardicMemoryHistory", historyTab.Draw);
        DrawTab(LocalisedText.SettingsTab, "bardicMemorySettings", settingsTab.Draw);
        DrawTab(LocalisedText.SupportTab, "bardicMemorySupport", SupportTab.Draw);
    }

    private static bool DrawTab(LocalisedString label, string id, Action body)
    {
        using var tab = ImRaii.TabItem($"{label.Text}###{id}");
        if (!tab.Success)
        {
            return false;
        }

        body();
        return true;
    }

    public void Dispose()
    {
    }
}
