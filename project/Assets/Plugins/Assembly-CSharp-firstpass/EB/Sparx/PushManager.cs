using System;
using System.Collections;
using EB.Net;

namespace EB.Sparx
{
	public class PushManager : SubSystem, Updatable
	{
		private PushAPI _api;

		private WebSocket _socket;

		private Deferred _deffered;

		private Uri _url;

		private bool _gotToken;

		public bool UpdateOffline
		{
			get
			{
				return false;
			}
		}

		public override void Initialize(Config config)
		{
			_api = new PushAPI(base.Hub.ApiEndPoint);
			_deffered = new Deferred(4);
			_url = null;
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public override void Connect()
		{
			base.State = SubSystemState.Connecting;
			Hashtable hashtable = Dot.Object("push", base.Hub.LoginManager.LoginData, null);
			if (hashtable != null)
			{
				OnGetPushToken(null, hashtable);
			}
			else
			{
				_api.GetPushToken(OnGetPushToken);
			}
		}

		public void Update()
		{
			_deffered.Dispatch();
		}

		private void OnSentApplePushToken(string error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				Debug.Log("failed to send push token  " + error);
			}
		}

		public override void Disconnect(bool isLogout)
		{
			base.State = SubSystemState.Disconnected;
			if (_socket != null)
			{
				_socket.Dispose();
				_socket = null;
			}
		}

		public override void Dispose()
		{
			if (_socket != null)
			{
				_socket.Dispose();
			}
		}

		private void SetupSocket()
		{
			if (_socket != null)
			{
				_socket.Dispose();
			}
			_socket = new WebSocket();
			_socket.OnConnect += OnConnect;
			_socket.OnError += OnError;
			_socket.OnMessage += OnMessage;
		}

		private void OnConnect()
		{
		}

		private void HandleMessage(string data)
		{
			Debug.Log("Got async message: " + data);
			try
			{
				object obj = JSON.Parse(data);
				string text = Dot.String("component", obj, string.Empty);
				string message = Dot.String("message", obj, string.Empty);
				object payload = Dot.Find("payload", obj);
				Manager manager = base.Hub.GetManager(text);
				if (manager != null)
				{
					manager.Async(message, payload);
				}
				else
				{
					Debug.LogError("Failed to find manager: " + text);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError("Failed to parse async notification " + ex);
			}
		}

		private void OnMessage(string data)
		{
			_deffered.Defer((Action)(() =>
			{
				HandleMessage(data);
			}));
		}

		private void OnError(string error)
		{
			_deffered.Defer((Action)(() =>
			{
				Debug.Log("Lost connect to push server: " + error);
				Coroutines.SetTimeout(() =>
				{
					ConnectWebsocket();
				}, 1000);
			}));
		}

		private void ConnectWebsocket()
		{
			if (base.State == SubSystemState.Connected)
			{
				if (_socket == null)
				{
					SetupSocket();
				}
				if (_socket.State < WebSocketState.Connecting)
				{
					_socket.ConnectAsync(_url, "io.sparx.push", null);
				}
			}
		}

		private void OnGetPushToken(string error, Hashtable result)
		{
			string text = Dot.String("websocket", result, string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				_url = new Uri(text);
				base.State = SubSystemState.Connected;
				ConnectWebsocket();
			}
			else
			{
				base.State = SubSystemState.Error;
			}
		}
	}
}
