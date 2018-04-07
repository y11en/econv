using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace EProjectFile
{
	internal class ConstantInfo : IHasId
	{
		private class ConstantValueConverter:JsonConverter
		{
			public override bool CanConvert(Type objectType)
			{
				return true;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				//IL_000a: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Invalid comparison between Unknown and I4
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0023: Invalid comparison between Unknown and I4
				//IL_003c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0041: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0046: Invalid comparison between Unknown and I4
				//IL_004a: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Invalid comparison between Unknown and I4
				//IL_0054: Unknown result type (might be due to invalid IL or missing references)
				//IL_0058: Invalid comparison between Unknown and I4
				//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
				JsonToken tokenType = reader.TokenType;
				if ((int)tokenType - 7 <= 3)
				{
					return reader.Value;
				}
				if ((int)reader.TokenType != 1)
				{
					throw new Exception();
				}
				object obj = null;
				while (reader.Read())
				{
					JsonToken tokenType2 = reader.TokenType;
					if ((int)tokenType2 != 4)
					{
						if ((int)tokenType2 != 5)
						{
							if ((int)tokenType2 == 13)
							{
								return obj;
							}
							throw new Exception();
						}
						continue;
					}
					if (obj != null)
					{
						throw new Exception();
					}
					string value = (string)reader.Value;
					reader.Read();
					if ("bytes".Equals(value))
					{
						obj = new HexConverter().ReadJson(reader, typeof(byte[]), (object)null, serializer);
						continue;
					}
					if ("date".Equals(value))
					{
						obj = new IsoDateTimeConverter().ReadJson(reader, typeof(byte[]), (object)null, serializer);
						continue;
					}
					throw new Exception();
				}
				throw new Exception();
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
			{
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				if (value is byte[])
				{
					writer.WriteStartObject();
					writer.WritePropertyName("bytes");
					new HexConverter().WriteJson(writer, value, serializer);
					writer.WriteEndObject();
				}
				else if (value is DateTime)
				{
					writer.WriteStartObject();
					writer.WritePropertyName("date");
					new IsoDateTimeConverter().WriteJson(writer, value, serializer);
					writer.WriteEndObject();
				}
				else
				{
					writer.WriteValue(value);
				}
			}

			//public ConstantValueConverter()
			//	: this()
			//{
			//}
		}

		private int id;

		public int Flags;

		public string Name;

		public string Comment;

		[JsonConverter(typeof(ConstantValueConverter))]
		public object Value;

		public int Id => id;

        public uint Type;


        public ConstantInfo(int id)
		{
			this.id = id;
		}

		public static ConstantInfo[] ReadConstants(BinaryReader r)
		{
			return r.ReadBlocksWithIdAndOffest(delegate(BinaryReader reader, int id)
			{
				ConstantInfo constantInfo = new ConstantInfo(id)
				{
					Flags = reader.ReadInt16(),
					Name = reader.ReadCStyleString(),
					Comment = reader.ReadCStyleString()
				};
				uint num = (uint)id >> 28;
                constantInfo.Type = num;
				if (num != 1)
                {
					if (num - 2 <= 1)
					{
                        constantInfo.Value = reader.ReadBytesWithLengthPrefix();
						goto IL_0107;
					}
					throw new Exception();
				}
				switch (reader.ReadByte())
				{
				case 22:
					constantInfo.Value = null;
					break;
				case 23:
					constantInfo.Value = reader.ReadDouble();
					break;
				case 24:
					constantInfo.Value = (reader.ReadInt32() != 0);
					break;
				case 25:
					constantInfo.Value = DateTime.FromOADate(reader.ReadDouble());
					break;
				case 26:
				{
					int num2 = (int)reader.BaseStream.Position + reader.ReadInt32() + 4;
					constantInfo.Value = reader.ReadCStyleString();
					reader.BaseStream.Position = num2;
					break;
				}
				default:
					throw new Exception();
				}
				goto IL_0107;
				IL_0107:
				return constantInfo;
			});
		}

		public static void WriteConstants(BinaryWriter w, ConstantInfo[] constants)
		{
			w.WriteBlocksWithIdAndOffest(constants, delegate(BinaryWriter writer, ConstantInfo elem)
			{
				writer.Write((short)elem.Flags);
				writer.WriteCStyleString(elem.Name);
				writer.WriteCStyleString(elem.Comment);
				if (elem.Value is byte[])
				{
					writer.WriteBytesWithLengthPrefix((byte[])elem.Value);
					return;
				}
				if (elem.Value == null)
				{
					writer.Write((byte)22);
					return;
				}
				if (elem.Value is double)
				{
					writer.Write((byte)23);
					writer.Write((double)elem.Value);
					return;
				}
				if (elem.Value is bool)
				{
					writer.Write((byte)24);
					writer.Write(((bool)elem.Value) ? 1 : 0);
					return;
				}
				if (elem.Value is DateTime)
				{
					writer.Write((byte)25);
					writer.Write(((DateTime)elem.Value).ToOADate());
					return;
				}
				if (elem.Value is string)
				{
					byte[] data = default(byte[]);
					using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
					{
						binaryWriter.WriteCStyleString((string)elem.Value);
						data = ((MemoryStream)binaryWriter.BaseStream).ToArray();
					}
					writer.Write((byte)26);
					writer.WriteBytesWithLengthPrefix(data);
					return;
				}
				throw new Exception();
			});
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
