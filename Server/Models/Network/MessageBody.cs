namespace KeyBoardChat.Models.Network
{
    public class MessageBody
    {
        public uint userid { get; private set; }
        public int roomid { get; private set; }
        public string message { get; private set; }
        public MessageBody(uint userid, int roomid, string message)
        {
            this.userid = userid;
            this.roomid = roomid;
            this.message = message;
        }

    }
}
