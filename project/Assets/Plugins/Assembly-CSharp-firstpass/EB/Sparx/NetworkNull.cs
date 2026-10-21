using System.Collections;

namespace EB.Sparx
{
	public class NetworkNull : Network
	{
		private uint _connId;

		public override NetworkStats Stats
		{
			get
			{
				return default(NetworkStats);
			}
		}

		public override bool IsLocal
		{
			get
			{
				return true;
			}
		}

		public override void Connect(string url, uint connId)
		{
			_connId = connId;
			DispatchConnect();
		}

		public override void Disconnect(bool now)
		{
			DispatchDisconnect(string.Empty);
		}

		public override void SendTo(uint playerId, Packet packet)
		{
			if (playerId == 0)
			{
				if (packet.Channel != Channel.Manager_Reliable)
				{
					return;
				}
				string utf8Data = packet.Utf8Data;
				string message = utf8Data;
				int num = utf8Data.IndexOf(':');
				object payload = null;
				if (num >= 0)
				{
					message = utf8Data.Substring(0, num);
					try
					{
						payload = JSON.Parse(utf8Data.Substring(num + 1));
					}
					catch
					{
					}
				}
				HandleGameCommand(message, payload);
			}
			else if (playerId == _connId)
			{
				DispatchReceive(playerId, packet);
			}
		}

		public override void Broadcast(Packet packet)
		{
		}

		public override void Update()
		{
		}

		private Hashtable CreateLocalPlayer()
		{
			Hashtable hashtable = new Hashtable();
			hashtable["uid"] = Hub.Instance.LoginManager.LocalUserId.ToString();
			hashtable["id"] = _connId;
			return hashtable;
		}

		private void HandleGameCommand(string message, object payload)
		{
			switch (message)
			{
			case "hello":
			{
				ArrayList arrayList2 = new ArrayList();
				arrayList2.Add(CreateLocalPlayer());
				ReturnGameCommand("roster", arrayList2);
				break;
			}
			case "start":
				ReturnGameCommand(message, payload);
				break;
			case "ai":
			{
				ArrayList arrayList = new ArrayList();
				arrayList.Add(CreateLocalPlayer());
				IEnumerator enumerator = AOT.GetEnumerator(payload);
				while (enumerator.MoveNext())
				{
					arrayList.Add(enumerator.Current);
				}
				ReturnGameCommand("roster", arrayList);
				break;
			}
			case "end":
				break;
			case "use":
				ReturnGameCommand(message, payload);
				break;
			}
		}

		private void ReturnGameCommand(string message, object payload)
		{
			string text = message;
			if (payload != null)
			{
				text = text + ":" + JSON.Stringify(payload);
			}
			DispatchReceive(0u, new Packet(Channel.Manager_Reliable, text));
		}
	}
}
