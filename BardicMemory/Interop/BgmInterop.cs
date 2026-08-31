using FFXIVClientStructs.FFXIV.Client.Game;

namespace BardicMemory.Interop;

/// <summary>
/// Wrapper over the games background music scene list.
/// Used BGMController.cs, BGMScene.cs and SceneFlags.cs from the Orchestrion plugin.
/// </summary>
public static unsafe class BgmInterop
{
    private const uint SceneIndex = 0;

    public const ushort SilentBgmId = 1;

    private const int TimerEnableOffset = 0x12;

    private const int TimerOffset = 0x14;

    private const int SceneFlagsOffset = 0x04;

    private const byte ResumeSceneFlag = 2;

    public static bool TryPlay(ushort bgmId)
    {
        var scene = SceneSlot();
        if (scene == null)
        {
            return false;
        }

        scene->EnableCustomFade = true;
        scene->FadeInTime = 0;
        scene->FadeInStartTime = 0;
        scene->FadeOutTime = 0;
        scene->ResumeFadeInTime = 0;

        scene->BgmId = bgmId;
        scene->PlayingBgmId = bgmId;
        scene->PreviousBgmId = bgmId;

        *((byte*)scene + TimerEnableOffset) = 0;
        *(float*)((byte*)scene + TimerOffset) = 0f;

        return true;
    }

    public static bool TryForcePlayState()
    {
        var scene = SceneSlot();
        if (scene == null)
        {
            return false;
        }

        scene->PlayState = BGMSystem.PlayState.Playing;

        return true;
    }

    public static bool TryRelease()
    {
        var scene = SceneSlot();
        if (scene == null)
        {
            return false;
        }

        scene->EnableCustomFade = false;

        scene->BgmId = 0;
        scene->PlayingBgmId = 0;
        scene->PreviousBgmId = 0;

        *((byte*)scene + TimerEnableOffset) = 0;
        *(float*)((byte*)scene + TimerOffset) = 0f;
        *((byte*)scene + SceneFlagsOffset) = ResumeSceneFlag;

        return true;
    }

    public static bool TrySetVolume(float volume)
    {
        var scene = SceneSlot();
        if (scene == null)
        {
            return false;
        }

        scene->InitialVolume = volume;

        return true;
    }

    public static ushort PlayingBgmId()
    {
        var scene = SceneSlot();

        return scene == null ? (ushort)0 : scene->PlayingBgmId;
    }

    private static BGMSystem.Scene* SceneSlot()
    {
        var system = BGMSystem.Instance();
        if (system == null || system->Scenes.LongCount <= SceneIndex)
        {
            return null;
        }

        return system->Scenes.First + SceneIndex;
    }
}
