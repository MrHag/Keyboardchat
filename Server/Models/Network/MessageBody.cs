namespace KeyBoardChat.Models.Network
{
    public class MessageBody
    {
        public uint userid { get; private set; }
        public string userName { get; private set; }
        public int roomid { get; private set; }
        public string message { get; private set; }
        public MessageBody(uint userid, string userName, int roomid, string message)
        {
            this.userid = userid;
            this.userName = userName;
            this.roomid = roomid;
            this.message = message;
        }

    }
}
