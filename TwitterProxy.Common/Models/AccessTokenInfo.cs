using System;
using MsgPack.Serialization;

namespace TwitterProxy.Common.Models
{
    public class AccessTokenInfo
    {
        [MessagePackMember(0)]
        public string ConsumerKey { get; set; }

        [MessagePackMember(1)]
        public string AccessTokenSecret { get; set; }

        [MessagePackMember(2)]
        public ulong UserId { get; set; }

        [MessagePackMember(3)]
        public DateTime CreatedAtUtc { get; set; }
    }
}
