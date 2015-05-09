using System.Net;
using LightNode.Formatter;
using LightNode.Server;
using Microsoft.Owin.Hosting;
using Owin;
using TwitterProxy.Common;

namespace TwitterProxy.CoreServer
{
    class Startup
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;

            WebAppRunner.Run<Startup>(new StartOptions(args[0]));

            Database.DisposeEngine();
        }

        public void Configuration(IAppBuilder app)
        {
            app.UseLightNode(
                new LightNodeOptions(
                    AcceptVerbs.Get | AcceptVerbs.Post,
                    new MsgPackContentFormatter(),
                    new JsonNetContentFormatter()
                )
                {
                    ErrorHandlingPolicy = ErrorHandlingPolicy.ReturnInternalServerErrorIncludeErrorDetails,
                    OperationMissingHandlingPolicy = OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails
                }
            );
        }
    }
}
