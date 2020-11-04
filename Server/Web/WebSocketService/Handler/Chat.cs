using Keyboardchat.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public class ChatHandler : WebSocketServiceHandler
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            _webSocketService.SendChatMessage(connection.Session.Room, Message, connection.Session.User.UID);
            return null;
        }

    }
}
