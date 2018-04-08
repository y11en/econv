using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EProjectFile
{
	public class FormControlInfo : FormElementInfo
	{
		public byte[] UnknownBeforeName;

		public string Comment;

		public int UnknownBeforeLeft;

		public int Left;

		public int Top;

		public int Width;

		public int Height;

		public int UnknownBeforeParent;

		public int Parent;

		public int[] Children;

		public byte[] Cursor;

		public string Tag;

		public int UnknownBeforeVisible;

		public int UnknownBeforeEvents;

		public KeyValuePair<int, int>[] Events;

		public byte[] UnknownBeforeExtensionData;

		public byte[] ExtensionData;

		internal static FormControlInfo ReadWithoutDataType(BinaryReader reader, int length)
		{
			long position = reader.BaseStream.Position;
			FormControlInfo formControlInfo = new FormControlInfo();
			formControlInfo.UnknownBeforeName = reader.ReadBytes(20);
			formControlInfo.Name = reader.ReadCStyleString();
			formControlInfo.Comment = reader.ReadCStyleString();
			formControlInfo.UnknownBeforeLeft = reader.ReadInt32();
			formControlInfo.Left = reader.ReadInt32();
			formControlInfo.Top = reader.ReadInt32();
			formControlInfo.Width = reader.ReadInt32();
			formControlInfo.Height = reader.ReadInt32();
			formControlInfo.UnknownBeforeParent = reader.ReadInt32();
			formControlInfo.Parent = reader.ReadInt32();
			formControlInfo.Children = reader.ReadInt32sWithLengthPrefix();
			formControlInfo.Cursor = reader.ReadBytesWithLengthPrefix();
			formControlInfo.Tag = reader.ReadCStyleString();
			formControlInfo.UnknownBeforeVisible = reader.ReadInt32();
			int num = reader.ReadInt32();
			formControlInfo.Visible = ((num & 1) != 0);
			formControlInfo.Disable = ((num & 2) != 0);
			formControlInfo.UnknownBeforeEvents = reader.ReadInt32();
			formControlInfo.Events = (from x in new object[reader.ReadInt32()]
			select new KeyValuePair<int, int>(reader.ReadInt32(), reader.ReadInt32())).ToArray();
			formControlInfo.UnknownBeforeExtensionData = reader.ReadBytes(20);
			formControlInfo.ExtensionData = reader.ReadBytes(length - (int)(reader.BaseStream.Position - position));
			return formControlInfo;
		}

		protected override void WriteWithoutId(BinaryWriter writer)
		{
			writer.Write(base.DataType);
			writer.Write(UnknownBeforeName);
			writer.WriteCStyleString(base.Name);
			writer.WriteCStyleString(Comment);
			writer.Write(UnknownBeforeLeft);
			writer.Write(Left);
			writer.Write(Top);
			writer.Write(Width);
			writer.Write(Height);
			writer.Write(UnknownBeforeParent);
			writer.Write(Parent);
			writer.WriteInt32sWithLengthPrefix(Children);
			writer.WriteBytesWithLengthPrefix(Cursor);
			writer.WriteCStyleString(Tag);
			writer.Write(UnknownBeforeVisible);
			writer.Write((base.Visible ? 1 : 0) | (base.Disable ? 2 : 0));
			writer.Write(UnknownBeforeEvents);
			writer.Write(Events.Length);
			Array.ForEach(Events, delegate(KeyValuePair<int, int> x)
			{
				writer.Write(x.Key);
				writer.Write(x.Value);
			});
			writer.Write(UnknownBeforeExtensionData);
			writer.Write(ExtensionData);
		}
	}
}
