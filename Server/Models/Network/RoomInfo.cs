namespace KeyBoardChat.Models.Network
{
    public struct RoomInfo
    {
        public int id;
        public string room;
        public bool haspass;

        public RoomInfo(int id, string room, bool haspass)
        {
            this.id = id;
            this.room = room;
            this.haspass = haspass;
        }

    }
}
