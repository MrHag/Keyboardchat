using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> CreateRoom(User user, string header, string RoomName, string Password)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {
                Room NewRoom;

                lock (_webSocketService._rooms)
                {
                    Room room = _webSocketService.GetRoom(RoomName);

                    if (room != null)
                    {
                        outcallback.Add(new HandlerCallBack(header: header, data: "roomExists", successfull: false, error: false));
                        return;
                    }

                    int Id = _webSocketService._roomUIDmanager.GetUID();

                    NewRoom = new Room(Id, RoomName, Password);

                    _webSocketService._rooms.Add(Id, NewRoom);
                }

                outcallback.Add(new HandlerCallBack(header: header, data: new RespondeRoomInfo(NewRoom.Id, NewRoom.Name, "Created room"), successfull: true, error: false));

                _webSocketService.Broadcast(_webSocketService.SCalls["RoomChange"]["header"].ToString(), "Changed: Created room", true, false);

                _webSocketService.JoinRoom(user, NewRoom);

            })).Invoke();


            return outcallback;

        }

    }
}
