using MsgPack.Serialization;

namespace TwitterProxy.WebServer.Models
{
    public class ProxyUser
    {
        [MessagePackMember(0)]
        public string AccessToken { get; set; }

        [MessagePackMember(1)]
        public string AccessTokenSecret { get; set; }
    }
}
