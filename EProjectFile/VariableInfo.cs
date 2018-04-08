using Newtonsoft.Json;
using System.IO;

namespace EProjectFile
{
	public class VariableInfo : IHasId
	{
		private int id;

		public int DataType;

		public int Flags;

		public int[] UBound;

		public string Name;

		public string Comment;

		public int Id => id;

        public string TypeName;

		public VariableInfo(int id)
		{
			this.id = id;
		}

		public static VariableInfo[] ReadVariables(BinaryReader r)
		{
			return r.ReadBlocksWithIdAndOffest((BinaryReader reader, int id) => {
                var v = new VariableInfo(id);
                v.DataType = reader.ReadInt32();
                v.Flags = reader.ReadInt16();
                v.UBound = reader.ReadInt32sWithFixedLength(reader.ReadByte());
                v.Name = reader.ReadCStyleString();
                v.Comment = reader.ReadCStyleString();
                switch ((uint)v.DataType)
                {
                    case 0x80000101: v.TypeName = "字节型"; break;
                    case 0x80000201: v.TypeName = "短整数型"; break;
                    case 0x80000301: v.TypeName = "整数型"; break;
                    case 0x80000401: v.TypeName = "长整数型"; break;
                    case 0x80000501: v.TypeName = "小数型"; break;
                    case 0x80000601: v.TypeName = "双精度小数型"; break;
                    case 0x80000002: v.TypeName = "逻辑型"; break;
                    case 0x80000003: v.TypeName = "日期时间型"; break;
                    case 0x80000004: v.TypeName = "文本型"; break;
                    case 0x80000005: v.TypeName = "字节集型"; break;
                    case 0x80000006: v.TypeName = "子程序指针型"; break;
                    case 0: v.TypeName = ""; break;
                    default: v.TypeName = "-Unknown-"; break;
                }
                return v;
            });
		}

		public static void WriteVariables(BinaryWriter w, VariableInfo[] variables)
		{
			w.WriteBlocksWithIdAndOffest(variables, delegate(BinaryWriter writer, VariableInfo elem)
			{
				writer.Write(elem.DataType);
				writer.Write((short)elem.Flags);
				writer.Write((byte)elem.UBound.Length);
				writer.WriteInt32sWithoutLengthPrefix(elem.UBound);
				writer.WriteCStyleString(elem.Name);
				writer.WriteCStyleString(elem.Comment);
			});
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
