using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Models.Network
{
    public struct LeftJoinedRoom
    {
        public string room;
        public string message;

        public LeftJoinedRoom(string room, string message)
        {
            this.room = room;
            this.message = message;
        }
    }
}
