var express = require('express');
var socket = require('socket.io');
var path = require('path');
const { func } = require('prop-types');
const { getuid } = require('process');
var cors = require('cors');

const Room = require("./room.js");
const User = require("./user.js");

const messageBody = require("./messageBody.js");
const responceBody = require("./responceBody.js");


var corsOptions = {
    origin: 'http://127.0.0.1:4000',
    optionsSuccessStatus: 200 // some legacy browsers (IE11, various SmartTVs) choke on 204
}

const Path = '../public/';

var app = express();
app.get('/', (req, res) => {
    console.log("Required files / = ", req.url);
    res.sendFile("index.html", {root: Path});
});

app.get('/*', (req, res) => {
    console.log("Required files = ", req.url);
    res.sendFile(req.url, {root: Path});
});

var server = app.listen(4000);

var io = socket(server);
io.origins('*:*');

var globalRoom = new Room("global");

var user_list = [];
var room_list = [];

const fakeRooms = require("./fake.js").rooms;

fakeRooms.forEach((room) => {
    room_list.push(new Room(room.name, null));
});

function GetUser(ws)
{
    for (var key in user_list) {

        user = user_list[key];
        if (user.client == ws)
            return user;
    }
}

function DeleteUser(ws) {
    for (var key in user_list) {

        user = user_list[key];
        if (user.client == ws)
            user_list.splice(key, 1);
    }
}

function AuthCheckReport(user)
{
    if (!user.Auth)
        access_error(user.client, 'You are not Authorizate');

    return user.Auth;
}

function SendMessage(to, message, name, ava)
{
    if (ava == undefined)
        ava = "images/unknown.png";

    console.log(to + " " + message + " " + name + " " + ava);
    body = new messageBody(name, message, ava);
    io.to(to).emit('chat', body);
}

function service_message(ws, type, data) {

    console.log(type + ": " + data);
    resp = new responceBody(type, data, false);
    console.log("This is our responce = ", resp);
    io.to(ws.id).emit('response', resp);
}

function access_error(ws, data) {

    console.log("access error: " + data);
    resp = new responceBody("accessError", data, true);
    io.to(ws.id).emit('response', resp);
}

function error_message(ws, data, type) {

    console.log("error: " + data);
    resp = new responceBody(type, data, true);
    io.to(ws.id).emit('response', resp);
}

function joinroom(ws, room) {

    var user = GetUser(ws);

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
        return true;
    });
}

function leaveroom(ws, room) {

    var user = GetUser(ws);

    if (!AuthCheckReport(user))
        return false;

    if (room != null) {
        SendMessage(room.name, user.name + " disconnected", "Server", "images/server.jpg");
        room.RemoveUser(user);
        ws.leave(room.name);

        if (room.users.length == 0) {
            for (var key in room_list) {
                let froom = room_list[key];
                if (froom == room) {
                    room_list.splice(key, 1);
                    console.log("delete room: ", froom.name);
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

        ws.on("auth", data => {

            if (data == null || data.name == null) {
                service_message(ws, "authFail", "incorrect nickname");
                return;
            }

            var user = GetUser(ws);

            user.name = data.name;
            user.Auth = true;

            joinroom(ws, globalRoom);

            service_message(ws, "authSucc", "Aunthentication successful");

        });

        ws.on('chat', (data) => {

            var user = GetUser(ws);

            if (user.client != ws)
                return;

            if (!AuthCheckReport(user))
                return;

            if (user.room == null)
                return;         

            console.log("Message = ", data);
            console.log("from = ", user.room.name);
            SendMessage(user.room.name, data.message, user.name);
            
        });

        ws.on('JoinRoom', req => {

            if (req == null || req.name == null) {
                error_message(ws, "Cant join room", "roomError");
                return;
            }

            if (req.password == null)
                req.password = "";
            
            for (var key in room_list) {
                let room = room_list[key];
                if (room.name === req.name) {
                    console.log(room.password);
                    console.log(req.password);
                    if (room.password !== "") {
                        joinroom(ws, room);
                    }
                    else
                    if (room.password === req.password) {
                        joinroom(ws, room);
                    }
                    return;
                }
            }

            let room = new Room(req.name, req.password);
            room_list.push(room);
            console.log("create room: ", room.name);
            joinroom(ws, room);

        });

        ws.on('getRooms', req => {
            var rooms = Array.from(room_list, (room) => { var obj = { name: room.name, qual: 0 }; return obj; });

            if (req != null && req.room != null) {
                for (var key in rooms) {
                    let room = rooms[key];

                    for (var i = req.room.length; i >= 0; i--) {
                        var str = req.room.substring(0, i);

                        if (room.name.includes(str)) {
                            room.qual = str.length;
                            break;
                        }

                    }

                    if (room.qual === 0) {
                        rooms.splice(key, 1);
                    }

                }
                rooms.sort((a, b) => { b.qual - a.qual });
            }

            var outrooms = Array.from(rooms, (room) => room.name);

            //console.log(room_list);
            //console.log(rooms);
            //console.log(outrooms);

            service_message(ws, "rooms", outrooms);

        });

        ws.on('error', error => {
            console.error(error);
        });

        ws.on('disconnect', () => {

            var user = GetUser(ws);

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