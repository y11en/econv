using System.Collections.Generic;
using System.IO;

namespace EProjectFile
{
	public class EPackageInfo
	{
		public const string SectionName = "易包信息段1";

		public string[] FileNames;

		public static EPackageInfo Parse(SectionInfo sectionInfo, bool cryptEc = false)
		{
            byte[] data = sectionInfo.Data;

            EPackageInfo ePackageInfo = new EPackageInfo();
			using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(data, false)))
			{
				List<string> list = new List<string>();
				while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
				{
					string text = binaryReader.ReadStringWithLengthPrefix();
					if ("".Equals(text))
					{
						text = null;
					}
					list.Add(text);
				}
				ePackageInfo.FileNames = list.ToArray();
			}
			return ePackageInfo;
		}
	}
}
