using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Models.Network
{
    public struct JoinedRoom
    {
        public string room;
        public string message;

        public JoinedRoom(string room, string message)
        {
            this.room = room;
            this.message = message;
        }
    }
}
