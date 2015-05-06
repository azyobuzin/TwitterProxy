using MsgPack.Serialization;

namespace TwitterProxy.Common.Models
{
    public class ProxyUser
    {
        [MessagePackMember(0)]
        public string AccessToken { get; set; }

        [MessagePackMember(1)]
        public string AccessTokenSecret { get; set; }

        [MessagePackMember(2)]
        public string ScreenName { get; set; }
    }
}
