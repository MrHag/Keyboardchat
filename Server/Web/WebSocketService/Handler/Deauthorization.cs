using System.Collections.Generic;
using KeyBoardChat.Models;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> Deauthorization(string header, Connection SocketConnection)
        {
            var outcallback = new List<HandlerCallBack>();

            DisconnectAction(SocketConnection, false);
            outcallback.Add(new HandlerCallBack(header: header, "Deaunthentication successful", true, false));

            return outcallback;
        }

    }
}
