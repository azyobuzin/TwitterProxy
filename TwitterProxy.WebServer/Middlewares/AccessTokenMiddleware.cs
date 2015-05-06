using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Helpers;
using TwitterProxy.Common;
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
                        Debug.WriteLine("Got access_token: " + resDic["screen_name"]);
                        await CoreServer.Client.AccessTokens.Insert(
                            oauthDic["oauth_consumer_key"],
                            resDic["oauth_token"],
                            resDic["oauth_token_secret"],
                            ulong.Parse(resDic["user_id"])
                        ).ConfigureAwait(false);
                    }
                }

                await context.Response.FromHttpResponseMessage(res).ConfigureAwait(false);
            }
        }
    }
}
