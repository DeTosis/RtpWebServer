using RtpWebServer.ServerCore.Configuration;
using RtpWebServer.ServerCore.Error;
using RtpWebServer.ServerCore.Redirect;
using RtpWebServer.ServerCore.Request;
using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtpWebServer.ServerCore;
public class HTTPSServer {
    HTTPSServerConfig httpsServerConfig;
    private int maxConnections => httpsServerConfig.MaxInstanceConnections;
    private int activeConnections = 0;
    public HTTPSServer(HTTPSServerConfig serverConfig) {
        httpsServerConfig = serverConfig;
    }

    public async Task RunServerAsync() {
        if (httpsServerConfig.RedirectHTTPToSecure) {
            _ = Task.Run(HTTPToSecureRedirect.RunHttpRedirectServer);
        }

        using Socket listener = new Socket(
            httpsServerConfig.ServerEndpoint.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp
        );

        listener.Bind(httpsServerConfig.ServerEndpoint);
        listener.Listen(httpsServerConfig.ServerEndpoint.Port);

        while (true) {
            var handler = await listener.AcceptAsync();

            if (activeConnections >= maxConnections) {
                handler.Close();
                Console.WriteLine("Connection Limit Excided");
            } else {
                _ = Task.Run(() => HandleClientSessionAsync(handler));
            }
        }
    }

    private async Task HandleClientSessionAsync(Socket handler) {
        try {
            Interlocked.Increment(ref activeConnections);
            using SslStream sslStream = new(new NetworkStream(handler), false);
            sslStream.AuthenticateAsServer(httpsServerConfig.ServerCert, false, true);

            using StreamWriter sw = new(sslStream);

            //*** NEED TO PROCESS REQ AND FORMAT RESPONSE ***
             
            HTTPStatus httpStatus = new();
            RequestData? rData = new RequestProcessor().ProcessRequest(sslStream, ref httpStatus);

            byte[] message = Encoding.UTF8.GetBytes($"HTTP/1.1 {httpStatus.StatusCode} {httpStatus.Description}\r\n" + "\r\n");
            await sslStream.WriteAsync(message);
            sw.Flush();
            sslStream.Close();
            //*** NEED TO PROCESS REQ AND FORMAT RESPONSE ***
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        } finally {
            handler.Dispose();
            Interlocked.Decrement(ref activeConnections);
        }
    }
}