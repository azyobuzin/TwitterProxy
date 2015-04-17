using System;
using System.IO;
using System.Net;
using System.Threading;
using Azyobuzi.OwinRazor;
using Microsoft.Owin.Hosting;

namespace TwitterProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            var uri = args[0];

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = (_, __, ___, ____) => true;
            AppTemplateManager.TemplateDirectory = Path.Combine("WebServer", "Views");

            using (WebApp.Start<WebServer.Startup>(new StartOptions(uri)))
            {
                Console.WriteLine(uri);

                var resetEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    resetEvent.Set();
                };
                resetEvent.WaitOne();
            }

            Database.DisposeEngine();
        }
    }
}
