using System.IO;

namespace tar_cs
{
	internal class DataWriter : IArchiveDataWriter
	{
		private readonly long size;

		private long remainingBytes;

		private bool canWrite = true;

		private readonly Stream stream;

		public bool CanWrite
		{
			get
			{
				return canWrite;
			}
		}

		public DataWriter(Stream data, long dataSizeInBytes)
		{
			size = dataSizeInBytes;
			remainingBytes = size;
			stream = data;
		}

		public int Write(byte[] buffer, int count)
		{
			if (remainingBytes == 0L)
			{
				canWrite = false;
				return -1;
			}
			int num = (int)((remainingBytes - count >= 0) ? count : remainingBytes);
			stream.Write(buffer, 0, num);
			remainingBytes -= num;
			return num;
		}
	}
}
