using System.Text;

namespace codecrafters_http_server.Helpers
{
    internal static class RequestParser
    {
        internal static (string urlLine, Dictionary<string, string> headers, string body) ParseRequest(byte[] request)
        {
            var decodedReceivedData = Encoding.ASCII.GetString(request).Split(Constants.Constants.Crlf);
            var requestLine = decodedReceivedData[0];
            var headers = ParseHeaders(decodedReceivedData[1..(decodedReceivedData.Length-1)]);
            var body = decodedReceivedData.Last();

            return (requestLine, headers, body);
        }

        private static Dictionary<string, string> ParseHeaders(IEnumerable<string> headers)
        {
            Dictionary<string, string> parsedHeaders = [];

            foreach (var header in headers)
            {
                if (!string.IsNullOrEmpty(header))
                {
                    var split = header.Split(":");
                    parsedHeaders.Add(split[0].Trim(), split[1].Trim());
                }
            }

            return parsedHeaders;
        }
    }
}
