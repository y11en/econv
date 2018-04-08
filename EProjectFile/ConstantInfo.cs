using System;
using System.IO;

namespace EProjectFile
{
	public class ConstantInfo : IHasId
	{
		private int id;

		public int Flags;

		public string Name;

		public string Comment;

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
	}
}
