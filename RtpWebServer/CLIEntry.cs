using RtpWebServer.ServerCore;
using RtpWebServer.ServerCore.Configuration;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RtpWebServer;
internal class CLIEntry {
    static async Task Main(string[] args) {
        ServerData.SetWorkingDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\Server");
        ServerData.AddProtectedPath(@"etc");

        var serverCertificate = X509CertificateLoader.LoadPkcs12FromFile(
            ServerData.ServerWorkingDirectory + @"\etc\cert\cert.pfx",
            File.ReadAllText(ServerData.ServerWorkingDirectory + @"\etc\cert\certPwd.txt"));

        HTTPSServerConfig scfg = new(new IPEndPoint(IPAddress.Any, 443), serverCertificate, maxInstanceConnections: 10);
        HTTPSServer server = new(scfg);

        Task servTask = Task.Run(server.RunServerAsync);
        Console.ReadLine();
    }
}
