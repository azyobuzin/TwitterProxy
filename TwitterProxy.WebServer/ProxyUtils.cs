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
            var requestUri = new UriBuilder(uri) { Query = request.QueryString.Value }.Uri;
            Debug.WriteLine("Requesting: " + requestUri.AbsoluteUri);

            var msg = new HttpRequestMessage(new HttpMethod(request.Method), requestUri);

            if (!request.IsGet() && !request.IsHead())
                msg.Content = new StreamContent(body ?? request.Body);

            foreach (var kvp in request.Headers)
            {
                if (kvp.Key.EqualsIgnoreCase("Host")) continue;

                try
                {
                    msg.Headers.Remove(kvp.Key);
                    msg.Headers.Add(kvp.Key, kvp.Value);
                    continue;
                }
                catch { }

                if (msg.Content != null)
                {
                    try
                    {
                        msg.Content.Headers.Remove(kvp.Key);
                        msg.Content.Headers.Add(kvp.Key, kvp.Value);
                        continue;
                    }
                    catch { }
                }

                Trace.TraceInformation("Could not add to header: {0}: {1}", kvp.Key, kvp.Value);
            }

            if (request.QueryString.HasValue && requestUri.Query.TrimStart('?') != request.QueryString.Value)
            {
                //TODO: re-create authorization header
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
                if (value.Length > 0 && !kvp.Key.EqualsIgnoreCase("strict-transport-security"))
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
