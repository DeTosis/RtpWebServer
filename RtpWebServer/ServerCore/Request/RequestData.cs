using System;
using System.Collections.Generic;

namespace RtpWebServer.ServerCore.Request; 
public class RequestData {
    public RequestData(string[] startLine, Dictionary<string,string> headers, string body) {
        StartLine = startLine;
        Headers = headers;
        Body = body;
    }

    public string[] StartLine { get; }
    public Dictionary<string, string> Headers { get; }
    public string? Body { get; }
}
