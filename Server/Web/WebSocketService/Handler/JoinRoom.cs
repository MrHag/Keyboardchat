using System;
using System.Collections.Generic;
using KeyBoardChat.Models;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {

        public IEnumerable<HandlerCallBack> JoinRoom(string header, User user, Room room, string Password)
        {

            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {

                if (room == null)
                {
                    outcallback.Add(new HandlerCallBack(header: header, data: "roomNotFound", successfull: false, error: false));
                    return;
                }

                string roomPassword = room.Password;
                lock (roomPassword)
                {
                    if (roomPassword == "" || roomPassword == Password)
                    {
                        _webSocketService.JoinRoom(user, room);
                    }
                    else
                    {
                        outcallback.Add(new HandlerCallBack(header: header, data: "invalidPass", successfull: false, error: false));
                    }
                }

            })).Invoke();

            return outcallback;
        }

    }
}
