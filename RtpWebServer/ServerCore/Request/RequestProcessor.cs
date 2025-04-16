using RtpWebServer.ServerCore.Configuration;
using RtpWebServer.ServerCore.Error;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RtpWebServer.ServerCore.Request;
public class RequestProcessor {
    public int startLineSize { get; } = 8192;
    public int headersSize { get; } = 8192;
    public int bodySize { get; } = 1048576;

    public RequestData? ProcessRequest(SslStream requestStream, ref HTTPStatus httpStatus) {
        var streamReader = new StreamReader(requestStream);

        var startLine = ProcessStartLine(streamReader, ref httpStatus);
        if (startLine == null) return null;

        var headers = ProcessHeaders(streamReader, ref httpStatus);
        if (headers == null) return null;

        var body = ProcessBody(streamReader, headers, ref httpStatus);

        httpStatus.SetStausCode(200);

        RequestData data = new RequestData(startLine, headers, body);
        return data;
    }

    private string[]? ProcessStartLine(StreamReader streamReader, ref HTTPStatus httpStatus) {
        string? line = null;
        int size = -1;

        string[] lineD = new string[3];
        if (!string.IsNullOrEmpty(line = streamReader.ReadLine())) {
            try {
                var lineData = line.Split(" ");
                if (lineData.Length != 3) {
                    httpStatus.SetStausCode(400);
                    return null;
                }
                lineD = lineData;
            } catch (Exception e) {
                httpStatus.SetStausCode(400);
                return null;
            }
        } else {
            httpStatus.SetStausCode(400);
            return null;
        }

        if (!ServerData.IsRequestedPathAccesable(lineD[1])) { 
            httpStatus.SetStausCode(403);
            return null;

        }

        size = Encoding.ASCII.GetByteCount(line);
        if (size > startLineSize) {
            httpStatus.SetStausCode(414);
            return null;
        }

        return lineD;
    }
    private Dictionary<string, string>? ProcessHeaders(StreamReader streamReader, ref HTTPStatus httpStatus) {
        string? line = null;
        int size = -1;

        Dictionary<string, string> headers = new();
        int rHeadersSize = 0;
        while (!string.IsNullOrEmpty(line = streamReader.ReadLine())) {
            size = Encoding.ASCII.GetByteCount(line);
            if (size > headersSize) {
                httpStatus.SetStausCode(400);
                return null;
            }

            var headerData = line.Split(":", 2);

            if (headerData.Length != 2) {
                httpStatus.SetStausCode(400);
                return null;
            }

            headerData[0] = headerData[0].ToLower() + ":";
            headerData[1] = headerData[1].ToLower().TrimStart();

            if (headerData[0].Contains(" ")) {
                httpStatus.SetStausCode(400);
                return null;
            }

            if (headerData[0].Contains("\r") || headerData[0].Contains("\n")) {
                httpStatus.SetStausCode(400);
                return null;
            }

            rHeadersSize += Encoding.ASCII.GetByteCount(line);
            headers.Add(headerData[0], headerData[1]);
        }
        if (rHeadersSize > headersSize) {
            httpStatus.SetStausCode(400);
            return null;
        }

        return headers;
    }
    private string? ProcessBody(StreamReader streamReader, Dictionary<string, string> headers, ref HTTPStatus httpStatus) {
        int bodySize = 0;
        string? body = null;
        if (headers.ContainsKey("Content-Length")) {
            try {
                bodySize = Convert.ToInt32(headers["Content-Length"]);
            } catch (Exception e) {
                httpStatus.SetStausCode(400);
                return null;
            }

            if (bodySize > this.bodySize) {
                httpStatus.SetStausCode(400);
                return null;
            }

            char[] buffer = new char[bodySize];
            streamReader.ReadBlock(buffer, 0, bodySize);
            body = new string(buffer);
            return body;
        }

        return null;
    }
}
