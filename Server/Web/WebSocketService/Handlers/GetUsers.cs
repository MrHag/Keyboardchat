using KeyBoardChat.DataBase;
using KeyBoardChat.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace KeyBoardChat.Web.WebSocketService.Handlers
{
    public class GetUsersHandler : WebSocketServiceHandler
    {
        [JsonProperty("Users", Required = Required.AllowNull)]
        public List<uint> Usersids { get; set; }

        [JsonProperty("Select")]
        public List<string> Select { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {
                Models.User currentUser;
                lock (_webSocketService._authUsers)
                {
                    currentUser = _webSocketService.GetAuthUser(connection);
                }

                List<DataBase.Models.User> dbusers = new List<DataBase.Models.User>();

                using (var dbcontext = new DatabaseContext())
                {
                    if (Usersids != null)
                    {
                        foreach (var id in Usersids)
                        {
                            try
                            {
                                var dbuser = dbcontext.Users.Single((user) => user.UserId == id);
                                dbusers.Add(dbuser);
                            }
                            catch (InvalidOperationException)
                            { }
                        }
                    }
                    else
                    {
                        dbusers.Add(dbcontext.Users.Single((user) => user.UserId == currentUser.UID));
                    }

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                        using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                        {

                            bool reqavatar = true;
                            bool reqname = true;

                            if (Select != null && Select.Count > 0 && !Select.Contains("all"))
                            {
                                reqavatar = Select.Contains("avatar");
                                reqname = Select.Contains("name");
                            }

                            jsonTextWriter.WriteStartArray();

                            foreach (var dbuser in dbusers)
                            {
                                jsonTextWriter.WriteStartObject();
                                jsonTextWriter.WritePropertyName("id");
                                jsonTextWriter.WriteValue(dbuser.UserId);

                                if (reqname)
                                {
                                    jsonTextWriter.WritePropertyName("name");
                                    jsonTextWriter.WriteValue(dbuser.Name);
                                }
                                if (reqavatar)
                                {
                                    jsonTextWriter.WritePropertyName("avatar");
                                    jsonTextWriter.WriteValue(dbuser.AvatarId);
                                }
                                jsonTextWriter.WriteEndObject();
                            }

                            jsonTextWriter.WriteEndArray();

                        }

                        string json = Encoding.UTF8.GetString(memoryStream.ToArray());

                        var jObject = JArray.Parse(json);

                        outcallback.Add(new HandlerCallBack(data: jObject));
                    }
                }

            })).Invoke();

            return outcallback;
        }

    }
}
