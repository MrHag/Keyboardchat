using KeyBoardChat.DataBase;
using KeyBoardChat.DataBase.Models;
using KeyBoardChat.Web.WebSocketService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;


namespace KeyBoardChat
{
    public class Startup
    {

        private WebSocketService WebSocketService;

        public Startup()
        {

            using (DatabaseContext dbcontext = new DatabaseContext())
            {
                Avatar serverAvatar = new Avatar() { Id = 1, AvatarData = Program._serverAvatar };
                Avatar uknownAvatar = new Avatar() { Id = 2, AvatarData = Program._unkownAvatar };
                dbcontext.Avatars.Add(serverAvatar);
                dbcontext.Avatars.Add(uknownAvatar);

                try
                {
                    dbcontext.SaveChanges();
                }
                catch (Exception)
                { }
            }

        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/avatar/{avatarid}", async (context) =>
                {
                    uint avatarid;

                    if (!uint.TryParse(context.Request.RouteValues["avatarid"].ToString(), out avatarid))
                    {
                        return;
                    }

                    using (var dbcontext = new DatabaseContext())
                    {
                        try
                        {
                            var dbavatar = dbcontext.Avatars.Single(avatar => avatar.Id == avatarid);
                            await context.Response.BodyWriter.WriteAsync(dbavatar.AvatarData);
                        }
                        catch (InvalidOperationException ex)
                        {
#if DEBUG
                            Program.LogService.Log(ex);
#endif
                        }
                    }
                });
            });

            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(Program.FilePath),
            });

            using (WebSocketService)
            {
                WebSocketService = new WebSocketService();
                try
                {
                    WebSocketService.Start();
                }
                catch (InvalidOperationException ex)
                {
                    Program.LogService.Log(ex);
                }
            }
        }
    }
}
