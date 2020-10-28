using Keyboardchat.DataBase;
using Keyboardchat.Extensions;
using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Keyboardchat.SaveCollections;
using Keyboardchat.UseClasses;
using Keyboardchat.Web.WebSocketService.Handler;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Keyboardchat.Web.WebSocketService
{
    public class WebSocketService
    {

        internal JToken Calls;
        internal JToken SCalls;
        internal SocketIOServer server;

        internal Dictionary<SocketIOSocket, Connection> _connectionConnections;

        internal SortedDictionary<uint, User> _authUsers;

        internal UIDmanager _roomUIDmanager;
        internal SortedDictionary<int, Room> _rooms;
        internal List<Room> _globalRooms;

        private WebSocketServiceHandler _webSocketServiceHandler;

        public WebSocketService()
        {
            Calls = Program.API.SelectToken("Calls");
            SCalls = Program.API.SelectToken("ServerCalls");
            server = new SocketIOServer(new SocketIOServerOption(4001));
            _authUsers = new SortedDictionary<uint, User>();
            _connectionConnections = new Dictionary<SocketIOSocket, Connection>();
            _roomUIDmanager = new UIDmanager();
            _rooms = new SortedDictionary<int, Room>();
            _globalRooms = new List<Room>();
            _webSocketServiceHandler = new WebSocketServiceHandler(this);
        }

        private void CallbackHandler(Connection connection, IEnumerable<HandlerCallBack> callBacks)
        {
            foreach (var callBack in callBacks)
                ResponseMessage(connection, callBack.Header, callBack.Data, callBack.Successfull, callBack.Error);
        }

        private void CallbackHandler(IEnumerable<Connection> connections, IEnumerable<HandlerCallBack> callBacks)
        {
            foreach (var connection in connections)
                CallbackHandler(connection, callBacks);
        }

        internal void ResponseMessage(Connection connetion, string header, object data, bool succ, bool err)
        {
            ResponseBody responseBody = new ResponseBody(data, succ, err);
            connetion.Socket.Emit(header, responseBody);

#if DEBUG

            Task.Factory.StartNew(() =>
            {

                string name = "";

                var user = GetAuthUser(connetion);

                if (user != null)
                {
                    name = user.Name;
                }

                lock (name)
                    Program.LogService.Log($"{header} Responsebody to {name}\n{responseBody.Json()}");

            }, TaskCreationOptions.DenyChildAttach);
#endif
        }

        internal void ResponseMessage(IEnumerable<Connection> connections, string header, object data, bool succ, bool err)
        {
            foreach (var connection in connections)
                ResponseMessage(connection, header, data, succ, err);
        }

        internal void SendChatMessage(Room room, string message, uint userId)
        {
            var MessageBody = new MessageBody(userId, room.Id, message);
            server.EmitTo(room, Calls["Chat"]["header"].ToString(), MessageBody);
#if DEBUG
            string name = room.Name;

            lock (name)
            {
                Program.LogService.Log($"Message to\n{name}, {message}, from {userId}");
            }
#endif
        }

        internal void ErrorResponseMessage(Connection connection, string header, object data)
        {
            ResponseMessage(connection, header, data, false, true);
        }

        internal void ErrorResponseMessage(IEnumerable<Connection> connections, string header, object data)
        {
            ResponseMessage(connections, header, data, false, true);
        }

        internal void ServiceResponseMessage(Connection connection, string header, object data, bool succ)
        {
            ResponseMessage(connection, header, data, succ, false);
        }

        internal void ServiceResponseMessage(IEnumerable<Connection> connections, string header, object data, bool succ)
        {
            ResponseMessage(connections, header, data, succ, false);
        }

        internal void OnQueryMeessage(string header)
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
            bool containsUser;

            lock (_authUsers)
                containsUser = _authUsers.Values.Contains(user);

            if (user == null || !containsUser)
            {
                ErrorResponseMessage(connection, SCalls["Access"]["header"].ToString(), "notAuth");
                return false;
            }

            return true;
        }

        public Connection GetConnection(SocketIOSocket client)
        {
            lock (_connectionConnections)
            {
                Connection connection;
                if (_connectionConnections.TryGetValue(client, out connection))
                    return connection;
            }
            return null;
        }

        public User GetAuthUser(Connection client)
        {
            lock (_authUsers)
            {
                foreach (var user in _authUsers)
                {
                    lock (user.Value.Connections)
                    {
                        foreach (var connection in user.Value.Connections)
                            if (connection == client)
                                return user.Value;
                    }
                }
            }
            return null;
        }

        public User GetAuthUser(uint id)
        {
            lock (_authUsers)
            {
                User user;
                if (_authUsers.TryGetValue(id, out user))
                    return user;
            }
            return null;
        }

        public Room GetRoom(string RoomName)
        {
            lock (_rooms)
            {
                foreach (var room in _rooms.Values)
                    if (room.Name == RoomName)
                        return room;
            }
            return null;
        }

        public Room GetRoom(int RoomId)
        {
            lock (_rooms)
            {
                Room room;
                if (_rooms.TryGetValue(RoomId, out room))
                    return room;
            }
            return null;
        }

        public void Broadcast(string header, object data, bool successful, bool error)
        {
            lock (_authUsers)
            {
                foreach (var user in _authUsers.Values)
                    ResponseMessage(user.Connections, header, data, successful, error);
            }
        }

        public bool DeleteUser(User user)
        {
            if (user == null)
                return false;

            lock (user.Connections)
            {
                foreach (var client in user.Connections)
                    if (!_connectionConnections.Remove(client.Socket))
                        return false;
            }
            return true;
        }

        public bool JoinRoom(User user, Room room)
        {
            lock (user) lock (room)
                {
                    Room userRoom = user.Room;

                    if (userRoom != null)
                    {
                        if (userRoom == room)
                            return false;

                        if (!LeaveRoom(user, userRoom))
                            return false;
                    }

                    room.AddUser(user);
                    user.Room = room;
                }

            string userName = user.Name;
            lock (userName)
            {
                SendChatMessage(room, userName + " connected", 0);
            }

            ServiceResponseMessage(user.Connections, Calls["JoinRoom"]["header"].ToString(), new RespondeRoomInfo(room.Id, room.Name, "Join room"), true);

            return true;
        }

        public bool LeaveRoom(User user, Room room)
        {
            lock (user) lock (room)
                {
                    if (room != null)
                    {
                        string userName = user.Name;

                        lock (userName)
                        {
                            SendChatMessage(room, userName + " disconnected", 0);
                        }

                        room.DeleteUser(user);

                        int UserCount;

                        var roomUsers = room.Users;

                        lock (roomUsers)
                        {
                            UserCount = roomUsers.Count;
                        }

                        if (UserCount == 0)
                        {
                            lock (_rooms)
                            {
                                foreach (var key in _rooms.Keys)
                                {
                                    var froom = _rooms[key];
                                    if (froom == room)
                                    {
                                        _rooms.Remove(key);
                                        _roomUIDmanager.ReleaseUID(key);

                                        string froomName = froom.Name;

                                        lock (froomName)
                                        {
                                            Program.LogService.Log("delete room: " + froomName);
                                        }

                                        Broadcast(SCalls["RoomChange"]["header"].ToString(), "Deleted room", true, false);
                                        break;
                                    }
                                }
                            }
                        }
                        user.Room = null;

                        var userConnections = user.Connections;
                        lock (userConnections)
                        {
                            ServiceResponseMessage(userConnections, Calls["LeaveRoom"]["header"].ToString(), new RespondeRoomInfo(room.Id, room.Name, "Leaved from room"), true);
                        }
                        return true;
                    }
                    return false;
                }
        }

        public static bool GetPropValue<T>(ref JToken iterator, string name, out T output)
        {
            output = default;

            if (iterator == null)
                return false;

            try
            {

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

            _globalRooms.Add(new Room(0, "global", ""));


#if DEBUG

            var fakedataPath = $"{Program.CurrentPath}/../../fake_data/fake.json";

            string allText = File.ReadAllText(fakedataPath);

            var Fakedata = JsonConvert.DeserializeObject(allText) as JObject;

            var fakearray = Fakedata["rooms"] as JArray;
            foreach (var fakeroom in fakearray)
            {
                string pass = "";
                var name = fakeroom["name"].ToString();
                var hassp = fakeroom["password"];
                if (hassp != null)
                    pass = hassp.ToString();

                int id = _roomUIDmanager.GetUID();
                _rooms.Add(id, new Room(id, name, pass));

            }

#endif


            server.Start();

            server.OnConnection((socket) =>
            {
                Program.LogService.Log("Connection");

                Connection SocketConnection = new Connection(socket);

                lock (_connectionConnections)
                {
                    _connectionConnections.Add(socket, SocketConnection);
                }

                socket.On(Calls["Registration"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Registration"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    Task.Factory.StartNew(() =>
                    {

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

                        var callBacks = _webSocketServiceHandler.Registration(header, name, pass);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);

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

                    var callBacks = _webSocketServiceHandler.Authorization(SocketConnection, header, name, pass);
                    CallbackHandler(SocketConnection, callBacks);

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

                    var callBacks = _webSocketServiceHandler.Deauthorization(header, SocketConnection);
                    CallbackHandler(SocketConnection, callBacks);

                });


                socket.On(Calls["Chat"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Chat"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    Task.Factory.StartNew(() =>
                    {
                        User user = GetAuthUser(SocketConnection);

                        if (!AuthCheckReport(user, SocketConnection))
                            return;

                        string message;

                        var iterator = data[0].First;
                        if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "message", out message))
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        Room room = user.Room;

                        if (room == null)
                            return;

                        message = message.Trim();

                        if (!ValidateDefaultText(message))
                            return;

                        _webSocketServiceHandler.Chat(room, message, user.UID);

                    }, TaskCreationOptions.DenyChildAttach);

                });

                socket.On(Calls["JoinRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["JoinRoom"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    Task.Factory.StartNew(() =>
                    {

                        User user = GetAuthUser(SocketConnection);

                        if (!AuthCheckReport(user, SocketConnection))
                            return;

                        int RoomId;
                        string Password;

                        var iterator = data[0].First;
                        if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "id", out RoomId))
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        if (!GetPropValue(ref iterator, "password", out Password))
                            Password = "";


                        Room room = GetRoom(RoomId);

                        var callBacks = _webSocketServiceHandler.JoinRoom(header, user, room, Password);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);

                });

                socket.On(Calls["LeaveRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["LeaveRoom"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    Task.Factory.StartNew(() =>
                    {

                        User user = GetAuthUser(SocketConnection);

                        if (!AuthCheckReport(user, SocketConnection) || user.Room == null)
                        {
                            ServiceResponseMessage(SocketConnection, header, "notInRoom", false);
                            return;
                        }

                        int RoomId;

                        var iterator = data[0].First;
                        if (data == null || data.Length == 0 || data[0] == null || !GetPropValue(ref iterator, "id", out RoomId))
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        Room room = GetRoom(RoomId);

                        var callBacks = _webSocketServiceHandler.LeaveRoom(header, user, room);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);

                });

                socket.On(Calls["GetRooms"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetRooms"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif
                    Task.Factory.StartNew(() =>
                    {

                        User user = GetAuthUser(SocketConnection);

                        if (!AuthCheckReport(user, SocketConnection))
                            return;

                        string roomname = null;

                        var iterator = data[0].First;
                        if (data != null && data.Length > 0 && data[0] != null && GetPropValue(ref iterator, "room", out roomname))
                        {
                        }

                        var callBacks = _webSocketServiceHandler.GetRooms(header, roomname);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);

                });

                socket.On(Calls["ChangeProfile"]["header"], (JToken[] data) =>
                {
                    string header = Calls["ChangeProfile"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    Task.Factory.StartNew(() =>
                    {

                        User user = GetAuthUser(SocketConnection);

                        if (!AuthCheckReport(user, SocketConnection))
                            return;

                        string name = null;
                        string avatar = null;
                        byte[] bytes = null;

                        var iterator = data[0].First;
                        if (data != null && data.Length <= 0 && data[0] != null)
                        {
                            if (GetPropValue(ref iterator, "name", out name))
                                if (!ValidateDefaultText(name, maxlength: 32))
                                {
                                    ServiceResponseMessage(SocketConnection, header, "badName", false);
                                    return;
                                }

                            if (GetPropValue(ref iterator, "avatar", out avatar))
                            {
                                try
                                {
                                    bytes = Convert.FromBase64String(avatar);
                                }
                                catch (FormatException)
                                {
                                    ErrorResponseMessage(SocketConnection, header, "invalidData");
                                    return;
                                }
                            }
                        }
                        else
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        var callBacks = _webSocketServiceHandler.ChangeProfile(header, user, name, bytes);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);



                });

                socket.On(Calls["GetUsers"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetUsers"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif
                    Task.Factory.StartNew(() =>
                    {
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

                        var callBacks = _webSocketServiceHandler.GetUsers(SocketConnection, header, userids, select);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);
                });

                socket.On(Calls["CreateRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["CreateRoom"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif


                    Task.Factory.StartNew(() =>
                    {
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

                        var callBacks = _webSocketServiceHandler.CreateRoom(user, header, RoomName, Password);
                        CallbackHandler(SocketConnection, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);

                });


                socket.On(SocketIOEvent.DISCONNECT, () =>
                {
                    _webSocketServiceHandler.DisconnectAction(SocketConnection, true);
                });

                socket.On(SocketIOEvent.ERROR, () =>
                {
                    _webSocketServiceHandler.DisconnectAction(SocketConnection, true);
                });

            });

        }

    }
}
