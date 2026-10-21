using System.Collections;
using System.Collections.Generic;
using EB.Collections;

namespace EB.Sparx
{
	public class TelemetryManager : SubSystem, Updatable
	{
		private static CircularBuffer _buffer = new CircularBuffer(256);

		private static List<string> _filter = new List<string>();

		private static float _flushInterval = 60f;

		private static float _flushTimer = 0f;

		private EndPoint _ep;

		public static int UserLevel { get; set; }

		public bool UpdateOffline
		{
			get
			{
				return false;
			}
		}

		public override void Initialize(Config config)
		{
			UserLevel = 1;
			_ep = base.Hub.ApiEndPoint;
		}

		public override IEnumerator LoadOfflineData()
		{
			UserLevel = 1;
			yield break;
		}

		public override void Connect()
		{
			Hashtable obj = Dot.Object("tele", base.Hub.LoginManager.LoginData, null);
			_flushInterval = Dot.Single("interval", obj, _flushInterval);
			_flushTimer = ((_buffer.Count <= 0) ? _flushInterval : 0f);
			ArrayList arrayList = Dot.Array("filter", obj, null);
			if (arrayList != null)
			{
				_filter.Clear();
				foreach (object item in arrayList)
				{
					_filter.Add(item.ToString());
				}
			}
			base.State = SubSystemState.Connected;
		}

		public void Update()
		{
			_flushTimer -= Time.deltaTime;
			if (_flushTimer < 0f)
			{
				Flush();
			}
		}

		public override void Disconnect(bool isLogout)
		{
			if (isLogout)
			{
				Flush();
			}
			_buffer.Clear();
		}

		private static bool IsFiltered(string name)
		{
			foreach (string item in _filter)
			{
				if (name.StartsWith(item))
				{
					return true;
				}
			}
			return false;
		}

		public static void Event(string name, int value, object eventData)
		{
			EventInternal(Time.Now, name, value, eventData);
		}

		private static void EventInternal(int time, string name, int value, object eventData)
		{
			name = StringUtil.SafeKey(name);
			if (IsFiltered(name))
			{
				Debug.Log("filtering out event " + name);
				return;
			}
			Debug.Log("Event: " + name + " " + value);
			_buffer.Push(new object[5] { time, name, value, UserLevel, eventData });
			if (_flushTimer < 0f)
			{
				_flushTimer = 0.5f;
			}
		}

		public void Flush()
		{
			if (_buffer.Count > 0 && base.Hub.State == HubState.Connected)
			{
				object[] value = _buffer.ToArray();
				_buffer.Clear();
				Request request = _ep.Post("/telemetry");
				request.AddData("events", value);
				_ep.Service(request, (Response r) =>
				{
				});
				_flushTimer = _flushInterval;
			}
			else
			{
				_flushTimer = 1f;
			}
		}
	}
}
