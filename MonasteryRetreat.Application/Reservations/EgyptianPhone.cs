using System.Text.RegularExpressions;

namespace MonasteryRetreat.Application.Reservations;

/// <summary>
/// Egyptian mobile numbers: 11 digits, starting with 010, 011, 012 or 015.
/// </summary>
public static class EgyptianPhone
{
    private static readonly Regex Pattern =
        new(@"^01[0125]\d{8}$", RegexOptions.Compiled);

    /// <summary>
    /// Trims the number and converts Arabic-Indic digits (٠-٩) to 0-9
    /// so that the same number is always stored the same way.
    /// </summary>
    public static string Normalize(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return string.Empty;

        var chars = phone.Trim().ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            if (chars[i] >= '٠' && chars[i] <= '٩')
                chars[i] = (char)('0' + (chars[i] - '٠'));
            else if (chars[i] >= '۰' && chars[i] <= '۹')
                chars[i] = (char)('0' + (chars[i] - '۰'));
        }

        return new string(chars);
    }

    public static bool IsValid(string? phone) =>
        Pattern.IsMatch(Normalize(phone));

    public const string ErrorMessage =
        "رقم الهاتف يجب أن يكون 11 رقمًا ويبدأ بـ 010 أو 011 أو 012 أو 015";
}
