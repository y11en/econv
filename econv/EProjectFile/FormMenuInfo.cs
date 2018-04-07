using Newtonsoft.Json;
using System.IO;

namespace EProjectFile
{
	internal class FormMenuInfo : FormElementInfo
	{
		[JsonIgnore]
		public byte[] UnknownBeforeName;

		public int HotKey;

		public int Level;

		public bool Selected;

		public string Text;

		public int ClickEvent;

		[JsonIgnore]
		public byte[] UnknownAfterClickEvent;

		internal static FormMenuInfo ReadWithoutDataType(BinaryReader reader, int length)
		{
			long position = reader.BaseStream.Position;
			FormMenuInfo formMenuInfo = new FormMenuInfo();
			formMenuInfo.UnknownBeforeName = reader.ReadBytes(20);
			formMenuInfo.Name = reader.ReadCStyleString();
			reader.ReadCStyleString();
			formMenuInfo.HotKey = reader.ReadInt32();
			formMenuInfo.Level = reader.ReadInt32();
			int num = reader.ReadInt32();
			formMenuInfo.Visible = ((num & 1) == 0);
			formMenuInfo.Disable = ((num & 2) != 0);
			formMenuInfo.Selected = ((num & 4) != 0);
			formMenuInfo.Text = reader.ReadCStyleString();
			formMenuInfo.ClickEvent = reader.ReadInt32();
			formMenuInfo.UnknownAfterClickEvent = reader.ReadBytes(length - (int)(reader.BaseStream.Position - position));
			return formMenuInfo;
		}

		protected override void WriteWithoutId(BinaryWriter writer)
		{
			writer.Write(base.DataType);
			writer.Write(UnknownBeforeName);
			writer.WriteCStyleString(base.Name);
			writer.WriteCStyleString("");
			writer.Write(HotKey);
			writer.Write(Level);
			writer.Write(((!base.Visible) ? 1 : 0) | (base.Disable ? 2 : 0) | (Selected ? 4 : 0));
			writer.WriteCStyleString(Text);
			writer.Write(ClickEvent);
			writer.Write(UnknownAfterClickEvent);
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
