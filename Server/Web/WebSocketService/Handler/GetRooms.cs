using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using KeyBoardChat.Models;
using KeyBoardChat.Models.Network;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> GetRooms(string header, string roomname)
        {
            var outcallback = new List<HandlerCallBack>();


            ((Action)(() =>
            {

                List<(Room room, int qual)> rooms = new List<(Room room, int qual)>();

                lock (_webSocketService._rooms)
                {
                    foreach (var room in _webSocketService._rooms.Values)
                        rooms.Add((room, 0));
                }


                if (roomname != null)
                {

                    for (int key = 0; key < rooms.Count; key++)
                    {
                        var room = rooms[key];

                        var firstPeace = "";
                        var secondPeace = "";
                        decimal minQual = Math.Ceiling(roomname.Length * (40 / 100M));

                        for (int i = roomname.Length; i > 0; i--)
                        {
                            firstPeace = roomname.Substring(0, i);
                            secondPeace = roomname.Substring(roomname.Length-i);

                            string roomName = room.room.Name;
                            lock (roomName)
                            {

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
                    string roomPassword = room.room.Password;

                    bool haspass;
                    lock (roomPassword)
                    {
                        haspass = roomPassword != "";
                    }

                    outrooms.Add(new RoomInfo(room.room.Id, room.room.Name, haspass));
                }

                outcallback.Add(new HandlerCallBack(header: header, data: outrooms, successfull: true, error: false));
            })).Invoke();


            return outcallback;

        }
    }
}
