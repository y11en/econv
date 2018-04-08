using Newtonsoft.Json;
using System;
using System.IO;

namespace EProjectFile
{
	public class FormInfo
	{
		public int Id;

		public int UnknownAfterId;

		public int UnknownBeforeClass;

		public int Class;

		public string Name;

		public string Comment;

		public FormElementInfo[] Elements;

		public static FormInfo[] ReadForms(BinaryReader reader)
		{
			int num = reader.ReadInt32();
			int num2 = num / 8;
			int[] array = reader.ReadInt32sWithFixedLength(num2);
			int[] array2 = reader.ReadInt32sWithFixedLength(num2);
			FormInfo[] array3 = new FormInfo[num2];
			for (int i = 0; i < num2; i++)
			{
				FormInfo formInfo = array3[i] = new FormInfo
				{
					Id = array[i],
					UnknownAfterId = array2[i],
					UnknownBeforeClass = reader.ReadInt32(),
					Class = reader.ReadInt32(),
					Name = reader.ReadStringWithLengthPrefix(),
					Comment = reader.ReadStringWithLengthPrefix(),
					Elements = FormElementInfo.ReadFormElements(reader)
				};
			}
			return array3;
		}

		public static void WriteForms(BinaryWriter writer, FormInfo[] forms)
		{
			writer.Write(forms.Length * 8);
			Array.ForEach(forms, delegate(FormInfo x)
			{
				writer.Write(x.Id);
			});
			Array.ForEach(forms, delegate(FormInfo x)
			{
				writer.Write(x.UnknownAfterId);
			});
			foreach (FormInfo formInfo in forms)
			{
				writer.Write(formInfo.UnknownBeforeClass);
				writer.Write(formInfo.Class);
				writer.WriteStringWithLengthPrefix(formInfo.Name);
				writer.WriteStringWithLengthPrefix(formInfo.Comment);
				FormElementInfo.WriteFormElements(writer, formInfo.Elements);
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
