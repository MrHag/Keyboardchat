using Keyboardchat.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

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
