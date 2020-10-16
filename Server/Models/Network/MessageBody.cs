using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Models.Network
{
    public class MessageBody
    {
        public string name { get; private set; }

        public string message { get; private set; }

        public string avatar { get; private set; }
        public MessageBody(string name, string message, string avatar)
        {
            this.name = name;
            this.message = message;
            this.avatar = avatar;
        }

    }
}
