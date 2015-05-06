using LightNode.Client;

namespace TwitterProxy.WebServer
{
    static class CoreServer
    {
        private static LightNodeClient client;
        private static string rootEndPoint;
        public static string RootEndPoint
        {
            get
            {
                return rootEndPoint;
            }
            set
            {
                if (rootEndPoint != value)
                {
                    rootEndPoint = value;
                    client = new LightNodeClient(value);
                }
            }
        }

        public static LightNodeClient Client
        {
            get
            {
                return client;
            }
        }
    }
}
