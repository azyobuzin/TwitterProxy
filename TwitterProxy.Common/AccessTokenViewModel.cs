using System;
using MsgPack.Serialization;

namespace TwitterProxy.Common
{
    public class AccessTokenViewModel
    {
        [MessagePackMember(0)]
        public Consumer Consumer { get; set; }

        [MessagePackMember(1)]
        public string AccessToken { get; set; }

        [MessagePackMember(2)]
        public DateTime CreatedAtUtc { get; set; }
    }
}
