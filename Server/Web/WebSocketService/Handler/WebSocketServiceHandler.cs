namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        private WebSocketService _webSocketService;

        public WebSocketServiceHandler(WebSocketService socketService)
        {
            _webSocketService = socketService;
        }
    }
}
