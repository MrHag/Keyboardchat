using KeyBoardChat.Models;
using System.Collections.Generic;

namespace KeyBoardChat.Web.WebSocketService.Handlers
{
    public abstract class WebSocketServiceHandler
    {
        protected WebSocketService _webSocketService;

        public WebSocketService WebSocketService { get { return _webSocketService; } set { _webSocketService = value; } }
        public WebSocketServiceHandler()
        {
        }

        public abstract IEnumerable<HandlerCallBack> Handle(Connection connection);
    }
}
