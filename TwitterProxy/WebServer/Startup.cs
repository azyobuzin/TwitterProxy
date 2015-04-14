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
                .Map("/twitter/api", ConfigureTwitterApi);
        }

        private static void ConfigureTwitterApi(IAppBuilder app)
        {
            app.Match<AccessTokenMiddleware>("/oauth/access_token")
                .Use<ProxyMiddleware>("https://api.twitter.com");
        }
    }
}
