namespace EB.Sparx
{
	public class Id
	{
		public static Id Null = new Id(0);

		public static Id Offline = new Id(-1);

		private long _value;

		public long Value
		{
			get
			{
				return _value;
			}
		}

		public bool Valid
		{
			get
			{
				return _value != 0;
			}
		}

		public Id()
		{
			_value = 0L;
		}

		public Id(object value)
		{
			if (!long.TryParse((value == null) ? string.Empty : value.ToString(), out _value))
			{
				_value = 0L;
			}
		}

		public override bool Equals(object obj)
		{
			if (obj != null)
			{
				Id id = new Id(obj);
				return id._value == _value;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Hash.StringHash(ToString());
		}

		public override string ToString()
		{
			return _value.ToString();
		}

		public static bool operator ==(Id a, Id b)
		{
			Id id = a ?? Null;
			Id id2 = b ?? Null;
			return id._value == id2._value;
		}

		public static bool operator !=(Id a, Id b)
		{
			return !(a == b);
		}
	}
}
