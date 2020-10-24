using Keyboardchat.DataBase;
using Keyboardchat.Extensions;
using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Keyboardchat.ModifiedObjects;
using Keyboardchat.SaveCollections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Keyboardchat.Web
{
    public class WebSocketService
    {

        private JToken Calls;
        private JToken SCalls;
        private SocketIOServer server;

        private Dictionary<SocketIOSocket, Connection> _connectionConnections;

        private Dictionary<uint, User> _authUsers;

        private List<Room> _rooms;
        private List<Room> _globalRooms;

        public WebSocketService()
        {
            Calls = Program.API.SelectToken("Calls");
            SCalls = Program.API.SelectToken("ServerCalls");
            server = new SocketIOServer(new SocketIOSharp.Server.SocketIOServerOption(4001));
            _authUsers = new Dictionary<uint, User>();
            _connectionConnections = new Dictionary<SocketIOSocket, Connection>();
            _rooms = new List<Room>();
            _globalRooms = new List<Room>();
        }

        private void ResponseMessage(Connection connetion, string header, object data, bool succ, bool err)
        {
            ResponseBody responseBody = new ResponseBody(data, succ, err);
            connetion.Socket.Emit(header, responseBody);

#if DEBUG
            string name = "";

            var user = GetAuthUser(connetion);
            if (user != null)
                name = user.Name;
            Program.LogService.Log($"{header} Responsebody to {name}\n{responseBody.Json()}");
#endif
        }

        private void ResponseMessage(IEnumerable<Connection> connections, string header, object data, bool succ, bool err)
        {
            foreach (var connection in connections)
                ResponseMessage(connection, header, data, succ, err);
        }

        private void SendChatMessage(Room room, string message, uint userId)
        {

            var MessageBody = new MessageBody(userId, message);
            server.EmitTo(room, Calls["Chat"]["header"].ToString(), MessageBody);
#if DEBUG
            Program.LogService.Log($"Message to\n{room.Name}, {message}, from {userId}");
#endif
        }

        private void ErrorResponseMessage(Connection connection, string header, object data)
        {
            ResponseMessage(connection, header, data, false, true);
        }

        private void ErrorResponseMessage(IEnumerable<Connection> connections, string header, object data)
        {
            ResponseMessage(connections, header, data, false, true);
        }

        private void ServiceResponseMessage(Connection connection, string header, object data, bool succ)
        {
            ResponseMessage(connection, header, data, succ, false);
        }

        private void ServiceResponseMessage(IEnumerable<Connection> connections, string header, object data, bool succ)
        {
            ResponseMessage(connections, header, data, succ, false);
        }

        private void OnQueryMeessage(string header)
        {
            Program.LogService.Log($"Called {header}");
        }

        private bool ValidateDefaultText(string text, int minlength = 0, int maxlength = -1)
        {

            if (text.Length < minlength)
                return false;

            if (maxlength != -1 && text.Length > maxlength)
                return false;

            if (!Regex.IsMatch(text, "(?i)[\\S]+"))
                return false;

            return true;
        }

        private bool AuthCheckReport(User user, Connection connection)
        {

            if (user == null || !_authUsers.Values.Contains(user))
            {
                ErrorResponseMessage(connection, SCalls["Access"]["header"].ToString(), "notAuth");
                return false;
            }
            return true;
        }

        public Connection GetConnection(SocketIOSocket client)
        {
            Connection connection;
            if (_connectionConnections.TryGetValue(client, out connection))
                return connection;
            return null;
        }

        public User GetAuthUser(Connection client)
        {
            foreach (var user in _authUsers)
            {
                foreach (var connection in user.Value.Connections)
                    if (connection == client)
                        return user.Value;
            }
            return null;
        }

        public User GetAuthUser(uint id)
        {
            User user;
            if (_authUsers.TryGetValue(id, out user))
                return user;
            return null;
        }

        public Room GetRoom(string RoomName)
        {
            foreach (var room in _rooms)
            {
                if (room.Name == RoomName)
                    return room;
            }

            return null;
        }

        public void Broadcast(string header, object data, bool successful, bool error)
        {
            foreach (var user in _authUsers.Values)
            {
                ResponseMessage(user.Connections, header, data, successful, error);
            }
        }

        public bool DeleteUser(User user)
        {
            if (user == null)
                return false;

            foreach (var client in user.Connections)
                if (!_connectionConnections.Remove(client.Socket))
                    return false;
            return true;
        }

        public void JoinRoom(User user, Room room)
        {

            if (user.Room != null)
            {

                if (user.Room == room)
                    return;

                LeaveRoom(user, user.Room);
            }

            room.AddUser(user);
            user.Room = room;

            SendChatMessage(room, user.Name + " connected", 0);
            ServiceResponseMessage(user.Connections, Calls["JoinRoom"]["header"].ToString(), new LeftJoinedRoom(room.Name, "Join room"), true);
        }

        public void LeaveRoom(User user, Room room)
        {

            if (room != null)
            {
                SendChatMessage(room, user.Name + " disconnected", 0);
                room.DeleteUser(user);

                int UserCount = 0;

                UserCount = room.Users.Count;

                if (UserCount == 0)
                {
                    for (int key = 0; key < _rooms.Count; key++)
                    {
                        var froom = _rooms[key];
                        if (froom == room)
                        {
                            _rooms.RemoveAt(key);
                            Program.LogService.Log("delete room: " + froom.Name);
                            Broadcast(SCalls["RoomChange"]["header"].ToString(), "Deleted room", true, false);
                            break;
                        }
                    }
                }
                user.Room = null;
            }

        }

        public static bool GetPropValue<T>(ref JToken iterator, string name, out T output)
        {
            try
            {
                output = default;

                if (iterator != null && iterator is JProperty nameproperty && nameproperty.Name == name && nameproperty.Value.Value<T>() is T value)
                {
                    output = value;
                    return true;
                }
                return false;
            }
            finally
            {
                iterator = iterator.Next;
            }
        }

        public void Start()
        {

            _globalRooms.Add(new Room("global", ""));

            server.Start();

            server.OnConnection((socket) =>
            {
                Program.LogService.Log("Connection");

                Connection SocketConnection = new Connection(socket);

                _connectionConnections.Add(socket, SocketConnection);

                Action<bool> DisconnectAction = (disconnected) =>
                {
#if DEBUG
                    OnQueryMeessage("disconnection");
#endif
                    

                    Connection currentConnection = GetConnection(socket);

                    User user = GetAuthUser(currentConnection);

                    if (user == null)
                    {
                        _connectionConnections.Remove(socket);
                        return;
                    }

                    for (int i = 0; i < user.Connections.Count; i++)
                    {
                        var connection = user.Connections[i];
                        if (connection == currentConnection)
                        {
                            user.Connections.RemoveAt(i);
                            break;
                        }
                    }

                    if (user.Connections.Count == 0)
                    {
                        Room userroom = user.Room;

                        LeaveRoom(user, userroom);

                        if (disconnected)
                            DeleteUser(user);

                        _authUsers.Remove(user.UID);
                    }

#if DEBUG
                        Program.LogService.Log("Users:" + _connectionConnections.Count);
#endif


                };

                socket.On(Calls["Registration"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Registration"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif
                    

                    string name = "";
                    string pass = "";

                    var iterator = data[0].First;
                    if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "name", out name) || !GetPropValue(ref iterator, "password", out pass))
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    if (!ValidateDefaultText(name, maxlength: 32))
                    {
                        ServiceResponseMessage(SocketConnection, header, "badName", false);
                        return;
                    }

                    if (!ValidateDefaultText(pass, maxlength: 64))
                    {
                        ServiceResponseMessage(SocketConnection, header, "badPass", false);
                        return;
                    }

                    using (var dbcontext = new DatabaseContext())
                    {
                        try
                        {
                            dbcontext.Users.Single(user => user.Name == name);

                            ServiceResponseMessage(SocketConnection, header, "nameExists", false);
                            return;
                        }
                        catch (InvalidOperationException)
                        {

                            SHA256 SHA256 = SHA256.Create();

                            var passwordHash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(pass));

                            var dbUser = new DataBase.Models.User() { Name = name, PasswordHash = passwordHash };
                            dbcontext.Add(dbUser);
                            if (dbcontext.SaveChanges() > 0)
                            {
                                ServiceResponseMessage(SocketConnection, header, "Registration successful", true);
                            }
                            else
                            {
                                ErrorResponseMessage(SocketConnection, header, "uknownError");
                            }
                        }
                    }

                });

                socket.On(Calls["Authorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Authorization"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif
                    
                    string name = "";
                    string pass = "";

                    var iterator = data[0].First;
                    if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "name", out name) || !GetPropValue(ref iterator, "password", out pass))
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    if (!ValidateDefaultText(name, maxlength: 32))
                    {
                        ServiceResponseMessage(SocketConnection, header, "badName", false);
                        return;
                    }

                    if (!ValidateDefaultText(pass, maxlength: 64))
                    {
                        ServiceResponseMessage(SocketConnection, header, "badPass", false);
                        return;
                    }

                    uint UserId;

                    using (var dbcontext = new DatabaseContext())
                    {
                        try
                        {
                            SHA256 SHA256 = SHA256.Create();
                            var passwordHash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(pass));

                            var dbUser = dbcontext.Users.Single(user => user.Name == name);

                            if (!dbUser.PasswordHash.SequenceEqual(passwordHash))
                                throw new InvalidOperationException();
                            else
                                UserId = dbUser.UserId;
                        }
                        catch (InvalidOperationException)
                        {
                            ServiceResponseMessage(SocketConnection, header, "wrongNamePass", false);
                            return;
                        }
                    }

                        
                    User user = GetAuthUser(UserId);
                    if (user != null)
                    {
                        _connectionConnections[socket] = SocketConnection;

                        user.Connections.Add(SocketConnection);

                        ServiceResponseMessage(SocketConnection, header, "Aunthentication successful", true);

                        Room room = user.Room;
                        if(room != null)
                        ServiceResponseMessage(SocketConnection, Calls["JoinRoom"]["header"].ToString(), new LeftJoinedRoom(room.Name, "Join room"), true);
                    }
                    else
                    {
                        user = new User(SocketConnection, UserId, name);

                        try
                        {
                            _authUsers.Add(user.UID, user);
                        }
                        catch (ArgumentException)
                        { }

                        Room globalRoom = _globalRooms[0];
                        JoinRoom(user, globalRoom);

                        ServiceResponseMessage(SocketConnection, header, "Aunthentication successful", true);
                    }
                });

                socket.On(Calls["DeAuthorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["DeAuthorization"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif
                    User user = GetAuthUser(SocketConnection);
                    if (user == null)
                    {
                        ServiceResponseMessage(SocketConnection, header, "notAuth", false);
                        return;
                    }

                    DisconnectAction.Invoke(false);

                    ServiceResponseMessage(SocketConnection, header, "Deaunthentication successful", true);

                });


                socket.On(Calls["Chat"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Chat"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif


                    User user = null;
                    Room room = null;

                    user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    room = user.Room;

                    string message;

                    var iterator = data[0].First;
                    if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "message", out message))
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    if (room == null)
                        return;

                    message = message.Trim();

                    if (!ValidateDefaultText(message))
                        return;

                    SendChatMessage(room, message, user.UID);

                });

                socket.On(Calls["JoinRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["JoinRoom"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    
                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    string RoomName;
                    string Password;

                    var iterator = data[0].First;
                    if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "name", out RoomName))
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    if (!GetPropValue(ref iterator, "password", out Password))
                        Password = "";

                    for (int key = 0; key < _rooms.Count; key++)
                    {
                        var room = _rooms[key];

                        if (room.Name == RoomName)
                        {
                            if (room.Password == "" || room.Password == Password)
                            {
                                JoinRoom(user, room);

                                return;
                            }
                            else
                            {
                                ServiceResponseMessage(SocketConnection, header, "invalidPass", false);
                                return;
                            }
                        }
                    }

                    ServiceResponseMessage(SocketConnection, header, "roomNotFound", false);
                });

                socket.On(Calls["LeaveRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["LeaveRoom"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    User user = null;

                    user = GetAuthUser(SocketConnection);
                    if (!AuthCheckReport(user, SocketConnection) || user.Room == null)
                    {
                        ServiceResponseMessage(SocketConnection, header, "notInRoom", false);
                        return;
                    }

                    string RoomName;

                    var iterator = data[0].First;
                    if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "name", out RoomName))
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    Room room = null;
                    Room globalRoom = null;


                    room = GetRoom(RoomName);

                    globalRoom = _globalRooms[0];

                    if (room == null || user.Room != room)
                    {
                        ServiceResponseMessage(SocketConnection, header, "roomNotFound", false);
                        return;
                    }

                    LeaveRoom(user, room);

                    ServiceResponseMessage(SocketConnection, header, new LeftJoinedRoom(RoomName, "Leaved from room"), true);

                    JoinRoom(user, globalRoom);

                });

                socket.On(Calls["GetRooms"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetRooms"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    List<(Room room, int qual)> rooms = new List<(Room room, int qual)>();

                    foreach (var room in _rooms)
                        rooms.Add((room, 0));

                    string roomname;


                    var iterator = data[0].First;
                    if (data != null && data.Length > 0 && data[0] != null && GetPropValue(ref iterator, "room", out roomname))
                    {
                        for (int key = 0; key < rooms.Count; key++)
                        {
                            var room = rooms[key];

                            var str = "";
                            decimal minQual = Math.Ceiling(roomname.Length * (50 / 100M));

                            for (int i = roomname.Length; i > 0; i--)
                            {
                                str = roomname.Substring(0, i);

                                if (Regex.IsMatch(room.room.Name, $"(?i)({str})+"))
                                {
                                    room.qual = str.Length;
                                    rooms[key] = room;
                                    break;
                                }
                            }

                            if (room.qual < minQual)
                            {
                                rooms.RemoveAt(key);
                                key--;
                            }
                        }
                        rooms.Sort((a, b) => { return b.qual - a.qual; });
                    }

                    List<RoomInfo> outrooms = new List<RoomInfo>();

                    foreach (var room in rooms)
                    {
                        var pass = room.room.Password;

                        var haspass = true;

                        if (pass == "")
                            haspass = false;

                        outrooms.Add(new RoomInfo(room.room.Name, haspass));
                    }

                    ServiceResponseMessage(SocketConnection, header, outrooms, true);

                });

                socket.On(Calls["ChangeProfile"]["header"], (JToken[] data) =>
                {
                    string header = Calls["ChangeProfile"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    string name = null;
                    string avatar = null;

                    bool reqname = true;
                    bool reqavatar = true;


                    var iterator = data[0].First;
                    if (data != null && data.Length <= 0 && data[0] != null)
                    {
                        reqname = !GetPropValue(ref iterator, "name", out name);
                        reqavatar = !GetPropValue(ref iterator, "avatar", out avatar);
                    }
                    else
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    using (var dbcontext = new DatabaseContext())
                    {
                        try
                        {
                            dbcontext.Users.Single(user => user.Name == name);

                            ServiceResponseMessage(SocketConnection, header, "nameExists", false);
                            return;
                        }
                        catch (InvalidOperationException)
                        {
                            var dbuser = dbcontext.Users.Single(dbuser => dbuser.UserId == user.UID);

                            if (reqname)
                            {
                                if (!ValidateDefaultText(name, maxlength: 32))
                                {
                                    ServiceResponseMessage(SocketConnection, header, "badName", false);
                                    return;
                                }
                                dbuser.Name = name;
                            }

                            if (reqavatar)
                            {
                                byte[] bytes;
                                try
                                {
                                    bytes = Convert.FromBase64String(avatar);
                                }
                                catch (FormatException)
                                {
                                    ServiceResponseMessage(SocketConnection, header, "invalidData", false);
                                    return;
                                }

                                using (MemoryStream ms = new MemoryStream(bytes))
                                {
                                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);
                                    if (bitmap.Width > 128 || bitmap.Height > 128)
                                    {
                                        ServiceResponseMessage(SocketConnection, header, "badImage", false);
                                        return;
                                    }

                                    dbuser.Avatar = bytes;

                                    var sha256 = SHA256.Create();
                                    var avatarHash = sha256.ComputeHash(dbuser.Avatar);

                                    dbuser.AvatarHash = avatarHash;
                                }

                            }

                            dbcontext.SaveChanges();

                            ServiceResponseMessage(SocketConnection, header, "Data changed", true);

                        }
                    }

                });


                socket.On(Calls["GetUsers"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetUsers"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    
                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    List<uint> userids = null;
                    List<string> select = null;


                    var iterator = data[0].First;
                    if (data != null && data.Length <= 0 && data[0] != null)
                    {
                        try
                        {
                            JToken iterateToken = iterator.First;
                            userids = (List<uint>)JsonConvert.DeserializeObject(iterateToken.First.ToString(), typeof(List<uint>));
                            select = (List<string>)JsonConvert.DeserializeObject(iterateToken.Next.First.ToString(), typeof(List<string>));
                        }
                        catch (Exception)
                        {
                        }
                    }

                    List<DataBase.Models.User> dbusers = new List<DataBase.Models.User>();

                    using (var dbcontext = new DatabaseContext())
                    {
                        if (userids != null)
                        {
                            foreach (var id in userids)
                            {
                                try
                                {
                                    var dbuser = dbcontext.Users.Single((user) => user.UserId == id);
                                    dbusers.Add(dbuser);
                                }
                                catch (InvalidOperationException)
                                { }
                            }
                        }
                        else
                        {
                            dbusers.Add(dbcontext.Users.Single((user) => user.UserId == user.UserId));
                        }
                    }
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                        using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                        {

                            bool reqavatar = true;
                            bool reqavatarHash = true;
                            bool reqname = true;

                            if (select != null && select.Count > 0)
                            {
                                reqavatar = select.Contains("avatar");
                                reqavatarHash = select.Contains("avatarHash");
                                reqname = select.Contains("name");
                            }

                            jsonTextWriter.WriteStartObject();

                            foreach (var dbuser in dbusers)
                            {

                                jsonTextWriter.WritePropertyName("id");
                                jsonTextWriter.WriteValue(dbuser.UserId);

                                if (reqname)
                                {
                                    jsonTextWriter.WritePropertyName("name");
                                    jsonTextWriter.WriteValue(dbuser.Name);
                                }
                                if (reqavatar)
                                {
                                    byte[] dbavatar = dbuser.Avatar;
                                    string avatar = "";


                                    if (dbavatar != null)
                                    {
                                        avatar = Convert.ToBase64String(dbavatar);
                                    }
                                    jsonTextWriter.WritePropertyName("avatar");
                                    jsonTextWriter.WriteValue(avatar);
                                }
                                if (reqavatarHash)
                                {
                                    byte[] dbavatarHash = dbuser.Avatar;
                                    string avatarHash = "";


                                    if (dbavatarHash != null)
                                    {
                                        avatarHash = Convert.ToBase64String(dbavatarHash);
                                    }

                                    jsonTextWriter.WritePropertyName("avatarHash");
                                    jsonTextWriter.WriteValue(avatarHash);
                                }
                            }

                            jsonTextWriter.WriteEndObject();
                        }

                        string json = Encoding.UTF8.GetString(memoryStream.ToArray());

                        var jObject = JObject.Parse(json);

                        ServiceResponseMessage(SocketConnection, header, jObject, true);
                    }

                });

                socket.On(Calls["CreateRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["CreateRoom"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    string RoomName;
                    string Password;


                    var iterator = data[0].First;
                    if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "name", out RoomName))
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    RoomName = RoomName.Trim();

                    if (!ValidateDefaultText(RoomName))
                    {
                        ServiceResponseMessage(SocketConnection, header, "badName", false);
                        return;
                    }

                    if (!GetPropValue(ref iterator, "password", out Password))
                        Password = "";

                    Room NewRoom = null;


                    foreach (var room in _rooms)
                    {
                        if (room.Name == RoomName)
                        {
                            ServiceResponseMessage(SocketConnection, header, "roomExists", false);
                            return;
                        }
                    }

                    NewRoom = new Room(RoomName, Password);

                    _rooms.Add(NewRoom);

                    ServiceResponseMessage(SocketConnection, header, "Created room", true);
                    Broadcast(SCalls["RoomChange"]["header"].ToString(), "Changed: Created room", true, false);

                    JoinRoom(user, NewRoom);

                });


                socket.On(SocketIOEvent.DISCONNECT, () => DisconnectAction(true));

                socket.On(SocketIOEvent.ERROR, () => DisconnectAction(true));

            });

        }

    }
}
