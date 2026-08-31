using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;

namespace BardicMemory.UI;

/// <summary>
/// The colours and the measurement helpers every tab shares.
/// </summary>
public static class Theme
{
    private const string Ellipsis = "...";

    public static readonly Vector4 Panel = new(0.106f, 0.110f, 0.118f, 1f);
    public static readonly Vector4 PanelHovered = new(0.145f, 0.153f, 0.161f, 1f);
    public static readonly Vector4 PanelSkipped = new(0.169f, 0.180f, 0.196f, 1f);
    public static readonly Vector4 Accent = new(0.239f, 0.706f, 0.247f, 1f);
    public static readonly Vector4 AccentHovered = new(0.290f, 0.780f, 0.298f, 1f);
    public static readonly Vector4 Wrong = new(0.839f, 0.353f, 0.353f, 1f);
    public static readonly Vector4 WrongHovered = new(0.890f, 0.427f, 0.427f, 1f);
    public static readonly Vector4 Text = new(0.949f, 0.949f, 0.949f, 1f);
    public static readonly Vector4 TextDim = new(0.541f, 0.557f, 0.573f, 1f);
    public static readonly Vector4 Divider = new(0.541f, 0.557f, 0.573f, 0.45f);

    public static float Scaled(float pixels)
    {
        return pixels * ImGuiHelpers.GlobalScale;
    }

    public static Vector2 Scaled(float width, float height)
    {
        return new Vector2(width, height) * ImGuiHelpers.GlobalScale;
    }

    public static float Unscaled(float pixels)
    {
        return pixels / ImGuiHelpers.GlobalScale;
    }

    public static uint Packed(Vector4 colour)
    {
        return ImGui.ColorConvertFloat4ToU32(colour);
    }

    public static void CentredText(string text, float width, Vector4 colour)
    {
        text = Truncate(text, width);

        var offset = (width - ImGui.CalcTextSize(text).X) * 0.5f;
        if (offset > 0f)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
        }

        ImGui.TextColored(colour, text);
    }

    public static void CentreForWidth(float width)
    {
        var available = ImGui.GetContentRegionAvail().X;
        if (available > width)
        {
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ((available - width) * 0.5f));
        }
    }

    public static int CentredWrapped(string text, float width, Vector4 colour, int maxLines)
    {
        var lines = WrapLines(text, width, maxLines);
        foreach (var line in lines)
        {
            CentredText(line, width, colour);
        }

        return lines.Count;
    }

    private static List<string> WrapLines(string text, float width, int maxLines)
    {
        var lines = new List<string>();
        if (width <= 0f)
        {
            return lines;
        }

        var line = new StringBuilder();

        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (line.Length > 0 && ImGui.CalcTextSize($"{line} {word}").X <= width)
            {
                line.Append(' ').Append(word);
                continue;
            }

            if (line.Length > 0)
            {
                lines.Add(line.ToString());
                line.Clear();
            }

            var rest = word;
            while (rest.Length > 1 && ImGui.CalcTextSize(rest).X > width)
            {
                var fits = LongestPrefixFitting(rest, width, 1);
                lines.Add(rest[..fits]);
                rest = rest[fits..];
            }

            line.Append(rest);
        }

        if (line.Length > 0)
        {
            lines.Add(line.ToString());
        }

        if (lines.Count <= maxLines)
        {
            return lines;
        }

        var lastIndex = maxLines - 1;
        var overflow = string.Join(' ', lines.GetRange(lastIndex, lines.Count - lastIndex));
        var capped = lines.GetRange(0, maxLines);
        capped[lastIndex] = Truncate(overflow, width);

        return capped;
    }

    public static string Truncate(string text, float width)
    {
        if (width <= 0f || ImGui.CalcTextSize(text).X <= width)
        {
            return text;
        }

        var limit = Math.Max(0f, width - ImGui.CalcTextSize(Ellipsis).X);
        var length = LongestPrefixFitting(text, limit, 0);

        return length == 0 ? Ellipsis : text[..length].TrimEnd() + Ellipsis;
    }

    private static int LongestPrefixFitting(string text, float width, int minimum)
    {
        var full = ImGui.CalcTextSize(text).X;
        if (full <= width)
        {
            return text.Length;
        }

        var length = full <= 0f
            ? text.Length
            : Math.Clamp((int)(text.Length * (width / full)), minimum, text.Length);

        while (length > minimum && ImGui.CalcTextSize(text[..length]).X > width)
        {
            length--;
        }

        while (length < text.Length && ImGui.CalcTextSize(text[..(length + 1)]).X <= width)
        {
            length++;
        }

        return length;
    }
}
