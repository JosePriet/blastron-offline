using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace EB
{
	public class Digest
	{
		private IDigest _digest;

		public IDigest Implementation
		{
			get
			{
				return _digest;
			}
		}

		public int DigestSize
		{
			get
			{
				return _digest.GetDigestSize();
			}
		}

		public Digest(IDigest digest)
		{
			_digest = digest;
		}

		public Digest Reset()
		{
			_digest.Reset();
			return this;
		}

		public Digest Update(byte[] data)
		{
			Update(data, 0, data.Length);
			return this;
		}

		public Digest Update(byte[] data, int offset, int length)
		{
			_digest.BlockUpdate(data, offset, length);
			return this;
		}

		public byte[] Final()
		{
			byte[] array = new byte[DigestSize];
			_digest.DoFinal(array, 0);
			return array;
		}

		public byte[] Hash(byte[] data)
		{
			Update(data);
			return Final();
		}

		public static Digest Sha1()
		{
			return new Digest(new Sha1Digest());
		}

		public static Digest MD5()
		{
			return new Digest(new MD5Digest());
		}

		public static Digest FNV32()
		{
			return new Digest(new FNV32_Digest());
		}

		public static Digest FNV64()
		{
			return new Digest(new FNV64_Digest());
		}
	}
}
