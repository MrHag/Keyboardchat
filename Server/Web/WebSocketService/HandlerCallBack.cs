namespace Keyboardchat.Web.WebSocketService
{
    public class HandlerCallBack
    {
        public object Data { get; set; }
        public bool Error { get; set; }

        public HandlerCallBack(object data, bool error)
        {
            Data = data;
            Error = error;
        }

    }
}
