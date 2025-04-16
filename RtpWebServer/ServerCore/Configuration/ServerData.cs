using System;
using System.Collections.Generic;
using System.IO;

namespace RtpWebServer.ServerCore.Configuration; 
public static class ServerData {
    public static string ServerWorkingDirectory { get; private set; } = "";
    public static List<string> ServerProtectedPaths { get; private set; } = new();
    public static string HostName { get; set; }
    public static string MainPage { get; set; }

    public static bool IsRequestedPathAccesable(string requestedPath) {
        foreach (var i in ServerProtectedPaths) {
            if (requestedPath.Contains(i)) {
                return false;
            }
        }
        return true;
    }

    public static bool SetWorkingDirectory(string directoryPath) {
        if (!Directory.Exists(directoryPath)) return false;

        ServerWorkingDirectory = directoryPath;

        return true;
    }
    
    public static bool AddProtectedPath(string relativePath) {
        if (ServerProtectedPaths.Contains(relativePath)) return false;

        if (relativePath.StartsWith(@"\") || relativePath.StartsWith("/")) {
            if (!Directory.Exists(ServerWorkingDirectory + relativePath)) return false;
        } else {
            if (!Directory.Exists(ServerWorkingDirectory + @"\" +  relativePath)) return false;
        }


            ServerProtectedPaths.Add(relativePath);
        return true;
    }
}
