var express = require('express');
var socket = require('socket.io');
var path = require('path');
const { func } = require('prop-types');
const { getuid } = require('process');
var cors = require('cors');

const Room = require("./Room.js");
const User = require("./User.js");

const MessageBody = require("./MessageBody.js");
const ResponceBody = require("./ResponceBody.js");


var corsOptions = {
    origin: 'http://127.0.0.1:4000',
    optionsSuccessStatus: 200 // some legacy browsers (IE11, various SmartTVs) choke on 204
}

const Path = './public/';

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
    body = new MessageBody(name, message, ava);
    io.to(to).emit('chat', body);
}

function service_message(ws, type, data) {

    console.log(type + ": " + data);
    resp = new ResponceBody(type, data, false);
    console.log("This is our responce = ", resp);
    io.to(ws.id).emit('responce', resp);
}

function access_error(ws, data) {

    console.log("access error: " + data);
    resp = new ResponceBody("accessError", data, true);
    io.to(ws.id).emit('responce', resp);
}

function error_message(ws, data, type) {

    console.log("error: " + data);
    resp = new ResponceBody(type, data, true);
    io.to(ws.id).emit('responce', resp);
}

function joinroom(ws, room) {

    var user = GetUser(ws);

    if (!AuthCheckReport(user))
        return false;

    if (user.room != null) {
        SendMessage(user.room.name, user.name + " disconnected", "Server", "images/server.jpg");
        ws.leave(user.room.name);
    }

    user.room = null;

    ws.join(room.name, (err) => {

        if (err) {
            console.error(err);
            return false;
        }
        console.log(room.name);
        SendMessage(room.name, user.name + " connected", "Server", "images/server.jpg");
        user.room = room;
        return true;
    });
}

function WebSocket(io) {
    io.on('connection', async ws => {

        console.log("Connection");

        user_list.push(new User(ws));

        ws.on("auth", data => {

            if (data == undefined || data.name.length == null) {
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
            SendMessage(user.room.name, data.message, user.name);
            
        });

        ws.on('JoinRoom', req => {

            if (req == null || req.name == null) {
                error_message(ws, "Cant join room", "roomError");
                return;
            }

            if (req.password == null)
                req.password == "";
            
            for (var key in room_list) {
                let room = room_list[key];
                if (room.name === req.name) {
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
            joinroom(ws, room);

        });

        ws.on('error', error => {
            console.error(error);
        });

        ws.on('disconnect', () => {

            var user = GetUser(ws);

            if (!user.Auth)
                return;

            if (user.client == ws) {
                SendMessage(user.room.name, user.name + ' disconnected', "Server", "images/server.jpg");
                if (user.room !== null);
                    user.room.RemoveUser(user);
                DeleteUser(ws);
            }

        });
    });
}

WebSocket(io);