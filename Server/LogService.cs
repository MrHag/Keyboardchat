using Keyboardchat.Extensions;
using System;
using System.Diagnostics;

namespace Keyboardchat
{
    public class LogService
    {
        public void Log(object obj)
        {
            string json = obj.Json();
            Log(json);
        }
        public void Log(string str)
        {
            Debug.WriteLine(str);
            Console.WriteLine(str);
        }

    }
}
