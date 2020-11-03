using Keyboardchat.Models;
using System.Collections.Generic;

namespace Keyboardchat.Web.WebSocketService.Handler
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
