﻿using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public class CreateRoomHandler : WebSocketServiceHandler
    {
        [JsonProperty("name")]
        public string RoomName { get; set; }

        [JsonProperty("password", Required = Required.AllowNull)]
        public string Password { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();


            ((Action)(() =>
            {

                RoomName = RoomName.Trim();

                if (!_webSocketService.ValidateDefaultText(RoomName))
                {
                    outcallback.Add(new HandlerCallBack(data: "badName", error: true));
                    return;
                }

                Room NewRoom;


                Room room = _webSocketService.GetRoom(RoomName);

                if (room != null)
                {
                    outcallback.Add(new HandlerCallBack(data: "roomExists", error: true));
                    return;
                }

                int Id = _webSocketService._roomUIDmanager.GetUID();

                NewRoom = new Room(RoomName, Password);

                _webSocketService.AddRoom(NewRoom);

                outcallback.Add(new HandlerCallBack(data: new RespondeRoomInfo(NewRoom.Id, NewRoom.Name, "Created room"), error: false));

                NewRoom.AddUser(connection.Session.User);

            })).Invoke();

            return outcallback;
        }

    }
}
