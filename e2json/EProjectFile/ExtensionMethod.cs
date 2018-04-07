using System;
using System.IO;
using System.Linq;
using System.Text;

namespace EProjectFile
{
	internal static class ExtensionMethod
	{
		public static byte[] ReadBytesWithLengthPrefix(this BinaryReader reader)
		{
			return reader.ReadBytes(reader.ReadInt32());
		}

		public static string ReadStringWithFixedLength(this BinaryReader reader, int length)
		{
			byte[] array = reader.ReadBytes(length);
			int num = Array.IndexOf(array, (byte)0);
			if (num != -1)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, array2, num);
				array = array2;
			}
			return Encoding.GetEncoding("gbk").GetString(array);
		}

		public static string ReadStringWithLengthPrefix(this BinaryReader reader)
		{
			return reader.ReadStringWithFixedLength(reader.ReadInt32());
		}

		public static string ReadCStyleString(this BinaryReader reader)
		{
			MemoryStream memoryStream = new MemoryStream();
			byte value;
			while ((value = reader.ReadByte()) != 0)
			{
				memoryStream.WriteByte(value);
			}
			return Encoding.GetEncoding("gbk").GetString(memoryStream.ToArray());
		}

		public static int ReadMfcStyleCountPrefix(this BinaryReader reader)
		{
			ushort num = reader.ReadUInt16();
			if (num != 65535)
			{
				return num;
			}
			return reader.ReadInt32();
		}

		public static string[] ReadStringsWithMfcStyleCountPrefix(this BinaryReader reader)
		{
			return (from x in new object[reader.ReadMfcStyleCountPrefix()]
			select reader.ReadStringWithLengthPrefix()).ToArray();
		}

		public static TElem[] ReadBlocksWithIdAndOffest<TElem>(this BinaryReader reader, Func<BinaryReader, int, TElem> readFunction)
		{
			return reader.ReadBlocksWithIdAndOffest((BinaryReader elemReader, int id, int length) => readFunction(elemReader, id));
		}

		public static TElem[] ReadBlocksWithIdAndOffest<TElem>(this BinaryReader reader, 
            Func<BinaryReader, int, int, TElem> readFunction)
		{
			int num = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			long position = reader.BaseStream.Position + num2;
			TElem[] array = new TElem[num];
			int[] array2 = reader.ReadInt32sWithFixedLength(num);
			int[] array3 = reader.ReadInt32sWithFixedLength(num);
			long position2 = reader.BaseStream.Position;
			for (int i = 0; i < num; i++)
			{
				reader.BaseStream.Position = position2 + array3[i];
				int arg = reader.ReadInt32();
				array[i] = readFunction(reader, array2[i], arg);
			}
			reader.BaseStream.Position = position;
			return array;
		}

		public static void WriteBlocksWithIdAndOffest<TElem>(this BinaryWriter writer, TElem[] data, Action<BinaryWriter, TElem> writeAction) where TElem : IHasId
		{
			int num = data.Length;
			byte[][] array = new byte[num][];
			for (int i = 0; i < num; i++)
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
				{
					writeAction(binaryWriter, data[i]);
					array[i] = ((MemoryStream)binaryWriter.BaseStream).ToArray();
				}
				using (BinaryWriter binaryWriter2 = new BinaryWriter(new MemoryStream()))
				{
					binaryWriter2.Write(array[i].Length);
					binaryWriter2.Write(array[i]);
					array[i] = ((MemoryStream)binaryWriter2.BaseStream).ToArray();
				}
			}
			int[] array2 = new int[num];
			if (num > 0)
			{
				array2[0] = 0;
			}
			for (int j = 1; j < num; j++)
			{
				array2[j] = array2[j - 1] + array[j - 1].Length;
			}
			writer.Write(num);
			writer.Write(num * 8 + array.Sum((byte[] x) => x.Length));
			Array.ForEach(data, delegate(TElem x)
			{
				writer.Write(x.Id);
			});
			writer.WriteInt32sWithoutLengthPrefix(array2);
			Array.ForEach(array, delegate(byte[] x)
			{
				writer.Write(x);
			});
		}

		public static int[] ReadInt32sWithFixedLength(this BinaryReader reader, int count)
		{
			return (from x in new object[count]
			select reader.ReadInt32()).ToArray();
		}

		public static int[] ReadInt32sWithLengthPrefix(this BinaryReader reader)
		{
			return reader.ReadInt32sWithFixedLength(reader.ReadInt32());
		}

		public static void WriteBytesWithLengthPrefix(this BinaryWriter writer, byte[] data)
		{
			writer.Write(data.Length);
			writer.Write(data);
		}

		public static void WriteInt32sWithoutLengthPrefix(this BinaryWriter writer, int[] data)
		{
			Array.ForEach(data, delegate(int x)
			{
				writer.Write(x);
			});
		}

		public static void WriteInt32sWithLengthPrefix(this BinaryWriter writer, int[] data)
		{
			writer.Write(data.Length);
			writer.WriteInt32sWithoutLengthPrefix(data);
		}

		public static void WriteStringWithLengthPrefix(this BinaryWriter writer, string data)
		{
			writer.WriteBytesWithLengthPrefix(Encoding.GetEncoding("gbk").GetBytes(data));
		}

		public static void WriteMfcStyleCountPrefix(this BinaryWriter writer, int data)
		{
			if (data < 65535)
			{
				writer.Write((ushort)data);
			}
			else
			{
				writer.Write(ushort.MaxValue);
				writer.Write(data);
			}
		}

		public static void WriteStringsWithMfcStyleCountPrefix(this BinaryWriter writer, string[] data)
		{
			writer.WriteMfcStyleCountPrefix(data.Length);
			Array.ForEach(data, delegate(string x)
			{
				writer.WriteStringWithLengthPrefix(x);
			});
		}

		public static void WriteCStyleString(this BinaryWriter writer, string data)
		{
			writer.Write(Encoding.GetEncoding("gbk").GetBytes(data));
			writer.Write((byte)0);
		}

		public static string ToHexString(this byte[] bytes)
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
	}
}
