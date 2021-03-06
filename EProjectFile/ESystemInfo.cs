using System;
using System.IO;

namespace EProjectFile
{
	public class ESystemInfo
    {
		public const string SectionName = "系统信息段";

		public Version ESystemVersion;

		public int Language = 1;

		public Version EProjectFormatVersion;

		public int FileType = 1;

		public int ProjectType;

		public static ESystemInfo Parse(SectionInfo sectionInfo, bool cryptEc = false)
		{
            byte[] data = sectionInfo.Data;

            ESystemInfo eSystemInfo = new ESystemInfo();
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data, false)))
			{
				eSystemInfo.ESystemVersion = new Version(binaryReader.ReadInt16(), binaryReader.ReadInt16());
				binaryReader.ReadInt32();
				eSystemInfo.Language = binaryReader.ReadInt32();
				eSystemInfo.EProjectFormatVersion = new Version(binaryReader.ReadInt16(), binaryReader.ReadInt16());
				eSystemInfo.FileType = binaryReader.ReadInt32();
				binaryReader.ReadInt32();
				eSystemInfo.ProjectType = binaryReader.ReadInt32();
			}
			return eSystemInfo;
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
			writer.Write((short)ESystemVersion.Major);
			writer.Write((short)ESystemVersion.Minor);
			writer.Write(1);
			writer.Write(Language);
			writer.Write((short)EProjectFormatVersion.Major);
			writer.Write((short)EProjectFormatVersion.Minor);
			writer.Write(FileType);
			writer.Write(0);
			writer.Write(ProjectType);
			writer.Write(new byte[32]);
		}
	}
}
