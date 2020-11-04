using EngineIOSharp.Client;
using Keyboardchat;
using Keyboardchat.Extensions;
using Keyboardchat.Web.WebSocketService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocketIOSharp.Client;
using SocketIOSharp.Common;
using SocketIOSharp.Server;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnitTest
{
    [TestClass]
    public class WebsocketTest
    {
        public delegate void TestWSDelegate(CancellationTokenSource ctoken, ref bool succ);
        private SocketIOClient _client;
        private WebSocketService webSocketService;

        private List<string> authHeaders = new List<string>() { "deAuth", "changeProfile", "getUsers", "getRooms", "sendMsg", "joinRoom", "leaveRoom", "createRoom" };


        [TestMethod]
        public void GlobalTest()
        {
            Program.Init($"{Environment.CurrentDirectory}/../../../..");

            _client = new SocketIOClient(new SocketIOClientOption(EngineIOSharp.Common.Enum.EngineIOScheme.http, "127.0.0.1", 4001));

            webSocketService = new WebSocketService();


            Start();
            NotauthRequests();
            authRequest();
            messageRequest();

        }

        public void Start()
        {
            webSocketService.Start();
            _client.Connect();
            Assert.IsTrue(_client.ReadyState == EngineIOSharp.Common.Enum.EngineIOReadyState.OPEN || _client.ReadyState == EngineIOSharp.Common.Enum.EngineIOReadyState.OPENING);
        }


        public JToken WaitData(string sendHeader, string inputHeader = null, object data = null)
        {
            if (inputHeader == null)
                inputHeader = sendHeader;

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            var token = cancellationTokenSource.Token;

            JToken outData = null;

            _client.On(inputHeader, (JToken[] data) =>
            {
                try
                {
                    outData = data[0];
                }
                finally
                {
                    cancellationTokenSource.Cancel();
                }
            });

            if (data != null)
                _client.Emit(sendHeader, data);
            else
                _client.Emit(sendHeader);

            try
            {
                Task.Delay(10000, token).Wait();
            }
            catch (Exception)
            { }

            if (!token.IsCancellationRequested)
                Assert.Fail();

            return outData;
        }

        public void NotauthRequests()
        {
            foreach (var header in authHeaders)
            {
                Assert.AreEqual(WaitData(header, "access")["error"], "notAuth");
            }
        }


        public void authRequest()
        {
            var header = "auth";

            JToken MakeStr(string name, string pass)
            {
                return JTokenExtensions.Make(("name", name), ("password", pass));
            }

            JToken sendData;
            string waitedData;

            waitedData = WaitData(header)["error"].ToString();
            Assert.AreEqual(waitedData, "invalidData");

            var stringbuilder = new StringBuilder();
            stringbuilder.Append('0', 33);

            sendData = MakeStr(stringbuilder.ToString(), "p");
            waitedData = WaitData(header, data: sendData)["error"].ToString();
            Assert.AreEqual(waitedData, "badName");

            stringbuilder.Clear();
            stringbuilder.Append('0', 65);

            sendData = MakeStr("MrHag", stringbuilder.ToString());
            waitedData = WaitData(header, data: sendData)["error"].ToString();
            Assert.AreEqual(waitedData, "badPassword");


            sendData = MakeStr("MrHag", "invalidPass");
            waitedData = WaitData(header, data: sendData)["error"].ToString();
            Assert.AreEqual(waitedData, "wrongNamePass");


            sendData = MakeStr("MrHag", "pass");
            waitedData = WaitData(header, data: sendData)["error"].ToString();
            Assert.AreEqual(waitedData, "");


            sendData = MakeStr("MrHag", "pass");
            waitedData = WaitData(header, data: sendData)["error"].ToString();
            Assert.AreEqual(waitedData, "alreadyInSess");

        }

        public void messageRequest()
        {
            var header = "sendMsg";

            var message = "mess";
            JToken sendData;
            JToken waitedData;

            waitedData = WaitData(header)["error"].ToString();
            Assert.AreEqual(waitedData, "invalidData");

            sendData = JTokenExtensions.Make(("message", message));
            waitedData = WaitData(header, inputHeader: "onNewMsg", data: sendData);
            Assert.AreNotEqual(waitedData["userid"], null);
            Assert.AreNotEqual(waitedData["roomid"], null);
            Assert.AreEqual(waitedData["message"], message);

        }

    }
}
