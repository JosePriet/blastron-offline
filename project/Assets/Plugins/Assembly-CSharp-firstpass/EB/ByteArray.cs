using System.Collections;

namespace EB
{
	internal class ByteArray
	{
		private ArrayList _bytes;

		public ByteArray()
		{
			_bytes = new ArrayList(40960);
		}

		public void writeByte(byte value)
		{
			_bytes.Add(value);
		}

		public byte[] GetAllBytes()
		{
			byte[] array = new byte[_bytes.Count];
			for (int i = 0; i < _bytes.Count; i++)
			{
				array[i] = (byte)_bytes[i];
			}
			return array;
		}
	}
}
