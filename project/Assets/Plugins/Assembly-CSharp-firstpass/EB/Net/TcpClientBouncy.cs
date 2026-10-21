using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using UnityEngine;

namespace EB.Net
{
	public class TcpClientBouncy : ITcpClient
	{
		private class MyTlsAuthentication : TlsAuthentication
		{
			public void NotifyServerCertificate(Certificate serverCertificate)
			{
				X509CertificateStructure[] certs = serverCertificate.GetCerts();
				X509CertificateStructure[] array = certs;
				foreach (X509CertificateStructure x509CertificateStructure in array)
				{
					byte[] derEncoded = x509CertificateStructure.GetDerEncoded();
					string item = Encoding.ToBase64String(derEncoded);
					if (_validCerts.IndexOf(item) >= 0)
					{
						return;
					}
				}
				throw new TlsFatalAlert(AlertDescription.bad_certificate);
			}

			public TlsCredentials GetClientCredentials(CertificateRequest certificateRequest)
			{
				return null;
			}
		}

		private class MyTlsClient : DefaultTlsClient
		{
			private TlsAuthentication _auth;

			public MyTlsClient(TlsAuthentication auth)
			{
				_auth = auth;
			}

			public override TlsAuthentication GetAuthentication()
			{
				return _auth;
			}
		}

		private TcpClient _client;

		private NetworkStream _net;

		private TlsProtocolHandler _handler;

		private MyTlsAuthentication _auth;

		private MyTlsClient _tlsClient;

		private Stream _stream;

		private bool _secure;

		private IAsyncResult _async;

		private byte[] _readBuffer;

		private int _readCount;

		private int _readOffset;

		private static List<string> _validCerts = new List<string>();

		private static int RefCount = 0;

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
				if (_secure)
				{
					return _handler != null && _handler.Stream != null;
				}
				return true;
			}
		}

		public bool DataAvailable
		{
			get
			{
				if (_net != null)
				{
					return _net.DataAvailable;
				}
				return false;
			}
		}

		public DateTime LastTime { get; private set; }

		public TcpClientBouncy(bool secure)
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
				X509CertificateParser x509CertificateParser = new X509CertificateParser();
				X509Certificate x509Certificate = x509CertificateParser.ReadCertificate(certData);
				if (x509Certificate != null)
				{
					Debug.Log("Adding cert " + x509Certificate.SubjectDN.ToString());
					_validCerts.Add(Encoding.ToBase64String(x509Certificate.GetEncoded()));
				}
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
				Debug.Log("doing ssl connect");
				try
				{
					System.Random random = new System.Random();
					byte[] array = new byte[20];
					random.NextBytes(array);
					SecureRandom sr = new SecureRandom(array);
					_auth = new MyTlsAuthentication();
					_tlsClient = new MyTlsClient(_auth);
					_handler = new TlsProtocolHandler(_net, sr);
					_handler.Connect(_tlsClient);
					_stream = _handler.Stream;
					if (_stream == null)
					{
						Debug.LogError("stream is null");
						return false;
					}
				}
				catch (Exception ex)
				{
					Debug.LogError("ssl connect failed " + ex);
					return false;
				}
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
			IAsyncResult asyncResult = _stream.BeginWrite(buffer, offset, count, null, null);
			_stream.EndWrite(asyncResult);
		}
	}
}
