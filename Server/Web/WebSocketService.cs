using Keyboardchat.DataBase;
using Keyboardchat.Extensions;
using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Keyboardchat.ModifiedObjects;
using Keyboardchat.SaveCollections;
using Microsoft.Extensions.Configuration;
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
using System.Threading.Tasks;

namespace Keyboardchat.Web
{
    public class WebSocketService
    {

        private JToken Calls;
        private JToken SCalls;
        private SocketIOServer server;

        private SaveDictionary<SocketIOSocket, User> _connectionUsers;

        private SaveDictionary<uint, User> _authUsers;

        private SaveList<Room> _rooms;
        private SaveList<Room> _globalRooms;

        public WebSocketService()
        {
            Calls = Program.API.SelectToken("Calls");
            SCalls = Program.API.SelectToken("ServerCalls");
            server = new SocketIOServer(new SocketIOSharp.Server.SocketIOServerOption(4001));
            _authUsers = new SaveDictionary<uint, User>();
            _connectionUsers = new SaveDictionary<SocketIOSocket, User>();
            _rooms = new SaveList<Room>();
            _globalRooms = new SaveList<Room>();
        }

        private void ResponseMessage(SocketIOSocket client, string header, object data, bool succ, bool err)
        {
            ResponseBody responseBody = new ResponseBody(data, succ, err);
            client.Emit(header, responseBody);

#if DEBUG
            Program.LogService.Log($"{header} Responsebody to {GetUser(client).Name}\n{responseBody.Json()}");
#endif
        }

        private void SendChatMessage(Room room, string message, uint userId)
        {

            var MessageBody = new MessageBody(userId, message);
            server.EmitTo(room, Calls["Chat"]["header"].ToString(), MessageBody);
#if DEBUG
            Program.LogService.Log($"Message to\n{room.Name}, {message}, from {userId}");
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

        private bool AuthCheckReport(User user)
        {
            if (user == null)
                return false;

            return _authUsers.Open((Interface) =>
            {
                if (!user.Auth)
                {
                    ErrorResponseMessage(user.Client, SCalls["Access"]["header"].ToString(), "notAuth");
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
            _authUsers.Open((Interface) =>
            {
                foreach (var user in Interface.Values)
                {
                    ResponseMessage(user.Client, header, data, successful, error);
                }
            });       
        }

        public bool DeleteUser(User user)
        {
            if (user == null)
                return false;
            return _connectionUsers.Open((Interface) =>
            {
                return Interface.Remove(user.Client);
            });
        }

        public void JoinRoom(User user, Room room)
        {
            try
            {
                _connectionUsers.Open((Interface) =>
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

                SendChatMessage(room, user.Name + " connected", 0);
                ServiceResponseMessage(user.Client, Calls["JoinRoom"]["header"].ToString(), new LeftJoinedRoom(room.Name, "Join room"), true);
            }
            catch (QueueExitException)
            { }
        }

        public void LeaveRoom(User user, Room room)
        {

            if (!AuthCheckReport(user))
                return;

            try
            {

                if (room != null)
                {
                    SendChatMessage(room, user.Name + " disconnected", 0);
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

                _connectionUsers.Open((Interface) =>
                {
                    Interface.Add(socket, new User(socket));
                });

                Action<bool> DisconnectAction = (disconnected) =>
                {
                    Task.Run(() =>
                    {
                        _connectionUsers.Open((UserInterface) =>
                        {
                            User user = GetUser(socket);

                            if (user != null)
                            {
                                Room userroom = user.Room;

                                LeaveRoom(user, userroom);
                            }

                            user.Auth = false;

                            if(disconnected)
                            DeleteUser(user);

                            _authUsers.Open((Interface) =>
                            {
                                Interface.Remove(user.UID);
                            });
                            
#if DEBUG
                            Program.LogService.Log("Users:" + UserInterface.Count);
#endif

                        });

                    });

                };

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
                                ServiceResponseMessage(socket, header, "wrongNamePass", false);
                                return;
                            }
                        }

                        User user = GetUser(socket);

                        if (user.Auth)
                        {
                            ServiceResponseMessage(socket, header, "alreadyInSess", false);
                            return;
                        }

                        _connectionUsers.Open((Interface) =>
                        {
                            user.Name = name;
                            user.UID = UserId;
                            user.Auth = true;
                        });
                        
                        _authUsers.Open((Interface) =>
                        {
                            try
                            {
                                Interface.Add(user.UID, user);
                            }
                            catch (ArgumentException)
                            { }
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


                        DisconnectAction.Invoke(false);

                        ServiceResponseMessage(socket, header, "Deaunthentication successful", true);
                    });

                });


                socket.On(Calls["Chat"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Chat"]["header"].ToString();

                    Task.Run(() =>
                    {

                        User user = null;
                        Room room = null;

                        user = GetUser(socket);

                        if (!AuthCheckReport(user))
                            return;

                        room = user.Room;
                        
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

                        SendChatMessage(room, message, user.UID);

                    });

                });

                socket.On(Calls["JoinRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["JoinRoom"]["header"].ToString();

                    Task.Run(() =>
                    {
                        User user = GetUser(socket);

                        if (!AuthCheckReport(user))
                            return;

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

                            _connectionUsers.Open((RoomInterface) =>
                            {
                                user = GetUser(socket);
                                if (!AuthCheckReport(user) || user.Room == null)
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

                            _connectionUsers.Open((RoomInterface) =>
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

                        User user = GetUser(socket);

                        if (!AuthCheckReport(user))
                            return;

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

                socket.On(Calls["ChangeProfile"]["header"], (JToken[] data) =>
                {
                    string header = Calls["ChangeProfile"]["header"].ToString();

                    Task.Run(() =>
                    {
                        User user = GetUser(socket);

                        if (!AuthCheckReport(user))
                            return;

                        string name = null;
                        string avatar = null;

                        bool reqname = true;
                        bool reqavatar = true;

                        if (data != null && data.GetValues(0, out List<JToken> values) != null)
                        {
                            try
                            {
                                reqname = values[0].GetValue(out name, "name") != null;
                                reqavatar = values[0].GetValue(out avatar, "avatar") != null;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        else
                        {
                            ErrorResponseMessage(socket, header, "invalidData");
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
                                var dbuser = dbcontext.Users.Single(dbuser => dbuser.UserId == user.UID);

                                if (reqname)
                                {
                                    if (!ValidateDefaultText(name, maxlength: 32))
                                    {
                                        ServiceResponseMessage(socket, header, "badName", false);
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
                                        ServiceResponseMessage(socket, header, "invalidData", false);
                                        return;
                                    }

                                    using (MemoryStream ms = new MemoryStream(bytes))
                                    {
                                        System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);
                                        if (bitmap.Width > 128 || bitmap.Height > 128)
                                        {
                                            ServiceResponseMessage(socket, header, "badImage", false);
                                            return;
                                        }

                                        dbuser.Avatar = bytes;

                                        var sha256 = SHA256.Create();
                                        var avatarHash = sha256.ComputeHash(dbuser.Avatar);

                                        dbuser.AvatarHash = avatarHash;
                                    }

                                }

                                dbcontext.SaveChanges();

                                ServiceResponseMessage(socket, header, "Data changed", true);

                            }
                        }

                    });

                });


                socket.On(Calls["GetUsers"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetUsers"]["header"].ToString();

                    Task.Run(() =>
                    {
                        User user = GetUser(socket);

                        if (!AuthCheckReport(user))
                            return;

                        List<uint> userids = null;
                        List<string> select = null;


                        if (data != null && data.GetValues(0, out List<JToken> values) != null)
                        {
                            try
                            {
                                JToken iterateToken = values[0].First;
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

                            ServiceResponseMessage(socket, header, jObject, true);
                        }

                        
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
                            
                            User user = GetUser(socket);

                            if (!AuthCheckReport(user))
                                return;

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


                socket.On(SocketIOEvent.DISCONNECT, () => DisconnectAction(true));

                socket.On(SocketIOEvent.ERROR, () => DisconnectAction(true));

            });

        }

    }
}
