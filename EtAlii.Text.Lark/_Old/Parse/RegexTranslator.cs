// TODO: Apologies, generated using an LLM, probably not the best approach.

using System.Text.RegularExpressions;

namespace EtAlii.Ubigia.Lark;

/// <summary>
/// Translates Python/Lark regex syntax to .NET Regex and options.
/// Handles trailing flags (/.../imsx), inline flags ((?i)), and named groups.
/// </summary>
public static class RegexTranslator
{
    public static (string pattern, RegexOptions options) Translate(string raw)
    {
        // The raw input is usually /.../ with optional trailing flags, or a bare Python-style re.
        // 1. Extract delimiters and flags.
        var body = raw;
        var flags = string.Empty;

        if (raw is ['/', _, ..] && raw[^1] == '/')
        {
            // No flags
            body = raw.Substring(1, raw.Length - 2);
        }
        else if (raw is ['/', _, _, ..] && raw[^2] == '/')
        {
            // trailing single-char flag (rare in input), handle generically:
            body = raw.Substring(1, raw.Length - 3);
            flags = raw[^1].ToString();
        }
        else
        {
            // Try to detect /.../flags format
            var lastSlash = raw.LastIndexOf('/');
            if (raw.StartsWith('/') && lastSlash > 0 && lastSlash < raw.Length - 1)
            {
                body = raw[1..lastSlash];
                flags = raw[(lastSlash + 1)..];
            }
        }

        // 2. Convert named groups: Python (?P<name>...) -> .NET (?<name>...)
        body = ReplacePythonNamedGroups(body);
        // 3. Convert backreferences: (?P=name) -> \k<name>
        body = ReplacePythonNamedBackrefs(body);

        // 4. Inline flags like (?i) are supported by .NET mostly; pass-through.
        // 5. Map trailing flags to RegexOptions.
        var options = MapFlagsToOptions(flags);

        // 6. Ensure we anchor at current position like Lark lexers: use \G
        return (body, options);
    }

    private static string ReplacePythonNamedGroups(string s)
    {
        // (?P<name>  -> (?<name>
        return Regex.Replace(s, @"\(\?P<([A-Za-z_][A-Za-z0-9_]*)>", "(?<$1>");
    }

    private static string ReplacePythonNamedBackrefs(string s)
    {
        // (?P=name) -> \k<name>
        return Regex.Replace(s, @"\(\?P=([A-Za-z_][A-Za-z0-9_]*)\)", @"\k<$1>");
    }

    private static RegexOptions MapFlagsToOptions(string flags)
    {
        var options = RegexOptions.None;
        foreach (var f in flags)
        {
            switch (f)
            {
                case 'i': options |= RegexOptions.IgnoreCase; break;
                case 'm': options |= RegexOptions.Multiline; break;
                case 's': options |= RegexOptions.Singleline; break; // DOTALL
                case 'x': options |= RegexOptions.IgnorePatternWhitespace; break; // VERBOSE
                // 'u' (UNICODE) is default in .NET
                //case 'u': options |= RegexOptions.UniCode; break;
                
                // 'a' (ASCII) not directly supported; can be approximated by character classes in syntax.
                // ReSharper disable once RedundantEmptySwitchSection
                default: break;
            }
        }
        return options;
    }
}
