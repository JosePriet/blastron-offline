using System;
using Org.BouncyCastle.Crypto;

namespace EB
{
	public class FNV64_Digest : IDigest
	{
		private ulong _hash = Hash.HASH_INIT_64;

		public string AlgorithmName
		{
			get
			{
				return "FVN64";
			}
		}

		public int GetDigestSize()
		{
			return 8;
		}

		public int GetByteLength()
		{
			return 1;
		}

		public void Update(byte b)
		{
			_hash *= Hash.HASH_PRIME_64;
			_hash ^= b;
		}

		public void BlockUpdate(byte[] input, int inOff, int length)
		{
			for (int i = 0; i < length; i++)
			{
				_hash *= Hash.HASH_PRIME_64;
				_hash ^= input[i + inOff];
			}
		}

		public int DoFinal(byte[] output, int outOff)
		{
			byte[] bytes = BitConverter.GetBytes(_hash);
			Array.Copy(bytes, 0, output, outOff, bytes.Length);
			return GetDigestSize();
		}

		public void Reset()
		{
			_hash = Hash.HASH_INIT_64;
		}
	}
}
