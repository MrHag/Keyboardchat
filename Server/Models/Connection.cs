using SocketIOSharp.Server.Client;

namespace KeyBoardChat.Models
{
    public class Connection
    {
        public SocketIOSocket Socket { get; }

        public Session Session { get; set; }

        public Connection(SocketIOSocket socket)
        {
            Socket = socket;
        }

    }
}
