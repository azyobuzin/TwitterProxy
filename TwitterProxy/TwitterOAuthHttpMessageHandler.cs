using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace TwitterProxy
{
    class TwitterOAuthHttpMessageHandler : DelegatingHandler
    {
        public TwitterOAuthHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
        public TwitterOAuthHttpMessageHandler() : this(new HttpClientHandler()) { }

        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string OAuthToken { get; set; }
        public string OAuthTokenSecret { get; set; }
        public string OAuthCallback { get; set; }
        public string OAuthVerifier { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var query = new StringBuilder(request.RequestUri.Query);
            if (query.Length > 0 && query[0] == '?') query.Remove(0, 1);

            var content = request.Content;
            if (content != null && content.Headers.ContentType.MediaType == "application/x-www-form-urlencoded")
            {
                query.Append('&');
                query.Append(await content.ReadAsStringAsync().ConfigureAwait(false));
            }

            var oauthParams = OAuth.OAuthParameters(this.ConsumerKey, this.OAuthToken, this.OAuthCallback, this.OAuthVerifier);
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "OAuth",
                OAuth.CreateHeader(oauthParams, OAuth.Signature(
                    OAuth.SignatureBaseString(request.Method.Method, request.RequestUri.AbsoluteUri, query.ToString(), oauthParams),
                    this.ConsumerSecret, this.OAuthTokenSecret
                ))
            );

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
