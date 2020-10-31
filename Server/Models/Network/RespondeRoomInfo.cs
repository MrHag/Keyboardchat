
namespace KeyBoardChat.Models.Network
{
    public struct RespondeRoomInfo
    {
        public int id;
        public string room;
        public string message;

        public RespondeRoomInfo(int id, string room, string message)
        {
            this.id = id;
            this.room = room;
            this.message = message;
        }
    }
}
