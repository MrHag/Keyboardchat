using System;
using System.Drawing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KeyBoardChat
{
    public class Program
    {
        public static string CurrentPath;
        public static string FilePath;
        public static JObject API;
        public static LogService LogService;
        public static byte[] _serverAvatar;
        public static byte[] _unkownAvatar;

        public static void Init(string directory = null)
        {
            if (directory == null)
                Environment.CurrentDirectory = $"{Environment.CurrentDirectory}/../../..";
            else
                Environment.CurrentDirectory = directory;

            CurrentPath = Environment.CurrentDirectory;

            string allText = System.IO.File.ReadAllText($"{CurrentPath}/API.json");
            API = JsonConvert.DeserializeObject(allText) as JObject;
            LogService = new LogService();

            FilePath = $"{CurrentPath}/public";

            var avatarsPath = $"{CurrentPath}/Server/avatars";

            _serverAvatar = System.IO.File.ReadAllBytes($"{avatarsPath}/server.png");
            _unkownAvatar = System.IO.File.ReadAllBytes($"{avatarsPath}/uknown.png");


        }

        public static void Main(string[] args)
        {
            Init();
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
