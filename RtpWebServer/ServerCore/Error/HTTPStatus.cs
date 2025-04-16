using System;

namespace RtpWebServer.ServerCore.Error; 
public class HTTPStatus {
    public bool Not200 { get; private set; } = false;
    public int StatusCode { get; private set; }
    public string Description { get; private set; }

    public void SetStausCode(int statusCode) {
        if (!HTTPStatusCodes.StatusCodes.ContainsKey(statusCode)) throw new Exception("Undefined http status code");

        if (statusCode != 200) {
            Not200 = true;
        }

        StatusCode = statusCode;
        Description = HTTPStatusCodes.StatusCodes[StatusCode];
    }
}
