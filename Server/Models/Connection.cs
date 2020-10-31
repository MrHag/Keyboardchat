using SocketIOSharp.Server.Client;

namespace KeyBoardChat.Models
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
