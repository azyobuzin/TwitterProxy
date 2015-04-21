using System;

namespace TwitterProxy
{
    static class EnvironmentVariables
    {
        public static string ConsumerKey
        {
            get
            {
                return Environment.GetEnvironmentVariable("TWPROXY_CK");
            }
        }

        public static string ConsumerSecret
        {
            get
            {
                return Environment.GetEnvironmentVariable("TWPROXY_CS");
            }
        }

        public static string BaseUri
        {
            get
            {
                return Environment.GetEnvironmentVariable("TWPROXY_BASEURI");
            }
        }
    }
}
