using Keyboardchat.DataBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Keyboardchat.Web.WebSocketService.Handler
{
    public partial class WebSocketServiceHandler
    {
        public IEnumerable<HandlerCallBack> Registration(string header, string name, string pass)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>   
            {

                using (var dbcontext = new DatabaseContext())
                {
                    try
                    {
                        dbcontext.Users.Single(user => user.Name == name);

                        outcallback.Add(new HandlerCallBack(header: header, data: "nameExists", successfull: false, error: false));
                    }
                    catch (InvalidOperationException)
                    {

                        SHA256 sha256 = SHA256.Create();

                        var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(pass));

                        var dbUser = new DataBase.Models.User() { Name = name, PasswordHash = passwordHash };
                        dbcontext.Add(dbUser);
                        if (dbcontext.SaveChanges() > 0)
                        {
                            outcallback.Add(new HandlerCallBack(header: header, data: "Registration successful", successfull: true, error: false));
                            return;
                        }
                        else
                        {
                            outcallback.Add(new HandlerCallBack(header: header, data: "uknownError", successfull: false, error: true));
                            return;
                        }
                    }
                }

            })).Invoke();

            return outcallback;
        }

    }
}
