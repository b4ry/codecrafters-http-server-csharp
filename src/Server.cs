using System.Net;
using System.Net.Sockets;
using System.Text;

TcpListener server = new(IPAddress.Any, 4221);
server.Start();

using (var socket = server.AcceptSocket())
{
    var response = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\n\r\n");

    try
    {
        await socket.SendAsync(response);
        await Task.Delay(100);
    }
    catch(Exception ex)
    {
        Console.WriteLine($"Exception while sending a response back! {ex}");
    }
}

server.Stop();