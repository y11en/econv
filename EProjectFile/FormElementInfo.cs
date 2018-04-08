using System.IO;

namespace EProjectFile
{
	public abstract class FormElementInfo : IHasId
	{
		private int id;

		public int DataType;

		public string Name;

		public bool Visible;

		public bool Disable;

		public int Id => id;

		public static FormElementInfo[] ReadFormElements(BinaryReader r)
		{
			return r.ReadBlocksWithIdAndOffest(delegate(BinaryReader reader, int id, int length)
			{
				int num = reader.ReadInt32();
				FormElementInfo formElementInfo = (num != 65539) ?
                ((FormElementInfo)FormControlInfo.ReadWithoutDataType(r, length - 4))
                : ((FormElementInfo)FormMenuInfo.ReadWithoutDataType(r, length - 4));
				formElementInfo.id = id;
				formElementInfo.DataType = num;
				return formElementInfo;
			});
		}

		public static void WriteFormElements(BinaryWriter w, FormElementInfo[] formElements)
		{
			w.WriteBlocksWithIdAndOffest(formElements, delegate(BinaryWriter writer, FormElementInfo elem)
			{
				elem.WriteWithoutId(writer);
			});
		}

		protected abstract void WriteWithoutId(BinaryWriter writer);
	}
}
