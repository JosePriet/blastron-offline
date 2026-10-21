namespace EB
{
	public static class Hash
	{
		public static ulong HASH_PRIME_64 = 1099511628211uL;

		public static ulong HASH_INIT_64 = 14695981039346656037uL;

		public static uint HASH_PRIME_32 = 16777619u;

		public static uint HASH_INIT_32 = 2166136261u;

		public static long FNV64(byte[] data, long init)
		{
			ulong num = (ulong)init;
			foreach (byte b in data)
			{
				num *= HASH_PRIME_64;
				num ^= b;
			}
			return (long)num;
		}

		public static int StringHash(string data)
		{
			return FNV32(data);
		}

		public static int FNV32(string data)
		{
			return FNV32(data, (int)HASH_INIT_32);
		}

		public static int FNV32(string data, int init)
		{
			return FNV32(Encoding.GetBytes(data), init);
		}

		public static int FNV32(byte[] data, int init)
		{
			return FNV32(data, 0, data.Length, init);
		}

		public static int FNV32(byte[] data, int offset, int count, int init)
		{
			uint num = (uint)init;
			for (int i = 0; i < count; i++)
			{
				num *= HASH_PRIME_32;
				num ^= data[i + offset];
			}
			return (int)num;
		}
	}
}
