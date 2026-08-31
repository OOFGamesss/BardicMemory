using FFXIVClientStructs.FFXIV.Client.Game;

namespace BardicMemory.Interop;

/// <summary>
/// Wrapper over the games own orchestrion player, the estate and inn one.
/// Used OrchestrionInnController.cs from the Orchestrion plugin.
/// </summary>
public static unsafe class OrchestrionInterop
{
    public static bool IsEstateTrackPlaying()
    {
        var manager = OrchestrionManager.Instance();

        return manager != null && manager->Mode != OrchestrionMode.Off;
    }
}
