using Keyboardchat.Models;
using Keyboardchat.Models.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public class GetRoomsHandler : WebSocketServiceHandler
    {
        [JsonProperty("room", Required = Required.AllowNull)]
        public string RoomName { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();


            ((Action)(() =>
            {

                List<(Room room, int qual)> rooms = new List<(Room room, int qual)>();

                var webRooms = _webSocketService.Rooms;
                lock (webRooms)
                {
                    foreach (var room in webRooms)
                        rooms.Add((room.Value, 0));
                }


                if (RoomName != null)
                {

                    for (int key = 0; key < rooms.Count; key++)
                    {
                        var room = rooms[key];

                        var firstPeace = "";
                        var secondPeace = "";
                        decimal minQual = Math.Ceiling(RoomName.Length * (40 / 100M));

                        for (int i = RoomName.Length; i > 0; i--)
                        {
                            firstPeace = RoomName.Substring(0, i);
                            secondPeace = RoomName.Substring(RoomName.Length - i);

                            string roomName = room.room.Name;

                            bool PeaceMatch(string peace)
                            {
                                if (Regex.IsMatch(roomName, $"(?i)({peace})+"))
                                {
                                    room.qual = peace.Length;
                                    rooms[key] = room;
                                    return true;
                                }
                                return false;
                            }

                            if (PeaceMatch(firstPeace) || PeaceMatch(secondPeace))
                                break;
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
                    bool haspass = room.room.Password != "";
                    outrooms.Add(new RoomInfo(room.room.Id, room.room.Name, haspass));
                }

                outcallback.Add(new HandlerCallBack(data: outrooms, error: false));
            })).Invoke();


            return outcallback;

        }
    }
}
