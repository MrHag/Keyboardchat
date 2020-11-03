using Keyboardchat.DataBase;
using Keyboardchat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public class AuthorizationHandler : WebSocketServiceHandler
    {

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
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
                        var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));

                        var dbUser = dbcontext.Users.Single(user => user.Name == Name);

                        if (!dbUser.PasswordHash.SequenceEqual(passwordHash))
                            throw new InvalidOperationException();
                        else
                            UserId = dbUser.UserId;
                    }
                    catch (InvalidOperationException)
                    {
                        outcallback.Add(new HandlerCallBack(data: "wrongNamePass", error: true));
                        return;
                    }
                }

                User user = _webSocketService.GetAuthUser(UserId);


                if (user != null)
                {
                    Session session = user.Session;

                    var sessionConnections = session.Connections;

                    if (sessionConnections.Contains(connection))
                    {
                        outcallback.Add(new HandlerCallBack(data: "alreadyInSess", error: true));
                        return;
                    }

                    session.AddConnection(connection);
                }
                else
                {
                    user = new User(UserId, Name);

                    Session session = _webSocketService.CreateSession();
                    session.User = user;
                    session.AddConnection(connection);

                    Room globalRoom;
                    globalRoom = _webSocketService._globalRooms[0];
                    globalRoom.AddUser(user);
                }
#if DEBUG
                Program.LogService.Log($"Connections: {_webSocketService._connectionConnections.Count}");
                Program.LogService.Log($"User {user.Name} has : {user.Session.Connections.Count()}");
                Program.LogService.Log($"Auth users: {_webSocketService._authUsers.Count}");
#endif

            })).Invoke();

            return outcallback;
        }

    }
}
