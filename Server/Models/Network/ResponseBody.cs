namespace Keyboardchat.Models.Network
{
    public class ResponseBody
    {
        public object data { get; private set; } 

        public bool successful { get; private set; }

        public bool error { get; private set; }
        public ResponseBody(object data, bool successful, bool error)
        {
            this.data = data;
            this.successful = successful;
            this.error = error;
        }
    }
}
