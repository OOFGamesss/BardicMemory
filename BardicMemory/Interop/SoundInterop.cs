using FFXIVClientStructs.FFXIV.Client.Sound;

namespace BardicMemory.Interop;

/// <summary>
/// Wrapper over the games sound manager.
/// </summary>
public static unsafe class SoundInterop
{
    public static float BusVolume(SoundBus bus)
    {
        var manager = SoundManager.Instance();
        if (manager == null)
        {
            return 0f;
        }

        var index = (int)bus;

        return index < 0 || index >= manager->Volume.Length ? 0f : manager->Volume[index];
    }

    public static bool SetBusVolume(SoundBus bus, float volume)
    {
        var manager = SoundManager.Instance();
        if (manager == null)
        {
            return false;
        }

        manager->SetVolume(bus, volume, 0);
        return true;
    }
}
