using System;
using System.Threading.Tasks;
using Microsoft.Owin;

namespace TwitterProxy.WebServer.Middlewares
{
    class RedirectMiddleware : OwinMiddleware
    {
        public RedirectMiddleware(OwinMiddleware next, string requestBase)
            : base(next)
        {
            this.requestBase = requestBase;
        }

        private readonly string requestBase;

        public override Task Invoke(IOwinContext context)
        {
            var req = context.Request;
            var res = context.Response;
            if (req.IsGet() || req.IsHead())
            {
                res.StatusCode = 301;
                res.Headers["Location"] = new UriBuilder(this.requestBase + req.Path.ToUriComponent())
                {
                    Query = req.QueryString.Value
                }.Uri.AbsoluteUri;
            }
            else
            {
                res.StatusCode = 405;
            }
            return Task.FromResult(true);
        }
    }
}
