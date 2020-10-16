using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Keyboardchat.Extensions
{
    public static class JTokenExtensions
    {

        public static List<JToken> GetValues(this JToken[] jTokens, int endindex, out List<JToken> output)
        {

            List<JToken> outObjects = new List<JToken>(jTokens.Length);

            if (jTokens.Length <= endindex)
            {
                output = null;
                return null;
            }

            for (int i = 0; i <= endindex; i++)
            {
                var token = jTokens[i];
                if (token == null)
                {
                    output = null;
                    return null;
                }
                else
                    outObjects.Add(token);
            }

            output = outObjects;

            return outObjects;
        }

        public static object GetValue<T>(this JToken jToken, out T output, object value)
        {
            object outdata;
            try
            {
                outdata = jToken[value].Value<T>();
            }
            catch (Exception)
            {
                outdata = null;
            }

            if (outdata != null && outdata.GetType() != typeof(T))
                outdata = null;

            output = (T)outdata;

            return outdata;

        }

    }
}
