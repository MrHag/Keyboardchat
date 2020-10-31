using Newtonsoft.Json;

namespace KeyBoardChat.Extensions
{
    public static class ObjectExtensions
    {
        public static string Json<T>(this T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

    }
}
