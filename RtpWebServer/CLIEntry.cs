using RtpWebServer.ServerCore;
using RtpWebServer.ServerCore.Configuration;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace RtpWebServer;
internal class CLIEntry {
    static async Task Main(string[] args) {
        //Website directory
        ServerData.SetWorkingDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Server");
        ServerData.AddProtectedPath(@"etc");
        ServerData.HostName = "detosis.hec.to";
        ServerData.MainPage = "index.html";

        //ssl cert
        var serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            ServerData.ServerWorkingDirectory + @"\etc\cert\cert.pfx",
            File.ReadAllText(ServerData.ServerWorkingDirectory + @"\etc\cert\certPwd.txt"));

        int serverPort = 443;
        int maxSocketConnections = 10;

        HTTPSServerConfig scfg = new(new IPEndPoint(IPAddress.Any, serverPort), serverCertificate, maxSocketConnections);
        HTTPSServer server = new(scfg);

        Thread serverThread = new(async () => await server.RunServerAsync());
        serverThread.Start();

        Console.ReadKey();
    }
}
