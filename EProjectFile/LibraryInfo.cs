using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.IO;
using System.Linq;

namespace EProjectFile
{
	public class LibraryInfo
	{
		public string FileName;

		public string GuidString;

		[JsonConverter(typeof(VersionConverter))]
		public Version Version;

		public string Name;

		public static LibraryInfo[] ReadLibraries(BinaryReader reader)
		{
			return reader.ReadStringsWithMfcStyleCountPrefix().Select(delegate(string x)
			{
				string[] array = x.Split('\r');
				return new LibraryInfo
				{
					FileName = array[0],
					GuidString = array[1],
					Version = new Version(int.Parse(array[2]), int.Parse(array[3])),
					Name = array[4]
				};
			}).ToArray();
		}

		public static void WriteLibraries(BinaryWriter writer, LibraryInfo[] methods)
		{
			writer.WriteStringsWithMfcStyleCountPrefix((from x in methods
			select $"{x.FileName}\r{x.GuidString}\r{x.Version.Major}\r{x.Version.Minor}\r{x.Name}").ToArray());
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject((object)this, Formatting.Indented);
		}
	}
}
