using EngineIOSharp.Common;
using Keyboardchat.Models;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using System;

namespace Keyboardchat.Extensions
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
        public static void EmitTo(this SocketIOServer server, SocketIOSocket socket, string header, object body)
        {
            socket.Emit(header, body);
        }

        private static void To(this SocketIOServer server, User user, string header, object body)
        {
            foreach (var connection in user.Session.Connections)
                if (server == connection.Socket.Server)
                {
                    server.EmitTo(connection.Socket, header, body);
                }
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
