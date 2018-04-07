namespace EProjectFile
{
	internal class SectionInfo
	{
		public string Name;

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
