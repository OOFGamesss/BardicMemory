using System.Globalization;
using Dalamud.Game;

namespace BardicMemory.Utility;

/// <summary>
/// One phrase in each language, resolved against the player's language.
/// </summary>
public readonly record struct LocalisedString(string English, string Japanese, string German, string French)
{
    public static CultureInfo Culture => Plugin.DataManager.Language switch
    {
        ClientLanguage.Japanese => CultureInfo.GetCultureInfo("ja-JP"),
        ClientLanguage.German => CultureInfo.GetCultureInfo("de-DE"),
        ClientLanguage.French => CultureInfo.GetCultureInfo("fr-FR"),
        _ => CultureInfo.GetCultureInfo("en-GB"),
    };

    public string Text => Plugin.DataManager.Language switch
    {
        ClientLanguage.Japanese => Japanese,
        ClientLanguage.German => German,
        ClientLanguage.French => French,
        _ => English,
    };

    public string Format(params object?[] values)
    {
        return string.Format(Culture, Text, values);
    }
}
