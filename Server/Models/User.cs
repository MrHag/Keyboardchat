using SocketIOSharp.Server.Client;
using System;

namespace Keyboardchat.Models
{
    public class User
    {
        public SocketIOSocket Client { get; protected set; }
        public string Name { get; set; }
        public Room Room { get; set; }
        public bool Authorizated { get; set; }

        public User(SocketIOSocket Client, string Name, Room Room, bool Authorizated)
        {
            this.Client = Client;
            this.Name = Name;
            this.Room = Room;
            this.Authorizated = Authorizated;
        }

        public User(SocketIOSocket Client)
        {
            this.Client = Client;
            Name = null;
            Room = null;
            Authorizated = false;
        }

        public User Copy()
        {
            return new User(Client, new string(Name), Room, Authorizated);
        }

    }
}
