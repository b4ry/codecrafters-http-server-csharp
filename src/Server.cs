using codecrafters_http_server.Constants;
using codecrafters_http_server.DTOs;
using codecrafters_http_server.Helpers;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 4221);
server.Start();

while (true)
{
    using (var socket = await server.AcceptSocketAsync())
    {
        var receivedData = new byte[2048];
        await socket.ReceiveAsync(receivedData);

        var (urLine, headers, body) = RequestParser.ParseRequest(receivedData);

        var urlPath = RegexParser.UrlPathRegex().Match(urLine).ToString();
        var response = new Response()
        {
            Protocol = HttpProtocols.Http11,
            HttpStatusCode = HttpStatusCode.OK
        };
        string requestArgument = string.Empty;

        switch (urlPath)
        {
            case string s when s.StartsWith(EndpointPaths.Files):
                var httpMethod = RegexParser.HttpMethodRegex.Match(urLine).ToString();
                requestArgument = urlPath[7..];
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
                else if(httpMethod == HttpMethod.Post.ToString())
                {
                    var contentLength = headers["Content-Length"];
                    var dataToWrite = body;

                    await File.WriteAllTextAsync(filePath, dataToWrite);

                    response.HttpStatusCode = HttpStatusCode.Created;
                }
                break;
            case string s when s.StartsWith(EndpointPaths.Echo):
                requestArgument = urlPath[6..];
                var encodings = headers["Accept-Encoding"].Split(", ");

                if (encodings?.Length > 0)
                {
                    var validEncoding = encodings.Intersect(Constants.ValidEncodings);

                    if(validEncoding.Contains("gzip"))
                    {
                        response.ContentEncoding = "gzip";
                        
                        var stream = new MemoryStream();
                        var gzipStream = new GZipStream(stream, CompressionLevel.Optimal, leaveOpen: true);

                            await gzipStream.WriteAsync(Encoding.ASCII.GetBytes(requestArgument));


                        stream.Position = 0;
                        requestArgument = Encoding.ASCII.GetString(stream.ToArray());
                    }
                }

                response.HttpStatusCode = HttpStatusCode.OK;
                response.ContentType = HttpContentTypes.TextPlain;
                response.ContentLength = requestArgument!.Length;
                response.Content = requestArgument;

                break;
            case EndpointPaths.UserAgent:
                var userAgentHeader = headers["User-Agent"];

                response.HttpStatusCode = HttpStatusCode.OK;
                response.ContentType = HttpContentTypes.TextPlain;
                response.ContentLength = userAgentHeader.Length;
                response.Content = userAgentHeader;

                break;
            case EndpointPaths.Default:
                break;
            default:
                response.HttpStatusCode = HttpStatusCode.NotFound;

                break;
        }

        try
        {
            var responseBytes = Encoding.ASCII.GetBytes(response.ToString());

            await socket.SendAsync(responseBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while sending a response back! {ex}");
        }
    }
}