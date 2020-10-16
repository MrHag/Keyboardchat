using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Keyboardchat.Extensions
{
    public static class ObjectExtensions
    {
        public static string Json<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

    }
}
