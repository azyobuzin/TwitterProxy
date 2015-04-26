using System;
using System.Threading;
using Microsoft.Owin.Hosting;

namespace TwitterProxy.Common
{
    public static class WebAppRunner
    {
        public static void Run<T>(StartOptions options)
        {
            using (WebApp.Start<T>(options))
            {
                foreach (var u in options.Urls)
                    Console.WriteLine(u);

                var resetEvent = new ManualResetEvent(false);
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    resetEvent.Set();
                };
                resetEvent.WaitOne();
            }
        }
    }
}
