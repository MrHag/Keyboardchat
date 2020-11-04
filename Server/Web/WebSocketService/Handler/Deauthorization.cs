using System.Collections.Generic;
using KeyBoardChat.Models;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public class DeauthorizationHandler : WebSocketServiceHandler
    {

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();

            connection.Session.RemoveConnection(connection);

            return outcallback;
        }
    }
}
