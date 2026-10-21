using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EB.Net
{
	public class WebSocket : IDisposable
	{
		public class WebSocketFrame
		{
			public bool fin;

			public bool rsv1;

			public bool rsv2;

			public bool rsv3;

			public int opcode;

			public int mask;

			public byte[] payload;
		}

		protected class Impl
		{
			public WebSocket _parent;

			public ITcpClient _client;

			public Uri _uri;

			public bool _running;

			public Thread _thread;

			public Random _random = new Random();

			public WebSocketState _state;

			public string _protocol;

			public int _ping;

			public DateTime _lastPing;

			public DateTime _lastRead;

			public DateTime _lastActivity;

			public string _key;

			public string _accept;

			public Dictionary<string, string> _responseHeaders = new Dictionary<string, string>();

			public Queue<WebSocketFrame> _writeQueue = new Queue<WebSocketFrame>();

			public Impl(WebSocket parent)
			{
				_parent = parent;
			}

			public void Connect(Uri uri, string protocol, byte[] key)
			{
				GenerateKeyAndAccept(key);
				_uri = uri;
				_protocol = protocol;
				_state = WebSocketState.Connecting;
				_running = true;
				_thread = new Thread(DoConnectSafe, 262144);
				_thread.Start();
			}

			public void QueueFrame(WebSocketFrame frame)
			{
				lock (_writeQueue)
				{
					_writeQueue.Enqueue(frame);
				}
			}

			private void Error(string err)
			{
				if (IsActive())
				{
					_parent.Error(err);
				}
			}

			private bool IsActive()
			{
				return _parent._impl == this;
			}

			private void OnConnect()
			{
				if (IsActive() && _parent.OnConnect != null)
				{
					_parent.OnConnect();
				}
			}

			private void OnMessage(string message)
			{
				if (IsActive() && _parent.OnMessage != null)
				{
					_parent.OnMessage(message);
				}
			}

			private void OnData(byte[] message)
			{
				if (IsActive() && _parent.OnData != null)
				{
					_parent.OnData(message);
				}
			}

			private void DoConnectSafe(object state)
			{
				try
				{
					Thread.CurrentThread.Name = "websocket";
				}
				catch
				{
				}
				try
				{
					if (DoConnect() && SendUpgradeRequest())
					{
						MainLoop();
					}
				}
				catch (ThreadAbortException)
				{
				}
				catch (Exception ex2)
				{
					Error("exception " + ex2.ToString());
				}
				_state = WebSocketState.None;
				Debug.Log("Disconnecting from socket " + _uri);
				try
				{
					if (_client != null)
					{
						_client.Close();
					}
				}
				catch
				{
				}
				_client = null;
				if (_parent != null)
				{
					_parent.DidClose(this);
				}
			}

			private bool DoConnect()
			{
				_client = TcpClientFactory.Create(_uri.Scheme == "wss");
				Debug.Log("connect " + _uri.Host + " " + _uri.Port);
				if (!_client.Connect(_uri.Host, _uri.Port, 5000))
				{
					_client.Close();
					_client = null;
					_state = WebSocketState.None;
					Error("failed to connect");
					return false;
				}
				_client.ReadTimeout = 5000;
				_client.WriteTimeout = 5000;
				if (!_running)
				{
					return false;
				}
				return true;
			}

			private void AddRange(List<byte> dst, byte[] src)
			{
				foreach (byte item in src)
				{
					dst.Add(item);
				}
			}

			private void GenerateKeyAndAccept(byte[] key)
			{
				if (key == null)
				{
					key = Crypto.RandomBytes(64);
				}
				_key = Convert.ToBase64String(key);
				List<byte> list = new List<byte>();
				AddRange(list, Encoding.GetBytes(_key));
				AddRange(list, Encoding.GetBytes("258EAFA5-E914-47DA-95CA-C5AB0DC85B11"));
				byte[] inArray = Digest.Sha1().Hash(list.ToArray());
				_accept = Convert.ToBase64String(inArray);
			}

			private string ReadLine(ITcpClient s)
			{
				List<byte> list = new List<byte>(64);
				byte[] array = new byte[1];
				while (true)
				{
					if (s.Read(array, 0, 1) == 0)
					{
						Thread.Sleep(10);
						continue;
					}
					byte b = array[0];
					switch (b)
					{
					case 13:
						continue;
					case 10:
						return Encoding.GetString(list.ToArray());
					}
					list.Add(b);
				}
			}

			private bool ParseHeader(string header)
			{
				int num = header.IndexOf(':');
				if (num < 0)
				{
					Debug.LogError("Failed to parse header: " + header);
					return false;
				}
				string key = header.Substring(0, num).Trim().ToLower();
				string value = header.Substring(num + 1).Trim();
				_responseHeaders[key] = value;
				return true;
			}

			private string GetResponseHeader(string key)
			{
				string value;
				if (_responseHeaders.TryGetValue(key.ToLower(), out value))
				{
					return value;
				}
				return string.Empty;
			}

			private byte[] Read(int count)
			{
				byte[] array = new byte[count];
				int num = 0;
				while (num < count)
				{
					if (!_running)
					{
						throw new Exception("Read aborted");
					}
					int num2 = _client.Read(array, num, count - num);
					if (num2 < 0)
					{
						throw new Exception("Read Failed");
					}
					num += num2;
					if (num2 == 0)
					{
						Thread.Sleep(10);
					}
				}
				return array;
			}

			private short ReadInt16()
			{
				byte[] array = Read(2);
				Array.Reverse(array);
				return BitConverter.ToInt16(array, 0);
			}

			private int ReadInt32()
			{
				byte[] array = Read(4);
				Array.Reverse(array);
				return BitConverter.ToInt32(array, 0);
			}

			private long ReadInt64()
			{
				byte[] array = Read(8);
				Array.Reverse(array);
				return BitConverter.ToInt64(array, 0);
			}

			private byte[] WriteInt16(short value)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				return bytes;
			}

			private byte[] WriteInt32(int value)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				return bytes;
			}

			private byte[] WriteInt64(long value)
			{
				byte[] bytes = BitConverter.GetBytes(value);
				Array.Reverse(bytes);
				return bytes;
			}

			private WebSocketFrame ReadFrame()
			{
				WebSocketFrame webSocketFrame = new WebSocketFrame();
				byte[] array = Read(2);
				byte b = array[0];
				byte b2 = array[1];
				webSocketFrame.fin = (b & 0x80) != 0;
				webSocketFrame.rsv1 = (b & 0x40) != 0;
				webSocketFrame.rsv2 = (b & 0x20) != 0;
				webSocketFrame.rsv3 = (b & 0x10) != 0;
				webSocketFrame.opcode = b & 0xF;
				bool flag = (b2 & 0x80) != 0;
				int num = b2 & 0x7F;
				if ((webSocketFrame.opcode & 8) != 0 && num > 125)
				{
					throw new Exception("Protocol error, control frame not allow to have payload > 125");
				}
				switch (num)
				{
				case 126:
					num = ReadInt16();
					break;
				case 127:
					num = (int)ReadInt64();
					break;
				}
				if (flag)
				{
					webSocketFrame.mask = ReadInt32();
				}
				webSocketFrame.payload = Read(num);
				return webSocketFrame;
			}

			private void WriteFrame(WebSocketFrame frame)
			{
				List<byte> list = new List<byte>();
				byte b = (byte)frame.opcode;
				if (frame.fin)
				{
					b |= 0x80;
				}
				list.Add(b);
				int num = ((frame.payload != null) ? frame.payload.Length : 0);
				if (num <= 125)
				{
					list.Add((byte)(0x80 | num));
				}
				else if (num < 32767)
				{
					list.Add(254);
					AddRange(list, WriteInt16((short)num));
				}
				else
				{
					list.Add(byte.MaxValue);
					AddRange(list, WriteInt64(num));
				}
				frame.mask = _random.Next();
				byte[] array = WriteInt32(frame.mask);
				AddRange(list, array);
				if (frame.payload != null)
				{
					for (int i = 0; i < frame.payload.Length; i++)
					{
						list.Add((byte)(frame.payload[i] ^ array[i % 4]));
					}
				}
				_client.Write(list.ToArray(), 0, list.Count);
			}

			private bool SendUpgradeRequest()
			{
				string hostAndPort = _uri.HostAndPort;
				string text = "http://" + hostAndPort;
				string str = "GET " + _uri.PathAndQuery + " HTTP/1.1\r\nUpgrade: WebSocket\r\nConnection: Upgrade\r\nHost: " + hostAndPort + "\r\nOrigin: " + text + "\r\nSec-WebSocket-Key: " + _key + "\r\nSec-WebSocket-Protocol: " + _protocol + "\r\nSec-WebSocket-Version: 13\r\n\r\n";
				byte[] bytes = Encoding.GetBytes(str);
				_client.Write(bytes, 0, bytes.Length);
				string text2 = ReadLine(_client);
				if (text2 != "HTTP/1.1 101 Switching Protocols")
				{
					throw new IOException("Invalid handshake response: " + text2);
				}
				while (true)
				{
					if (!_running)
					{
						return false;
					}
					string text3 = ReadLine(_client);
					if (text3.Length == 0)
					{
						break;
					}
					if (!ParseHeader(text3))
					{
						Debug.LogError("Failed to parse header: " + text3);
						break;
					}
				}
				if (GetResponseHeader("connection").ToLower() != "upgrade")
				{
					throw new IOException("Unknow connection header: " + GetResponseHeader("connection"));
				}
				if (GetResponseHeader("upgrade").ToLower() != "websocket")
				{
					throw new IOException("Unknow upgrade header: " + GetResponseHeader("upgrade"));
				}
				if (GetResponseHeader("Sec-WebSocket-Accept") != _accept)
				{
					throw new IOException("Unknow connection accpet key " + GetResponseHeader("Sec-WebSocket-Accept"));
				}
				_state = WebSocketState.Open;
				OnConnect();
				return true;
			}

			private int Since(DateTime dt)
			{
				return (int)(DateTime.Now - dt).TotalMilliseconds;
			}

			private bool NeedPing()
			{
				if (_parent.PingTimeout == 0)
				{
					return false;
				}
				int num = Since(_lastPing);
				int num2 = _parent.PingTimeout / 2;
				if (num > num2)
				{
					return true;
				}
				return false;
			}

			private void CheckTimeout()
			{
				if (_parent.PingTimeout > 0 && Since(_lastRead) > _parent.PingTimeout)
				{
					throw new IOException("connection timed out");
				}
				if (_parent.ActivityTimeout > 0 && Since(_lastActivity) > _parent.ActivityTimeout)
				{
					Debug.Log("Closing idle connection");
					_running = false;
				}
			}

			private void SendPing()
			{
				_lastPing = DateTime.Now;
				WebSocketFrame webSocketFrame = new WebSocketFrame();
				webSocketFrame.fin = true;
				webSocketFrame.opcode = 9;
				WriteFrame(webSocketFrame);
			}

			private void MainLoop()
			{
				_lastPing = DateTime.Now;
				_lastActivity = DateTime.Now;
				_lastRead = DateTime.Now;
				while (_running)
				{
					WebSocketFrame webSocketFrame = null;
					lock (_writeQueue)
					{
						if (_writeQueue.Count > 0)
						{
							webSocketFrame = _writeQueue.Dequeue();
						}
					}
					if (!_client.Connected)
					{
						Error("Lost connection");
						break;
					}
					if (webSocketFrame != null)
					{
						_lastActivity = DateTime.Now;
						WriteFrame(webSocketFrame);
					}
					else if (_client.DataAvailable)
					{
						webSocketFrame = ReadFrame();
						switch (webSocketFrame.opcode)
						{
						case 1:
							_lastActivity = DateTime.Now;
							OnMessage(Encoding.GetString(webSocketFrame.payload));
							break;
						case 2:
							_lastActivity = DateTime.Now;
							OnData(webSocketFrame.payload);
							break;
						case 9:
						{
							WebSocketFrame webSocketFrame2 = new WebSocketFrame();
							webSocketFrame2.fin = true;
							webSocketFrame2.opcode = 10;
							WriteFrame(webSocketFrame2);
							break;
						}
						case 10:
							_ping = Since(_lastPing);
							break;
						}
						_lastPing = DateTime.Now;
						_lastRead = DateTime.Now;
					}
					else if (NeedPing())
					{
						SendPing();
					}
					else
					{
						CheckTimeout();
						Thread.Sleep(10);
					}
				}
			}
		}

		private Impl _impl;

		public WebSocketState State
		{
			get
			{
				return (_impl != null) ? _impl._state : WebSocketState.None;
			}
		}

		public int PingTimeout { get; set; }

		public int ActivityTimeout { get; set; }

		public int Ping
		{
			get
			{
				return (_impl != null) ? _impl._ping : 0;
			}
		}

		public event Action OnConnect;

		public event Action<string> OnError;

		public event Action<string> OnMessage;

		public event Action<byte[]> OnData;

		public WebSocket()
		{
			PingTimeout = 10000;
		}

		private void DisposeImpl()
		{
			if (_impl != null)
			{
				_impl._running = false;
				_impl = null;
			}
		}

		private void QueueFrame(WebSocketFrame frame)
		{
			if (_impl != null)
			{
				_impl.QueueFrame(frame);
			}
			else
			{
				Error("sending without connecting");
			}
		}

		public virtual void Dispose()
		{
			OnError = null;
			OnMessage = null;
			OnData = null;
			DisposeImpl();
		}

		public void ConnectAsync(Uri uri, string protocol, byte[] key)
		{
			DisposeImpl();
			_impl = new Impl(this);
			_impl.Connect(uri, protocol, key);
		}

		public void SendUTF8(string message)
		{
			WebSocketFrame webSocketFrame = new WebSocketFrame();
			webSocketFrame.fin = true;
			webSocketFrame.opcode = 1;
			webSocketFrame.payload = Encoding.GetBytes(message);
			QueueFrame(webSocketFrame);
		}

		public void SendBinary(byte[] binary)
		{
			WebSocketFrame webSocketFrame = new WebSocketFrame();
			webSocketFrame.fin = true;
			webSocketFrame.opcode = 2;
			webSocketFrame.payload = binary;
			QueueFrame(webSocketFrame);
		}

		public void SendBinary(ArraySegment<byte> segment)
		{
			WebSocketFrame webSocketFrame = new WebSocketFrame();
			webSocketFrame.fin = true;
			webSocketFrame.opcode = 2;
			webSocketFrame.payload = new byte[segment.Count];
			Array.Copy(segment.Array, segment.Offset, webSocketFrame.payload, 0, segment.Count);
			QueueFrame(webSocketFrame);
		}

		protected virtual void Error(string err)
		{
			Debug.LogError("WebSocketError: " + err);
			DisposeImpl();
			if (OnError != null)
			{
				OnError(err);
			}
		}

		public void SendBinary(Buffer buffer)
		{
			SendBinary(buffer.ToArraySegment(false));
		}

		protected virtual void DidClose(Impl impl)
		{
		}
	}
}
