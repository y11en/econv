using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EProjectFile
{
	public class HexConverter:JsonConverter
	{
		public static byte[] HexToBytes(string src)
		{
			byte[] array = new byte[src.Length / 2];
			int num = 0;
			int num2 = 0;
			while (num < src.Length)
			{
				array[num2] = Convert.ToByte(src.Substring(num, 2), 16);
				num += 2;
				num2++;
			}
			return array;
		}

		public static string BytesToHex(byte[] bytes)
		{
			string text = "";
			if (bytes != null)
			{
				for (int i = 0; i < bytes.Length; i++)
				{
					text += bytes[i].ToString("X2");
				}
			}
			return text;
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				byte[] bytes = (byte[])value;
				writer.WriteValue(BytesToHex(bytes));
			}
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Invalid comparison between Unknown and I4
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Invalid comparison between Unknown and I4
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Invalid comparison between Unknown and I4
			if ((int)reader.TokenType == 11)
			{
				return null;
			}
			byte[] result;
			if ((int)reader.TokenType == 2)
			{
				result = ReadByteArray(reader);
				goto IL_005d;
			}
			if ((int)reader.TokenType == 9)
			{
				string src = reader.Value.ToString();
				result = HexToBytes(src);
				goto IL_005d;
			}
			throw new Exception();
			IL_005d:
			return result;
		}

		private byte[] ReadByteArray(JsonReader reader)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Invalid comparison between Unknown and I4
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Invalid comparison between Unknown and I4
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			List<byte> list = new List<byte>();
			while (reader.Read())
			{
				JsonToken tokenType = reader.TokenType;
				if ((int)tokenType != 5)
				{
					if ((int)tokenType != 7)
					{
						if ((int)tokenType == 14)
						{
							return list.ToArray();
						}
						throw new Exception();
					}
					list.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
				}
			}
			throw new Exception();
		}

		public override bool CanConvert(Type objectType)
		{
			return objectType.IsAssignableFrom(typeof(byte[]));
		}

		//public HexConverter()
		//	: this()
		//{
		//}
	}
}
