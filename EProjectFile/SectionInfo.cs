namespace EProjectFile
{
	public class SectionInfo
	{
		public string SectionName;

		public byte[] Key = new byte[4]
		{
			25,
			115,
			0,
			7
		};

		public byte[] Data;

		public int Flags;
	}
}
