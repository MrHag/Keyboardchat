const express = require('express');
const socket = require('socket.io');

const Room = require("./room.js");
const User = require("./user.js");

const messageBody = require("./messageBody.js");
const responceBody = require("./responseBody.js");

const Path = '../public/';

const API = require("../API");

const Calls = API.Calls;
const SCalls = API.ServerCalls;

const app = express();
app.get('/', (req, res) => {
    console.log("Required files / = ", req.url);
    res.sendFile("index.html", {root: Path});
});

app.get('/*', (req, res) => {
    console.log("Required files = ", req.url);
    res.sendFile(req.url, {root: Path});
});

const server = app.listen(4000);

const io = socket(server);
io.origins('*:*');

const globalRoom = new Room("global");

const user_list = [];
const room_list = [];


const fakeRooms = require("./fake.js").rooms;

fakeRooms.forEach((room) => {
    if (room.password == null)
        room.password = null;
    room_list.push(new Room(room.name, room.password));
});

function validateDefaultText(text, minlength = 0, maxlength = -1) {
    if (text.length < minlength)
        return false;

    if (maxlength != -1 && text.length > maxlength)
        return false;

    if (text.match(/[\S]+/g) == null)
        return false;

    return true;
}

function GetUser(ws)
{
    for (let user of user_list) {

        if (user.client == ws)
            return user;
    }
}

function DeleteUser(ws) {
    for (let key in user_list) {

        user = user_list[key];
        if (user.client == ws)
            user_list.splice(key, 1);
    }
}

function AuthCheckReport(user)
{
    if (!user.Auth)
        error_message(user.client, SCalls.Access.header, "notAuth");

    return user.Auth;
}

function SendMessage(to, message, name, ava)
{
    if (ava == null)
        ava = "images/unknown.png";

    console.log(to + " " + message + " " + name + " " + ava);
    let body = new messageBody(name, message, ava);
    io.to(to).emit(Calls.Chat.header, body);
}

function message(ws, header, data, succ, error) {

    console.log(header + ": " + data);
    let resp = new responceBody(data, succ, error);
    console.log("This is our response = ", resp);
    io.to(ws.id).emit(header, resp);
}

function service_message(ws, header, data, succ) {

    message(ws, header, data, succ, false);
}

function BroadCast(header, data, succ, error) {

    for (let user of user_list) {
        message(user.client, header, data, succ, error);
    }

}

function error_message(ws, header, data) {

    console.log("error: " + data);
    message(ws, header, data, false, true);
}

function joinroom(ws, room) {

    let user = GetUser(ws);

    if (!AuthCheckReport(user))
        return false;

    leaveroom(ws, user.room);

    ws.join(room.name, (err) => {

        if (err) {
            console.error(err);
            return false;
        }
        SendMessage(room.name, user.name + " connected", "Server", "images/server.jpg");
        room.AddUser(user);
        user.room = room;

        service_message(ws, Calls.JoinRoom.header, { message: "Join room", room: room.name }, true);
        return true;
    });
}

function leaveroom(ws, room) {

    let user = GetUser(ws);

    if (!AuthCheckReport(user))
        return false;

    if (room != null) {
        SendMessage(room.name, user.name + " disconnected", "Server", "images/server.jpg");
        room.RemoveUser(user);
        ws.leave(room.name);

        if (room.users.length == 0) {
            for (let key in room_list) {
                let froom = room_list[key];
                if (froom == room) {
                    room_list.splice(key, 1);
                    console.log("delete room: ", froom.name);
                    BroadCast(SCalls.RoomChange.header, "Deleted room", true, false);
                    break;
                }
            }
        }
        user.room = null;
    }

}

function WebSocket(io) {
    io.on('connection', async ws => {

        console.log("Connection");

        user_list.push(new User(ws));

        ws.on(Calls.Authorization.header, data => {

            let header = Calls.Authorization.header;

            if (data == null || data.name == null) {
                error_message(ws, header, "invalidData");
                return;
            }

            data.name = data.name.trim();

            if (!validateDefaultText(data.name, 4, 64)) {
                service_message(ws, header, "badName", false);
                return;
            }

            let user = GetUser(ws);

            user.name = data.name;
            user.Auth = true;

            joinroom(ws, globalRoom);

            service_message(ws, header, "Aunthentication successful", true);

        });

        ws.on(Calls.Chat.header, (data) => {

            if (data == null || data.message == null)
                return;

            let user = GetUser(ws);

            if (!AuthCheckReport(user))
                return;

            if (user.room == null)
                return;         

            data.message = data.message.trim();

            if (!validateDefaultText(data.message, 1, 2000))
                return;

            console.log("Message = ", data);
            console.log("from = ", user.room.name);
            SendMessage(user.room.name, data.message, user.name);
            
        });

        ws.on(Calls.JoinRoom.header, req => {

            let header = Calls.JoinRoom.header;

            if (req == null || req.name == null) {
                error_message(ws, header, "invalidData");
                return;
            }

            if (req.password == null)
                req.password = "";

            console.log("try to join:" + req.name + " " + req.password);
            
            for (let room of room_list) {
                if (room.name === req.name) {
                    console.log("roompass: "+room.password);
                    if (room.password == "" || room.password == null || room.password === req.password) {
                        joinroom(ws, room);
                    }
                    service_message(ws, header, "invalidPass", false);
                    return;
                }
            }

            service_message(ws, header, "roomNotFound", false);
        });


        ws.on(Calls.CreateRoom.header, req => {

            let header = Calls.CreateRoom.header;

            if (req == null || req.name == null) {
                error_message(ws, header, "invalidData");
                return;
            }

            if (!validateDefaultText(req.name, 0, 64)) {
                service_message(ws, header, "badName", false);
                return;
            }

            if (req.password == null)
                req.password = "";

            for (let room of room_list) {
                if (room.name === req.name) {
                    service_message(ws, header, "roomExists", false);
                    return;
                }
            }

            let room = new Room(req.name, req.password);
            room_list.push(room);
            console.log("create room: ", room.name);
            service_message(ws, header, "Created room", true);
            BroadCast(SCalls.RoomChange.header, "Created room", true, false);
            joinroom(ws, room);

        });

        ws.on(Calls.GetRooms.header, req => {

            let header = Calls.GetRooms.header;

            let rooms = Array.from(room_list, (room) => { let obj = { name: room.name, password: room.password, qual: 0 }; return obj; });

            if (req != null && req.room != null) {
                for (let key = 0; key < rooms.length; key++) {
                    let room = rooms[key];

                    let str = "";
                    let minQual = Math.ceil(req.room.length * (50 / 100));;

                    for (let i = req.room.length; i > 0; i--) {
                        str = req.room.substring(0, i);

                        let regex = new RegExp(`(${str})+`, "i");

                        console.log(room.name + " include " + str + "     result " + regex.test(room.name));

                        if (regex.test(room.name)) {
                            room.qual = str.length;
                            break;
                        }

                    }

                    console.log("Qual = " + room.qual);
                    console.log("minQual = " + minQual);

                    if (room.qual < minQual) {
                        rooms.splice(key, 1);
                        key--;
                    }

                }
                rooms.sort((a, b) => { return b.qual - a.qual });
            }

            let outrooms = Array.from(rooms, (room) => {

                let pass = room.password;

                console.log(room);

                let haspass = true;

                if (pass == null || pass == "")
                    haspass = false;

                return { room: room.name, haspass: haspass };
            });

            console.log(room_list);
            console.log(rooms);
            console.log(outrooms);

            service_message(ws, header, outrooms, true);

        });

        ws.on('error', error => {
            console.error(error);
        });

        ws.on('disconnect', () => {

            let user = GetUser(ws);

            if (!user.Auth)
                return;

            if (user.client == ws) {
                leaveroom(ws, user.room);
                DeleteUser(ws);
            }

        });
    });
}

WebSocket(io);