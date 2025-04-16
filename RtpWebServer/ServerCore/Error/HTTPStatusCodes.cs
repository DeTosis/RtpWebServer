using System.Collections.Generic;

namespace RtpWebServer.ServerCore.Error; 
public static class HTTPStatusCodes {
    public static Dictionary<int, string> StatusCodes { get; } = new() {
        {200,"OK" },
        {400,"Bad Request" },
        {403,"Forbidden" },
        {404,"Not Found" },
        {406,"Not Acceptable" },
        {413,"Payload Too Large" },
        {414,"URI Too Long" },
        {500,"Internal Server Error" },
    };
}
