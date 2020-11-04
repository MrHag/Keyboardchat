using KeyBoardChat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public class LeaveRoomHandler : WebSocketServiceHandler
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();

            var room = _webSocketService.GetRoom(Id);

            ((Action)(() =>
            {
                var user = connection.Session.User;

                if (room == null || user.Session.Room != room)
                {
                    outcallback.Add(new HandlerCallBack(data: "roomNotFound", error: true));
                    return;
                }

                if (!room.DeleteUser(user))
                    return;

                var globalrooms = _webSocketService._globalRooms;
                globalrooms[0].AddUser(user);

            })).Invoke();

            return outcallback;
        }

    }
}
