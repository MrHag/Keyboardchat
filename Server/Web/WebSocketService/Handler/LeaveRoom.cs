using System;
using System.Collections.Generic;
using KeyBoardChat.Models;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> LeaveRoom(string header, User user, Room room)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {
                lock (room)
                {
                    if (room == null || user.Room != room)
                    {
                        outcallback.Add(new HandlerCallBack(header: header, data: "roomNotFound", successfull: false, error: false));
                        return;
                    }

                    if (!_webSocketService.LeaveRoom(user, room))
                        return;
                }

                Room globalRoom;

                lock (_webSocketService._globalRooms)
                {
                    globalRoom = _webSocketService._globalRooms[0];
                }

                _webSocketService.JoinRoom(user, globalRoom);

            })).Invoke();

            return outcallback;
        }

    }
}
