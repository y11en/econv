using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace EProjectFile
{
	internal class EPackageInfo
	{
		public const string SectionName = "易包信息段1";

		public string[] FileNames;

		public static EPackageInfo Parse(byte[] data)
		{
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

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
