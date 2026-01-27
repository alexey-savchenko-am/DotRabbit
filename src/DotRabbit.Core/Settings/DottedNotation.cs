using System.Text;

namespace DotRabbit.Core.Settings;

internal static class DottedNotation
{
    public static string FromPascalCase(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(nameof(name));

        var sb = new StringBuilder(name.Length + 4);

        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (i > 0 && char.IsUpper(c))
                sb.Append('.');

            sb.Append(char.ToLowerInvariant(c));
        }

        return sb.ToString();
    }
}

public static class StringExtensions
{
    public static string FromPascalCase(this string str)
    {
        return DottedNotation.FromPascalCase(str);
    }
}
