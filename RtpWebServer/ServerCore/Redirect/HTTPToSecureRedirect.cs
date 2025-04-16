using System.IO;
using System.Net.Sockets;
using System.Text;
using System;
using System.Net;
using System.Threading.Tasks;

namespace RtpWebServer.ServerCore.Redirect; 
public class HTTPToSecureRedirect {
    public static async Task RunHttpRedirectServer() {
        TcpListener httpListener = new TcpListener(IPAddress.Any, 80);
        httpListener.Start();

        Console.WriteLine(" > HTTPS redirected to HTTP automaticly");

        while (true) {
            var client = await httpListener.AcceptTcpClientAsync();
            _ = Task.Run(() => HandleHttpRedirect(client));
        }
    }

    static void HandleHttpRedirect(TcpClient client) {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.ASCII, leaveOpen: true);

        string requestLine = reader.ReadLine();
        if (string.IsNullOrWhiteSpace(requestLine)) return;

        string host = "";
        string line;
        while (!string.IsNullOrEmpty(line = reader.ReadLine())) {
            if (line.StartsWith("Host:", StringComparison.OrdinalIgnoreCase)) {
                host = line.Substring(5).Trim();
            }
        }

        string httpsUrl = $"https://{host}/";
        string response =
            $"HTTP/1.1 301 Moved Permanently\r\n" +
            $"Location: {httpsUrl}\r\n" +
            $"Connection: close\r\n" + "\r\n";
        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
        stream.Write(responseBytes, 0, responseBytes.Length);
        client.Close();
    }
}
