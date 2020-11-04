﻿using Newtonsoft.Json.Linq;

namespace Keyboardchat.Extensions
{
    public static class JTokenExtensions
    {

        public static JToken Make(params (string name, object data)[] properties)
        {
            JTokenWriter jTokenWriter = new JTokenWriter();
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
