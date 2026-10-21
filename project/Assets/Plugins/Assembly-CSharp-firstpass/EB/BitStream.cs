using System;
using EB.Replication;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace EB
{
	public class BitStream : IDisposable
	{
		private Buffer _buffer;

		private int _bufferStart;

		private int _bitIndex = -1;

		private int _bitShift = -1;

		public bool isReading { get; private set; }

		public bool isWriting
		{
			get
			{
				return !isReading;
			}
		}

		public bool DataAvailable
		{
			get
			{
				return _buffer.Length < _buffer.Capacity;
			}
		}

		public BitStream(int capacity)
		{
			isReading = false;
			_buffer = new Buffer(capacity);
		}

		public BitStream(byte[] data)
		{
			isReading = true;
			_buffer = new Buffer(data, false);
			_buffer.Reset();
		}

		public BitStream(Buffer buffer, bool write)
		{
			isReading = !write;
			_buffer = buffer;
			_bufferStart = buffer.Length;
		}

		public Buffer Slice()
		{
			if (isWriting)
			{
				int bufferStart = _bufferStart;
				_bufferStart = _buffer.Length;
				return _buffer.Slice(bufferStart, _buffer.Length, true);
			}
			throw new Exception("Can't call to buffer when writing");
		}

		public void Reset()
		{
			_buffer.Reset();
			_bufferStart = _buffer.Length;
		}

		public int Reserve()
		{
			if (isWriting)
			{
				int length = _buffer.Length;
				_buffer.WriteByte(0);
				return length;
			}
			throw new Exception("Can't reserve space while reading");
		}

		public void Poke(int index, byte data)
		{
			if (isWriting)
			{
				_buffer[index] = data;
				return;
			}
			throw new Exception("Can't poke while reading");
		}

		public void ResetBoolFlag()
		{
			_bitIndex = -1;
		}

		public void Serialize(ref bool data)
		{
			if (isReading)
			{
				if (_bitIndex == -1)
				{
					_bitIndex = _buffer.Length;
					_buffer.ReadByte();
					_bitShift = 0;
				}
				byte b = _buffer[_bitIndex];
				int num = 1 << _bitShift;
				data = (b & num) != 0;
				_bitShift++;
				if (_bitShift == 8)
				{
					_bitIndex = -1;
				}
				return;
			}
			if (_bitIndex == -1)
			{
				_bitIndex = _buffer.Length;
				_buffer.WriteByte(0);
				_bitShift = 0;
			}
			byte b2 = _buffer[_bitIndex];
			if (data)
			{
				b2 |= (byte)(1 << _bitShift);
			}
			_buffer[_bitIndex] = b2;
			_bitShift++;
			if (_bitShift == 8)
			{
				_bitIndex = -1;
			}
		}

		public void Serialize(ref byte data)
		{
			if (isReading)
			{
				data = _buffer.ReadByte();
			}
			else
			{
				_buffer.WriteByte(data);
			}
		}

		public void Serialize(ref SafeInt data)
		{
			if (isReading)
			{
				data = _buffer.ReadInt32LE();
			}
			else
			{
				_buffer.WriteInt32LE(data);
			}
		}

		public void Serialize(ref SafeFloat data)
		{
			if (isReading)
			{
				data = _buffer.ReadFloatLE();
			}
			else
			{
				_buffer.WriteFloatLE(data);
			}
		}

		public void Serialize(ref byte[] data)
		{
			if (isReading)
			{
				int num = _buffer.ReadByte();
				if (num == 255)
				{
					num = _buffer.ReadUInt16LE();
				}
				ArraySegment<byte> arraySegment = _buffer.ReadBytes(num);
				data = new byte[num];
				Array.Copy(arraySegment.Array, arraySegment.Offset, data, 0, num);
				return;
			}
			int num2 = data.Length;
			if (num2 >= 65535)
			{
				throw new ArgumentException("Byte array too large for bitstream " + num2);
			}
			if (num2 >= 255)
			{
				_buffer.WriteByte(byte.MaxValue);
				_buffer.WriteUInt16LE((ushort)num2);
			}
			else
			{
				_buffer.WriteByte((byte)num2);
			}
			_buffer.WriteBytes(data);
		}

		public void Serialize(ref Buffer data)
		{
			if (isReading)
			{
				int num = _buffer.ReadByte();
				if (num == 255)
				{
					num = _buffer.ReadUInt16LE();
				}
				data = new Buffer(_buffer.ReadBytes(num), false);
				return;
			}
			int length = data.Length;
			if (length >= 65535)
			{
				throw new ArgumentException("Byte array too large for bitstream " + length);
			}
			if (length >= 255)
			{
				_buffer.WriteByte(byte.MaxValue);
				_buffer.WriteUInt16LE((ushort)length);
			}
			else
			{
				_buffer.WriteByte((byte)length);
			}
			_buffer.WriteBuffer(data);
		}

		public void Serialize(ref ushort data)
		{
			if (isReading)
			{
				data = _buffer.ReadUInt16LE();
			}
			else
			{
				_buffer.WriteUInt16LE(data);
			}
		}

		public void Serialize(ref short data)
		{
			if (isReading)
			{
				data = _buffer.ReadInt16LE();
			}
			else
			{
				_buffer.WriteInt16LE(data);
			}
		}

		public void Serialize(ref uint data)
		{
			if (isReading)
			{
				data = _buffer.ReadUInt32LE();
			}
			else
			{
				_buffer.WriteUInt32LE(data);
			}
		}

		public void Serialize(ref int data)
		{
			if (isReading)
			{
				data = _buffer.ReadInt32LE();
			}
			else
			{
				_buffer.WriteInt32LE(data);
			}
		}

		public void Serialize(ref ulong data)
		{
			if (isReading)
			{
				data = _buffer.ReadUInt64LE();
			}
			else
			{
				_buffer.WriteUInt64LE(data);
			}
		}

		public void Serialize(ref long data)
		{
			if (isReading)
			{
				data = _buffer.ReadInt64LE();
			}
			else
			{
				_buffer.WriteInt64LE(data);
			}
		}

		public void Serialize(ref float data)
		{
			if (isReading)
			{
				data = _buffer.ReadFloatLE();
			}
			else
			{
				_buffer.WriteFloatLE(data);
			}
		}

		public void Serialize(ref double data)
		{
			if (isReading)
			{
				data = _buffer.ReadDoubleLE();
			}
			else
			{
				_buffer.WriteDoubleLE(data);
			}
		}

		public void Serialize(ref string data)
		{
			if (isReading)
			{
				data = _buffer.ReadString();
			}
			else
			{
				_buffer.WriteString(data);
			}
		}

		public void Serialize(ref Vector2 data)
		{
			Serialize(ref data.x);
			Serialize(ref data.y);
		}

		public void Serialize(ref Vector3 data)
		{
			Serialize(ref data.x);
			Serialize(ref data.y);
			Serialize(ref data.z);
		}

		public void Serialize(ref Vector4 data)
		{
			Serialize(ref data.x);
			Serialize(ref data.y);
			Serialize(ref data.z);
			Serialize(ref data.w);
		}

		public void Serialize(ref Quaternion data)
		{
			if (isReading)
			{
				Vector3 data2 = default(Vector3);
				Serialize(ref data2);
				data = Quaternion.Euler(data2);
			}
			else
			{
				Vector3 data3 = data.eulerAngles;
				Serialize(ref data3);
			}
		}

		public void Serialize(ref FVector2 data)
		{
			Serialize(ref data.X);
			Serialize(ref data.Y);
		}

		public void Serialize(ref FVector3 data)
		{
			Serialize(ref data.X);
			Serialize(ref data.Y);
			Serialize(ref data.Z);
		}

		public void Dispose()
		{
		}

		public void SerializeFixed8(ref float value, int bits)
		{
			if (isReading)
			{
				byte value2 = _buffer.ReadByte();
				value = Fixed.FromFixed8(value2, bits);
			}
			else
			{
				byte value3 = Fixed.ToFixed8(value, bits);
				_buffer.WriteByte(value3);
			}
		}

		public void SerializeFixed16(ref float value, int bits)
		{
			if (isReading)
			{
				short value2 = _buffer.ReadInt16LE();
				value = Fixed.FromFixed16(value2, bits);
			}
			else
			{
				short value3 = Fixed.ToFixed16(value, bits);
				_buffer.WriteInt16LE(value3);
			}
		}

		public void SerializeUInt24(ref uint value)
		{
			if (isReading)
			{
				value = 0u;
				value += (uint)(_buffer.ReadByte() << 16);
				value += (uint)(_buffer.ReadByte() << 8);
				value += _buffer.ReadByte();
				return;
			}
			if (value > 16777215)
			{
				throw new Exception("uint too big to store in 24-bits");
			}
			uint num = (value & 0xFF0000) >> 16;
			_buffer.WriteByte((byte)num);
			num = (value & 0xFF00) >> 8;
			_buffer.WriteByte((byte)num);
			num = value & 0xFF;
			_buffer.WriteByte((byte)num);
		}

		public void Serialize(ref ViewId data)
		{
			Serialize(ref data.p);
			SerializeUInt24(ref data.n);
		}
	}
}
