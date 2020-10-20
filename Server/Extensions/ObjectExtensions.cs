using Newtonsoft.Json;

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
