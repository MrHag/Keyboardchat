using Keyboardchat.Models;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
            server.Queue.WaitOne();

            server.To(user, header, body);

            server.Queue.Release();
        }

        public static void EmitTo(this SocketIOServer server, Room room, string header, object body)
        {
            server.Queue.WaitOne();

            server.To(room, header, body);

            server.Queue.Release();
        }

        private static void To(this SocketIOServer server, User user, string header, object body)
        {
            if (server.Clients.Contains(user.Client))
                user.Client.Emit(header, body);
        }

        private static void To(this SocketIOServer server, Room room, string header, object body)
        {
            room.Users.Open((Interface)=> 
            { 
                foreach (var user in Interface)
                {
                    server.To(user, header, body);
                }
            });
        }

    }
}
