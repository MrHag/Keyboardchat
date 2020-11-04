using Newtonsoft.Json.Linq;

namespace KeyBoardChat.Extensions
{
    public static class JTokenExtensions
    {

        public static JToken Make(params (string name, object data)[] properties)
        {
            using (JTokenWriter jTokenWriter = new JTokenWriter())
            {
                jTokenWriter.WriteStartObject();

                foreach (var property in properties)
                {
                    jTokenWriter.WritePropertyName(property.name);
                    jTokenWriter.WriteValue(property.data);
                }

                jTokenWriter.WriteEndObject();

                return jTokenWriter.Token;
            }
        }

    }
}
