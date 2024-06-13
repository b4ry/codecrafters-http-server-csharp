using System.Text.RegularExpressions;

namespace codecrafters_http_server.Helpers
{
    internal static partial class RegexParser
    {
        internal static readonly Regex HttpMethodRegex = new($"^{HttpMethod.Get}|{HttpMethod.Post}");

        [GeneratedRegex("\\/\\S*")]
        internal static partial Regex UrlPathRegex();

        [GeneratedRegex("[Uu]ser-[Aa]gent: \\S*")]
        internal static partial Regex UserAgentRegex();

        [GeneratedRegex("[Cc]ontent-[Ll]ength: \\S*")]
        internal static partial Regex ContentLengthRegex();

        [GeneratedRegex("[Aa]ccept-[Ee]ncoding: \\S*")]
        internal static partial Regex AcceptEncodingRegex();
    }
}
