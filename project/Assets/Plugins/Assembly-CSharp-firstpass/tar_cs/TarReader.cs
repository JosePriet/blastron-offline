using System.Collections.Generic;
using System.IO;

namespace tar_cs
{
	public class TarReader
	{
		private readonly byte[] dataBuffer = new byte[512];

		private readonly UsTarHeader header;

		private readonly Stream inStream;

		private long remainingBytesInFile;

		public ITarHeader FileInfo
		{
			get
			{
				return header;
			}
		}

		public TarReader(Stream tarredData)
		{
			inStream = tarredData;
			header = new UsTarHeader();
		}

		public void ReadToEnd(string destDirectory)
		{
			while (MoveNext(false))
			{
				string text = destDirectory + Path.DirectorySeparatorChar + FileInfo.FileName;
				string fileName = Path.GetFileName(text);
				string path = text.Remove(text.Length - fileName.Length);
				Directory.CreateDirectory(path);
				using (FileStream dataDestanation = new FileStream(text, FileMode.Create, FileAccess.Write))
				{
					Read(dataDestanation);
				}
			}
		}

		public void Read(Stream dataDestanation)
		{
			int count;
			byte[] buffer;
			while ((count = Read(out buffer)) != -1)
			{
				dataDestanation.Write(buffer, 0, count);
			}
		}

		protected int Read(out byte[] buffer)
		{
			if (remainingBytesInFile == 0L)
			{
				buffer = null;
				return -1;
			}
			int num = -1;
			long num2 = remainingBytesInFile - 512;
			if (num2 > 0)
			{
				num2 = 512L;
			}
			else
			{
				num = 512 - (int)remainingBytesInFile;
				num2 = remainingBytesInFile;
			}
			int num3 = inStream.Read(dataBuffer, 0, (int)num2);
			remainingBytesInFile -= num3;
			if (inStream.CanSeek && num > 0)
			{
				inStream.Seek(num, SeekOrigin.Current);
			}
			else
			{
				while (num > 0)
				{
					inStream.ReadByte();
					num--;
				}
			}
			buffer = dataBuffer;
			return num3;
		}

		private static bool IsEmpty(IEnumerable<byte> buffer)
		{
			foreach (byte item in buffer)
			{
				if (item != 0)
				{
					return false;
				}
			}
			return true;
		}

		public bool MoveNext(bool skipData)
		{
			if (remainingBytesInFile > 0)
			{
				if (!skipData)
				{
					throw new TarException("You are trying to change file while not all the data from the previous one was read. If you do want to skip files use skipData parameter set to true.");
				}
				if (inStream.CanSeek)
				{
					long num = remainingBytesInFile % 512;
					inStream.Seek(remainingBytesInFile + (512 - ((num != 0L) ? num : 512)), SeekOrigin.Current);
				}
				else
				{
					byte[] buffer;
					while (Read(out buffer) != -1)
					{
					}
				}
			}
			byte[] bytes = header.GetBytes();
			int num2 = inStream.Read(bytes, 0, header.HeaderSize);
			if (num2 < 512)
			{
				throw new TarException("Can not read header");
			}
			if (IsEmpty(bytes))
			{
				num2 = inStream.Read(bytes, 0, header.HeaderSize);
				if (num2 == 512 && IsEmpty(bytes))
				{
					return false;
				}
				throw new TarException("Broken archive");
			}
			if (header.UpdateHeaderFromBytes())
			{
				throw new TarException("Checksum check failed");
			}
			remainingBytesInFile = header.SizeInBytes;
			return true;
		}
	}
}
