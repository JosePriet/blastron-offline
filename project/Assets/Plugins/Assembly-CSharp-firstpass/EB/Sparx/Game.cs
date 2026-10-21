using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EB.Sparx
{
	public class Game : IDisposable
	{
		private readonly GameParams _pars;

		private Network _network;

		private List<Player> _players = new List<Player>();

		private Player _local;

		private Hashtable _attributes = new Hashtable();

		private bool _sentJoin;

		private bool _sentStart;

		private bool _sentEnd;

		private bool _started;

		private string _disconnectMessage = string.Empty;

		private bool _wantDisconnect;

		private string _privateKey;

		public Player LocalPlayer
		{
			get
			{
				return _local;
			}
		}

		public Player HostPlayer
		{
			get
			{
				return (_players.Count <= 0) ? null : _players[0];
			}
		}

		public List<Player> Players
		{
			get
			{
				return _players;
			}
		}

		public Hashtable Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public bool Connected
		{
			get
			{
				return _sentJoin;
			}
		}

		public long Id
		{
			get
			{
				return _pars.GameId;
			}
		}

		public bool IsHost
		{
			get
			{
				return _local != null && _local.IsHost;
			}
		}

		public bool Started
		{
			get
			{
				return _started;
			}
		}

		public string PrivateKey
		{
			get
			{
				return _privateKey;
			}
		}

		public bool IsPrivate
		{
			get
			{
				return !string.IsNullOrEmpty(_privateKey);
			}
		}

		public NetworkStats Stats
		{
			get
			{
				return _network.Stats;
			}
		}

		public Network Network
		{
			get
			{
				return _network;
			}
		}

		public Game(GameParams pars)
		{
			_pars = pars;
			_network = NetworkFactory.Create(pars.Url);
			_network.OnConnect += OnNetworkConnect;
			_network.OnError += OnNetworkError;
			_network.OnReceive += OnNetworkReceive;
			_network.OnDisconnect += OnNetworkDisconnect;
			Screen.sleepTimeout = -1;
		}

		public void Connect()
		{
			_network.Connect(_pars.Url, _pars.ConnId);
		}

		public void Update()
		{
			_network.Update();
		}

		public void Disconnect(bool now)
		{
			_wantDisconnect = true;
			_network.Disconnect(now);
		}

		public void LeaveGame(string message)
		{
			_disconnectMessage = message;
			Disconnect(false);
		}

		public void Dispose()
		{
			_network.Dispose();
			Screen.sleepTimeout = -2;
		}

		public virtual void Start()
		{
			if (IsHost && !_started && !_sentStart)
			{
				_sentStart = true;
				SendGameCommand(0u, "start", null);
			}
		}

		public virtual void End()
		{
			if (IsHost && _started && !_sentEnd)
			{
				_sentEnd = true;
				SendGameCommand(0u, "end", null);
			}
		}

		public Player GetPlayer(uint playerId)
		{
			foreach (Player player in _players)
			{
				if (player.PlayerId == playerId)
				{
					return player;
				}
			}
			return null;
		}

		public Player GetPlayer(Id userId)
		{
			foreach (Player player in _players)
			{
				if (player.UserId == userId)
				{
					return player;
				}
			}
			return null;
		}

		public void SetAttributes(Hashtable values)
		{
			if (!IsHost)
			{
				return;
			}
			SendGameCommand(0u, "attributes", values);
			foreach (DictionaryEntry value in values)
			{
				_attributes[value.Key] = value.Value;
			}
		}

		public void UpdatePlayerAttributes(Hashtable values)
		{
			if (LocalPlayer != null)
			{
				Dot.DeepCopy(values, LocalPlayer.Attributes);
			}
			SendGameCommand(0u, "playerattributes", values);
		}

		public void SendTo(uint playerId, Channel channel, Buffer data)
		{
			Packet packet = new Packet(channel, data);
			_network.SendTo(playerId, packet);
		}

		public void Broadcast(Channel channel, Buffer data)
		{
			Packet packet = new Packet(channel, data);
			_network.Broadcast(packet);
		}

		public void SendGameCommand(uint playerId, string command, object data)
		{
			string text = command;
			if (data != null)
			{
				text = text + ":" + JSON.Stringify(data);
			}
			_network.SendTo(playerId, new Packet(Channel.Manager_Reliable, text));
		}

		public void KickPlayer(uint playerId)
		{
			if (IsHost)
			{
				SendGameCommand(0u, "kick", playerId);
			}
		}

		private void OnReceiveGameCommand(uint playerId, string command, object data)
		{
			Debug.Log("Got game command {0} {1}", command, data);
			switch (command)
			{
			case "private":
				_privateKey = data.ToString();
				break;
			case "attributes":
				if (data is Hashtable)
				{
					Debug.Log("Updating attributes");
					_attributes = new Hashtable((Hashtable)data);
					if (_sentJoin)
					{
						_pars.Listener.OnAttributesUpdated(this);
					}
				}
				break;
			case "roster":
			{
				ArrayList arrayList = (ArrayList)data;
				List<Player> list = new List<Player>();
				List<Player> list2 = new List<Player>();
				List<Player> list3 = new List<Player>();
				foreach (object item in arrayList)
				{
					Player player = new Player(item);
					Player player2 = GetPlayer(player.PlayerId);
					if (player2 == null)
					{
						list2.Add(player);
					}
					else
					{
						player2.Update(item);
						player = player2;
					}
					list.Add(player);
				}
				List<Player> players = _players;
				_players = list;
				foreach (Player item2 in players)
				{
					if (GetPlayer(item2.PlayerId) == null)
					{
						list3.Add(item2);
					}
				}
				for (int i = 0; i < _players.Count; i++)
				{
					_players[i].Index = i;
				}
				if (_sentJoin)
				{
					foreach (Player item3 in list3)
					{
						_pars.Listener.OnPlayerLeft(this, item3);
					}
					{
						foreach (Player item4 in list2)
						{
							_pars.Listener.OnPlayerJoined(this, item4);
						}
						break;
					}
				}
				_sentJoin = true;
				_local = GetPlayer(_pars.ConnId);
				_pars.Listener.OnJoinedGame(this);
				break;
			}
			case "start":
				_sentEnd = false;
				_started = true;
				_pars.Listener.OnGameStarted(this);
				break;
			case "end":
				_sentStart = false;
				_started = false;
				_pars.Listener.OnGameEnded(this);
				break;
			case "kick":
				_disconnectMessage = "ID_SPARX_ERROR_GAME_KICKED";
				Disconnect(false);
				break;
			case "use":
				if (data is Hashtable)
				{
					Hub.Instance.InventoryManager.UseItems(data as Hashtable);
				}
				break;
			}
		}

		private void OnNetworkConnect()
		{
			Debug.Log("Network connected, sending hello");
			SendGameCommand(0u, "hello", null);
		}

		private void OnNetworkReceive(uint playerId, Packet packet)
		{
			switch (packet.Channel)
			{
			case Channel.Manager_Reliable:
			case Channel.Manager_Unreliable:
			{
				string text = packet.Utf8Data;
				object data = null;
				int num = text.IndexOf(':');
				if (num > 0)
				{
					data = JSON.Parse(text.Substring(num + 1));
					text = text.Substring(0, num);
				}
				OnReceiveGameCommand(playerId, text, data);
				break;
			}
			case Channel.Game_Reliable:
			case Channel.Game_UnReliable:
			{
				Player player = GetPlayer(playerId);
				_pars.Listener.OnReceive(this, player, packet);
				break;
			}
			}
		}

		private void OnNetworkDisconnect(string message)
		{
			Debug.Log("OnNetworkDisconnect: " + message);
			if (!string.IsNullOrEmpty(_disconnectMessage))
			{
				message = Localizer.GetString(_disconnectMessage);
			}
			else if (!_wantDisconnect)
			{
				message = Localizer.GetString("ID_SPARX_ERROR_GAME_LOST_CONNECTION");
			}
			Hub.Instance.GameManager.DestroyGameSoon(message);
		}

		private void OnNetworkError(string error)
		{
			Debug.LogError("OnNetworkError: " + error);
			Hub.Instance.GameManager.DestroyGameSoon(error);
		}
	}
}
