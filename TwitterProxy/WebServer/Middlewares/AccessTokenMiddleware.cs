using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Helpers;
using TwitterProxy.WebServer.Models;

namespace TwitterProxy.WebServer.Middlewares
{
    class AccessTokenMiddleware : OwinMiddleware
    {
        public AccessTokenMiddleware(OwinMiddleware next) : base(next) { }

        public override async Task Invoke(IOwinContext context)
        {
            var req = context.Request;
            using (var res = await ProxyUtils.DoRequest(req, "https://api.twitter.com/oauth/access_token").ConfigureAwait(false))
            {
                if (res.IsSuccessStatusCode && !req.IsHead())
                {
                    var oauthDic = req.ParseAuthorizationHeader();
                    if (oauthDic != null)
                    {
                        await res.Content.LoadIntoBufferAsync().ConfigureAwait(false);
                        var resDic = WebHelpers.ParseForm(await res.Content.ReadAsStringAsync2().ConfigureAwait(false));
                        var userId = ulong.Parse(resDic["user_id"]);
                        var screenName = resDic["screen_name"];
                        Debug.WriteLine("Got access_token: " + screenName);
                        using (var tran = Database.GetTransaction())
                        {
                            tran.Insert("tokens", resDic["oauth_token"], new AccessToken()
                            {
                                ConsumerKey = oauthDic["oauth_consumer_key"],
                                AccessTokenSecret = resDic["oauth_token_secret"],
                                UserId = userId,
                                CreatedAtUtc = DateTime.UtcNow
                            });
                            tran.Insert("screenNames", userId, screenName);
                            tran.Commit();
                        }
                    }
                }

                await context.Response.FromHttpResponseMessage(res).ConfigureAwait(false);
            }
        }
    }
}
