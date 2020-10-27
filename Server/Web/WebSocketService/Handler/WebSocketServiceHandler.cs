using Keyboardchat.DataBase;
using Keyboardchat.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Keyboardchat.Web.WebSocketService.Handler
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
