using System.Text;

namespace DotRabbit.Core.Settings;

internal static class NamingNotations
{
    public static string ToDottedNotation(string name)
    {
        return ToNotation(name, '.');
    }

    public static string ToKebabNotation(string name)
    {
        return ToNotation(name, '-');
    }

    private static string ToNotation(string name, char replaceSymbol)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));

        var sb = new StringBuilder(name.Length + 4);

        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (i > 0 && char.IsUpper(c))
                sb.Append(replaceSymbol);

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}

public static class StringExtensions
{
    public static string ToDottedNotation(this string str)
    {
        return NamingNotations.ToDottedNotation(str);
    }

    public static string ToKebabNotation(this string str)
    {
        return NamingNotations.ToKebabNotation(str);
    }
}
