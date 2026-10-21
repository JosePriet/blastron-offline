namespace EB
{
	public static class Fixed
	{
		public static short ToFixed16(float value, int bits)
		{
			int num = 1 << bits;
			return (short)(value * (float)num);
		}

		public static float FromFixed16(short value, int bits)
		{
			int num = 1 << bits;
			return (float)value / (float)num;
		}

		public static byte ToFixed8(float value, int bits)
		{
			int num = 1 << bits;
			return (byte)(value * (float)num);
		}

		public static float FromFixed8(byte value, int bits)
		{
			int num = 1 << bits;
			return (float)(int)value / (float)num;
		}
	}
}
