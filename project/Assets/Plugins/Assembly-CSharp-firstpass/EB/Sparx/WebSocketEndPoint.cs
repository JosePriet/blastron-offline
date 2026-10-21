using System.Collections;
using EB.Net;

namespace EB.Sparx
{
	public class WebSocketEndPoint : HttpEndPoint
	{
		private TalkWebSocket _socket;

		private Deferred _deferred = new Deferred(8);

		private Uri _wsUrl;

		public WebSocketEndPoint(string endPoint, EndPointOptions options)
			: base(MakeHttpEndpoint(endPoint), options)
		{
			_wsUrl = new Uri(endPoint);
			_socket = new TalkWebSocket();
			_socket.ActivityTimeout = options.ActivityTimeout;
			_socket.OnError += OnError;
			if (options.KeepAlive)
			{
				ConnectIfNeeded();
			}
		}

		private static string MakeHttpEndpoint(string endPoint)
		{
			return endPoint.Replace("wss://", "https://").Replace("ws://", "http://");
		}

		public override void Dispose()
		{
			base.Dispose();
			if (_socket != null)
			{
				_socket.Dispose();
				_socket = null;
			}
		}

		private void OnError(string error)
		{
			if (!base.Options.KeepAlive)
			{
				return;
			}
			_deferred.Defer((Action)(() =>
			{
				Coroutines.SetTimeout(() =>
				{
					Debug.Log("Reconnecting");
					ConnectIfNeeded();
				}, 1000);
			}));
		}

		private void ConnectIfNeeded()
		{
			if (_socket.State < WebSocketState.Connecting)
			{
				_socket.ConnectAsync(_wsUrl, base.Options.Protocol, base.Options.Key);
			}
		}

		public override void Update()
		{
			base.Update();
			_deferred.Dispatch();
		}

		public override void RPC(string name, ArrayList args, Action<string, object> callback)
		{
			ConnectIfNeeded();
			_socket.RPC(name, args, (string err, object result) =>
			{
				_deferred.Defer(callback, err, result);
			});
		}
	}
}
