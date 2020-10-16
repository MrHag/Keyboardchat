using Keyboardchat.Extensions;
using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Keyboardchat.ModifiedObjects;
using Keyboardchat.SaveCollections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Common;
using SocketIOSharp.Server.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Keyboardchat
{
    public class WebSocketService
    {

        private JToken Calls;
        private JToken SCalls;
        private SocketIOServer server;

        private SaveList<User> _users;

        private SaveList<Room> _rooms;
        private SaveList<Room> _globalRooms;

        public WebSocketService()
        {
            Calls = Program.API.SelectToken("Calls");
            SCalls = Program.API.SelectToken("ServerCalls");
            server = new SocketIOServer(new SocketIOSharp.Server.SocketIOServerOption(4001));
            _users = new SaveList<User>();
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

        private bool AuthCheckReport(User user)
        {
            bool userAuth = user.Authorizated;

            if (!userAuth)
                ErrorResponseMessage(user.Client, SCalls["Access"]["header"].ToString(), "notAuth");

            return userAuth;
        }

        public User GetUser(SocketIOSocket client, SaveList<User>.SaveListInterface Interface)
        {
            foreach (var user in Interface)
            {
                if (user.Client == client)
                    return user;
            }
            return null;
        }

        public Room GetRoom(string RoomName, SaveList<Room>.SaveListInterface Interface)
        {

            foreach (var room in Interface)
            {
                if (room.Name == RoomName)
                    return room;
            }

            return null;
        }

        public void Broadcast(string header, object data, bool successful, bool error)
        {
            var Interface = _users.EnterInQueue();
            foreach (var user in Interface)
            {
                if(user.Authorizated)
                ResponseMessage(user.Client, header, data, successful, error);
            }
            Interface.ExitFromQueue();
        }

        public void DeleteUser(User user, SaveList<User>.SaveListInterface Interface)
        {
            Interface.Remove(user);
        }

        public void JoinRoom(User user, Room room)
        {

            if (!AuthCheckReport(user))
                return;

            var Interface = _users.EnterInQueue();

            if (user.Room != null)
            {
                Interface.ExitFromQueue();

                if (user.Room == room)
                    return;

                LeaveRoom(user, user.Room);

                Interface = _users.EnterInQueue();
            }

            

            room.AddUser(user);
            user.Room = room;

            Interface.ExitFromQueue();

            SendChatMessage(room, user.Name + " connected", "Server", "images/server.jpg");
            ServiceResponseMessage(user.Client, Calls["JoinRoom"]["header"].ToString(), new LeftJoinedRoom(room.Name, "Join room"), true);
        }

        public void LeaveRoom(User user, Room room)
        {

            if (!AuthCheckReport(user))
                return;

            if (room != null)
            {
                SendChatMessage(room, user.Name + " disconnected", "Server", "images/server.jpg");
                room.DeleteUser(user);

                var UserInterface = room.Users.EnterInQueue();

                var UserCount = UserInterface.Count;

                UserInterface.ExitFromQueue();

                if (UserCount == 0)
                {
                    var RoomInterface = _rooms.EnterInQueue();

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

                    RoomInterface.ExitFromQueue();
                }
                var ForRoomInterface = _users.EnterInQueue();
                user.Room = null;
                ForRoomInterface.ExitFromQueue();
            }

        }

        public void Start()
        {

            var Interface = _globalRooms.EnterInQueue();

            Interface.Add(new Room("global", ""));

            Interface.ExitFromQueue();

            server.Start();

            server.OnConnection((socket) =>
            {
                
                Program.LogService.Log("Connection");

                var Interface = _users.EnterInQueue();

                Interface.Add(new User(socket));

                Program.LogService.Log("Users:"+Interface.Count);

                Interface.ExitFromQueue();
                

                socket.On(Calls["Authorization"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Authorization"]["header"].ToString();
                    Task.Run(() => {

                        List<JToken> values;
                        string name;

                        if (data == null || data.GetValues(0, out values) == null || values[0].GetValue(out name, "name") == null)
                        {
                            ErrorResponseMessage(socket, header, "invalidData");
                            return;
                        }

                        if (!ValidateDefaultText(name))
                        {
                            ServiceResponseMessage(socket, header, "badName", false);
                            return;
                        }

                        var Interface = _users.EnterInQueue();

                        User user = GetUser(socket, Interface);

                        user.Name = name;
                        user.Authorizated = true;

                        var RoomInterface = _globalRooms.EnterInQueue();

                        RoomInterface.ExitFromQueue();

                        Interface.ExitFromQueue();

                        JoinRoom(user, RoomInterface[0]);

                        ServiceResponseMessage(socket, header, "Aunthentication successful", true);
                    });

                });


                socket.On(Calls["Chat"]["header"], (JToken[] data) =>
                {
                    string header = Calls["Chat"]["header"].ToString();

                    Task.Run(() =>
                    {

                        var Interface = _users.EnterInQueue();

                        var user = GetUser(socket, Interface);

                        var room = user.Room;

                        Interface.ExitFromQueue();

                        if (!AuthCheckReport(user))
                            return;

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

                        var UserInterface = _users.EnterInQueue();

                        User user = GetUser(socket, UserInterface);

                        UserInterface.ExitFromQueue();

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

                        var Interface = _rooms.EnterInQueue();

                        for (int key = 0; key < Interface.Count; key++)
                        {
                            var room = Interface[key];

                            if (room.Name == RoomName)
                            {
                                if (room.Password == "" || room.Password == Password)
                                {
                                    Interface.ExitFromQueue();

                                    JoinRoom(user, room);

                                    return;
                                }
                                else
                                {
                                    ServiceResponseMessage(socket, header, "invalidPass", false);
                                    Interface.ExitFromQueue();
                                    return;
                                }
                            }
                        }

                        Interface.ExitFromQueue();

                        ServiceResponseMessage(socket, header, "roomNotFound", false);

                    });

                });

                socket.On(Calls["LeftRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["LeftRoom"]["header"].ToString();

                    Task.Run(() =>
                    {

                        var Interface = _users.EnterInQueue();

                        User user = GetUser(socket, Interface);

                        Interface.ExitFromQueue();

                        if (!AuthCheckReport(user) || user.Room == null)
                        {
                            ServiceResponseMessage(socket, header, "notInRoom", false);
                            return;
                        }

                        string RoomName;
                        if (data == null || data.GetValues(0, out List<JToken> values) == null || values[0].GetValue(out RoomName, "name") == null)
                        {
                            ErrorResponseMessage(socket, header, "invalidData");
                            return;
                        }

                        var RoomInterface = _rooms.EnterInQueue();

                        Room room = GetRoom(RoomName, RoomInterface);

                        RoomInterface.ExitFromQueue();

                        if (room == null || user.Room == room)
                        {
                            ServiceResponseMessage(socket, header, "roomNotFound", false);
                            return;
                        }

                        LeaveRoom(user, room);

                        ServiceResponseMessage(socket, header, new LeftJoinedRoom(RoomName, "Leaved from room"), true);

                    });
                });

                socket.On(Calls["GetRooms"]["header"], (JToken[] data) =>
                {
                    string header = Calls["GetRooms"]["header"].ToString();

                    Task.Run(() =>
                    {

                        var UserInterface = _users.EnterInQueue();

                        User user = GetUser(socket, UserInterface);

                        UserInterface.ExitFromQueue();

                        if (!AuthCheckReport(user))
                            return;

                        var Interface = _rooms.EnterInQueue();

                        List<(Room room, int qual)> rooms = new List<(Room room, int qual)>();

                        foreach (var room in Interface)
                            rooms.Add((room, 0));

                        Interface.ExitFromQueue();

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

                socket.On(Calls["CreateRoom"]["header"], (JToken[] data) =>
                {
                    string header = Calls["CreateRoom"]["header"].ToString();

                    Program.LogService.Log("create room call");

                    Task.Run(() => 
                    {

                        var UserInterface = _users.EnterInQueue();

                        User user = GetUser(socket, UserInterface);

                        UserInterface.ExitFromQueue();

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

                        var Interface = _rooms.EnterInQueue();

                        foreach (var room in Interface)
                        {

                            if (room.Name == RoomName)
                            {
                                ServiceResponseMessage(socket, header, "roomExists", false);
                                Interface.ExitFromQueue();
                                return;
                            }

                        }

                        Room NewRoom = new Room(RoomName, Password);

                        Interface.Add(NewRoom);

                        Interface.ExitFromQueue();

                        ServiceResponseMessage(socket, header, "Created room", true);
                        Broadcast(SCalls["RoomChange"]["header"].ToString(), "Created room", true, false);

                        JoinRoom(user, NewRoom);

                    });
                    
                });

                Action DisconnectAction = () =>
                {
                    Task.Run(() =>
                    {

                        var Interface = _users.EnterInQueue();

                        User user = GetUser(socket, Interface);

                        Room userroom = user.Room;

                        bool auth = user.Authorizated;

                        if (auth)
                        {
                            Interface.ExitFromQueue();

                            LeaveRoom(user, userroom);

                            Interface = _users.EnterInQueue();

                            DeleteUser(user, Interface);
                            
                        }
                        else
                        {
                            DeleteUser(user, Interface);
                        }
#if DEBUG
                        Program.LogService.Log("Users:" + Interface.Count);
#endif

                        Interface.ExitFromQueue();

                    });

                };

                socket.On(SocketIOEvent.DISCONNECT, DisconnectAction);

                socket.On(SocketIOEvent.ERROR, DisconnectAction);

            });

        }

    }
}
