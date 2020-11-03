using Keyboardchat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public class JoinRoomHandler : WebSocketServiceHandler
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("password", Required = Required.AllowNull)]
        public string Password { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {

            var outcallback = new List<HandlerCallBack>();

            Room room = _webSocketService.GetRoom(Id);

            ((Action)(() =>
            {

                if (room == null)
                {
                    outcallback.Add(new HandlerCallBack(data: "roomNotFound", error: true));
                    return;
                }

                string roomPassword = room.Password;

                if (roomPassword == null || roomPassword == "" || roomPassword == Password)
                {
                    room.AddUser(connection.Session.User);
                }
                else
                {
                    outcallback.Add(new HandlerCallBack(data: "invalidPass", error: true));
                }

            })).Invoke();

            return outcallback;
        }

    }
}
