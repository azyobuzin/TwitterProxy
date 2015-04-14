using MsgPack.Serialization;

namespace TwitterProxy.WebServer.Models
{
    public class AccessToken
    {
        [MessagePackMember(0)]
        public string ConsumerKey { get; set; }

        [MessagePackMember(1)]
        public string AccessTokenSecret { get; set; }

        [MessagePackMember(2)]
        public ulong UserId { get; set; }
    }
}
