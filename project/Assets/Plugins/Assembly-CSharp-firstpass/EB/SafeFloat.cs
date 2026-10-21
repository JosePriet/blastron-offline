using System;

namespace EB
{
	public struct SafeFloat
	{
		private SafeValue _v;

		public float Value
		{
			get
			{
				return (_v == null) ? 0f : BitConverter.ToSingle(_v.Get(), 0);
			}
			set
			{
				_v = new SafeValue(BitConverter.GetBytes(value));
			}
		}

		public object Serialize()
		{
			return Value;
		}

		private void Deserialize(object src)
		{
			float result = 0f;
			if (float.TryParse(src.ToString(), out result))
			{
				Value = result;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is float)
			{
				return Value == (float)obj;
			}
			if (obj is SafeFloat)
			{
				return Value == ((SafeFloat)obj).Value;
			}
			return obj.Equals(Value);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static implicit operator SafeFloat(float value)
		{
			return new SafeFloat
			{
				Value = value
			};
		}

		public static implicit operator float(SafeFloat rhs)
		{
			return rhs.Value;
		}

		public static SafeFloat operator +(SafeFloat a, SafeFloat b)
		{
			return a.Value + b.Value;
		}

		public static SafeFloat operator -(SafeFloat a, SafeFloat b)
		{
			return a.Value - b.Value;
		}

		public static SafeFloat operator --(SafeFloat a)
		{
			return a.Value - 1f;
		}

		public static SafeFloat operator ++(SafeFloat a)
		{
			return a.Value + 1f;
		}
	}
}
