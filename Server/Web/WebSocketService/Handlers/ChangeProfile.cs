using KeyBoardChat.DataBase;
using KeyBoardChat.DataBase.Models;
using KeyBoardChat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KeyBoardChat.Web.WebSocketService.Handlers
{
    public class ChangeProfileHandler : WebSocketServiceHandler
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        private object locker = new object();

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();

            byte[] bytes = null;

            ((Action)(() =>
            {
                if (Avatar != null)
                {
                    try
                    {
                        bytes = Convert.FromBase64String(Avatar);
                    }
                    catch (Exception)
                    {
                        outcallback.Add(new HandlerCallBack(data: "invalidData", error: true));
                        return;
                    }
                }

                var user = connection.Session.User;


                using (var dbcontext = new DatabaseContext())
                {
                    lock (locker)
                    {
                        try
                        {
                            _ = dbcontext.Users.Single(user => user.Name == Name);

                            outcallback.Add(new HandlerCallBack(data: "nameExists", error: true));
                            return;
                        }
                        catch (InvalidOperationException)
                        {
                            var dbuser = dbcontext.Users.Single(dbuser => dbuser.UserId == user.UID);

                            if (Name != null)
                            {
                                dbuser.Name = Name;
                            }

                            if (bytes != null)
                            {
                                using (MemoryStream ms = new MemoryStream(bytes))
                                {
                                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);

                                    if (bitmap.Width > 128 || bitmap.Height > 128)
                                    {
                                        outcallback.Add(new HandlerCallBack(data: "badImage", error: true));
                                        return;
                                    }

                                    Avatar dbAvatar;

                                    try
                                    {
                                        dbAvatar = dbcontext.Avatars.Single(avatar => avatar.Id == dbuser.AvatarId);

                                        if (dbAvatar.Id == 2)
                                        {
                                            dbAvatar = new Avatar();

                                            dbcontext.Avatars.Add(dbAvatar);
                                        }

                                        dbAvatar.AvatarData = bytes;

                                        dbcontext.SaveChanges();

                                        dbuser.AvatarId = dbAvatar.Id;

                                    }
                                    catch (InvalidOperationException)
                                    {      
                                    }

                                }

                            }

                            outcallback.Add(new HandlerCallBack(data: "Data changed"));

                        }
                    }
                }

            })).Invoke();

            return outcallback;
        }
    }
}
