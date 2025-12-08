using Microsoft.AspNetCore.Http;
using System.Net;

namespace Achiever
{
    public static class HttpRequestExtensions
    {
        public static bool IsLocal(this HttpRequest request)
        {
            var connection = request.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                // Compare the remote IP address to the local IP address
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }
                // Check if the remote IP address is a loopback address (127.0.0.1, ::1, etc.)
                else
                {
                    return IPAddress.IsLoopback(connection.RemoteIpAddress);
                }
            }

            // When running with Microsoft.AspNetCore.TestHost, both IPs might be null
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
}
