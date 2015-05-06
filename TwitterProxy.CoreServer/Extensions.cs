using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace TwitterProxy.CoreServer
{
    static class Extensions
    {
        public static JObject ReadAsBsonObject(this byte[] bytes)
        {
            using (var reader = new BsonReader(new MemoryStream(bytes)))
                return JObject.Load(reader);
        }

        public static byte[] ToBson(this JToken jt)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BsonWriter(ms))
            {
                jt.WriteTo(writer);
                return ms.ToArray();
            }
        }

        public static bool IsNull(this JToken jt)
        {
            return jt == null || jt.Type == JTokenType.Null;
        }
    }
}
