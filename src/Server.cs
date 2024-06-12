using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

TcpListener server = new(IPAddress.Any, 4221);
server.Start();

Regex httpMethodRegex = new("^GET|POST");
Regex urlPathRegex = new("\\/\\S*");
Regex userAgentRegex = new("[Uu]ser-[Aa]gent: \\S*");
Regex contentLengthRegex = new("[Cc]ontent-[Ll]ength: \\S*");
Regex acceptEncodingRegex = new("[Aa]ccept-[Ee]ncoding: \\S*");
var crlf = "\r\n";

while (true)
{
    using (var socket = await server.AcceptSocketAsync())
    {
        var receivedData = new byte[2048];
        await socket.ReceiveAsync(receivedData);

        var decodedReceivedData = Encoding.ASCII.GetString(receivedData);
        var urlPath = urlPathRegex.Match(decodedReceivedData).ToString();
        var response = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK{crlf}{crlf}");
        string requestArgument = string.Empty;

        switch (urlPath)
        {
            case string s when s.StartsWith("/files/"):
                var httpMethod = httpMethodRegex.Match(decodedReceivedData).ToString();
                requestArgument = urlPath[7..];
                var filePath = $"{Environment.GetCommandLineArgs()[2]}{requestArgument}";

                if (httpMethod == "GET")
                {
                    if (File.Exists(filePath))
                    {
                        var file = await File.ReadAllBytesAsync(filePath);
                        var fileContent = Encoding.ASCII.GetString(file);

                        response =
                            Encoding.ASCII.GetBytes(
                                $"HTTP/1.1 200 OK{crlf}" +
                                $"Content-Type: application/octet-stream{crlf}" +
                                $"Content-Length:{file.Length}{crlf}{crlf}{fileContent}");
                    }
                    else
                    {
                        response = Encoding.ASCII.GetBytes($"HTTP/1.1 404 Not Found{crlf}{crlf}");
                    }
                }
                else if(httpMethod == "POST")
                {
                    var contentLength = int.Parse(contentLengthRegex.Match(decodedReceivedData).ToString()[16..]);
                    var dataToWrite = decodedReceivedData.Split($"{crlf}{crlf}")[1][..contentLength];

                    await File.WriteAllTextAsync(filePath, dataToWrite);

                    response = Encoding.ASCII.GetBytes($"HTTP/1.1 201 Created{crlf}{crlf}");
                }
                break;
            case string s when s.StartsWith("/echo/"):
                requestArgument = urlPath[6..];
                var acceptEncoding = acceptEncodingRegex.Match(decodedReceivedData).ToString()[17..];

                if (acceptEncoding == "invalid-encoding")
                {
                    response =
                        Encoding.ASCII.GetBytes(
                            $"HTTP/1.1 200 OK{crlf}" +
                            $"Content-Type: text/plain{crlf}" +
                            $"Content-Length:{requestArgument.Length}{crlf}{crlf}{requestArgument}");
                }
                else
                {
                    response =
                        Encoding.ASCII.GetBytes(
                            $"HTTP/1.1 200 OK{crlf}" +
                            $"Content-Encoding: {acceptEncoding}{crlf}" + 
                            $"Content-Type: text/plain{crlf}" +
                            $"Content-Length:{requestArgument.Length}{crlf}{crlf}{requestArgument}");
                }

                break;
            case "/user-agent":
                var userAgentHeader = userAgentRegex.Match(decodedReceivedData).ToString()[12..];

                response =
                    Encoding.ASCII.GetBytes(
                        $"HTTP/1.1 200 OK{crlf}" +
                        $"Content-Type: text/plain{crlf}" +
                        $"Content-Length:{userAgentHeader.Length}{crlf}{crlf}{userAgentHeader}");
                break;
            case "/":
                break;
            default:
                response = Encoding.ASCII.GetBytes($"HTTP/1.1 404 Not Found{crlf}{crlf}");
                break;
        }

        try
        {
            await socket.SendAsync(response);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception while sending a response back! {ex}");
        }
    }
}