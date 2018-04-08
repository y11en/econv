using System;
using System.IO;

namespace EProjectFile
{
	public class ClassInfo : IHasId
	{
		private int id;

		public int UnknownAfterId;

		public int UnknownBeforeBaseClass;

		public int BaseClass;

		public string Name;

		public string Comment;

		public int[] Method;

		public VariableInfo[] Variables;

		public int Id => id;

		public ClassInfo(int id)
		{
			this.id = id;
		}

		public static ClassInfo[] ReadClasses(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = num / 8;
			int[] array = reader.ReadInt32sWithFixedLength(num2);
			int[] array2 = reader.ReadInt32sWithFixedLength(num2);
			ClassInfo[] array3 = new ClassInfo[num2];
			for (int i = 0; i < num2; i++)
			{
				ClassInfo classInfo = array3[i] = new ClassInfo(array[i])
				{
					UnknownAfterId = array2[i],
					UnknownBeforeBaseClass = reader.ReadInt32(),
					BaseClass = reader.ReadInt32(),
					Name = reader.ReadStringWithLengthPrefix(),
					Comment = reader.ReadStringWithLengthPrefix(),
					Method = reader.ReadInt32sWithFixedLength(reader.ReadInt32() / 4),
					Variables = VariableInfo.ReadVariables(reader)
				};
			}
			return array3;
		}

		public static void WriteClasses(BinaryWriter writer, ClassInfo[] classes)
		{
			writer.Write(classes.Length * 8);
			Array.ForEach(classes, delegate(ClassInfo x)
			{
				writer.Write(x.Id);
			});
			Array.ForEach(classes, delegate(ClassInfo x)
			{
				writer.Write(x.UnknownAfterId);
			});
			foreach (ClassInfo classInfo in classes)
			{
				writer.Write(classInfo.UnknownBeforeBaseClass);
				writer.Write(classInfo.BaseClass);
				writer.WriteStringWithLengthPrefix(classInfo.Name);
				writer.WriteStringWithLengthPrefix(classInfo.Comment);
				writer.Write(classInfo.Method.Length * 4);
				writer.WriteInt32sWithoutLengthPrefix(classInfo.Method);
				VariableInfo.WriteVariables(writer, classInfo.Variables);
			}
		}
	}
}
