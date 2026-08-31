using System.Globalization;
using System.Text;

namespace BardicMemory.Utility;

/// <summary>
/// Compares song titles loosely, ignoring case, accents and punctuation.
/// </summary>
public static class SongNameMatcher
{
    public static bool Matches(string left, string right)
    {
        return Normalise(left) == Normalise(right);
    }

    public static string Normalise(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var decomposed = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }
}
