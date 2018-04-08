using Newtonsoft.Json;
using System;
using System.IO;

namespace EProjectFile
{
	public class DllDeclareInfo : IHasId
	{
		private int id;

		[JsonIgnore]
		public int UnknownAfterId;

		public int Flags;

		public uint ReturnDataType;

		public string Name;

		public string Comment;

		public string NameInLibrary;

		public string LibraryFile;

		public VariableInfo[] Parameters;

		public int Id => id;

		public DllDeclareInfo(int id)
		{
			this.id = id;
		}

		public static DllDeclareInfo[] ReadDllDeclares(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = num / 8;
			int[] array = reader.ReadInt32sWithFixedLength(num2);
			int[] array2 = reader.ReadInt32sWithFixedLength(num2);
			DllDeclareInfo[] array3 = new DllDeclareInfo[num2];
			for (int i = 0; i < num2; i++)
			{
				DllDeclareInfo dllDeclareInfo = array3[i] = new DllDeclareInfo(array[i])
				{
					UnknownAfterId = array2[i],
					Flags = reader.ReadInt32(),
					ReturnDataType = reader.ReadUInt32(),
					Name = reader.ReadStringWithLengthPrefix(),
					Comment = reader.ReadStringWithLengthPrefix(),
					LibraryFile = reader.ReadStringWithLengthPrefix(),
					NameInLibrary = reader.ReadStringWithLengthPrefix(),
					Parameters = VariableInfo.ReadVariables(reader)
				};
			}
			return array3;
		}

		public static void WriteDllDeclares(BinaryWriter writer, DllDeclareInfo[] dllDeclares)
		{
			writer.Write(dllDeclares.Length * 8);
			Array.ForEach(dllDeclares, delegate(DllDeclareInfo x)
			{
				writer.Write(x.Id);
			});
			Array.ForEach(dllDeclares, delegate(DllDeclareInfo x)
			{
				writer.Write(x.UnknownAfterId);
			});
			foreach (DllDeclareInfo dllDeclareInfo in dllDeclares)
			{
				writer.Write(dllDeclareInfo.Flags);
				writer.Write(dllDeclareInfo.ReturnDataType);
				writer.WriteStringWithLengthPrefix(dllDeclareInfo.Name);
				writer.WriteStringWithLengthPrefix(dllDeclareInfo.Comment);
				writer.WriteStringWithLengthPrefix(dllDeclareInfo.LibraryFile);
				writer.WriteStringWithLengthPrefix(dllDeclareInfo.NameInLibrary);
				VariableInfo.WriteVariables(writer, dllDeclareInfo.Parameters);
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
