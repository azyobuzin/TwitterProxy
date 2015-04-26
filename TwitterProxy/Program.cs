using System.Net;
using Microsoft.Owin.Hosting;
using TwitterProxy.Common;

namespace TwitterProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;

            WebAppRunner.Run<WebServer.Startup>(new StartOptions(args[0]));

            Database.DisposeEngine();
        }
    }
}
