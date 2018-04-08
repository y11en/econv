using Newtonsoft.Json;
using System.IO;

namespace EProjectFile
{
	public class ResourceSectionInfo
	{
		public const string SectionName = "程序资源段";

		public FormInfo[] Forms;

		public ConstantInfo[] Constants;

		public static ResourceSectionInfo Parse(SectionInfo sectionInfo, bool cryptEc = false)
		{
            byte[] data = sectionInfo.Data;

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
