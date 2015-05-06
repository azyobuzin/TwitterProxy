using System;
using DBreeze;
using DBreeze.Transactions;
using MsgPack.Serialization;
using TwitterProxy.Common;

namespace TwitterProxy.CoreServer
{
    static class Database
    {
        private readonly static Lazy<DBreezeEngine> engine = new Lazy<DBreezeEngine>(() =>
        {
            DBreeze.Utils.CustomSerializator.ByteArraySerializator =
                value => MessagePackSerializer.Get(value.GetType()).PackSingleObject(value);

            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator =
                (bytes, type) => MessagePackSerializer.Get(type).UnpackSingleObject(bytes);

            var directory = EnvironmentVariables.DbDirectory;
            return new DBreezeEngine(string.IsNullOrEmpty(directory) ? "db" : directory);
        });

        public static Transaction GetTransaction()
        {
            return engine.Value.GetTransaction();
        }

        public static void DisposeEngine()
        {
            if (engine.IsValueCreated)
                engine.Value.Dispose();
        }

        /// <summary>
        /// Key: User ID (ulong)
        /// Value: Consumer[]
        /// </summary>
        public const string Consumers = "consumers";

        /// <summary>
        /// Key: Access token (string)
        /// Value: AccessTokenInfo
        /// </summary>
        public const string AccessTokens = "accessTokens";

        /// <summary>
        /// Key: User ID (ulong)
        /// Value: ProxyUser
        /// </summary>
        public const string ProxyUsers = "proxyUsers";

        /// <summary>
        /// Key: User ID (ulong)
        /// Value: byte[] (BSON of a user)
        /// </summary>
        public const string TwitterUsers = "twitterUsers";

        /// <summary>
        /// Key: User ID (ulong)
        /// Value: FriendsInfo
        /// </summary>
        public const string Friends = "friends";
    }
}
