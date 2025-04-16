using RtpWebServer.ServerCore.Configuration;
using RtpWebServer.ServerCore.Error;
using RtpWebServer.ServerCore.Redirect;
using RtpWebServer.ServerCore.Request;
using RtpWebServer.ServerCore.Response;
using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtpWebServer.ServerCore;
public class HTTPSServer {
    HTTPSServerConfig httpsServerConfig;
    public string EndLine { get; } = "\r\n";
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

            HTTPStatus httpStatus = new();
            RequestData? reqData = new RequestProcessor().ProcessRequest(sslStream, ref httpStatus);

            ResponseData? respData = new();
            if (reqData == null) {
                respData = new ResponseBuilder().ExceptionResponse(ref reqData, ref httpStatus);
            } else {
                respData = new ResponseBuilder().BuildResponse(reqData, ref httpStatus);
            }

            bool keepConnection = false;
            if (respData.Headers.ContainsKey("Connection".ToLower())) {
                if (respData.Headers["Connection"] == "keep-alive") {
                    keepConnection = true;
                } else {
                    keepConnection = false;
                }
            }

            string headers = "";
            foreach (var i in respData.Headers) {
                headers += $"{i.Key} {i.Value}{EndLine}";
            }

            string respStr =
                respData.StartLine +
                headers +
                EndLine;

            byte[] respMsg = Encoding.ASCII.GetBytes(respStr);
            byte[] respRtS = respMsg.Concat(respData.Body).ToArray();

            await sslStream.WriteAsync(respRtS);
            sw.Flush();

            if (!keepConnection) {
                sslStream.Close();
            } 
        } catch (Exception e) {
            Console.WriteLine(e.Message);
        } finally {
            handler.Dispose();
            Interlocked.Decrement(ref activeConnections);
        }
    }
}