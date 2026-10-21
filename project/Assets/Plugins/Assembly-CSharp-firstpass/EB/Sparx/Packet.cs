namespace EB.Sparx
{
	public class Packet
	{
		public Channel Channel { get; set; }

		public Buffer Data { get; set; }

		public string Utf8Data
		{
			get
			{
				return Encoding.GetString(Data);
			}
		}

		public Packet(Channel channel, Buffer buffer)
		{
			Channel = channel;
			Data = buffer;
		}

		public Packet(Channel channel, byte[] data)
			: this(channel, new Buffer(data, true))
		{
		}

		public Packet(Channel channel, string message)
			: this(channel, Encoding.GetBytes(message))
		{
		}
	}
}
