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

        var (urlLine, headers, body) = RequestParser.ParseRequest(receivedData);

        var urlPath = RegexParser.UrlPathRegex().Match(urlLine).ToString();
        var response = new Response()
        {
            Protocol = HttpProtocols.Http11,
            HttpStatusCode = HttpStatusCode.OK
        };

        switch (urlPath)
        {
            case string s when s.StartsWith(EndpointPaths.Files):
                await EndpointsHandler.HandleFiles(urlLine, headers, body, urlPath, response);

                break;
            case string s when s.StartsWith(EndpointPaths.Echo):
                await EndpointsHandler.HandleEcho(headers, urlPath, response);

                break;
            case EndpointPaths.UserAgent:
                EndpointsHandler.HandleUserAgent(headers, response);

                break;
            case EndpointPaths.Home:
                break;
            default:
                response.HttpStatusCode = HttpStatusCode.NotFound;

                break;
        }

        try
        {
            var responseBytes = Encoding.ASCII.GetBytes(response.ToString());

            if (response.CompressedContent?.Length > 0)
            {
                responseBytes = [..responseBytes, ..response.CompressedContent];
            }

            await socket.SendAsync(responseBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while sending a response back! {ex}");
        }
    }
}