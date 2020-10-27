using Keyboardchat.DataBase;
using Keyboardchat.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> GetUsers(Connection connection, string header, List<uint> userids, List<string> select)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {

                User currentUser = _webSocketService.GetAuthUser(connection);

                List<DataBase.Models.User> dbusers = new List<DataBase.Models.User>();

                using (var dbcontext = new DatabaseContext())
                {
                    if (userids != null)
                    {
                        foreach (var id in userids)
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
                }
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (StreamWriter streamWriter = new StreamWriter(memoryStream))
                    using (JsonTextWriter jsonTextWriter = new JsonTextWriter(streamWriter))
                    {

                        bool reqavatar = true;
                        bool reqavatarHash = true;
                        bool reqname = true;

                        if (select != null && select.Count > 0)
                        {
                            reqavatar = select.Contains("avatar");
                            reqavatarHash = select.Contains("avatarHash");
                            reqname = select.Contains("name");
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
                                byte[] dbavatar = dbuser.Avatar;
                                string avatar = "";


                                if (dbavatar != null)
                                {
                                    avatar = Convert.ToBase64String(dbavatar);
                                }
                                jsonTextWriter.WritePropertyName("avatar");
                                jsonTextWriter.WriteValue(avatar);
                            }
                            if (reqavatarHash)
                            {
                                byte[] dbavatarHash = dbuser.Avatar;
                                string avatarHash = "";


                                if (dbavatarHash != null)
                                {
                                    avatarHash = Convert.ToBase64String(dbavatarHash);
                                }

                                jsonTextWriter.WritePropertyName("avatarHash");
                                jsonTextWriter.WriteValue(avatarHash);
                                jsonTextWriter.WriteEndObject();
                            }
                        }

                        jsonTextWriter.WriteEndArray();

                    }

                    string json = Encoding.UTF8.GetString(memoryStream.ToArray());

                    var jObject = JObject.Parse(json);

                    outcallback.Add(new HandlerCallBack(header: header, data: jObject, successfull: true, error: false));
                }

            })).Invoke();

            return outcallback;
        }

    }
}
