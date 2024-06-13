using codecrafters_http_server.Constants;
using codecrafters_http_server.DTOs;
using codecrafters_http_server.Helpers;
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

        var decodedReceivedData = Encoding.ASCII.GetString(receivedData);

        var urlPath = RegexParser.UrlPathRegex().Match(decodedReceivedData).ToString();
        var response = new Response()
        {
            Protocol = HttpProtocols.Http11,
            HttpStatusCode = HttpStatusCode.OK
        };
        string requestArgument = string.Empty;

        switch (urlPath)
        {
            case string s when s.StartsWith(EndpointPaths.Files):
                var httpMethod = RegexParser.HttpMethodRegex.Match(decodedReceivedData).ToString();
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
                    var contentLength = int.Parse(RegexParser.ContentLengthRegex().Match(decodedReceivedData).ToString()[16..]);
                    var dataToWrite = decodedReceivedData.Split($"{Constants.Crlf}{Constants.Crlf}")[1][..contentLength];

                    await File.WriteAllTextAsync(filePath, dataToWrite);

                    response.HttpStatusCode = HttpStatusCode.Created;
                }
                break;
            case string s when s.StartsWith(EndpointPaths.Echo):
                requestArgument = urlPath[6..];
                var encodings = decodedReceivedData.Split(Constants.Crlf).FirstOrDefault(x => x.StartsWith("Accept-Encoding:"))?[16..].Split(",").Select(x => x.Trim());

                if (encodings?.Count() > 0)
                {
                    var validEncoding = encodings.Intersect(Constants.ValidEncodings).FirstOrDefault();

                    if(validEncoding != null)
                    {
                        response.ContentEncoding = validEncoding;
                    }
                }

                response.HttpStatusCode = HttpStatusCode.OK;
                response.ContentType = HttpContentTypes.TextPlain;
                response.ContentLength = requestArgument.Length;
                response.Content = requestArgument;

                break;
            case EndpointPaths.UserAgent:
                var userAgentHeader = RegexParser.UserAgentRegex().Match(decodedReceivedData).ToString()[12..];

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