using SocketIOSharp.Server.Client;
using System;

namespace Keyboardchat.Models
{
    public class User
    {
        public SocketIOSocket Client { get; private set; }
        public uint UID { get; protected set; }
        public string Name { get; set; }
        public Room Room { get; set; }

        public User(string Name, Room Room, SocketIOSocket Client)
        {
            this.Name = Name;
            this.Room = Room;
            this.Client = Client;
        }

        public User(SocketIOSocket Client)
        {
            Name = null;
            Room = null;
            this.Client = Client;
        }

        public User Copy()
        {
            return new User(new string(Name), Room, Client);
        }

    }
}
