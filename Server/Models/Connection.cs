using SocketIOSharp.Server.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Keyboardchat.Models
{
    public class Connection
    {
        public SocketIOSocket Socket {get;}

        public Connection(SocketIOSocket socket)
        {
            Socket = socket;
        }

    }
}
