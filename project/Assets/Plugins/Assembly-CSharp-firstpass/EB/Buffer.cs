using System;

namespace EB
{
	public class Buffer
	{
		private byte[] _buffer;

		private int _offset;

		private int _capacity;

		private int _length;

		public int Length
		{
			get
			{
				return _length;
			}
		}

		public int Capacity
		{
			get
			{
				return _capacity;
			}
		}

		public byte this[int index]
		{
			get
			{
				return _buffer[_offset + index];
			}
			set
			{
				if (index >= _capacity)
				{
					throw new IndexOutOfRangeException("Buffer Overflow");
				}
				_buffer[_offset + index] = value;
			}
		}

		public Buffer(int capacity)
		{
			_buffer = new byte[capacity];
			_offset = 0;
			_capacity = capacity;
			_length = 0;
		}

		public Buffer(byte[] bytes, bool setLength)
		{
			_buffer = bytes;
			_offset = 0;
			_capacity = bytes.Length;
			_length = (setLength ? _capacity : 0);
		}

		public Buffer(ArraySegment<byte> segment, bool setLength)
		{
			_buffer = segment.Array;
			_offset = segment.Offset;
			_capacity = segment.Count;
			_length = (setLength ? _capacity : 0);
		}

		public void Reset()
		{
			_length = 0;
		}

		public byte ReadByte()
		{
			return this[_length++];
		}

		public int Digest()
		{
			return Hash.FNV32(_buffer, _offset, _length, (int)Hash.HASH_INIT_32);
		}

		public void WriteBuffer(Buffer other)
		{
			for (int i = 0; i < other.Length; i++)
			{
				WriteByte(other[i]);
			}
		}

		public void WriteByte(byte value)
		{
			this[_length++] = value;
		}

		public void Sign(Hmac hmac)
		{
			hmac.Update(_buffer, _offset, Length);
			byte[] array = hmac.Final();
			WriteBytes(array);
		}

		public bool Verify(Hmac hmac)
		{
			int digestSize = hmac.DigestSize;
			if (_capacity < digestSize)
			{
				return false;
			}
			hmac.Update(_buffer, _offset, _capacity - digestSize);
			byte[] array = hmac.Final();
			int num = _offset + _capacity - array.Length;
			int num2 = 0;
			while (num2 < array.Length)
			{
				if (_buffer[num] != array[num2])
				{
					return false;
				}
				num2++;
				num++;
			}
			_capacity -= array.Length;
			return true;
		}

		public void WriteBytes(byte[] array)
		{
			int num = array.Length;
			if (num + _length > _capacity)
			{
				throw new ArgumentOutOfRangeException("WriteBytes buffer overflow length: " + Length + " count:" + num + " capacity" + _capacity);
			}
			int num2 = _offset + _length;
			for (int i = 0; i < num; i++)
			{
				_buffer[num2++] = array[i];
			}
			_length += array.Length;
		}

		public ArraySegment<byte> ReadBytes(int count)
		{
			if (count + Length > _capacity)
			{
				throw new ArgumentOutOfRangeException("ReadBytes buffer underun length: " + Length + " count:" + count + " capacity" + _capacity);
			}
			ArraySegment<byte> result = new ArraySegment<byte>(_buffer, _offset + _length, count);
			_length += count;
			return result;
		}

		public Buffer Slice(int start, int end, bool setLength)
		{
			return new Buffer(new ArraySegment<byte>(_buffer, _offset + start, end - start), setLength);
		}

		public Buffer Slice(int start)
		{
			return Slice(start, Length, true);
		}

		public Buffer Slice()
		{
			return Slice(0, Length, true);
		}

		public Buffer Clone()
		{
			return Slice(0, Length, false);
		}

		public Buffer Remaining()
		{
			return Slice(Length, Capacity, false);
		}

		public ArraySegment<byte> ToArraySegment(bool all)
		{
			return new ArraySegment<byte>(_buffer, _offset, (!all) ? _length : _capacity);
		}

		public ushort ReadUInt16LE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 2);
			}
			ushort result = BitConverter.ToUInt16(_buffer, _offset + _length);
			_length += 2;
			return result;
		}

		public ushort ReadUInt16BE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 2);
			}
			ushort result = BitConverter.ToUInt16(_buffer, _offset + _length);
			_length += 2;
			return result;
		}

		public void WriteUInt16LE(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteUInt16BE(ushort value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public short ReadInt16LE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 2);
			}
			short result = BitConverter.ToInt16(_buffer, _offset + _length);
			_length += 2;
			return result;
		}

		public short ReadInt16BE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 2);
			}
			short result = BitConverter.ToInt16(_buffer, _offset + _length);
			_length += 2;
			return result;
		}

		public void WriteInt16LE(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteInt16BE(short value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public uint ReadUInt32LE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 4);
			}
			uint result = BitConverter.ToUInt32(_buffer, _offset + _length);
			_length += 4;
			return result;
		}

		public uint ReadUInt32BE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 4);
			}
			uint result = BitConverter.ToUInt32(_buffer, _offset + _length);
			_length += 4;
			return result;
		}

		public void WriteUInt32LE(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteUInt32BE(uint value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public int ReadInt32LE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 4);
			}
			int result = BitConverter.ToInt32(_buffer, _offset + _length);
			_length += 4;
			return result;
		}

		public int ReadInt32BE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 4);
			}
			int result = BitConverter.ToInt32(_buffer, _offset + _length);
			_length += 4;
			return result;
		}

		public void WriteInt32LE(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteInt32BE(int value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public ulong ReadUInt64LE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 8);
			}
			ulong result = BitConverter.ToUInt64(_buffer, _offset + _length);
			_length += 8;
			return result;
		}

		public ulong ReadUInt64BE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 8);
			}
			ulong result = BitConverter.ToUInt64(_buffer, _offset + _length);
			_length += 8;
			return result;
		}

		public void WriteUInt64LE(ulong value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteUInt64BE(ulong value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public long ReadInt64LE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 8);
			}
			long result = BitConverter.ToInt64(_buffer, _offset + _length);
			_length += 8;
			return result;
		}

		public long ReadInt64BE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 8);
			}
			long result = BitConverter.ToInt64(_buffer, _offset + _length);
			_length += 8;
			return result;
		}

		public void WriteInt64LE(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteInt64BE(long value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public float ReadFloatLE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 4);
			}
			float result = BitConverter.ToSingle(_buffer, _offset + _length);
			_length += 4;
			return result;
		}

		public float ReadFloatBE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 4);
			}
			float result = BitConverter.ToSingle(_buffer, _offset + _length);
			_length += 4;
			return result;
		}

		public void WriteFloatLE(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteFloatBE(float value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public double ReadDoubleLE()
		{
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 8);
			}
			double result = BitConverter.ToDouble(_buffer, _offset + _length);
			_length += 8;
			return result;
		}

		public double ReadDoubleBE()
		{
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(_buffer, _offset + _length, 8);
			}
			double result = BitConverter.ToDouble(_buffer, _offset + _length);
			_length += 8;
			return result;
		}

		public void WriteDoubleLE(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteDoubleBE(double value)
		{
			byte[] bytes = BitConverter.GetBytes(value);
			if (BitConverter.IsLittleEndian)
			{
				Array.Reverse(bytes);
			}
			WriteBytes(bytes);
		}

		public void WriteString(string value)
		{
			byte[] bytes = Encoding.GetBytes(value);
			WriteBytes(bytes);
			WriteByte(0);
		}

		public string ReadString()
		{
			int length = _length;
			while (_length < _capacity && this[_length] != 0)
			{
				_length++;
			}
			string result = Encoding.GetString(_buffer, _offset + length, _length - length);
			_length++;
			return result;
		}

		public string ToHexString()
		{
			return Encoding.ToHexString(_buffer, _offset, Length);
		}

		public override string ToString()
		{
			return string.Format("[Buffer: Length={0}, Capacity={1}, Offset={2}]", Length, Capacity, _offset);
		}
	}
}
