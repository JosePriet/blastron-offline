using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EB.Director.Serialization
{
	[System.Serializable]
	public class KeyFrame
	{
		public int frame;

		public BlendMode mode = BlendMode.Linear;

		public byte[] data = new byte[0];

		private static System.Text.Encoding _encoding = new UTF8Encoding();

		public bool BoolValue
		{
			get
			{
				if (data.Length > 0)
				{
					return data[0] != 0;
				}
				return false;
			}
			set
			{
				data = new byte[1] { (byte)(value ? 1 : 0) };
			}
		}

		public byte ByteValue
		{
			get
			{
				if (data.Length > 0)
				{
					return data[0];
				}
				return 0;
			}
			set
			{
				data = new byte[1] { value };
			}
		}

		public float FloatValue
		{
			get
			{
				if (data.Length > 0)
				{
					return BitConverter.ToSingle(data, 0);
				}
				return 0f;
			}
			set
			{
				data = BitConverter.GetBytes(value);
			}
		}

		public Vector2 Vector2Value
		{
			get
			{
				if (data.Length > 0)
				{
					return new Vector2(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4));
				}
				return Vector2.zero;
			}
			set
			{
				List<byte> list = new List<byte>(8);
				list.AddRange(BitConverter.GetBytes(value.x));
				list.AddRange(BitConverter.GetBytes(value.y));
				data = list.ToArray();
			}
		}

		public Vector3 Vector3Value
		{
			get
			{
				if (data.Length > 0)
				{
					return new Vector3(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8));
				}
				return Vector3.zero;
			}
			set
			{
				List<byte> list = new List<byte>(12);
				list.AddRange(BitConverter.GetBytes(value.x));
				list.AddRange(BitConverter.GetBytes(value.y));
				list.AddRange(BitConverter.GetBytes(value.z));
				data = list.ToArray();
			}
		}

		public Vector4 Vector4Value
		{
			get
			{
				if (data.Length > 0)
				{
					return new Vector4(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
				}
				return Vector4.zero;
			}
			set
			{
				List<byte> list = new List<byte>(16);
				list.AddRange(BitConverter.GetBytes(value.x));
				list.AddRange(BitConverter.GetBytes(value.y));
				list.AddRange(BitConverter.GetBytes(value.z));
				list.AddRange(BitConverter.GetBytes(value.w));
				data = list.ToArray();
			}
		}

		public Color ColorValue
		{
			get
			{
				if (data.Length > 0)
				{
					return new Color(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
				}
				return new Color(0f, 0f, 0f, 0f);
			}
			set
			{
				List<byte> list = new List<byte>(16);
				list.AddRange(BitConverter.GetBytes(value.r));
				list.AddRange(BitConverter.GetBytes(value.g));
				list.AddRange(BitConverter.GetBytes(value.b));
				list.AddRange(BitConverter.GetBytes(value.a));
				data = list.ToArray();
			}
		}

		public QuatPos QuatPosValue
		{
			get
			{
				if (data.Length > 0)
				{
					QuatPos result = default(QuatPos);
					result.quat = new Quaternion(BitConverter.ToSingle(data, 0), BitConverter.ToSingle(data, 4), BitConverter.ToSingle(data, 8), BitConverter.ToSingle(data, 12));
					result.pos = new Vector3(BitConverter.ToSingle(data, 16), BitConverter.ToSingle(data, 20), BitConverter.ToSingle(data, 24));
					return result;
				}
				return default(QuatPos);
			}
			set
			{
				List<byte> list = new List<byte>(28);
				list.AddRange(BitConverter.GetBytes(value.quat.x));
				list.AddRange(BitConverter.GetBytes(value.quat.y));
				list.AddRange(BitConverter.GetBytes(value.quat.z));
				list.AddRange(BitConverter.GetBytes(value.quat.w));
				list.AddRange(BitConverter.GetBytes(value.pos.x));
				list.AddRange(BitConverter.GetBytes(value.pos.y));
				list.AddRange(BitConverter.GetBytes(value.pos.z));
				data = list.ToArray();
			}
		}

		public string StringValue
		{
			get
			{
				if (data.Length > 0)
				{
					return _encoding.GetString(data);
				}
				return string.Empty;
			}
			set
			{
				data = _encoding.GetBytes(value);
			}
		}

		public KeyFrame Clone()
		{
			KeyFrame keyFrame = new KeyFrame();
			keyFrame.frame = frame;
			keyFrame.mode = mode;
			keyFrame.data = (byte[])data.Clone();
			return keyFrame;
		}
	}
}
