using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Ionic.Zlib;
using UnityEngine;

namespace EB.Net
{
	public class WebRequest
	{
		private const int kReadTimeout = 25;

		private const int kConnectionTimeout = 45;

		private string _url;

		private Uri _uri;

		private Dictionary<string, string> _requestHeaders = new Dictionary<string, string>();

		private Dictionary<string, string> _responseHeaders = new Dictionary<string, string>();

		private string _method;

		private byte[] _body;

		private string _error;

		private bool _done;

		private List<byte> _data = new List<byte>();

		private DateTime _requestSent;

		private DateTime _headersReceived;

		private static Dictionary<string, List<ITcpClient>> _connections = new Dictionary<string, List<ITcpClient>>();

		private static int _randomSeed = 0;

		private static int _connectionFailures = 0;

		private static ThreadPool _pool;

		private static TimeSpan _span = TimeSpan.FromSeconds(25.0);

		public string url
		{
			get
			{
				return _url;
			}
		}

		public Dictionary<string, string> responseHeaders
		{
			get
			{
				return _responseHeaders;
			}
		}

		public bool isDone
		{
			get
			{
				return _done;
			}
		}

		public Action<byte[]> OnDataCallback { get; set; }

		public Action<WebRequest> OnHeaders { get; set; }

		public int statusCode { get; set; }

		public string statusText { get; set; }

		public byte[] bytes
		{
			get
			{
				return _data.ToArray();
			}
		}

		public int size
		{
			get
			{
				return _data.Count;
			}
		}

		public int responseTime
		{
			get
			{
				if (statusCode == 200)
				{
					return (int)(_headersReceived - _requestSent).TotalMilliseconds;
				}
				return -1;
			}
		}

		public string text
		{
			get
			{
				if (_data != null && _data.Count > 0)
				{
					return System.Text.Encoding.UTF8.GetString(_data.ToArray());
				}
				return null;
			}
		}

		public string error
		{
			get
			{
				if (_error != null)
				{
					return _error;
				}
				if (statusCode != 200)
				{
					return string.Format("Non-200 error ({0})", statusCode);
				}
				return null;
			}
		}

		private string connectionKey
		{
			get
			{
				return _uri.HostAndPort;
			}
		}

		public WebRequest(string url)
		{
			url = GetUrl(url);
			_url = url;
			_uri = new Uri(url);
			_method = "GET";
		}

		public WebRequest(string url, byte[] postData, Hashtable headers)
		{
			url = GetUrl(url);
			_url = url;
			_uri = new Uri(url);
			_method = "POST";
			foreach (DictionaryEntry header in headers)
			{
				_requestHeaders[header.Key.ToString()] = header.Value.ToString();
			}
			_requestHeaders["Content-Length"] = postData.Length.ToString();
			_body = postData;
		}

		public void Dispose()
		{
		}

		private string GetUrl(string url)
		{
			return url;
		}

		public void Start()
		{
			if (_randomSeed == 0)
			{
				_randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
			}
			if (_pool == null)
			{
				_pool = new ThreadPool(1);
			}
			_pool.Queue(ThreadTask, this);
		}

		private void ThreadTask(object ignore)
		{
			ITcpClient tcpClient = null;
			try
			{
				tcpClient = GetClient();
				if (Connect(tcpClient))
				{
					if (WriteRequest(tcpClient))
					{
						ReadResponse(tcpClient);
					}
				}
				else
				{
					_connectionFailures++;
				}
			}
			catch (Exception ex)
			{
				_connectionFailures++;
				Error(ex.Message);
			}
			if (tcpClient != null)
			{
				ReturnClient(tcpClient);
			}
			_done = true;
		}

		private ITcpClient GetClient()
		{
			lock (_connections)
			{
				List<ITcpClient> value = null;
				if (_connections.TryGetValue(connectionKey, out value))
				{
					value.RemoveAll((ITcpClient client) =>
					{
						if (!client.Connected || Time.Since(client.LastTime) >= 45)
						{
							client.Close();
							return true;
						}
						return false;
					});
					if (value.Count > 0)
					{
						ITcpClient result = value[value.Count - 1];
						value.RemoveAt(value.Count - 1);
						return result;
					}
				}
			}
			return TcpClientFactory.Create(_uri.Scheme == "https");
		}

		private void ReturnClient(ITcpClient client)
		{
			try
			{
				if (statusCode == 200 && client.Connected && GetResponseHeader("Connection") != "close" && string.IsNullOrEmpty(_error))
				{
					lock (_connections)
					{
						List<ITcpClient> value = null;
						if (!_connections.TryGetValue(connectionKey, out value))
						{
							value = new List<ITcpClient>();
							_connections[connectionKey] = value;
						}
						value.Add(client);
						return;
					}
				}
				client.Close();
			}
			catch
			{
			}
		}

		private bool Connect(ITcpClient client)
		{
			if (client.Connected)
			{
				return true;
			}
			Debug.Log("Connecting to " + _uri.Host + " on port " + _uri.Port);
			if (!client.Connect(_uri.Host, _uri.Port, 5000))
			{
				client.Close();
				Error("Connect failed");
				return false;
			}
			client.ReadTimeout = 25000;
			client.WriteTimeout = 10000;
			_connectionFailures = 0;
			return true;
		}

		private void Error(string fmt, params object[] args)
		{
			_randomSeed = 0;
			if (args.Length > 0)
			{
				_error = string.Format(fmt, args);
			}
			else
			{
				_error = fmt;
			}
			Debug.LogError("http error: (" + url + "): " + _error);
		}

		private void AddData(byte[] bytes)
		{
			if (OnDataCallback != null)
			{
				OnDataCallback(bytes);
			}
			else if (bytes != null && bytes.Length > 0)
			{
				foreach (byte item in bytes)
				{
					_data.Add(item);
				}
			}
		}

		private byte[] Decompress(byte[] src)
		{
			MemoryStream stream = new MemoryStream(src);
			GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
			MemoryStream memoryStream = new MemoryStream(src.Length * 5);
			byte[] array = new byte[4096];
			while (true)
			{
				int num = gZipStream.Read(array, 0, array.Length);
				if (num == 0)
				{
					break;
				}
				memoryStream.Write(array, 0, num);
			}
			return memoryStream.ToArray();
		}

		private bool WriteRequest(ITcpClient stream)
		{
			if (!WriteHeaders(stream))
			{
				return false;
			}
			if (_body != null && _body.Length > 0)
			{
				return WriteRequestBody(stream);
			}
			_requestSent = DateTime.Now;
			return true;
		}

		private bool WriteHeaders(ITcpClient stream)
		{
			_requestHeaders["Host"] = _uri.Host;
			if (OnDataCallback == null)
			{
				_requestHeaders["Accept-Encoding"] = "gzip";
			}
			_requestHeaders["User-Agent"] = "Sparx/1.0";
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("{0} {1} HTTP/1.1\r\n", _method, _uri.PathAndQuery);
			foreach (KeyValuePair<string, string> requestHeader in _requestHeaders)
			{
				stringBuilder.AppendFormat("{0}:{1}\r\n", requestHeader.Key, requestHeader.Value);
			}
			stringBuilder.Append("\r\n");
			byte[] array = System.Text.Encoding.UTF8.GetBytes(stringBuilder.ToString());
			stream.Write(array, 0, array.Length);
			return true;
		}

		private bool WriteRequestBody(ITcpClient stream)
		{
			stream.Write(_body, 0, _body.Length);
			return true;
		}

		private bool ReadResponse(ITcpClient stream)
		{
			string text = ReadLine(stream);
			if (!ParseStatus(text))
			{
				Error("Failed to parse status header: " + text);
				return false;
			}
			while (true)
			{
				string text2 = ReadLine(stream);
				if (text2.Length == 0)
				{
					break;
				}
				if (!ParseHeader(text2))
				{
					Error("Failed to parse header: " + text2);
					return false;
				}
			}
			if (OnHeaders != null)
			{
				OnHeaders(this);
			}
			_headersReceived = DateTime.Now;
			bool flag = false;
			flag = ((!(GetResponseHeader("Transfer-Encoding") == "chunked")) ? ReadBody(stream) : ReadBodyChunked(stream));
			if (flag && GetResponseHeader("Content-Encoding") == "gzip")
			{
				byte[] array = _data.ToArray();
				_data = new List<byte>(array.Length * 5);
				byte[] array2 = Decompress(array);
				foreach (byte item in array2)
				{
					_data.Add(item);
				}
			}
			return flag;
		}

		private bool ReadBody(ITcpClient stream)
		{
			string responseHeader = GetResponseHeader("Content-Length");
			if (string.IsNullOrEmpty(responseHeader))
			{
				if (!(GetResponseHeader("Connection") == "close"))
				{
					Error("Missing Content-Length header!");
					return false;
				}
				List<byte> list = new List<byte>();
				byte[] array = new byte[1024];
				while (true)
				{
					try
					{
						int num = stream.Read(array, 0, array.Length);
						if (num > 0)
						{
							for (int i = 0; i < num; i++)
							{
								list.Add(array[i]);
							}
						}
					}
					catch
					{
						break;
					}
				}
			}
			int result = 0;
			if (!int.TryParse(responseHeader, out result))
			{
				Error("Failed to read body, content length header is missing");
				return false;
			}
			if (result == 0)
			{
				Debug.LogWarning("Zero length response for url " + url);
				return true;
			}
			int b = 65536;
			while (result > 0)
			{
				int num2 = Mathf.Min(result, b);
				AddData(Read(stream, num2));
				result -= num2;
			}
			return true;
		}

		private bool ReadBodyChunked(ITcpClient stream)
		{
			int num;
			do
			{
				num = ReadChunk(stream);
				if (num < 0)
				{
					return false;
				}
			}
			while (num != 0);
			return true;
		}

		private int ReadChunk(ITcpClient stream)
		{
			string text = ReadLine(stream);
			int num = 0;
			try
			{
				num = int.Parse(text, NumberStyles.HexNumber);
			}
			catch
			{
				Error("Failed to decoded chunked response, line:" + text);
				return -1;
			}
			if (num == 0)
			{
				ReadLine(stream);
				return 0;
			}
			byte[] array = Read(stream, num);
			AddData(array);
			ReadLine(stream);
			return array.Length;
		}

		private byte[] Read(ITcpClient stream, int count)
		{
			byte[] array = new byte[count];
			int num = 0;
			DateTime dateTime = DateTime.Now + _span;
			while (num < count)
			{
				int num2 = stream.Read(array, num, count - num);
				if (num2 < 0)
				{
					throw new IOException("Read Failed");
				}
				if (num2 == 0)
				{
					if (DateTime.Now > dateTime)
					{
						throw new IOException("Read Timed out");
					}
					Thread.Sleep(TimeSpan.FromMilliseconds(1.0));
				}
				else
				{
					dateTime = DateTime.Now + _span;
					num += num2;
				}
			}
			return array;
		}

		private string ReadLine(ITcpClient s)
		{
			List<byte> list = new List<byte>(64);
			while (true)
			{
				byte[] array = Read(s, 1);
				byte b = array[0];
				switch (b)
				{
				case 13:
					break;
				default:
					goto IL_002a;
				case 10:
					return Encoding.GetString(list.ToArray());
				}
				continue;
				IL_002a:
				list.Add(b);
			}
		}

		public string GetResponseHeader(string key)
		{
			string value;
			if (_responseHeaders.TryGetValue(key.ToLower(), out value))
			{
				return value;
			}
			return string.Empty;
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

		private bool ParseStatus(string status)
		{
			string[] array = status.Split(' ');
			if (array.Length < 3)
			{
				return false;
			}
			int result = 0;
			if (!int.TryParse(array[1], out result))
			{
				return false;
			}
			statusCode = result;
			statusText = array[2];
			for (int i = 3; i < array.Length; i++)
			{
				statusText = statusText + " " + array[3];
			}
			return true;
		}
	}
}
