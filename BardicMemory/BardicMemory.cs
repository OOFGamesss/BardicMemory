using BardicMemory.Config;
using BardicMemory.GameData;
using BardicMemory.Services;
using BardicMemory.UI;
using BardicMemory.Utility;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace BardicMemory;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/bardicmemory";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private readonly WindowSystem windowSystem = new("BardicMemory");
    private readonly Configuration configuration;
    private readonly SongRepository repository;
    private readonly PlaybackService playback;
    private readonly HistoryService history;
    private readonly SongSearchService search;
    private readonly RoundService rounds;
    private readonly MainWindow mainWindow;

    public Plugin()
    {
        configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        repository = new SongRepository();
        playback = new PlaybackService(configuration);
        history = new HistoryService(configuration);
        search = new SongSearchService(repository);
        rounds = new RoundService(configuration, repository, playback, history);

        mainWindow = new MainWindow(configuration, repository, rounds, search, history);
        windowSystem.AddWindow(mainWindow);

        PluginInterface.UiBuilder.Draw += windowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleMainUi;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = LocalisedText.CommandHelp.Text,
        });
    }

    private void OnCommand(string command, string arguments) => ToggleMainUi();

    private void ToggleMainUi() => mainWindow.Toggle();

    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);

        PluginInterface.UiBuilder.OpenConfigUi -= ToggleMainUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        PluginInterface.UiBuilder.Draw -= windowSystem.Draw;

        windowSystem.RemoveAllWindows();
        mainWindow.Dispose();
        playback.Dispose();
    }
}
