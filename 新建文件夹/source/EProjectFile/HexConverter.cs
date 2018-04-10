using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Newtonsoft.Json.Converters;
using System.Globalization;

namespace EProjectFile
{
    public class HexConverter : JsonConverter
    {
        public static byte[] HexToBytes(String src)
        {
            byte[] result = new byte[src.Length / 2];
            for (int i = 0, c = 0; i < src.Length; i += 2, c++)
            {
                result[c] = Convert.ToByte(src.Substring(i, 2), 16);
            }
            return result;
        }

        public static string BytesToHex(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            var data = (byte[])value;
            writer.WriteValue(BytesToHex(data));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }
            byte[] data;
            if (reader.TokenType == JsonToken.StartArray)
            {
                data = ReadByteArray(reader);
            }
            else if (reader.TokenType == JsonToken.String)
            {
                string encodedData = reader.Value.ToString();
                data = HexToBytes(encodedData);
            }
            else
            {
                throw new Exception();
            }
            return data;
            
        }
        private byte[] ReadByteArray(JsonReader reader)
        {
            List<byte> byteList = new List<byte>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                        byteList.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.EndArray:
                        return byteList.ToArray();
                    case JsonToken.Comment:
                        break;
                    default:
                        throw new Exception();
                }
            }
            throw new Exception();
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(byte[]));
        }
    }
}

