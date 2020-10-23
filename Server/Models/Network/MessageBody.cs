using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Models.Network
{
    public class MessageBody
    {
        public uint userid { get; private set; }

        public string message { get; private set; }
        public MessageBody(uint userid, string message)
        {
            this.userid = userid;
            this.message = message;
        }

    }
}
