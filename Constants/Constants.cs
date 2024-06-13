namespace codecrafters_http_server.Constants
{
    internal static class Constants
    {
        internal const string Crlf = "\r\n";
        internal const string InvalidEncoding = "invalid-encoding";

        internal static readonly HashSet<string> ValidEncodings = ["gzip"];
    }
}
