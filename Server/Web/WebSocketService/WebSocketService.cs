using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KeyBoardChat.DataBase;
using KeyBoardChat.Extensions;
using KeyBoardChat.Models;
using KeyBoardChat.Models.Network;
using KeyBoardChat.UseClasses;
using KeyBoardChat.Web.WebSocketService.Handlers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using SocketIOSharp.Server.Client;

namespace KeyBoardChat.Web.WebSocketService
{
    public class WebSocketService : IDisposable
    {

        internal JToken Calls;
        internal JToken SCalls;
        internal SocketIOServer server;

        internal Dictionary<SocketIOSocket, Connection> _connectionConnections;

        internal SortedDictionary<uint, User> _authUsers;

        internal UIDmanager _roomUIDmanager;
        private SortedDictionary<int, Room> _rooms;
        public IEnumerable<KeyValuePair<int, Room>> Rooms { get { return _rooms; } }

        internal event DataDelegate<Room> OnRoomAdded;
        internal event DataDelegate<Room> OnRoomRemoved;

        internal List<Room> _globalRooms;

        private User _serverUser;

        public WebSocketService()
        {
            Calls = Program.API.SelectToken("Calls");
            SCalls = Program.API.SelectToken("ServerCalls");
            server = new SocketIOServer(new SocketIOServerOption(4001));
            _authUsers = new SortedDictionary<uint, User>();
            _connectionConnections = new Dictionary<SocketIOSocket, Connection>();
            _roomUIDmanager = new UIDmanager();
            _rooms = new SortedDictionary<int, Room>();

            _serverUser = new User(0, "Server");

            OnRoomAdded += WebSocketService_OnRoomAdded;
            OnRoomRemoved += WebSocketService_OnRoomRemoved;

            _globalRooms = new List<Room>();
        }

        private void WebSocketService_OnRoomRemoved(object sender, Room room)
        {
            Broadcast(SCalls["RoomListChange"]["header"].ToString(), "Deleted room");

            room.UserJoined -= Room_UserJoined;
            room.UserLeaved -= Room_UserLeaved;
        }

        private void WebSocketService_OnRoomAdded(object sender, Room room)
        {
            Broadcast(SCalls["RoomListChange"]["header"].ToString(), "Created room");

            room.UserJoined += Room_UserJoined;
            room.UserLeaved += Room_UserLeaved;
        }

        private void Room_UserLeaved(object room, User user)
        {
            user.OnNameChanged -= User_OnNameChanged;

            Room instRoom = (Room)room;

            var roomUsers = instRoom.Users;

            if (roomUsers.Count() == 0)
            {
                RemoveRoom(instRoom.Id);
            }

        }

        private void Room_UserJoined(object room, User user)
        {
            user.OnNameChanged += User_OnNameChanged;
        }

        private void User_OnNameChanged(object sender, string data)
        {

        }

        public Session CreateSession()
        {
            Session session = new Session();

            session.OnConnectionAdded += Session_OnConnectionAdded;
            session.OnConnectionRemoved += Session_OnConnectionRemoved;
            session.RoomChanged += Session_RoomChanged;
            session.UserChanged += Session_UserChanged;

            return session;
        }

        public void DeleteSession(Session session)
        {
            session.Room = null;
            session.User = null;
            session.ClearConnections();

            session.OnConnectionAdded -= Session_OnConnectionAdded;
            session.OnConnectionRemoved -= Session_OnConnectionRemoved;
            session.RoomChanged -= Session_RoomChanged;
            session.UserChanged -= Session_UserChanged;
        }

        private void Session_UserChanged(object session, User user)
        {
            Session sess = (Session)session;

            if (sess.User != null)
            {
                _authUsers.Remove(sess.User.UID);
            }

            if (user != null)
            {
                _authUsers.Add(user.UID, user);
            }

        }

        private void Session_RoomChanged(object session, Room room)
        {
            Session sess = (Session)session;
            var sessConnections = sess.Connections;

            var sessRoom = sess.Room;
            if (sessRoom != null)
            {
                ServiceResponseMessage(sessConnections, Calls["LeaveRoom"]["header"].ToString(), new RespondeRoomInfo(sessRoom.Id, sessRoom.Name, "Leaved from room"));
                SendChatMessage(sessRoom, sess.User.Name + " disconnected", _serverUser);
            }

            if (room != null)
            {
                ServiceResponseMessage(sessConnections, Calls["JoinRoom"]["header"].ToString(), new RespondeRoomInfo(room.Id, room.Name, "Join room"));
                SendChatMessage(room, sess.User.Name + " connected", _serverUser);
            }
        }


        private void Session_OnConnectionRemoved(object session, Connection connection)
        {
            ServiceResponseMessage(connection, Calls["DeAuthorization"]["header"].ToString(), "Deauthorization successful");

            _connectionConnections.Remove(connection.Socket);

            var sess = (Session)session;
            var sessConnections = sess.Connections;

            if (sessConnections.Count() == 0)
            {
                DeleteSession(sess);
            }

        }

        private void Session_OnConnectionAdded(object session, Connection connection)
        {
            ServiceResponseMessage(connection, Calls["Authorization"]["header"].ToString(), "Aunthentication successful");

            Session sess = (Session)session;

            var sessRoom = sess.Room;
            if (sessRoom != null)
                ServiceResponseMessage(connection, Calls["JoinRoom"]["header"].ToString(), new RespondeRoomInfo(sessRoom.Id, sessRoom.Name, "Join room"));
        }

        internal bool AddRoom(Room room)
        {
            var id = _roomUIDmanager.GetUID();
            room.Id = id;

            if (_rooms.TryAdd(room.Id, room))
            {
                OnRoomAdded?.Invoke(this, room);
                return true;
            }
            return false;
        }

        internal bool RemoveRoom(int roomid)
        {
            Room room;
            if (_rooms.Remove(roomid, out room))
            {
                var id = room.Id;
                _roomUIDmanager.ReleaseUID(id);

                OnRoomRemoved?.Invoke(this, room);
                return true;
            }
            return false;
        }

        private void CallbackHandler(Connection connection, string header, IEnumerable<HandlerCallBack> callBacks)
        {
            foreach (var callBack in callBacks)
            {
                if (callBack.Error)
                    ErrorResponseMessage(connection, header, callBack.Data);
                else
                    ServiceResponseMessage(connection, header, callBack.Data);

            }
        }

        private void CallbackHandler(IEnumerable<Connection> connections, string header, IEnumerable<HandlerCallBack> callBacks)
        {
            foreach (var connection in connections)
                CallbackHandler(connection, header, callBacks);
        }

        internal void ResponseMessage(Connection connetion, string header, object data, object err = null)
        {
            ResponseBody responseBody = new ResponseBody(data, err);
            connetion.Socket.Server.EmitTo(connetion.Socket, header, responseBody);

#if DEBUG

            Task.Factory.StartNew(() =>
            {

                string name = "";

                var user = GetAuthUser(connetion);

                if (user != null)
                {
                    name = user.Name;
                }

                Program.LogService.Log($"{header} Responsebody to {name}\n{responseBody.Json()}");

            }, TaskCreationOptions.DenyChildAttach);
#endif
        }

        internal void ResponseMessage(IEnumerable<Connection> connections, string header, object data, object err = null)
        {
            foreach (var connection in connections)
                ResponseMessage(connection, header, data, err);
        }

        internal void SendChatMessage(Room room, string message, User user)
        {
            uint avatarid = 1;

            if (user.UID != 0)
            {
                var dbuser = GetDbUser(user.UID);
                avatarid = dbuser.AvatarId;
            }

            var MessageBody = new ResponseBody(new MessageBody(user.UID, user.Name, avatarId: avatarid, room.Id, message), null);
            server.EmitTo(room, SCalls["OnNewMsg"]["header"].ToString(), MessageBody);
#if DEBUG
            string name = room.Name;

            Program.LogService.Log($"Message to\n{name}, {message}, from {user.UID}");
#endif
        }


        internal DataBase.Models.User GetDbUser(uint id)
        {
            DataBase.Models.User dbuser = null;
            using (DatabaseContext dbcontext = new DatabaseContext())
            {
                try
                {
                    dbuser = dbcontext.Users.Single(user => user.UserId == id);
                }
                catch (InvalidOperationException ex)
                {
#if DEBUG
                    Program.LogService.Log(ex);
#endif
                }
            }

            return dbuser;
        }


        internal void ErrorResponseMessage(Connection connection, string header, object error, object data = null)
        {
            ResponseMessage(connection, header, data, error);
        }

        internal void ErrorResponseMessage(IEnumerable<Connection> connections, string header, object data, object error)
        {
            ResponseMessage(connections, header, data, error);
        }

        internal void ServiceResponseMessage(Connection connection, string header, object data)
        {
            ResponseMessage(connection, header, data);
        }

        internal void ServiceResponseMessage(IEnumerable<Connection> connections, string header, object data)
        {
            ResponseMessage(connections, header, data);
        }

        internal void OnQueryMeessage(string header)
        {
            Program.LogService.Log($"Called {header}");
        }

        internal bool ValidateDefaultText(string text, int minlength = 0, int maxlength = -1)
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
            Connection connection;
            if (_connectionConnections.TryGetValue(client, out connection))
                return connection;
            return null;
        }

        public User GetAuthUser(Connection client)
        {
            foreach (var user in _authUsers)
            {
                var connections = user.Value.Session.Connections;
                foreach (var connection in connections)
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
            foreach (var room in _rooms.Values)
                if (room.Name == RoomName)
                    return room;
            return null;
        }

        public Room GetRoom(int RoomId)
        {
            Room room;
            if (_rooms.TryGetValue(RoomId, out room))
                return room;
            return null;
        }

        public void Broadcast(string header, object data, string error = null)
        {
            foreach (var user in _authUsers.Values)
            {
                var sessionConnections = user.Session.Connections;
                ResponseMessage(sessionConnections, header, data, error);
            }
        }

        public void DisconnectAction(Connection SocketConnection)
        {
#if DEBUG
            OnQueryMeessage("disconnection");
#endif

            try
            {
                if (SocketConnection.Session != null)
                {
                    SocketConnection.Session.RemoveConnection(SocketConnection);
                }
                _connectionConnections.Remove(SocketConnection.Socket);
            }
            finally
            {
#if DEBUG
                Program.LogService.Log("Connections:" + _connectionConnections.Count);
#endif
            }
        }

        public void Start()
        {
            int UID = _roomUIDmanager.GetUID();
            var room = new Room("global", "") { Id = UID };
            _globalRooms.Add(room);
            OnRoomAdded?.Invoke(this, room);

#if DEBUG

            var fakedataPath = $"{Program.CurrentPath}/Server/fake_data/fake.json";

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
                AddRoom(new Room(name, pass));
            }

#endif

            server.Start();

            server.OnConnection((socket) =>
            {
                Program.LogService.Log("Connection");

                Connection SocketConnection = new Connection(socket);

                _connectionConnections.Add(socket, SocketConnection);

                socket.On(Calls["Registration"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Registration"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    RegistrationHandler registrationHandler;

                    try
                    {
                        registrationHandler = data[0].ToObject<RegistrationHandler>();
                    }
                    catch (Exception)
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    registrationHandler.WebSocketService = this;

                    if (!ValidateDefaultText(registrationHandler.Name, maxlength: 32))
                    {
                        ErrorResponseMessage(SocketConnection, header, "badName");
                        return;
                    }

                    if (!ValidateDefaultText(registrationHandler.Password, maxlength: 64))
                    {
                        ErrorResponseMessage(SocketConnection, header, "badPass");
                        return;
                    }

                    var callBacks = registrationHandler.Handle(SocketConnection);
                    CallbackHandler(SocketConnection, header, callBacks);

                });

                socket.On(Calls["Authorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Authorization"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    AuthorizationHandler authorizationHandler;

                    try
                    {
                        authorizationHandler = data[0].ToObject<AuthorizationHandler>();
                    }
                    catch (Exception)
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    authorizationHandler.WebSocketService = this;

                    if (!ValidateDefaultText(authorizationHandler.Name, maxlength: 32))
                    {
                        ErrorResponseMessage(SocketConnection, header, "badName");
                        return;
                    }

                    if (!ValidateDefaultText(authorizationHandler.Password, maxlength: 64))
                    {
                        ErrorResponseMessage(SocketConnection, header, "badPassword");
                        return;
                    }

                    var callBacks = authorizationHandler.Handle(SocketConnection);
                    CallbackHandler(SocketConnection, header, callBacks);

                });

                socket.On(Calls["DeAuthorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["DeAuthorization"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    DeauthorizationHandler deauthorizationHandler = new DeauthorizationHandler();
                    deauthorizationHandler.WebSocketService = this;

                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    var callBacks = deauthorizationHandler.Handle(SocketConnection);
                    CallbackHandler(SocketConnection, header, callBacks);

                });


                socket.On(Calls["SendMsg"]["header"], (JToken[] data) =>
                {
                    string header = Calls["SendMsg"]["header"].ToString();
#if DEBUG
                    OnQueryMeessage(header);
#endif

                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection))
                        return;

                    ChatHandler chatHandler;

                    try
                    {
                        chatHandler = data[0].ToObject<ChatHandler>();
                    }
                    catch (Exception)
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    chatHandler.WebSocketService = this;

                    Room room = user.Session.Room;

                    if (room == null)
                        return;

                    string message = chatHandler.Message.Trim();

                    if (!ValidateDefaultText(message))
                        return;

                    chatHandler.Message = message;

                    chatHandler.Handle(SocketConnection);

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

                    JoinRoomHandler joinRoomHandler;

                    try
                    {
                        joinRoomHandler = data[0].ToObject<JoinRoomHandler>();
                    }
                    catch (Exception)
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    joinRoomHandler.WebSocketService = this;

                    var callBacks = joinRoomHandler.Handle(SocketConnection);
                    CallbackHandler(SocketConnection, header, callBacks);

                });

                socket.On(Calls["LeaveRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["LeaveRoom"]["header"].ToString();

#if DEBUG
                    OnQueryMeessage(header);
#endif

                    User user = GetAuthUser(SocketConnection);

                    if (!AuthCheckReport(user, SocketConnection) || user.Session.Room == null)
                    {
                        ErrorResponseMessage(SocketConnection, header, "notInRoom");
                        return;
                    }

                    LeaveRoomHandler leaveRoomHandler;

                    try
                    {
                        leaveRoomHandler = data[0].ToObject<LeaveRoomHandler>();
                    }
                    catch (Exception)
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    leaveRoomHandler.WebSocketService = this;

                    var callBacks = leaveRoomHandler.Handle(SocketConnection);
                    CallbackHandler(SocketConnection, header, callBacks);

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

                    Task.Factory.StartNew(() =>
                    {
                        GetRoomsHandler getRoomsHandler;

                        try
                        {
                            getRoomsHandler = data[0].ToObject<GetRoomsHandler>();
                        }
                        catch (Exception)
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        getRoomsHandler.WebSocketService = this;

                        var callBacks = getRoomsHandler.Handle(SocketConnection);
                        CallbackHandler(SocketConnection, header, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);
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

                    Task.Factory.StartNew(() =>
                    {

                        ChangeProfileHandler changeProfileHandler;

                        try
                        {
                            changeProfileHandler = data[0].ToObject<ChangeProfileHandler>();
                        }
                        catch (Exception)
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        changeProfileHandler.WebSocketService = this;

                        var callBacks = changeProfileHandler.Handle(SocketConnection);
                        CallbackHandler(SocketConnection, header, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);



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

                    Task.Factory.StartNew(() =>
                    {

                        GetUsersHandler getUsersHandler;

                        try
                        {
                            getUsersHandler = data[0].ToObject<GetUsersHandler>();
                        }
                        catch (Exception)
                        {
                            ErrorResponseMessage(SocketConnection, header, "invalidData");
                            return;
                        }

                        getUsersHandler.WebSocketService = this;

                        var callBacks = getUsersHandler.Handle(SocketConnection);
                        CallbackHandler(SocketConnection, header, callBacks);

                    }, TaskCreationOptions.DenyChildAttach);

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

                    CreateRoomHandler createRoomHandler;

                    try
                    {
                        createRoomHandler = data[0].ToObject<CreateRoomHandler>();
                    }
                    catch (Exception)
                    {
                        ErrorResponseMessage(SocketConnection, header, "invalidData");
                        return;
                    }

                    createRoomHandler.WebSocketService = this;

                    var callBacks = createRoomHandler.Handle(SocketConnection);
                    CallbackHandler(SocketConnection, header, callBacks);

                });


                socket.On(SocketIOEvent.DISCONNECT, () =>
                {
                    DisconnectAction(SocketConnection);
                });

                socket.On(SocketIOEvent.ERROR, () =>
                {
                    DisconnectAction(SocketConnection);
                });

            });

        }

        public void Dispose()
        {
            ((IDisposable)server).Dispose();
        }
    }
}
