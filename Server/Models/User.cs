using Keyboardchat.SaveCollections;
using SocketIOSharp.Server.Client;
using System;
using System.Collections.Generic;

namespace Keyboardchat.Models
{
    public class User
    {
        public List<Connection> Connections { get; private set; }
        public uint UID { get; set; }
        public string Name { get; set; }
        public Room Room { get; set; }

        public User(Connection Connection, uint id, string Name, Room Room = null)
        {
            UID = id;
            this.Name = Name;
            this.Room = Room;
            Connections = new List<Connection>();
            Connections.Add(Connection);
        }

    }
}
