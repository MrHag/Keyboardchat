namespace KeyBoardChat.Web.WebSocketService
{
    public class HandlerCallBack
    {
        public string Header { get; set; }
        public object Data { get; set; }
        public bool Successfull { get; set; }
        public bool Error { get; set; }

        public HandlerCallBack(string header, object data, bool successfull, bool error)
        {
            Header = header;
            Data = data;
            Successfull = successfull;
            Error = error;
        }

    }
}
