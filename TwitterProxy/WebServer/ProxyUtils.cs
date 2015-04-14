using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace TwitterProxy.WebServer
{
    static class ProxyUtils
    {
        public static Task<HttpResponseMessage> DoRequest(IOwinRequest request, string uri, Stream body = null)
        {
            var uriBuilder = new UriBuilder(uri) { Query = request.QueryString.Value };
            Debug.WriteLine("Requesting: " + uriBuilder.ToString());
            var msg = new HttpRequestMessage(new HttpMethod(request.Method), uriBuilder.Uri);
            foreach (var kvp in request.Headers)
            {
                if (!kvp.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                    msg.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }
            if (!request.IsGet() && !request.IsHead())
            {
                msg.Content = new StreamContent(body ?? request.Body);
            }
            var client = new HttpClient(new HttpClientHandler()
            {
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None
            });
            return client.SendAsync(msg, HttpCompletionOption.ResponseHeadersRead, request.CallCancelled);
        }


        public static Task FromHttpResponseMessage(this IOwinResponse response, HttpResponseMessage msg)
        {
            response.StatusCode = (int)msg.StatusCode;
            response.ReasonPhrase = msg.ReasonPhrase;
            foreach (var kvp in msg.Headers)
            {
                var value = kvp.Value.ToArray();
                if (value.Length > 0 && kvp.Key.ToLowerInvariant() != "strict-transport-security")
                    response.Headers.Add(kvp.Key, value);
            }
            foreach (var kvp in msg.Content.Headers)
            {
                var value = kvp.Value.ToArray();
                if (value.Length > 0)
                    response.Headers.Add(kvp.Key, value);
            }
            response.Headers.Add("x-twitterapi-url", new[] { msg.RequestMessage.RequestUri.AbsoluteUri });
            return msg.Content.CopyToAsync(response.Body);
        }
    }
}
