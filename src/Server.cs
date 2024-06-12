using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

TcpListener server = new(IPAddress.Any, 4221);
server.Start();

Regex urlPathRegex = new("\\/\\S*");

using (var socket = server.AcceptSocket())
{
    var receivedData = new byte[2048];
    await socket.ReceiveAsync(receivedData);

    var decodedReceivedData = Encoding.ASCII.GetString(receivedData);
    var urlPath = urlPathRegex.Match(decodedReceivedData).ToString();
    var response = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");

    if (urlPath != "/")
    {
        response = Encoding.ASCII.GetBytes("HTTP/1.1 404 Not Found\r\n\r\n");
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

server.Stop();