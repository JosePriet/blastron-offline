using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace EB.Sparx
{
	public class QosProbe : IDisposable
	{
		private UdpClient _client;

		private int _sent;

		private int _received;

		private int _size;

		private int _num;

		private string _host;

		private int _port;

		private int _timeout;

		private IPAddress _hostEntry;

		private byte[] _data;

		private float _sum;

		private float _min;

		private float _max;

		public int AvgPing
		{
			get
			{
				if (_received == 0)
				{
					return 0;
				}
				return Mathf.RoundToInt(_sum / (float)_received);
			}
		}

		public byte[] Data
		{
			get
			{
				return _data;
			}
		}

		public event Action OnComplete;

		public event Action<string> OnError;

		public QosProbe(int numProbes, int timeoutMs, int size, string hostname, int port)
		{
			_num = numProbes;
			_sent = 0;
			_size = size;
			_host = hostname;
			_port = port;
			_timeout = timeoutMs;
			_min = 2.1474836E+09f;
			_max = 0f;
			int num = 10000;
			while (_client == null)
			{
				try
				{
					_client = new UdpClient(num, AddressFamily.InterNetwork);
				}
				catch
				{
					num++;
				}
			}
			Debug.Log("QosProbe Bound to " + num + " for  " + hostname);
			SendProbe();
		}

		public void Dispose()
		{
			OnComplete = null;
			try
			{
				_client.Close();
			}
			catch
			{
			}
		}

		private void SendProbe()
		{
			if (_sent < _num)
			{
				Coroutines.Run(_Send());
			}
			else if (OnComplete != null)
			{
				OnComplete();
			}
		}

		private IEnumerator _Send()
		{
			_sent++;
			byte[] bytes = Crypto.RandomBytes(_size);
			if (_hostEntry == null)
			{
				IAsyncResult dns = Dns.BeginGetHostAddresses(_host, null, null);
				while (!dns.IsCompleted)
				{
					yield return 1;
				}
				try
				{
					IPAddress[] ips = Dns.EndGetHostAddresses(dns);
					IPAddress[] array = ips;
					foreach (IPAddress ip in array)
					{
						if (ip.AddressFamily == AddressFamily.InterNetwork)
						{
							_hostEntry = ip;
							break;
						}
					}
				}
				catch
				{
				}
			}
			if (_hostEntry == null && OnError != null)
			{
				OnError(Localizer.GetString("ID_SPARX_ERROR_FAILED_DNS"));
				yield break;
			}
			IPEndPoint ep = new IPEndPoint(_hostEntry, _port);
			try
			{
				_client.Send(bytes, bytes.Length, ep);
			}
			catch
			{
				OnError(Localizer.GetString("ID_SPARX_ERROR_UNKNOWN"));
				yield break;
			}
			float ping = 0f;
			DateTime start = DateTime.Now;
			IAsyncResult async = _client.BeginReceive((IAsyncResult ar) =>
			{
				ping = (float)(DateTime.Now - start).TotalMilliseconds;
			}, null);
			float timeout = Time.realtimeSinceStartup + (float)_timeout / 1000f;
			while (!async.IsCompleted)
			{
				if (Time.realtimeSinceStartup >= timeout)
				{
					Debug.LogWarning("QosProbe timed out to " + _host);
					yield return new WaitForFixedUpdate();
					SendProbe();
					yield break;
				}
				yield return 1;
			}
			_received++;
			_data = _client.EndReceive(async, ref ep);
			_sum += ping;
			_max = Mathf.Max(_max, ping);
			_min = Mathf.Min(_min, ping);
			SendProbe();
		}
	}
}
