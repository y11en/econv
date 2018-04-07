using Newtonsoft.Json;
using System;
using System.IO;

namespace EProjectFile
{
	internal class MethodInfo : IHasId
	{
		private int id;

		[JsonIgnore]
		public int UnknownAfterId;

		public int Type;

		public int Flags;

		public int ReturnDataType;

		public string Name;

		public string Comment;

		public VariableInfo[] Variables;

		public VariableInfo[] Parameters;

		[JsonProperty]
		public byte[][] CodeData;

		public int Id => id;

		public MethodInfo(int id)
		{
			this.id = id;
		}

		public static MethodInfo[] ReadMethods(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = num / 8;
			int[] array = reader.ReadInt32sWithFixedLength(num2);
			int[] array2 = reader.ReadInt32sWithFixedLength(num2);
			MethodInfo[] array3 = new MethodInfo[num2];
			for (int i = 0; i < num2; i++)
			{
				MethodInfo methodInfo = new MethodInfo(array[i]);
				methodInfo.UnknownAfterId = array2[i];
				methodInfo.Type = reader.ReadInt32();
				methodInfo.Flags = reader.ReadInt32();
				methodInfo.ReturnDataType = reader.ReadInt32();
				methodInfo.Name = reader.ReadStringWithLengthPrefix();
				methodInfo.Comment = reader.ReadStringWithLengthPrefix();
				methodInfo.Variables = VariableInfo.ReadVariables(reader);
				methodInfo.Parameters = VariableInfo.ReadVariables(reader);
				methodInfo.CodeData = new byte[6][];
				MethodInfo methodInfo2 = methodInfo;
				for (int j = 0; j < methodInfo2.CodeData.Length; j++)
				{
					methodInfo2.CodeData[j] = reader.ReadBytesWithLengthPrefix();
				}
				array3[i] = methodInfo2;
			}
			return array3;
		}

		public static void WriteMethods(BinaryWriter writer, MethodInfo[] methods)
		{
			writer.Write(methods.Length * 8);
			Array.ForEach(methods, delegate(MethodInfo x)
			{
				writer.Write(x.Id);
			});
			Array.ForEach(methods, delegate(MethodInfo x)
			{
				writer.Write(x.UnknownAfterId);
			});
			foreach (MethodInfo methodInfo in methods)
			{
				writer.Write(methodInfo.Type);
				writer.Write(methodInfo.Flags);
				writer.Write(methodInfo.ReturnDataType);
				writer.WriteStringWithLengthPrefix(methodInfo.Name);
				writer.WriteStringWithLengthPrefix(methodInfo.Comment);
				VariableInfo.WriteVariables(writer, methodInfo.Variables);
				VariableInfo.WriteVariables(writer, methodInfo.Parameters);
				Array.ForEach(methodInfo.CodeData, delegate(byte[] x)
				{
					writer.WriteBytesWithLengthPrefix(x);
				});
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
