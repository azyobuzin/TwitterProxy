using System;
using System.IO;
using DBreeze;
using DBreeze.Transactions;
using MsgPack.Serialization;

namespace TwitterProxy
{
    static class Database
    {
        private readonly static Lazy<DBreezeEngine> engine = new Lazy<DBreezeEngine>(() =>
        {
            DBreeze.Utils.CustomSerializator.ByteArraySerializator =
                value => MessagePackSerializer.Get(value.GetType()).PackSingleObject(value);

            DBreeze.Utils.CustomSerializator.ByteArrayDeSerializator =
                (bytes, type) => MessagePackSerializer.Get(type).UnpackSingleObject(bytes);

            return new DBreezeEngine(Path.Combine("App_Data", "db"));
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

        public const string ScreenNames = "screenNames";
        public const string Tokens = "tokens";
        public const string Users = "users";
    }
}
