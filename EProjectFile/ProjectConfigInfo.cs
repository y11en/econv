using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;

namespace EProjectFile
{
	public class ProjectConfigInfo
	{
		public const string SectionName = "用户信息段";

		public string Name;

		public string Description;

		public string Author;

		public string ZipCode;

		public string Address;

		public string TelephoneNumber;

		public string FaxNumber;

		public string Email;

		public string Homepage;

		public string CopyrightNotice;

		[JsonConverter(typeof(VersionConverter))]
		public Version Version;

		public bool NotWriteVersion;

		public string CompilePlugins;

		public bool ExportPublicClassMethod;

		public static ProjectConfigInfo Parse(SectionInfo section)
		{
            byte[] data = section.Data;

            ProjectConfigInfo projectConfigInfo = new ProjectConfigInfo();
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data, false)))
			{
				projectConfigInfo.Name = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.Description = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.Author = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.ZipCode = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.Address = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.TelephoneNumber = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.FaxNumber = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.Email = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.Homepage = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.CopyrightNotice = binaryReader.ReadStringWithLengthPrefix();
				projectConfigInfo.Version = new Version(binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32(), binaryReader.ReadInt32());
				projectConfigInfo.NotWriteVersion = (binaryReader.ReadInt32() != 0);
				projectConfigInfo.CompilePlugins = binaryReader.ReadStringWithFixedLength(20);
				projectConfigInfo.ExportPublicClassMethod = (binaryReader.ReadInt32() != 0);
			}
			return projectConfigInfo;
		}
	}
}
