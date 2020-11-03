using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Keyboardchat
{
    public class Program
    {
        public static string CurrentPath;
        public static string FilePath;
        public static JObject API;
        public static LogService LogService;

        static Program()
        {
            CurrentPath = Environment.CurrentDirectory;

            string allText = System.IO.File.ReadAllText($"{CurrentPath}/../../../API.json");
            API = JsonConvert.DeserializeObject(allText) as JObject;
            LogService = new LogService();

            FilePath = $"{CurrentPath}/../../../public";
        }

        public static void Main(string[] args)
        {
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
