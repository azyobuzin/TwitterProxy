using System;

namespace TwitterProxy.Common
{
    public static class EnvironmentVariables
    {
        private const string prefix = "TWPROXY_";

        public static string ConsumerKey
        {
            get
            {
                return Environment.GetEnvironmentVariable(prefix + "CK");
            }
        }

        public static string ConsumerSecret
        {
            get
            {
                return Environment.GetEnvironmentVariable(prefix + "CS");
            }
        }

        public static string BaseUri
        {
            get
            {
                return Environment.GetEnvironmentVariable(prefix + "BASEURI");
            }
        }

        public static string DbDirectory
        {
            get
            {
                return Environment.GetEnvironmentVariable(prefix + "DB");
            }
        }
    }
}
