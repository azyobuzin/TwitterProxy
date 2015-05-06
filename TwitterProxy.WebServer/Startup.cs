using System.Net;
using Microsoft.Owin.Diagnostics;
using Microsoft.Owin.Hosting;
using Owin;
using TwitterProxy.Common;
using TwitterProxy.WebServer.Middlewares;

namespace TwitterProxy.WebServer
{
    class Startup
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;
            CoreServer.RootEndPoint = args[1];

            WebAppRunner.Run<Startup>(new StartOptions(args[0]));
        }

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
