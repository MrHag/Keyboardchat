using KeyBoardChat.DataBase;
using KeyBoardChat.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace KeyBoardChat.Web.WebSocketService.Handlers
{
    public class RegistrationHandler : WebSocketServiceHandler
    {

        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty("password", Required = Required.Always)]
        public string Password { get; set; }

        public override IEnumerable<HandlerCallBack> Handle(Connection connection)
        {
            var outcallback = new List<HandlerCallBack>();

            ((Action)(() =>
            {

                using (var dbcontext = new DatabaseContext())
                {
                    try
                    {
                        _ = dbcontext.Users.Single(user => user.Name == Name);

                        outcallback.Add(new HandlerCallBack(data: "nameExists", error: true));
                    }
                    catch (InvalidOperationException)
                    {

                        SHA256 sha256 = SHA256.Create();

                        var passwordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(Password));

                        var dbUser = new DataBase.Models.User() { Name = Name, PasswordHash = passwordHash };
                        dbcontext.Add(dbUser);
                        if (dbcontext.SaveChanges() > 0)
                        {
                            outcallback.Add(new HandlerCallBack(data: "Registration successful"));
                            return;
                        }
                        else
                        {
                            outcallback.Add(new HandlerCallBack(data: "uknownError", error: true));
                            return;
                        }
                    }
                }

            })).Invoke();

            return outcallback;
        }
    }
}
