using Keyboardchat.Models;
using System.Collections.Generic;

namespace Keyboardchat.Web.WebSocketService.Handler
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
