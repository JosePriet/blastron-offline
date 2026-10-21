using System;

namespace EB
{
	public struct SafeInt
	{
		private SafeValue _v;

		public int Value
		{
			get
			{
				return (_v != null) ? BitConverter.ToInt32(_v.Get(), 0) : 0;
			}
			set
			{
				_v = new SafeValue(BitConverter.GetBytes(value));
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is int)
			{
				return Value == (int)obj;
			}
			if (obj is SafeInt)
			{
				return Value == ((SafeInt)obj).Value;
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

		public object Serialize()
		{
			return Value;
		}

		private void Deserialize(object src)
		{
			int result = 0;
			if (int.TryParse(src.ToString(), out result))
			{
				Value = result;
			}
		}

		public static implicit operator SafeInt(int value)
		{
			return new SafeInt
			{
				Value = value
			};
		}

		public static implicit operator int(SafeInt rhs)
		{
			return rhs.Value;
		}

		public static SafeInt operator +(SafeInt a, SafeInt b)
		{
			return a.Value + b.Value;
		}

		public static SafeInt operator -(SafeInt a, SafeInt b)
		{
			return a.Value - b.Value;
		}

		public static SafeInt operator --(SafeInt a)
		{
			return a.Value - 1;
		}

		public static SafeInt operator ++(SafeInt a)
		{
			return a.Value + 1;
		}
	}
}
