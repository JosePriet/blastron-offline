using System;
using System.Collections;

namespace EB.Sparx
{
	public class GameManager : SubSystem, Updatable
	{
		public struct CreateGameParams
		{
			public int MaxPlayers;

			public Hashtable Attributes;

			public Hashtable PlayerAttributes;
		}

		public struct LocalCreateGameParams
		{
			public Hashtable Attributes;

			public Hashtable PlayerAttributes;

			public ArrayList AIPlayers;

			public Hashtable CampaignAttributes;

			public void AddAIPlayer(Hashtable attributes)
			{
				if (AIPlayers == null)
				{
					AIPlayers = new ArrayList();
				}
				Hashtable hashtable = new Hashtable();
				hashtable["uid"] = Hub.Instance.LoginManager.LocalUserId;
				hashtable["id"] = (long)AIPlayers.Count + 10L;
				hashtable["attributes"] = attributes;
				AIPlayers.Add(hashtable);
			}
		}

		public struct JoinGameParams
		{
			public long GameId;

			public Hashtable PlayerAttributes;
		}

		public struct FindGameParams
		{
			public Hashtable SearchAttributes;

			public Hashtable PlayerAttributes;
		}

		public struct FindOrCreateGameParams
		{
			public int MaxPlayers;

			public Hashtable SearchAttributes;

			public Hashtable ResetAttributes;

			public Hashtable PlayerAttributes;
		}

		private class WalkServers
		{
			private Action<string, object> _onDone;

			private Action<WalkServers> _onNext;

			private MasterServer[] _servers;

			private int _index;

			public MasterServer Server
			{
				get
				{
					return _servers[_index];
				}
			}

			public WalkServers(MasterServer[] servers, Action<string, object> onDone, Action<WalkServers> onNext)
			{
				_onDone = onDone;
				_onNext = onNext;
				_servers = servers;
				_index = 0;
				Next(string.Empty);
			}

			public void OnResult(string err, object result)
			{
				if (!string.IsNullOrEmpty(err))
				{
					_index++;
					Next(err);
				}
				else
				{
					_onDone(err, result);
				}
			}

			private void Next(string err)
			{
				if (_index < _servers.Length)
				{
					_onNext(this);
					return;
				}
				if (string.IsNullOrEmpty(err))
				{
					err = "ID_SPARX_ERROR_NOFREEGAMES";
				}
				_onDone(err, null);
			}
		}

		private GameManagerAPI _api;

		private GameManagerConfig _config;

		private int _pingsRemaining;

		public MasterServer[] MasterServers { get; private set; }

		public MasterServer BestMasterServer
		{
			get
			{
				if (MasterServers.Length > 0)
				{
					return MasterServers[0];
				}
				return null;
			}
		}

		public Game Game { get; private set; }

		public bool UpdateOffline
		{
			get
			{
				return false;
			}
		}

		public override void Initialize(Config config)
		{
			_config = config.GameManagerConfig;
			_api = new GameManagerAPI(base.Hub.ApiEndPoint);
			MasterServers = new MasterServer[0];
			NetworkFactory.Register("null", typeof(NetworkNull));
			NetworkFactory.Register("ws", typeof(NetworkWS));
			NetworkFactory.Register("wss", typeof(NetworkWS));
			BugReport.OnBugReport += OnCrash;
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public override void Connect()
		{
			base.State = SubSystemState.Connecting;
			ArrayList arrayList = Dot.Array("master", base.Hub.LoginManager.LoginData, null);
			if (arrayList != null)
			{
				OnGetMasterServers(null, _api.ParseMasterServers(arrayList));
			}
			else
			{
				RefreshMasterServerList();
			}
		}

		public void Update()
		{
			if (Game != null)
			{
				Game.Update();
			}
		}

		public override void Disconnect(bool isLogout)
		{
			if (isLogout || Game == null || !Game.Network.IsLocal)
			{
				DestroyGameInternal(string.Empty);
			}
		}

		private void OnCrash()
		{
			if (Game != null)
			{
				Game.Disconnect(true);
			}
		}

		public override void Dispose()
		{
			_api.Dispose();
			if (Game != null)
			{
				Game.Dispose();
			}
			base.Dispose();
		}

		public MasterServer GetMasterServer(string name)
		{
			MasterServer[] masterServers = MasterServers;
			foreach (MasterServer masterServer in masterServers)
			{
				if (masterServer.Name == name)
				{
					return masterServer;
				}
			}
			return null;
		}

		public void ListGames(MasterServer server, Hashtable attributes, Action complete)
		{
			_api.ListGames(server, attributes, (string err, MasterServerGame[] games) =>
			{
				if (string.IsNullOrEmpty(err))
				{
					server.Games = games;
				}
				if (complete != null)
				{
					complete();
				}
			});
		}

		public void FindGame(FindGameParams pars)
		{
			new WalkServers(MasterServers, OnJoinGameResult, (WalkServers walk) =>
			{
				pars.PlayerAttributes["ping"] = walk.Server.Ping;
				_api.FindGame(walk.Server, pars.SearchAttributes, pars.PlayerAttributes, walk.OnResult);
			});
		}

		public void FindOrCreateGame(FindOrCreateGameParams pars)
		{
			new WalkServers(MasterServers, OnJoinGameResult, (WalkServers walk) =>
			{
				pars.PlayerAttributes["ping"] = walk.Server.Ping;
				_api.FindOrCreateGame(walk.Server, pars.SearchAttributes, pars.MaxPlayers, pars.ResetAttributes, pars.PlayerAttributes, walk.OnResult);
			});
		}

		public void DestroyGameSoon(string err)
		{
			Coroutines.NextFrame(() =>
			{
				DestroyGameInternal(err);
			});
		}

		private void DestroyGameInternal(string error)
		{
			if (Game != null)
			{
				_config.Listener.OnLeaveGame(Game, error);
				Game.Dispose();
				Game = null;
			}
		}

		public void LeaveGame()
		{
			if (Game != null)
			{
				Game.Disconnect(false);
			}
		}

		public void RefreshMasterServerList()
		{
			_api.GetMasterServers(OnGetMasterServers);
		}

		public Game CreateLocalGame(LocalCreateGameParams pars)
		{
			OnJoinGameResult(string.Empty, new Hashtable
			{
				{ "connId", 1 },
				{ "gameId", 1 },
				{ "url", "null://locahost" }
			});
			Game.SetAttributes(pars.Attributes);
			Game.SetAttributes(pars.CampaignAttributes);
			Game.HostPlayer.Update(new Hashtable { { "attributes", pars.PlayerAttributes } });
			if (pars.AIPlayers != null)
			{
				Game.SendGameCommand(0u, "ai", pars.AIPlayers);
			}
			return Game;
		}

		public void CreateOrJoinPrivateGame(string key, CreateGameParams pars)
		{
			_api.FindPrivateGame(key, (string error, Hashtable game) =>
			{
				if (!string.IsNullOrEmpty(error))
				{
					_config.Listener.OnJoinGameFailed(error);
				}
				else
				{
					long num = Dot.Long("gameId", game, 0L);
					string name = Dot.String("master", game, string.Empty);
					if (num > 0)
					{
						MasterServer masterServer = GetMasterServer(name);
						if (masterServer == null)
						{
							_config.Listener.OnJoinGameFailed(Localizer.GetString("ID_SPARX_ERROR_MASTER_SERVER_UNKNOWN"));
						}
						else
						{
							_api.JoinGame(masterServer, num, pars.PlayerAttributes, OnJoinGameResult);
						}
					}
					else
					{
						new WalkServers(MasterServers, OnJoinGameResult, (WalkServers walk) =>
						{
							pars.PlayerAttributes["ping"] = walk.Server.Ping;
							_api.ResetServer(walk.Server, pars.MaxPlayers, pars.Attributes, pars.PlayerAttributes, key, walk.OnResult);
						});
					}
				}
			});
		}

		public void CreateGame(MasterServer server, CreateGameParams pars)
		{
			new WalkServers(MasterServers, OnJoinGameResult, (WalkServers obj) =>
			{
				_api.ResetServer(obj.Server, pars.MaxPlayers, pars.Attributes, pars.PlayerAttributes, null, obj.OnResult);
			});
		}

		public void JoinGame(MasterServer server, JoinGameParams pars)
		{
			_api.JoinGame(server, pars.GameId, pars.PlayerAttributes, OnJoinGameResult);
		}

		private void OnJoinGameResult(string err, object result)
		{
			long num = Dot.Long("gameId", result, 0L);
			if (!string.IsNullOrEmpty(err) || num == 0L)
			{
				Debug.LogError("Join game failed: " + err);
				if (!Localizer.GetString(err, out err))
				{
					err = Localizer.GetString("ID_SPARX_ERROR_JOIN_GAME");
				}
				_config.Listener.OnJoinGameFailed(err);
			}
			else
			{
				Debug.Log("Connecting to game");
				GameParams gameParams = new GameParams(result);
				gameParams.Listener = _config.Listener;
				Game = new Game(gameParams);
				Game.Connect();
				_api.DisconnectEndPoints();
			}
		}

		private void OnGetMasterServers(string error, MasterServer[] list)
		{
			if (!string.IsNullOrEmpty(error) || list.Length == 0)
			{
				Debug.LogError("Failed to get master server list!");
				FatalError(Localizer.GetString("ID_SPARX_ERROR_UNKNOWN"));
				return;
			}
			base.State = SubSystemState.Connected;
			if (_config.PingEnabled)
			{
				MasterServers = list;
				_pingsRemaining = MasterServers.Length;
				MasterServer[] masterServers = MasterServers;
				foreach (MasterServer server in masterServers)
				{
					_api.PingMasterServer(server, OnPingResult);
				}
			}
			else
			{
				base.State = SubSystemState.Connected;
			}
		}

		private void OnPingResult(string error, MasterServer server)
		{
			Debug.Log("Ping Result: " + server);
			_pingsRemaining--;
			if (_pingsRemaining == 0)
			{
				Debug.Log("Ping complete");
				Array.Sort(MasterServers, (MasterServer m1, MasterServer m2) => m1.CompareTo(m2));
				base.State = SubSystemState.Connected;
			}
		}

		private void OnListGames(string error, MasterServerGame[] result)
		{
			Debug.Log("OnListGames {0}, {1}", error, result);
		}

		public override void OnEnteredBackground()
		{
			if (Game != null && !Game.Network.IsLocal)
			{
				Debug.LogError("Entered Background, disconnecting game");
				Game.Disconnect(true);
			}
		}
	}
}
