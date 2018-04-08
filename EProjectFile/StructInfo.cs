using Newtonsoft.Json;
using System;
using System.IO;

namespace EProjectFile
{
	public class StructInfo : IHasId
	{
		private int id;

		[JsonIgnore]
		public int UnknownAfterId;

		public int Flags;

		public string Name;

		public string Comment;

		public VariableInfo[] Member;

		public int Id => id;

		public StructInfo(int id)
		{
			this.id = id;
		}

		public static StructInfo[] ReadStructs(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = num / 8;
			int[] array = reader.ReadInt32sWithFixedLength(num2);
			int[] array2 = reader.ReadInt32sWithFixedLength(num2);
			StructInfo[] array3 = new StructInfo[num2];
			for (int i = 0; i < num2; i++)
			{
				StructInfo structInfo = array3[i] = new StructInfo(array[i])
				{
					UnknownAfterId = array2[i],
					Flags = reader.ReadInt32(),
					Name = reader.ReadStringWithLengthPrefix(),
					Comment = reader.ReadStringWithLengthPrefix(),
					Member = VariableInfo.ReadVariables(reader)
				};
			}
			return array3;
		}

		public static void WriteStructs(BinaryWriter writer, StructInfo[] structs)
		{
			writer.Write(structs.Length * 8);
			Array.ForEach(structs, delegate(StructInfo x)
			{
				writer.Write(x.Id);
			});
			Array.ForEach(structs, delegate(StructInfo x)
			{
				writer.Write(x.UnknownAfterId);
			});
			foreach (StructInfo structInfo in structs)
			{
				writer.Write(structInfo.Flags);
				writer.WriteStringWithLengthPrefix(structInfo.Name);
				writer.WriteStringWithLengthPrefix(structInfo.Comment);
				VariableInfo.WriteVariables(writer, structInfo.Member);
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
