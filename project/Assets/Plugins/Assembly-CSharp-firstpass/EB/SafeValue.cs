using System;

namespace EB
{
	public class SafeValue
	{
		private byte[] _v;

		private long _h;

		private long _r;

		public static bool Breach;

		public SafeValue(byte[] v)
		{
			_v = v;
			_r = DateTime.Now.Ticks;
			_h = Hash.FNV64(_v, _r);
		}

		public byte[] Get()
		{
			long num = Hash.FNV64(_v, _r);
			if (num != _h)
			{
				Breach = true;
				return new byte[_v.Length];
			}
			return _v;
		}
	}
}
