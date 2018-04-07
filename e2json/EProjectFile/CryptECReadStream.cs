using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace EProjectFile
{
	internal class CryptECReadStream : Stream
	{
		private Stream stream;

		private byte[] keyTable;

		private byte[] passwordHash_ASCII;

		private long lengthOfUncryptedBlock;

		public byte[] PasswordHash_ASCII => passwordHash_ASCII;

		public long LengthOfUncryptedBlock => lengthOfUncryptedBlock;

		public Stream Stream => stream;

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override long Length => stream.Length;

		public override long Position
		{
			get
			{
				return stream.Position;
			}
			set
			{
				stream.Position = value;
			}
		}

		public CryptECReadStream(Stream stream, string password, long lengthOfUncryptedBlock)
		{
			this.stream = stream;
			byte[] bytes = Encoding.GetEncoding("gbk").GetBytes(password);
			byte[] array = new MD5CryptoServiceProvider().ComputeHash(bytes);
			byte b = (byte)(array[7] & 0xF);
			byte b2 = (byte)(array[7] & 0xF0);
			byte b3 = (byte)(array[8] & 0xF);
			byte b4 = (byte)(array[8] & 0xF0);
			array[7] = (byte)(b2 | b4 >> 4);
			array[8] = (byte)(b << 4 | b3);
			StringBuilder stringBuilder = new StringBuilder();
			for (int num = array.Length - 1; num >= 0; num--)
			{
				stringBuilder.Append(array[num].ToString("x2"));
			}
			passwordHash_ASCII = Encoding.ASCII.GetBytes(stringBuilder.ToString());
			InitKeyTable(bytes, out keyTable);
			this.lengthOfUncryptedBlock = lengthOfUncryptedBlock;
		}

		public static void InitKeyTable(byte[] key, out byte[] keyTable)
		{
			keyTable = new byte[258]
			{
				240,
				94,
				153,
				161,
				136,
				227,
				30,
				238,
				17,
				158,
				201,
				151,
				27,
				144,
				79,
				124,
				82,
				203,
				130,
				250,
				39,
				222,
				246,
				168,
				218,
				211,
				176,
				207,
				86,
				214,
				133,
				66,
				26,
				156,
				181,
				14,
				184,
				237,
				16,
				28,
				36,
				106,
				105,
				206,
				135,
				85,
				31,
				150,
				108,
				123,
				186,
				101,
				20,
				170,
				44,
				221,
				163,
				182,
				125,
				99,
				245,
				233,
				142,
				32,
				65,
				35,
				120,
				140,
				252,
				34,
				159,
				166,
				180,
				111,
				167,
				119,
				89,
				192,
				191,
				58,
				48,
				162,
				21,
				42,
				83,
				93,
				116,
				77,
				147,
				251,
				247,
				64,
				115,
				40,
				110,
				118,
				213,
				177,
				45,
				149,
				112,
				244,
				60,
				52,
				229,
				76,
				91,
				187,
				95,
				80,
				88,
				141,
				107,
				183,
				97,
				9,
				242,
				72,
				202,
				129,
				55,
				69,
				239,
				208,
				190,
				217,
				212,
				231,
				157,
				51,
				145,
				113,
				47,
				59,
				230,
				13,
				254,
				121,
				73,
				103,
				25,
				165,
				8,
				175,
				128,
				178,
				235,
				62,
				210,
				185,
				209,
				68,
				87,
				143,
				138,
				75,
				57,
				241,
				102,
				234,
				226,
				223,
				243,
				122,
				152,
				205,
				171,
				139,
				4,
				98,
				84,
				22,
				18,
				67,
				2,
				216,
				54,
				114,
				6,
				127,
				37,
				224,
				46,
				5,
				15,
				byte.MaxValue,
				173,
				3,
				7,
				225,
				148,
				23,
				193,
				50,
				195,
				81,
				215,
				219,
				232,
				228,
				117,
				63,
				1,
				38,
				74,
				41,
				100,
				71,
				134,
				61,
				189,
				220,
				131,
				43,
				104,
				29,
				70,
				236,
				196,
				154,
				200,
				49,
				78,
				169,
				164,
				53,
				155,
				172,
				92,
				11,
				146,
				204,
				10,
				132,
				19,
				12,
				0,
				160,
				179,
				96,
				24,
				90,
				197,
				198,
				137,
				126,
				33,
				249,
				194,
				109,
				188,
				199,
				174,
				56,
				253,
				248,
				0,
				0
			};
			byte b = 0;
			int num = 0;
			for (int i = 0; i < 256; i++)
			{
				b = (byte)(keyTable[i] + key[num] + b);
				byte b2 = keyTable[i];
				keyTable[i] = keyTable[b];
				keyTable[b] = b2;
				num = (num + 1) % key.Length;
			}
		}

		public static void SkipDataButUpdateKeyTable(long length, byte[] keyTable)
		{
			DecodeDataAndUpdateKeyTable(null, 0L, length, keyTable);
		}

		public static void DecodeDataAndUpdateKeyTable(byte[] data, long start, long length, byte[] keyTable)
		{
			byte b = keyTable[256];
			byte b2 = keyTable[257];
			long num = start + length;
			for (long num2 = start; num2 < num; num2++)
			{
				b = (byte)(b + 1);
				byte b3 = keyTable[b];
				b2 = (byte)(b2 + b3);
				keyTable[b] = keyTable[b2];
				keyTable[b2] = b3;
				if (data != null)
				{
					data[num2] ^= keyTable[(byte)(b3 + keyTable[b])];
				}
			}
			keyTable[256] = b;
			keyTable[257] = b2;
		}

		public override void Flush()
		{
			stream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			long num = stream.Position;
			int num2 = stream.Read(buffer, 0, count);
			long num3 = 0L;
			long num4 = num2;
			long num5 = lengthOfUncryptedBlock - num;
			if (num5 > 0)
			{
				num4 -= num5;
				num3 += num5;
				num += num5;
			}
			if (num4 > 0)
			{
				byte[] array = new byte[40];
				Array.Copy(passwordHash_ASCII, 0, array, 8, 32);
				byte[] destinationArray = new byte[258];
				Array.Copy(keyTable, destinationArray, 258);
				SkipDataButUpdateKeyTable(8 * (num / 4096), destinationArray);
				int num6 = 0;
				byte[] array2 = new byte[(num4 / 4096 + 2) * 8];
				DecodeDataAndUpdateKeyTable(array2, 0L, array2.Length, destinationArray);
				long num7 = num % 4096;
				if (num7 >= 0)
				{
					Array.Copy(array2, num6, array, 0, 8);
					num6 += 8;
					InitKeyTable(array, out destinationArray);
					SkipDataButUpdateKeyTable(num7, destinationArray);
					long num8 = Math.Min(4096 - num7, num4);
					DecodeDataAndUpdateKeyTable(buffer, num3, num8, destinationArray);
					num3 += num8;
					num4 -= num8;
				}
				if (num4 > 0)
				{
					while (true)
					{
						Array.Copy(array2, num6, array, 0, 8);
						num6 += 8;
						InitKeyTable(array, out destinationArray);
						if (num4 > 4096)
						{
							DecodeDataAndUpdateKeyTable(buffer, num3, 4096L, destinationArray);
							num3 += 4096;
							num4 -= 4096;
							continue;
						}
						break;
					}
					if (num4 > 0)
					{
						DecodeDataAndUpdateKeyTable(buffer, num3, num4, destinationArray);
						num3 += num4;
						num4 = 0L;
					}
				}
			}
			return num2;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			stream.Dispose();
		}
	}
}
