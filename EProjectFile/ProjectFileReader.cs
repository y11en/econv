using System;
using System.IO;
using System.Linq;
using System.Text;

namespace EProjectFile
{
	public class ProjectFileReader : IDisposable
	{
		private BinaryReader reader;

		private bool cryptEc = false;

		private bool disposedValue = false;

		public bool CryptEc => cryptEc;

		public ProjectFileReader(string filename, string password)
		{
            var stream = File.OpenRead(filename);
            reader = new BinaryReader(stream, Encoding.GetEncoding("gbk"));
			int num = reader.ReadInt32();
			int num2 = reader.ReadInt32();
			if (num == 1162630231)
			{
                if (num2 != 131073)
                {
                    throw new Exception("不支持此类加密文件");
                }
                string arg = reader.ReadStringWithLengthPrefix();

                CryptECReadStream cryptECReadStream = new CryptECReadStream(stream, password, stream.Position);
                reader = new BinaryReader(cryptECReadStream, Encoding.GetEncoding("gbk"));
                if (!reader.ReadBytes(32).SequenceEqual(cryptECReadStream.PasswordHash_ASCII))
                {
                    throw new Exception("密码错误");
                }
                cryptEc = true;
                num = reader.ReadInt32();
                num2 = reader.ReadInt32();
            }
			if (num == 1415007811 && num2 == 1196576837)
			{
				return;
			}
			throw new Exception("不是易语言工程文件");
		}

		public bool IsFinish()
		{
			return reader.BaseStream.Position == reader.BaseStream.Length;
		}

        public SectionInfo ReadSection()
        {
            SectionInfo sectionInfo = new SectionInfo();
            if (reader.ReadInt32() != 353465113)
            {
                throw new Exception("Magic错误");
            }
            reader.ReadInt32();
            sectionInfo.Key = reader.ReadBytes(4);
            sectionInfo.SectionName = DecodeName(sectionInfo.Key, reader.ReadBytes(30));
            reader.ReadInt16();
            reader.ReadInt32();
            sectionInfo.Flags = reader.ReadInt32();
            reader.ReadInt32();
            int num = reader.ReadInt32();
            if (cryptEc)
            {
                num ^= 1;
            }
            reader.ReadBytes(40);
            sectionInfo.Data = new byte[num];
            reader.Read(sectionInfo.Data, 0, num);
            return sectionInfo;
        }

        private static string DecodeName(byte[] key, byte[] encodedName)
		{
			if (encodedName == null || key == null)
			{
				return string.Empty;
			}
			byte[] array = (byte[])encodedName.Clone();
			if (key.Length != 4)
			{
				throw new Exception(string.Format("{0}应为4字节", "key"));
			}
			if (key[0] != 25 || key[1] != 115 || key[2] != 0 || key[3] != 7)
			{
				for (int i = 0; i < array.Length; i++)
				{
					array[i] ^= key[i + 1 & 3];
				}
			}
			int num = Array.IndexOf(array, (byte)0);
			if (num != -1)
			{
				byte[] array2 = new byte[num];
				Array.Copy(array, array2, num);
				array = array2;
			}
			return Encoding.GetEncoding("gbk").GetString(array);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					reader.Dispose();
				}
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
