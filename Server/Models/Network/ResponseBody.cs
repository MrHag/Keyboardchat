namespace KeyBoardChat.Models.Network
{
    public class ResponseBody
    {
        public object data { get; private set; }
        public object error { get; private set; }
        public ResponseBody(object data, object error)
        {
            this.data = data;
            this.error = error;
        }
    }
}
