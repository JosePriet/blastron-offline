using UnityEngine;

namespace EB.Sparx
{
	public static class Nonce
	{
		private static short inc;

		private static short pid;

		private static int device;

		public static string Generate()
		{
			Buffer buffer = new Buffer(12);
			buffer.WriteInt32BE(Time.Now);
			if (device == 0)
			{
				device = Hash.FNV32(Device.UniqueIdentifier);
			}
			buffer.WriteInt32BE(device);
			if (pid == 0)
			{
				pid = (short)Random.Range(1, 32767);
			}
			buffer.WriteInt16BE(pid);
			buffer.WriteInt16BE(inc++);
			return buffer.ToHexString().ToLower();
		}
	}
}
