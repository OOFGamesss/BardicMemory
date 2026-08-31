using System;
using System.IO;
using System.Numerics;
using BardicMemory.Utility;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Utility;

namespace BardicMemory.UI.Tabs;

/// <summary>
/// The support tab: the logo, a short set of answers and the Discord link.
/// </summary>
public static class SupportTab
{
    private const string DiscordUrl = "https://discord.gg/vM6ff4h5Ym";
    private const float LogoWidth = 160f;
    private const float StudioLogoWidth = 56f;

    private static readonly string ImageDirectory =
        Path.Combine(Plugin.PluginInterface.AssemblyLocation.DirectoryName ?? string.Empty, "Images");

    private static readonly string LogoPath = Path.Combine(ImageDirectory, "bardicmemory.png");
    private static readonly string StudioLogoPath = Path.Combine(ImageDirectory, "oofgames.png");

    private static readonly (LocalisedString Question, LocalisedString Answer)[] Faq =
    [
        (LocalisedText.FaqPlayQuestion, LocalisedText.FaqPlayAnswer),
        (LocalisedText.FaqSourceQuestion, LocalisedText.FaqSourceAnswer),
        (LocalisedText.FaqQuietQuestion, LocalisedText.FaqQuietAnswer),
        (LocalisedText.FaqWrongGuessQuestion, LocalisedText.FaqWrongGuessAnswer),
    ];

    public static void Draw()
    {
        using var region = ImRaii.Child(
            "##bardicsupport",
            ImGui.GetContentRegionAvail(),
            false,
            ImGuiWindowFlags.NoScrollbar);
        if (!region.Success)
        {
            return;
        }

        DrawCentredImage(LogoPath, LogoWidth);
        ImGuiHelpers.ScaledDummy(6f);
        ImGui.Separator();
        ImGuiHelpers.ScaledDummy(8f);

        DrawFaq();
        DrawDiscordLink();
    }

    private static void DrawFaq()
    {
        var footer = Theme.Scaled(StudioLogoWidth)
            + ImGui.GetFrameHeight()
            + (ImGui.GetStyle().ItemSpacing.Y * 2f);
        var height = Math.Max(ImGui.GetContentRegionAvail().Y - footer, ImGui.GetFrameHeight());

        using var list = ImRaii.Child("##bardicsupportfaq", new Vector2(0f, height), false);
        if (!list.Success)
        {
            return;
        }

        for (var index = 0; index < Faq.Length; index++)
        {
            var (question, answer) = Faq[index];

            if (ImGui.CollapsingHeader($"{question.Text}###bardicMemoryFaq{index}"))
            {
                ImGui.TextWrapped(answer.Text);
            }

            ImGuiHelpers.ScaledDummy(6f);
        }
    }

    private static void DrawDiscordLink()
    {
        DrawCentredImage(StudioLogoPath, StudioLogoWidth);
        var label = LocalisedText.DiscordButton.Text;
        Theme.CentreForWidth(ImGui.CalcTextSize(label).X + (ImGui.GetStyle().FramePadding.X * 2f));

        if (ImGui.Button($"{label}###bardicMemoryDiscord"))
        {
            Util.OpenLink(DiscordUrl);
        }
    }

    private static void DrawCentredImage(string path, float baseWidth)
    {
        var width = Theme.Scaled(baseWidth);
        var image = Plugin.TextureProvider.GetFromFile(path).GetWrapOrDefault();

        if (image is null || image.Width == 0)
        {
            Theme.CentreForWidth(width);
            ImGui.Dummy(new Vector2(width, width));
            return;
        }

        var size = new Vector2(width, width * image.Height / image.Width);
        Theme.CentreForWidth(size.X);
        ImGui.Image(image.Handle, size);
    }
}
