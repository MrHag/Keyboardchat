using SocketIOSharp.Server.Client;
using System;

namespace Keyboardchat.Models
{
    public class User
    {
        public uint UID { get; protected set; }
        public SocketIOSocket Client { get; protected set; }
        public string Name { get; set; }
        public Room Room { get; set; }

        public User(SocketIOSocket Client, string Name, Room Room)
        {
            this.Client = Client;
            this.Name = Name;
            this.Room = Room;
        }

        public User(SocketIOSocket Client)
        {
            this.Client = Client;
            Name = null;
            Room = null;
        }

        public User Copy()
        {
            return new User(Client, new string(Name), Room);
        }

    }
}
