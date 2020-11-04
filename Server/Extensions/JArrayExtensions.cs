using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KeyBoardChat.Extensions
{
    public static class JArrayExtensions
    {

        public static JArray Make(params object[] data)
        {
            JTokenWriter jTokenWriter = new JTokenWriter();
            jTokenWriter.WriteStartArray();

            foreach (var variable in data)
            {
                jTokenWriter.WriteValue(variable);
            }

            jTokenWriter.WriteEndArray();

            return jTokenWriter.Token as JArray;
        }

    }
}
