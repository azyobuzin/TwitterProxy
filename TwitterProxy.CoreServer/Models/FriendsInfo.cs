using System.Collections.Generic;
using MsgPack.Serialization;

namespace TwitterProxy.CoreServer.Models
{
    public class FriendsInfo
    {
        public FriendsInfo()
        {
            this.Friends = new List<ulong>();
            this.FollowRequestSent = new List<ulong>();
            this.Muting = new List<ulong>();
        }

        [MessagePackMember(0)]
        public IList<ulong> Friends { get; set; }

        [MessagePackMember(1)]
        public IList<ulong> FollowRequestSent { get; set; }

        [MessagePackMember(2)]
        public IList<ulong> Muting { get; set; }
    }
}
