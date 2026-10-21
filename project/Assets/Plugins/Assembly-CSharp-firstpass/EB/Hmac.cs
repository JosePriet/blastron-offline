using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;

namespace EB
{
	public class Hmac
	{
		private KeyParameter _key;

		private Digest _digest;

		private IMac _mac;

		public int DigestSize
		{
			get
			{
				return _digest.DigestSize;
			}
		}

		public byte[] Key
		{
			get
			{
				return _key.GetKey();
			}
		}

		public Hmac(Digest digest, byte[] key)
		{
			_digest = digest;
			_key = new KeyParameter(key);
			_mac = new HMac(_digest.Implementation);
			_mac.Init(_key);
		}

		public void Reset()
		{
			_mac.Reset();
		}

		public void Update(byte[] data)
		{
			Update(data, 0, data.Length);
		}

		public void Update(byte[] data, int offset, int length)
		{
			_mac.BlockUpdate(data, offset, length);
		}

		public byte[] Final()
		{
			byte[] array = new byte[DigestSize];
			_mac.DoFinal(array, 0);
			return array;
		}

		public byte[] Hash(byte[] data)
		{
			Update(data);
			return Final();
		}

		public static Hmac Sha1(byte[] key)
		{
			return new Hmac(Digest.Sha1(), key);
		}

		public static Hmac MD5(byte[] key)
		{
			return new Hmac(Digest.MD5(), key);
		}
	}
}
