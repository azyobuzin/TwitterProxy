﻿using MsgPack.Serialization;

namespace TwitterProxy.WebServer.Models
{
    class Consumer
    {
        [MessagePackMember(0)]
        public string Key { get; set; }

        [MessagePackMember(1)]
        public string Secret { get; set; }

        [MessagePackMember(2)]
        public string Name { get; set; }
    }
}
