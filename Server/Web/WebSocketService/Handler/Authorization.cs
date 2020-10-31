using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using KeyBoardChat.DataBase;
using KeyBoardChat.Models;
using KeyBoardChat.Models.Network;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        private object AuthorizationLocker = new object();
        public IEnumerable<HandlerCallBack> Authorization(Connection SocketConnection, string header, string name, string pass)
        {

            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {

                uint UserId;
                using (var dbcontext = new DatabaseContext())
                {
                    try
                    {
                        SHA256 sha256 = SHA256.Create();
                        var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(pass));

                        var dbUser = dbcontext.Users.Single(user => user.Name == name);

                        if (!dbUser.PasswordHash.SequenceEqual(passwordHash))
                            throw new InvalidOperationException();
                        else
                            UserId = dbUser.UserId;
                    }
                    catch (InvalidOperationException)
                    {
                        outcallback.Add(new HandlerCallBack(header, "wrongNamePass", false, false));
                        return;
                    }
                }

                User user = _webSocketService.GetAuthUser(UserId);


                lock (AuthorizationLocker)
                {
                    if (user != null)
                    {
                        lock (_webSocketService._connectionConnections)
                        {
                            _webSocketService._connectionConnections[SocketConnection.Socket] = SocketConnection;
                        }

                        var userConnections = user.Connections;

                        lock (userConnections)
                        {
                            if (userConnections.Contains(SocketConnection))
                            {
                                outcallback.Add(new HandlerCallBack(header, "alreadyInSess", false, false));
                                return;
                            }

                            userConnections.Add(SocketConnection);
                        }

                        Room room = user.Room;
                        if (room != null)
                        {
                            string roomName = room.Name;
                            lock (roomName)
                            {
                                outcallback.Add(new HandlerCallBack(header, "Aunthentication successful", true, false));
                                outcallback.Add(new HandlerCallBack(_webSocketService.Calls["JoinRoom"]["header"].ToString(), new RespondeRoomInfo(room.Id, roomName, "Join room"), true, false));
                            }
                        }
                    }
                    else
                    {
                        user = new User(SocketConnection, UserId, name);

                        try
                        {
                            lock (_webSocketService._authUsers)
                            {
                                _webSocketService._authUsers.Add(user.UID, user);
                            }
                        }
                        catch (ArgumentException)
                        { }

                        Room globalRoom;

                        lock (_webSocketService._globalRooms)
                        {
                            globalRoom = _webSocketService._globalRooms[0];
                        }
                        _webSocketService.JoinRoom(user, globalRoom);

                        outcallback.Add(new HandlerCallBack(header, "Aunthentication successful", true, false));

                    }
#if DEBUG
                    lock (_webSocketService._connectionConnections) lock(user.Connections) lock(_webSocketService._authUsers)
                    Program.LogService.Log($"Connections: {_webSocketService._connectionConnections.Count}");
                    Program.LogService.Log($"User {user.Name} has : {user.Connections.Count}");
                    Program.LogService.Log($"Auth users: {_webSocketService._authUsers.Count}");
#endif
                }

            })).Invoke();

            return outcallback;

        }
    }
}
