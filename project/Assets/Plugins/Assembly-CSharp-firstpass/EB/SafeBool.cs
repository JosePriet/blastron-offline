using System;

namespace EB
{
	public struct SafeBool
	{
		private SafeValue _v;

		public bool Value
		{
			get
			{
				return _v != null && BitConverter.ToBoolean(_v.Get(), 0);
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
			bool result = false;
			if (bool.TryParse(src.ToString(), out result))
			{
				Value = result;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj is bool)
			{
				return Value == (bool)obj;
			}
			if (obj is SafeBool)
			{
				return Value == ((SafeBool)obj).Value;
			}
			return obj.Equals(Value);
		}

		public override int GetHashCode()
		{
			return Value ? 1 : 0;
		}

		public override string ToString()
		{
			return Value.ToString();
		}

		public static implicit operator SafeBool(bool value)
		{
			return new SafeBool
			{
				Value = value
			};
		}

		public static implicit operator bool(SafeBool rhs)
		{
			return rhs.Value;
		}
	}
}
