using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace RtpWebServer.ServerCore.Configuration;

public class HTTPSServerConfig : HTTPServerConfig {
    public X509Certificate ServerCert { get; set; }
    public bool RedirectHTTPToSecure { get; set; }
    public HTTPSServerConfig(IPEndPoint serverIP, X509Certificate serverCert, int maxInstanceConnections, bool redirectHTTPToSecure = true)
        : base(serverIP, maxInstanceConnections) {
        ServerCert = serverCert;
        RedirectHTTPToSecure = redirectHTTPToSecure;
    }
}
