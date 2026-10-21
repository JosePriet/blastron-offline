using System;

namespace EB.Net
{
	public interface ITcpClient
	{
		int ReadTimeout { get; set; }

		int WriteTimeout { get; set; }

		DateTime LastTime { get; }

		bool Connected { get; }

		bool DataAvailable { get; }

		bool Connect(string host, int port, int connectTimeout);

		void Close();

		int Read(byte[] buffer, int index, int count);

		void Write(byte[] buffer, int index, int count);
	}
}
