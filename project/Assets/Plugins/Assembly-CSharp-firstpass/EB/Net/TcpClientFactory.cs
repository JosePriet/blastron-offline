namespace EB.Net
{
	public static class TcpClientFactory
	{
		public static ITcpClient Create(bool secure)
		{
			return new TcpClientBouncy(secure);
		}
	}
}
