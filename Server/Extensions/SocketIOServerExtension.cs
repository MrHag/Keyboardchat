using KeyBoardChat.Models;
using SocketIOSharp.Server;

namespace KeyBoardChat.Extensions
{
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
            lock (user.Connections)
            {
                foreach (var connection in user.Connections)
                    if (server == connection.Socket.Server)
                        connection.Socket.Emit(header, body);
            }
        }

        private static void To(this SocketIOServer server, Room room, string header, object body)
        {
            lock (room.Users)
            {
                foreach (var user in room.Users)
                {
                    server.To(user, header, body);
                }
            }
        }

    }
}
