using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using KeyBoardChat.DataBase;
using KeyBoardChat.Models;

namespace KeyBoardChat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> ChangeProfile(string header, User user, string name, byte[] avatar)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {
                lock (user)
                {
                    using (var dbcontext = new DatabaseContext())
                    {
                        try
                        {
                            string userName = user.Name;
                            lock (userName)
                            {
                                dbcontext.Users.Single(user => userName == name);
                            }

                            outcallback.Add(new HandlerCallBack(header: header, data: "nameExists", successfull: false, error: false));
                            return;
                        }
                        catch (InvalidOperationException)
                        {
                            var dbuser = dbcontext.Users.Single(dbuser => dbuser.UserId == user.UID);

                            if (name != null)
                            {
                                dbuser.Name = name;
                            }

                            if (avatar != null)
                            {
                                using (MemoryStream ms = new MemoryStream(avatar))
                                {
                                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(ms);

                                    if (bitmap.Width > 128 || bitmap.Height > 128)
                                    {
                                        outcallback.Add(new HandlerCallBack(header: header, data: "badImage", successfull: false, error: false));
                                        return;
                                    }

                                    dbuser.Avatar = avatar;

                                    var sha256 = SHA256.Create();
                                    var avatarHash = sha256.ComputeHash(dbuser.Avatar);

                                    dbuser.AvatarHash = avatarHash;
                                }

                            }

                            dbcontext.SaveChanges();

                            outcallback.Add(new HandlerCallBack(header: header, data: "Data changed", successfull: true, error: false));

                        }
                    }
                }

            })).Invoke();

            return outcallback;
        }
    }
}
