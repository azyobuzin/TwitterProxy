using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Diagnostics;
using Owin;
using TwitterProxy.WebServer.Middlewares;

namespace TwitterProxy.WebServer
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseErrorPage(ErrorPageOptions.ShowAll)
                .Use<MypageMiddleware>(app)
                .Map("/twitter/api", ConfigureTwitterApi);
        }

        private static void ConfigureTwitterApi(IAppBuilder app)
        {
            const string baseUri = "https://api.twitter.com";
            app.Match<AccessTokenMiddleware>("/oauth/access_token")
                .Match<RedirectMiddleware>(new[] { "/oauth/authenticate", "/oauth/authorize" }, baseUri)
                .Use<ProxyMiddleware>(baseUri);
        }
    }
}
