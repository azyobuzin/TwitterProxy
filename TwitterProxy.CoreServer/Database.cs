using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBreeze;
using DBreeze.Transactions;
using MsgPack.Serialization;

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

            return new DBreezeEngine("db");
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
    }
}
