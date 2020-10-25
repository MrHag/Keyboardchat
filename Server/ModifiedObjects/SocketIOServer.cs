using Keyboardchat.Models;
using SocketIOSharp.Server;
using System.Threading;

namespace Keyboardchat.ModifiedObjects
{

    public class SocketIOServer : SocketIOSharp.Server.SocketIOServer
    {
        public Semaphore Queue { get; protected set; }
        public SocketIOServer(SocketIOServerOption Option) : base(Option)
        {
            Queue = new Semaphore(1, 1);
        }

    }


    public static class SocketIOServerExtension
    {
        public static void EmitTo(this SocketIOServer server, User user, string header, object body)
        {
            server.To(user, header, body);
        }

        public static void EmitTo(this SocketIOServer server, Room room, string header, object body)
        {
            server.To(room, header, body);
        }

        private static void To(this SocketIOServer server, User user, string header, object body)
        {
            foreach(var connection in user.Connections)
            if (server.Clients.Contains(connection.Socket))
                connection.Socket.Emit(header, body);         
        }

        private static void To(this SocketIOServer server, Room room, string header, object body)
        {
            foreach (var user in room.Users)
            {
                server.To(user, header, body);
            }
        }

    }
}
