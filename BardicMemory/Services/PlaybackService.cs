using System;
using System.Diagnostics;
using BardicMemory.Config;
using BardicMemory.Interop;
using BardicMemory.Utility;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Sound;

namespace BardicMemory.Services;

/// <summary>
/// Plays song clips through the games background music scene and holds the zone music off.
/// Used BGMController.cs and BGMManager.cs from the Orchestrion plugin.
/// </summary>
public sealed class PlaybackService : IDisposable
{
    private const float PlayToEnd = float.PositiveInfinity;

    private const int OverwrittenFramesBeforeGivingUp = 30;

    private readonly Configuration configuration;
    private readonly Stopwatch sinceAudible = new();

    private ushort requestedBgmId;
    private ushort heldBgmId;
    private float clipSeconds;
    private float progressSeconds;
    private bool roundActive;
    private float? orchestrionVolumeBeforeRound;
    private int framesOverwritten;

    public PlaybackService(Configuration configuration)
    {
        this.configuration = configuration;

        Plugin.Framework.Update += OnFrameworkUpdate;
    }

    public bool IsPlaying => requestedBgmId != 0 && sinceAudible.IsRunning;

    public float ProgressSeconds => progressSeconds;

    public string BlockedReason { get; private set; } = string.Empty;

    public bool BeginRound()
    {
        framesOverwritten = 0;

        UpdateBlockedReason();

        if (BlockedReason.Length > 0)
        {
            Plugin.Log.Information("Not starting a game, an orchestrion is sounding.");
            return false;
        }

        roundActive = true;
        MuteEstateOrchestrion();
        HoldZoneMusicOff();

        return true;
    }

    public void EndRound()
    {
        Stop();
        roundActive = false;
        heldBgmId = 0;
        framesOverwritten = 0;

        RestoreEstateOrchestrion();
        BgmInterop.TryRelease();
    }

    private void MuteEstateOrchestrion()
    {
        if (orchestrionVolumeBeforeRound.HasValue)
        {
            return;
        }

        var current = SoundInterop.BusVolume(SoundBus.Orchestrion);
        if (current <= 0f)
        {
            return;
        }

        if (!SoundInterop.SetBusVolume(SoundBus.Orchestrion, 0f))
        {
            return;
        }

        orchestrionVolumeBeforeRound = current;
    }

    private void RestoreEstateOrchestrion()
    {
        if (orchestrionVolumeBeforeRound is not { } previous)
        {
            return;
        }

        orchestrionVolumeBeforeRound = null;

        SoundInterop.SetBusVolume(SoundBus.Orchestrion, previous);
    }

    public void PlayClip(ushort bgmId, float seconds)
    {
        clipSeconds = seconds;
        Start(bgmId);
    }

    public void PlayWhole(ushort bgmId)
    {
        clipSeconds = PlayToEnd;
        Start(bgmId);
    }

    public bool TryExtendClip(float seconds)
    {
        if (requestedBgmId == 0 || seconds <= clipSeconds)
        {
            return false;
        }

        clipSeconds = seconds;

        return true;
    }

    public void StopAndRewind()
    {
        Stop();
        progressSeconds = 0f;
    }

    public void StopClip() => Stop();

    public void ApplyClipVolume()
    {
        BgmInterop.TrySetVolume(Math.Clamp(configuration.ClipVolume, 0f, 1f));
    }

    private void Start(ushort bgmId)
    {
        Stop();

        if (bgmId == 0)
        {
            Plugin.Log.Warning("The chosen song has no background music id, so it cannot be played.");
            return;
        }

        if (BlockedReason.Length > 0)
        {
            Plugin.Log.Information($"Not playing {bgmId}, an orchestrion is sounding.");
            return;
        }

        ApplyClipVolume();

        if (!BgmInterop.TryPlay(bgmId))
        {
            Plugin.Log.Warning($"Could not reach the background music scene to play {bgmId}.");
            return;
        }

        BgmInterop.TryForcePlayState();

        requestedBgmId = bgmId;
        heldBgmId = bgmId;
        progressSeconds = 0f;
        sinceAudible.Restart();
    }

    private void Stop()
    {
        if (requestedBgmId == 0)
        {
            return;
        }

        requestedBgmId = 0;
        sinceAudible.Reset();
        HoldZoneMusicOff();
    }

    private void HoldZoneMusicOff()
    {
        if (roundActive && configuration.SilenceZoneMusic)
        {
            heldBgmId = BgmInterop.SilentBgmId;
            BgmInterop.TryPlay(heldBgmId);
            BgmInterop.TryForcePlayState();
            return;
        }

        heldBgmId = 0;
        BgmInterop.TryRelease();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        UpdateBlockedReason();

        if (heldBgmId == 0)
        {
            return;
        }

        if (BlockedReason.Length > 0)
        {
            RestoreEstateOrchestrion();
            Stop();
            return;
        }

        MuteEstateOrchestrion();
        ReassertScene();

        if (requestedBgmId == 0)
        {
            return;
        }

        progressSeconds = (float)sinceAudible.Elapsed.TotalSeconds;

        if (progressSeconds >= clipSeconds)
        {
            progressSeconds = clipSeconds;
            Stop();
        }
    }

    private void UpdateBlockedReason()
    {
        if (OrchestrionInterop.IsEstateTrackPlaying())
        {
            BlockedReason = LocalisedText.OrchestrionInTheWay.Text;
            return;
        }

        BlockedReason = framesOverwritten >= OverwrittenFramesBeforeGivingUp
            ? LocalisedText.SceneTakenOver.Text
            : string.Empty;
    }

    private void ReassertScene()
    {
        if (BgmInterop.PlayingBgmId() == heldBgmId)
        {
            framesOverwritten = 0;
            return;
        }

        framesOverwritten++;

        BgmInterop.TryPlay(heldBgmId);
        BgmInterop.TryForcePlayState();
    }

    public void Dispose()
    {
        Plugin.Framework.Update -= OnFrameworkUpdate;

        requestedBgmId = 0;
        heldBgmId = 0;
        roundActive = false;
        RestoreEstateOrchestrion();
        BgmInterop.TryRelease();
    }
}
