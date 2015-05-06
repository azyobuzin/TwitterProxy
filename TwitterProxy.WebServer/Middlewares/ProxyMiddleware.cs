using System.Threading.Tasks;
using Microsoft.Owin;

namespace TwitterProxy.WebServer.Middlewares
{
    class ProxyMiddleware : OwinMiddleware
    {
        public ProxyMiddleware(OwinMiddleware next, string requestBase)
            : base(next)
        {
            this.requestBase = requestBase;
        }

        private readonly string requestBase;

        public override async Task Invoke(IOwinContext context)
        {
            var req = context.Request;
            using (var res = await ProxyUtils.DoRequest(req, this.requestBase + req.Path.ToUriComponent()).ConfigureAwait(false))
                await context.Response.FromHttpResponseMessage(res).ConfigureAwait(false);
        }
    }
}
