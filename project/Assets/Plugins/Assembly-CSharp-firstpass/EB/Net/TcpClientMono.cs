using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

namespace EB.Net
{
	public class TcpClientMono : ITcpClient
	{
		private TcpClient _client;

		private NetworkStream _net;

		private SslStream _ssl;

		private Stream _stream;

		private bool _secure;

		private IAsyncResult _async;

		private byte[] _readBuffer;

		private int _readCount;

		private int _readOffset;

		private static int RefCount = 0;

		private static List<string> _validCerts = new List<string>();

		public int Available
		{
			get
			{
				return _readCount - _readOffset;
			}
		}

		public int ReadTimeout
		{
			get
			{
				return _client.ReceiveTimeout;
			}
			set
			{
				_client.ReceiveTimeout = value;
			}
		}

		public int WriteTimeout
		{
			get
			{
				return _client.SendTimeout;
			}
			set
			{
				_client.SendTimeout = value;
			}
		}

		public bool Connected
		{
			get
			{
				if (!_client.Connected)
				{
					return false;
				}
				if (_ssl != null)
				{
					return _ssl.IsAuthenticated;
				}
				return true;
			}
		}

		public bool DataAvailable
		{
			get
			{
				CheckRead();
				return Available > 0;
			}
		}

		public DateTime LastTime { get; private set; }

		public TcpClientMono(bool secure)
		{
			_client = new TcpClient();
			_client.NoDelay = true;
			_secure = secure;
			_readBuffer = new byte[4096];
			_readOffset = 0;
			_readCount = 0;
			RefCount++;
		}

		public static void AddCertificate(byte[] certData)
		{
			try
			{
				X509Certificate2 x509Certificate = new X509Certificate2(certData);
				string text = Encoding.ToHexString(x509Certificate.GetCertHash());
				_validCerts.Add(text);
				Debug.Log("got cert hash: " + text);
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to load certificate! " + ex);
			}
		}

		private bool TryConnect(string hostname, IPAddress ip, int port, int connectTimeout)
		{
			Debug.Log("Try connect " + ip);
			IAsyncResult asyncResult = _client.BeginConnect(ip, port, null, null);
			DateTime dateTime = DateTime.Now + TimeSpan.FromMilliseconds(connectTimeout);
			while (!asyncResult.IsCompleted && DateTime.Now < dateTime)
			{
				Thread.Sleep(100);
			}
			if (!asyncResult.IsCompleted)
			{
				return false;
			}
			_client.EndConnect(asyncResult);
			if (!_client.Connected)
			{
				Debug.LogError("Failed to connect to " + ip);
				return false;
			}
			_net = _client.GetStream();
			_stream = _net;
			if (_secure)
			{
				_ssl = new SslStream(_stream, true, RemoteCertificateValidationCallback, null);
				try
				{
					_ssl.AuthenticateAsClient(hostname);
				}
				catch (Exception ex)
				{
					Debug.LogError("Failed to authenticate: " + ex);
					return false;
				}
				_stream = _ssl;
			}
			Debug.Log("Connected to  " + ip);
			LastTime = DateTime.Now;
			return true;
		}

		public bool Connect(string host, int port, int connectTimeout)
		{
			IPAddress[] array = DNS.Lookup(host);
			if (array.Length == 0)
			{
				Debug.LogError("failed to lookup " + host);
			}
			IPAddress[] array2 = array;
			foreach (IPAddress iPAddress in array2)
			{
				if (TryConnect(host, iPAddress, port, connectTimeout))
				{
					DNS.StoreLast(host, iPAddress);
					return true;
				}
			}
			return false;
		}

		public void Close()
		{
			RefCount--;
			try
			{
				if (_ssl != null)
				{
					_ssl.Dispose();
				}
			}
			catch
			{
			}
			_ssl = null;
			try
			{
				if (_client != null)
				{
					IDisposable client = _client;
					client.Dispose();
				}
			}
			catch
			{
			}
			_client = null;
		}

		private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			string text = Encoding.ToHexString(certificate.GetCertHash());
			Debug.Log("checking cert hash: " + text);
			foreach (string validCert in _validCerts)
			{
				if (text == validCert)
				{
					return true;
				}
			}
			Debug.LogError("Cant find certificate in pinned certificate!");
			return false;
		}

		private void CheckRead()
		{
			if (_stream == null)
			{
				return;
			}
			if (_async != null)
			{
				if (_async.IsCompleted)
				{
					_readCount = _stream.EndRead(_async);
					_readOffset = 0;
					_async = null;
					if (_readCount > 0)
					{
						LastTime = DateTime.Now;
					}
				}
			}
			else if (Available == 0)
			{
				_async = _stream.BeginRead(_readBuffer, 0, _readBuffer.Length, null, null);
			}
		}

		public int Read(byte[] buffer, int offset, int count)
		{
			CheckRead();
			int num = 0;
			if (Available > 0)
			{
				num = Mathf.Min(count, Available);
				Array.Copy(_readBuffer, _readOffset, buffer, offset, num);
				_readOffset += num;
			}
			return num;
		}

		public void Write(byte[] buffer, int offset, int count)
		{
			_stream.Write(buffer, offset, count);
		}
	}
}
