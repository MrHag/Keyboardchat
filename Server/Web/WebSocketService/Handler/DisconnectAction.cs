using KeyBoardChat.Models;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {

        public void DisconnectAction(Connection SocketConnection, bool disconnected)
        {
#if DEBUG
            _webSocketService.OnQueryMeessage("disconnection");
#endif

            try
            {
                if (disconnected)
                {
                    lock (_webSocketService._connectionConnections)
                    {
                        _webSocketService._connectionConnections.Remove(SocketConnection.Socket);
                    }
                }

                Connection currentConnection = _webSocketService.GetConnection(SocketConnection.Socket);

                User user = _webSocketService.GetAuthUser(currentConnection);

                if (user == null)
                {
                    return;
                }

                var userConnections = user.Connections;

                int userConnectionsCount = 0;

                lock (userConnections)
                {

                    for (int i = 0; i < userConnections.Count; i++)
                    {
                        var connection = userConnections[i];
                        if (connection == currentConnection)
                        {
                            userConnections.RemoveAt(i);
                            break;
                        }
                    }

                    userConnectionsCount = userConnections.Count;
                }


                if (userConnectionsCount == 0)
                {
                    Room userroom = user.Room;

                    _webSocketService.LeaveRoom(user, userroom);

                    if (disconnected)
                    {
                        _webSocketService.DeleteUser(user);
                    }

                    lock (_webSocketService._authUsers)
                    {
                        _webSocketService._authUsers.Remove(user.UID);
                    }
                }

            }
            finally
            {
#if DEBUG
                lock (_webSocketService._connectionConnections)
                {
                    Program.LogService.Log("Connections:" + _webSocketService._connectionConnections.Count);
                }
#endif
            }
        }

    }
}
