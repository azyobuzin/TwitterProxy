using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azyobuzi.OwinRazor;
using Microsoft.Owin;
using Microsoft.Owin.Helpers;
using Microsoft.Owin.Security.DataProtection;
using Owin;
using TwitterProxy.Common;
using TwitterProxy.WebServer.Models;

namespace TwitterProxy.WebServer.Middlewares
{
    class MypageMiddleware : OwinMiddleware
    {
        private const string SessionCookie = "twproxysess";
        private const string RequestTokenCookie = "twproxyrt";

        public MypageMiddleware(OwinMiddleware next, IAppBuilder app)
            : base(next)
        {
            this.dataProtector = app.CreateDataProtector(typeof(MypageMiddleware).FullName);
        }

        private IDataProtector dataProtector;

        public override Task Invoke(IOwinContext context)
        {
            switch (context.Request.Path.Value)
            {
                case "/mypage":
                    return this.Index(context);
                case "/mypage/callback":
                    return this.Callback(context);
            }

            return this.Next.Invoke(context);
        }

        private static Uri GetBaseUri(IOwinRequest request)
        {
            var baseUriEnv = EnvironmentVariables.BaseUri;
            return new Uri(baseUriEnv != null
                ? baseUriEnv
                : (request.Uri.GetLeftPart(UriPartial.Authority) + request.PathBase.Value)
            );
        }

        private async Task<ulong?> Authorize(IOwinContext context)
        {
            var req = context.Request;
            var cookie = req.Cookies[SessionCookie];
            if (cookie != null)
            {
                try
                {
                    return BitConverter.ToUInt64(this.dataProtector.Unprotect(Convert.FromBase64String(cookie)), 0);
                }
                catch { }
            }

            using (var client = new HttpClient(new TwitterOAuthHttpMessageHandler()
            {
                ConsumerKey = EnvironmentVariables.ConsumerKey,
                ConsumerSecret = EnvironmentVariables.ConsumerSecret,
                OAuthCallback = new Uri(GetBaseUri(req), "mypage/callback").AbsoluteUri
            }))
            using (var res = await client.PostAsync("https://api.twitter.com/oauth/request_token", null).ConfigureAwait(false))
            {
                var resDic = WebHelpers.ParseForm(await res.Content.ReadAsStringAsync().ConfigureAwait(false));
                var protectedTokenSecret = Convert.ToBase64String(this.dataProtector.Protect(Encoding.UTF8.GetBytes(resDic["oauth_token_secret"])));
                context.Response.Cookies.Append(RequestTokenCookie, protectedTokenSecret, new CookieOptions() { HttpOnly = true });
                context.Response.Redirect("https://api.twitter.com/oauth/authenticate?oauth_token=" + resDic["oauth_token"]);
            }

            return null;
        }

        private async Task Callback(IOwinContext context)
        {
            var req = context.Request;
            var res = context.Response;

            var requestTokenCookie = req.Cookies[RequestTokenCookie];
            var requestToken = req.Query["oauth_token"];
            var verifier = req.Query["oauth_verifier"];

            res.Cookies.Delete(RequestTokenCookie);

            if (!string.IsNullOrEmpty(requestTokenCookie) && !string.IsNullOrEmpty(requestToken) && !string.IsNullOrEmpty(verifier))
            {
                var requestTokenSecret = Encoding.UTF8.GetString(this.dataProtector.Unprotect(Convert.FromBase64String(requestTokenCookie)));

                IFormCollection resDic;

                using (var client = new HttpClient(new TwitterOAuthHttpMessageHandler()
                {
                    ConsumerKey = EnvironmentVariables.ConsumerKey,
                    ConsumerSecret = EnvironmentVariables.ConsumerSecret,
                    OAuthToken = requestToken,
                    OAuthTokenSecret = requestTokenSecret,
                    OAuthVerifier = verifier
                }))
                using (var tokenRes = await client.PostAsync("https://api.twitter.com/oauth/access_token", null).ConfigureAwait(false))
                {
                    tokenRes.EnsureSuccessStatusCode();
                    resDic = WebHelpers.ParseForm(await tokenRes.Content.ReadAsStringAsync().ConfigureAwait(false));
                }

                var userId = ulong.Parse(resDic["user_id"]);
                await CoreServer.Client.ProxyUsers.Insert(userId, resDic["oauth_token"], resDic["oauth_token_secret"]).ConfigureAwait(false);

                res.Cookies.Append(
                    SessionCookie,
                    Convert.ToBase64String(this.dataProtector.Protect(BitConverter.GetBytes(userId))),
                    new CookieOptions()
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddDays(14)
                    }
                );
            }

            res.Redirect(new Uri(GetBaseUri(req), "mypage").AbsoluteUri);
        }

        private async Task Index(IOwinContext context)
        {
            var res = context.Response;

            var userId = await this.Authorize(context).ConfigureAwait(false);
            if (!userId.HasValue) return;

            var model = new MypageModel();

            try
            {
                model.ScreenName = await CoreServer.Client.TwitterUsers.GetScreenName(userId.Value).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Trace.TraceWarning(ex.ToString());
                res.Cookies.Delete(SessionCookie);
                this.Authorize(context).Wait();
                return;
            }

            await res.View("Mypage", model).ConfigureAwait(false);
        }
    }
}
