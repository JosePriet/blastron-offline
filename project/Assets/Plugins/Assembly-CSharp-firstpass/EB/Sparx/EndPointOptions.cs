namespace EB.Sparx
{
	public struct EndPointOptions
	{
		public byte[] Key;

		public string Protocol;

		public bool KeepAlive;

		public string KeepAliveUrl;

		public int KeepAliveInterval;

		public int ActivityTimeout;
	}
}
