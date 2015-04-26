using System;
using MsgPack.Serialization;

namespace TwitterProxy.Common
{
    public class AccessTokenInfo
    {
        [MessagePackMember(0)]
        public string ConsumerKey { get; set; }

        [MessagePackMember(1)]
        public string ConsumerSecret { get; set; }

        [MessagePackMember(2)]
        public string AccessTokenSecret { get; set; }

        [MessagePackMember(3)]
        public ulong UserId { get; set; }

        [MessagePackMember(4)]
        public DateTime CreatedAtUtc { get; set; }
    }
}
