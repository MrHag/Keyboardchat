using System;
using System.Collections.Generic;
using System.Linq;

namespace Keyboardchat.Models.Network
{
    public struct RoomInfo
    {

        public string room;
        public bool haspass;

        public RoomInfo(string room, bool haspass)
        {
            this.room = room;
            this.haspass = haspass;
        }

    }
}
