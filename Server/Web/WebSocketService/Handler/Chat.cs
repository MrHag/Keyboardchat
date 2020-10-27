using Keyboardchat.Models;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {

        public void Chat(Room room, string message, uint id)
        {
           _webSocketService.SendChatMessage(room, message, id);
        }

    }
}
