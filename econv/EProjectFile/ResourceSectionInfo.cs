using Newtonsoft.Json;
using System.IO;

namespace EProjectFile
{
	internal class ResourceSectionInfo : ToJson
	{
		public const string SectionName = "程序资源段";

		public FormInfo[] Forms;

		public ConstantInfo[] Constants;

		public static ResourceSectionInfo Parse(byte[] data)
		{
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data, false)))
			{
				return new ResourceSectionInfo
				{
					Forms = FormInfo.ReadForms(binaryReader),
					Constants = ConstantInfo.ReadConstants(binaryReader)
				};
			}
		}

		public byte[] ToBytes()
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream()))
			{
				WriteTo(binaryWriter);
				binaryWriter.Flush();
				return ((MemoryStream)binaryWriter.BaseStream).ToArray();
			}
		}

		public void WriteTo(BinaryWriter writer)
		{
			FormInfo.WriteForms(writer, Forms);
			ConstantInfo.WriteConstants(writer, Constants);
			writer.Write(0);
		}
	}
}
