using RtpWebServer.ServerCore.Configuration;
using RtpWebServer.ServerCore.Error;
using RtpWebServer.ServerCore.Request;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RtpWebServer.ServerCore.Response;
public class ResponseBuilder {
    public string EndLine { get; } = "\r\n";
    public string HTTPVersion { get; } = "HTTP/1.1";
    public ResponseData BuildResponse(RequestData requestData, ref HTTPStatus httpStatus) {
        ResponseData respData = new();
        switch (requestData.StartLine[0]) {
            case "GET":
                respData = FormatResponseToGet(ref requestData, ref httpStatus);
                break;
            case "POST":
                break;
            case "PUT":
                break;
            case "PATCH":
                break;
            case "DELETE":
                break;
            case "HEAD":
                break;
            case "OPTIONS":
                break;
            default:
                httpStatus.SetStausCode(400);
                Console.WriteLine("from switch");
                ExceptionResponse(ref requestData, ref httpStatus);
                break;
        }
        string startLine = $"{HTTPVersion} {httpStatus.StatusCode} {httpStatus.Description} {EndLine}";
        respData.StartLine = startLine;

        return respData;
    }

    public ResponseData ExceptionResponse(ref RequestData requestData, ref HTTPStatus httpStatus) {
        Console.WriteLine("=== Exc response ===");
        string startLine = $"{HTTPVersion} {httpStatus.StatusCode} {httpStatus.Description} {EndLine}";

        ResponseData rd = new ResponseData();
        rd.StartLine = startLine;
        rd.Headers = new Dictionary<string, string>() {{"Connection:", "close" }};
        rd.Body = Encoding.ASCII.GetBytes("");

        return rd;
    }

    private ResponseData FormatResponseToGet(ref RequestData requestData, ref HTTPStatus httpStatus) {
        Dictionary<string, string> responseHeaders = new();

        byte[] responseBody = Encoding.ASCII.GetBytes("");

        if (requestData.StartLine[1] == "/" || requestData.StartLine[1] == "/") {
            requestData.StartLine[1] += ServerData.MainPage;
        }

        if (CheckRequestedFile(ref requestData, ref httpStatus)) {
            responseBody = File.ReadAllBytes(ServerData.ServerWorkingDirectory + requestData.StartLine[1]);
        }

        string requestedFileEXT = requestData.StartLine[1].Split(".").Last();
        string contentType = GetContentType(requestedFileEXT);
        responseHeaders.Add("Content-Type:", contentType);

        if (requestData.Headers.ContainsKey("Connection".ToLower())) {
            responseHeaders.Add("Connection:", requestData.Headers["Connection".ToLower()]);
        }

        int contentLength = responseBody.Length;
        responseHeaders.Add("Content-Length:", contentLength.ToString());

        ResponseData respData = new();
        respData.Headers = responseHeaders;
        respData.Body = responseBody;

        return respData;
    }

    private string GetContentType(string fileExt) {
        string contentType = "";
        switch (fileExt) {
            case "html":
                contentType += "text/html ";
                break;
            case "css":
                contentType += "text/css ";
                break;
            default:
                contentType = "text/plain ";
                break;
        }
        return contentType;
    }

    private bool CheckRequestedFile(ref RequestData requestData, ref HTTPStatus httpStatus) {
        string filePath = ServerData.ServerWorkingDirectory + requestData.StartLine[1];
        if (!File.Exists(filePath)) {
            httpStatus.SetStausCode(404);
            return false;
        }

        return true;
    }
}
