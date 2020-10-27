using Keyboardchat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Web.WebSocketService.Handler
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
