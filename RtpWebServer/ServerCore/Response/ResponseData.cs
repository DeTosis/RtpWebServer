using System.Collections.Generic;

namespace RtpWebServer.ServerCore.Response;
public class ResponseData {
    public string StartLine { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public byte[] Body { get; set; }
}
