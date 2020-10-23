using SocketIOSharp.Server.Client;
using System;

namespace Keyboardchat.Models
{
    public class User
    {
        public SocketIOSocket Client { get; private set; }
        public uint UID { get; set; }
        public string Name { get; set; }
        public Room Room { get; set; }

        public bool Auth { get; set; }

        public User(string Name, Room Room, SocketIOSocket Client, bool Auth)
        {
            this.Name = Name;
            this.Room = Room;
            this.Client = Client;
            this.Auth = Auth;
        }

        public User(SocketIOSocket Client)
        {
            Name = null;
            Room = null;
            this.Client = Client;
            Auth = false;
        }

        public User Copy()
        {
            return new User(new string(Name), Room, Client, Auth);
        }

    }
}
