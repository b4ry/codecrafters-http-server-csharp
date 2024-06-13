using codecrafters_http_server.Constants;
using codecrafters_http_server.DTOs;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace codecrafters_http_server.Helpers
{
    internal static class EndpointsHandler
    {
        internal static async Task HandleFiles(string urLine, Dictionary<string, string> headers, string body, string urlPath, Response response)
        {
            var httpMethod = RegexParser.HttpMethodRegex.Match(urLine).ToString();
            var requestArgument = urlPath[7..];
            var filePath = $"{Environment.GetCommandLineArgs()[2]}{requestArgument}";

            if (httpMethod == HttpMethod.Get.ToString())
            {
                if (File.Exists(filePath))
                {
                    var file = await File.ReadAllBytesAsync(filePath);
                    var fileContent = Encoding.ASCII.GetString(file);

                    response.HttpStatusCode = HttpStatusCode.OK;
                    response.ContentType = HttpContentTypes.ApplicationOctetStream;
                    response.ContentLength = file.Length;
                    response.Content = fileContent;
                }
                else
                {
                    response.HttpStatusCode = HttpStatusCode.NotFound;
                }
            }
            else if (httpMethod == HttpMethod.Post.ToString())
            {
                var contentLength = headers["Content-Length"];
                var dataToWrite = body[..int.Parse(contentLength)];

                await File.WriteAllTextAsync(filePath, dataToWrite);

                response.HttpStatusCode = HttpStatusCode.Created;
            }
        }

        internal static async Task HandleEcho(Dictionary<string, string> headers, string urlPath, Response response)
        {
            var requestArgument = urlPath[6..];
            headers.TryGetValue("Accept-Encoding", out string? encodings);

            if (encodings?.Length > 0)
            {
                var validEncoding = encodings.Split(", ").Intersect(Constants.Constants.ValidEncodings);

                if (validEncoding.Contains("gzip"))
                {
                    response.ContentEncoding = "gzip";

                    using var stream = new MemoryStream();
                    using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true))
                    {
                        await gzipStream.WriteAsync(Encoding.ASCII.GetBytes(requestArgument));
                    }

                    stream.Position = 0;
                    response.CompressedContent = stream.ToArray();
                }
            }

            response.HttpStatusCode = HttpStatusCode.OK;
            response.ContentType = HttpContentTypes.TextPlain;

            if (response.CompressedContent?.Length > 0)
            {
                response.ContentLength = response.CompressedContent.Length;
            }
            else
            {
                response.ContentLength = requestArgument!.Length;
                response.Content = requestArgument;
            }
        }

        internal static void HandleUserAgent(Dictionary<string, string> headers, Response response)
        {
            var userAgentHeader = headers["User-Agent"];

            response.HttpStatusCode = HttpStatusCode.OK;
            response.ContentType = HttpContentTypes.TextPlain;
            response.ContentLength = userAgentHeader.Length;
            response.Content = userAgentHeader;
        }
    }
}
