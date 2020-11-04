using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

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
