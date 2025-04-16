using System.Net;

namespace RtpWebServer.ServerCore.Configuration;

public class HTTPServerConfig {
    public IPEndPoint ServerEndpoint { get; private set; }
    public int MaxInstanceConnections { get; private set; }

    public HTTPServerConfig(IPEndPoint serverIP, int maxSocketConnections) {
        ServerEndpoint = serverIP;
        MaxInstanceConnections = maxSocketConnections;
    }
}
