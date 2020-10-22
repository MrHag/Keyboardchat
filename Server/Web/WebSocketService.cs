using Keyboardchat.DataBase;
using Keyboardchat.Extensions;
using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Keyboardchat.ModifiedObjects;
using Keyboardchat.SaveCollections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace Keyboardchat.Web
{
    public class WebSocketService
    {

        private JToken Calls;
        private JToken SCalls;
        private SocketIOServer server;

        private SaveDictionary<SocketIOSocket, User> _connectionUsers;

        private SaveList<User> _users;

        private SaveList<Room> _rooms;
        private SaveList<Room> _globalRooms;

        public WebSocketService()
        {
            Calls = Program.API.SelectToken("Calls");
            SCalls = Program.API.SelectToken("ServerCalls");
            server = new SocketIOServer(new SocketIOSharp.Server.SocketIOServerOption(4001));
            _users = new SaveList<User>();
            _connectionUsers = new SaveDictionary<SocketIOSocket, User>();
            _rooms = new SaveList<Room>();
            _globalRooms = new SaveList<Room>();
        }

        private void ResponseMessage(SocketIOSocket client, string header, object data, bool succ, bool err)
        {
            ResponseBody responseBody = new ResponseBody(data, succ, err);
            client.Emit(header, responseBody);

#if DEBUG
            Program.LogService.Log($"Responsebody to\n{responseBody.Json()}");
#endif
        }

        private void SendChatMessage(Room room, string message, string userName, string avatar = null)
        {
            if (avatar == null)
                avatar = "images/unknown.png";

            var MessageBody = new MessageBody(userName, message, avatar);
            server.EmitTo(room, Calls["Chat"]["header"].ToString(), MessageBody);
#if DEBUG
            Program.LogService.Log($"Message to\n{room.Name}, {message}, from {userName}");
#endif
        }

        private void ErrorResponseMessage(SocketIOSocket client, string header, object data)
        {
            ResponseMessage(client, header, data, false, true);
        }

        private void ServiceResponseMessage(SocketIOSocket client, string header, object data, bool succ)
        {
            ResponseMessage(client, header, data, succ, false);
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

        private bool AuthCheckReport(SocketIOSocket client)
        {
            return _users.Open((Interface) => {

                User user = GetUser(client);

                if (user == null)
                {
                    ErrorResponseMessage(client, SCalls["Access"]["header"].ToString(), "notAuth");
                    return false;
                }

                return true;
            });
                
        }

        public User GetUser(SocketIOSocket client)
        {
            return _connectionUsers.Open((DInterface) => 
            {
                User user;
                if (DInterface.TryGetValue(client, out user))
                    return user;
                return null;
            });           
        }

        public Room GetRoom(string RoomName)
        {
            return _rooms.Open((Interface) => 
            {
                foreach (var room in Interface)
                {
                    if (room.Name == RoomName)
                        return room;
                }

                return null;

            });        
        }

        public void Broadcast(string header, object data, bool successful, bool error)
        {
            _connectionUsers.Open((DInterface) =>
            {
                foreach (var connection in DInterface.Keys)
                {
                    ResponseMessage(connection, header, data, successful, error);
                }
            });       
        }

        public bool DeleteUser(User user)
        {
            return _users.Open((Interface) =>
            {
                return Interface.Remove(user);
            });
        }

        public void JoinRoom(User user, Room room)
        {
            try
            {
                _users.Open((Interface) =>
                {

                    if (user.Room != null)
                    {

                        if (user.Room == room)
                            throw new QueueExitException();

                        LeaveRoom(user, user.Room);
                    } 

                    room.AddUser(user);
                    user.Room = room;
                });

                SendChatMessage(room, user.Name + " connected", "Server", "images/server.jpg");
                ServiceResponseMessage(user.Client, Calls["JoinRoom"]["header"].ToString(), new LeftJoinedRoom(room.Name, "Join room"), true);
            }
            catch (QueueExitException)
            { }
        }

        public void LeaveRoom(User user, Room room)
        {

            if (!AuthCheckReport(user.Client))
                return;

            try
            {

                if (room != null)
                {
                    SendChatMessage(room, user.Name + " disconnected", "Server", "images/server.jpg");
                    room.DeleteUser(user);

                    int UserCount = 0;
                    room.Users.Open((UserInterface) => 
                    {
                        UserCount = UserInterface.Count;
                    });

                    if (UserCount == 0)
                    {

                        _rooms.Open((RoomInterface) =>
                        {
                            for (int key = 0; key < RoomInterface.Count; key++)
                            {
                                var froom = RoomInterface[key];
                                if (froom == room)
                                {
                                    RoomInterface.RemoveAt(key);
                                    Program.LogService.Log("delete room: " + froom.Name);
                                    Broadcast(SCalls["RoomChange"]["header"].ToString(), "Deleted room", true, false);
                                    break;
                                }
                            }

                        });
                    }
                    _rooms.Open((RoomInterface) =>
                    {
                        user.Room = null;
                    });
                }

            }
            catch (QueueExitException)
            { 
            }

        }

        public void Start()
        {
            _globalRooms.Open((Interface) =>
            {
                Interface.Add(new Room("global", ""));
            });

            server.Start();

            server.OnConnection((socket) =>
            {
                Program.LogService.Log("Connection");

                socket.On(Calls["Registration"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Registration"]["header"].ToString();

                    Task.Run(() => 
                    {
                        List<JToken> values;
                        string name;
                        string pass;

                        if (data == null || data.GetValues(0, out values) == null || values[0].GetValue(out name, "name") == null || values[0].GetValue(out pass, "password") == null)
                        {
                            ErrorResponseMessage(socket, header, "invalidData");
                            return;
                        }

                        if (!ValidateDefaultText(name))
                        {
                            ServiceResponseMessage(socket, header, "badName", false);
                            return;
                        }

                        if (!ValidateDefaultText(pass))
                        {
                            ServiceResponseMessage(socket, header, "badPass", false);
                            return;
                        }

                        using (var dbcontext = new DatabaseContext())
                        {
                            try
                            {
                                dbcontext.Users.Single(user => user.Name == name);

                                ServiceResponseMessage(socket, header, "nameExists", false);
                                return;
                            }
                            catch (InvalidOperationException)
                            {

                                SHA256 SHA256 = SHA256.Create();

                                var passwordHash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(pass));

                                var dbUser = new DataBase.Models.User() { Name = name,  PasswordHash = passwordHash};
                                dbcontext.Add(dbUser);
                                if (dbcontext.SaveChanges() > 0)
                                {
                                    ServiceResponseMessage(socket, header, "Registration successful", true);
                                }
                                else
                                {
                                    ErrorResponseMessage(socket, header, "uknownError");                             
                                }
                            }
                        }

                    });
                });

                socket.On(Calls["Authorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Authorization"]["header"].ToString();
                    Task.Run(() => {

                        List<JToken> values;
                        string name;
                        string pass;

                        if (data == null || data.GetValues(0, out values) == null || values[0].GetValue(out name, "name") == null || values[0].GetValue(out pass, "password") == null)
                        {
                            ErrorResponseMessage(socket, header, "invalidData");
                            return;
                        }

                        if (!ValidateDefaultText(name, maxlength: 32))
                        {
                            ServiceResponseMessage(socket, header, "badName", false);
                            return;
                        }

                        if (!ValidateDefaultText(pass, maxlength: 64))
                        {
                            ServiceResponseMessage(socket, header, "badPass", false);
                            return;
                        }

                        using (var dbcontext = new DatabaseContext())
                        {
                            try
                            {
                                SHA256 SHA256 = SHA256.Create();
                                var passwordHash = SHA256.ComputeHash(Encoding.UTF8.GetBytes(pass));

                                var dbUser = dbcontext.Users.Single(user => user.Name == name);

                                if (!dbUser.PasswordHash.SequenceEqual(passwordHash))
                                    throw new InvalidOperationException();
                            }
                            catch (InvalidOperationException)
                            {
                                ServiceResponseMessage(socket, header, "wrongNamePass", false);
                                return;
                            }
                        }

                        User user = GetUser(socket);

                        if (user != null)
                        {
                            ServiceResponseMessage(socket, header, "alreadyInSess", false);
                            return;
                        }

                        _users.Open((Interface) =>
                        {
                            user = new User(name, null, socket);
                            Interface.Add(user);
                        });

                        _connectionUsers.Open((Interface) =>
                        {
                            Interface.Add(socket, user);
                        });

                        _globalRooms.Open((RoomInterface) =>
                        {
                            Room globalRoom = RoomInterface[0];
                            JoinRoom(user, globalRoom);
                        });
                        
                        ServiceResponseMessage(socket, header, "Aunthentication successful", true);
                    });

                });

                socket.On(Calls["DeAuthorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["DeAuthorization"]["header"].ToString();

                    Task.Run(() =>
                    {
                        User user = GetUser(socket);
                        if (user == null)
                        {
                            ServiceResponseMessage(socket, header, "notAuth", false);
                            return;
                        }

                        DeleteUser(user);

                        _connectionUsers.Open((Interface) =>
                        {
                            Interface.Remove(socket);
                        });

                        ServiceResponseMessage(socket, header, "Deaunthentication successful", true);
                    });

                });


                socket.On(Calls["Chat"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Chat"]["header"].ToString();

                    Task.Run(() =>
                    {

                        if (!AuthCheckReport(socket))
                            return;

                        User user = null;
                        Room room = null;

                        _users.Open((Interface) =>
                        {
                            user = GetUser(socket);
                            room = user.Room;
                        });

                        string message;

                        if (data == null || data.GetValues(0, out List<JToken> values) == null || values[0].GetValue(out message, "message") == null)
                        {
                            return;
                        }

                        if (room == null)
                            return;

                        message = message.Trim();

                        if (!ValidateDefaultText(message))
                            return;

                        SendChatMessage(room, message, user.Name);

                    });

                });

                socket.On(Calls["JoinRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["JoinRoom"]["header"].ToString();

                    Task.Run(() =>
                    {

                        if (!AuthCheckReport(socket))
                            return;

                        User user = null;

                        _users.Open((Interface) =>
                        {
                            user = GetUser(socket);
                        });

                        string RoomName;
                        string Password;

                        if (data == null || data.GetValues(0, out List<JToken> values) == null || values[0].GetValue(out RoomName, "name") == null)
                        {
                            ErrorResponseMessage(socket, header, "invalidData");
                            return;
                        }

                        if (values[0].GetValue(out Password, "password") == null)
                            Password = "";


                        try
                        {
                            _rooms.Open((Interface) =>
                            {

                                for (int key = 0; key < Interface.Count; key++)
                                {
                                    var room = Interface[key];

                                    if (room.Name == RoomName)
                                    {
                                        if (room.Password == "" || room.Password == Password)
                                        {
                                            JoinRoom(user, room);

                                            throw new QueueExitException();
                                        }
                                        else
                                        {
                                            ServiceResponseMessage(socket, header, "invalidPass", false);
                                            throw new QueueExitException();
                                        }
                                    }
                                }
                            });
                        }
                        catch (QueueExitException)
                        { }

                        ServiceResponseMessage(socket, header, "roomNotFound", false);
                    });
                });

                socket.On(Calls["LeaveRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["LeaveRoom"]["header"].ToString();

                    Task.Run(() =>
                    {
                        try
                        {
                            User user = null;

                            _users.Open((RoomInterface) =>
                            {
                                user = GetUser(socket);
                                if (!AuthCheckReport(user.Client) || user.Room == null)
                                {
                                    ServiceResponseMessage(socket, header, "notInRoom", false);
                                    throw new QueueExitException();
                                }
                            });


                            string RoomName;
                            if (data == null || data.GetValues(0, out List<JToken> values) == null || values[0].GetValue(out RoomName, "name") == null)
                            {
                                ErrorResponseMessage(socket, header, "invalidData");
                                return;
                            }

                            Room room = null;
                            Room globalRoom = null;

                            _rooms.Open((RoomInterface) =>
                            {
                                room = GetRoom(RoomName);
                            });

                            _globalRooms.Open((RoomInterface) =>
                            {
                                globalRoom = RoomInterface[0];
                            });

                            _users.Open((RoomInterface) =>
                            {
                                if (room == null || user.Room != room)
                                {
                                    ServiceResponseMessage(socket, header, "roomNotFound", false);
                                    throw new QueueExitException();
                                }

                                LeaveRoom(user, room);

                                ServiceResponseMessage(socket, header, new LeftJoinedRoom(RoomName, "Leaved from room"), true);

                                JoinRoom(user, globalRoom);
                            });

                        }
                        catch (QueueExitException)
                        { }
                    });
                
                });

                socket.On(Calls["GetRooms"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetRooms"]["header"].ToString();

                    Task.Run(() =>
                    {

                        if (!AuthCheckReport(socket))
                            return;

                        User user = null;
                        _users.Open((UserInterface) =>
                        {
                            user = GetUser(socket);
                        });

                        List<(Room room, int qual)> rooms = new List<(Room room, int qual)>();

                        _rooms.Open((Interface) =>
                        {                     
                            foreach (var room in Interface)
                                rooms.Add((room, 0));
                        });

                        string roomname;

                        if (data != null && data.GetValues(0, out List<JToken> values) != null && values[0].GetValue(out roomname, "room") != null)
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

                        ServiceResponseMessage(socket, header, outrooms, true);

                    });
                }); 

                socket.On(Calls["GetUsers"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetUsers"]["header"].ToString();

                    Task.Run(() =>
                    {
                        if (!AuthCheckReport(socket))
                            return;

                        List<uint> userids = null;

                        List<UserInfo> userInfos = new List<UserInfo>();


                        if (data != null && data.GetValues(0, out List<JToken> values) != null)
                        {
                            try
                            {
                                userids = (List<uint>)JsonConvert.DeserializeObject(values[0].ToString(), typeof(List<uint>));
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
                                dbusers = new List<DataBase.Models.User>(dbcontext.Users);
                            }

                            foreach (var dbuser in dbusers)
                            {
                                byte[] dbavatar = dbuser.Avatar;
                                string avatar = "";

                                if (dbavatar != null)
                                {
                                    avatar = Convert.ToBase64String(dbavatar);
                                }

                                var userinfo = new UserInfo(dbuser.UserId, dbuser.Name, avatar);
                                userInfos.Add(userinfo);
                            }
                        }

                        ServiceResponseMessage(socket, header, userInfos, true);
                    });

                });

                socket.On(Calls["CreateRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["CreateRoom"]["header"].ToString();

                    Program.LogService.Log("create room call");

                    Task.Run(() => 
                    {
                        try
                        {

                            if (!AuthCheckReport(socket))
                                return;
                            User user = null;
                            _users.Open((UserInterface) =>
                            {
                                user = GetUser(socket);
                            });

                            string RoomName;
                            string Password;
                            if (data == null || data.GetValues(0, out List<JToken> values) == null || values[0].GetValue(out RoomName, "name") == null)
                            {
                                ErrorResponseMessage(socket, header, "invalidData");
                                return;
                            }

                            RoomName = RoomName.Trim();

                            if (!ValidateDefaultText(RoomName))
                            {
                                ServiceResponseMessage(socket, header, "badName", false);
                                return;
                            }

                            if (values[0].GetValue(out Password, "password") == null)
                                Password = "";

                            Room NewRoom = null;

                            _rooms.Open((Interface) =>
                            {

                                foreach (var room in Interface)
                                {
                                    if (room.Name == RoomName)
                                    {
                                        ServiceResponseMessage(socket, header, "roomExists", false);
                                        throw new QueueExitException();
                                    }
                                }

                                NewRoom = new Room(RoomName, Password);

                                Interface.Add(NewRoom);

                            });

                            ServiceResponseMessage(socket, header, "Created room", true);
                            Broadcast(SCalls["RoomChange"]["header"].ToString(), "Created room", true, false);

                            JoinRoom(user, NewRoom);
                        }
                        catch (QueueExitException)
                        { }

                    });
                    
                });

                Action DisconnectAction = () =>
                {
                    Task.Run(() =>
                    {
                        _users.Open((UserInterface) =>
                        {
                            User user = GetUser(socket);
                        

                            if (user != null)
                            {
                                Room userroom = user.Room;

                                LeaveRoom(user, userroom);

                                DeleteUser(user); 
                            }
                            else
                            {
                                DeleteUser(user);
                            }
#if DEBUG
                            Program.LogService.Log("Users:" + UserInterface.Count);
#endif

                        });

                    });

                };

                socket.On(SocketIOEvent.DISCONNECT, DisconnectAction);

                socket.On(SocketIOEvent.ERROR, DisconnectAction);

            });

        }

    }
}
