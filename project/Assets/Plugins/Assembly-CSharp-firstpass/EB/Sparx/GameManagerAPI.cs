using System;
using System.Collections;
using System.Collections.Generic;

namespace EB.Sparx
{
	public class GameManagerAPI : IDisposable
	{
		private EndPoint _api;

		private Dictionary<string, EndPoint> _masterServerEndpoints = new Dictionary<string, EndPoint>();

		public GameManagerAPI(EndPoint api)
		{
			_api = api;
		}

		public void DisconnectEndPoints()
		{
			Coroutines.NextFrame(() =>
			{
				foreach (KeyValuePair<string, EndPoint> masterServerEndpoint in _masterServerEndpoints)
				{
					masterServerEndpoint.Value.Dispose();
				}
				_masterServerEndpoints.Clear();
			});
		}

		private EndPoint GetEndPoint(MasterServer server)
		{
			EndPoint value;
			if (_masterServerEndpoints.TryGetValue(server.EndPoint, out value))
			{
				return value;
			}
			value = server.CreateEndPoint();
			_masterServerEndpoints.Add(server.EndPoint, value);
			return value;
		}

		public MasterServer[] ParseMasterServers(ArrayList arrayList)
		{
			List<MasterServer> list = new List<MasterServer>();
			if (arrayList != null)
			{
				foreach (object array in arrayList)
				{
					list.Add(new MasterServer(array));
				}
			}
			return list.ToArray();
		}

		public void GetMasterServers(Action<string, MasterServer[]> callback)
		{
			Request request = _api.Get("/master/list");
			_api.Service(request, (Response r) =>
			{
				if (r.sucessful)
				{
					callback(null, ParseMasterServers(r.arrayList));
				}
				else
				{
					callback(r.localizedError, null);
				}
			});
		}

		public void PingMasterServer(MasterServer server, Action<string, MasterServer> callback)
		{
			QosProbe qosProbe = new QosProbe(4, 2000, 128, server.Hostname, server.PingPort);
			qosProbe.OnComplete += () =>
			{
				Debug.Log("Probes complete " + qosProbe.AvgPing);
				server.Ping = qosProbe.AvgPing;
				if (qosProbe.Data != null && qosProbe.Data.Length > 0)
				{
					try
					{
						string json = Encoding.GetString(qosProbe.Data);
						object obj = JSON.Parse(json);
						server.GameCount = Dot.Integer("g", obj, 0);
						server.PlayerCount = Dot.Integer("p", obj, 0);
					}
					catch
					{
					}
				}
				callback(null, server);
				qosProbe.Dispose();
			};
			qosProbe.OnError += (string obj) =>
			{
				callback(obj, server);
				qosProbe.Dispose();
			};
		}

		public void JoinGame(MasterServer server, long gameId, Hashtable playerAttributes, Action<string, object> callback)
		{
			EndPoint endPoint = GetEndPoint(server);
			ArrayList arrayList = new ArrayList();
			arrayList.Add(gameId);
			arrayList.Add(playerAttributes);
			endPoint.RPC("joinGame", arrayList, (string err, object obj) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					callback(err, null);
				}
				else
				{
					callback(null, obj);
				}
			});
		}

		public void FindPrivateGame(string name, Action<string, Hashtable> callback)
		{
			Request request = _api.Post("/push/key-get");
			request.AddData("key", name);
			_api.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					callback(null, result.hashtable);
				}
				else
				{
					callback(result.localizedError, null);
				}
			});
		}

		public void ResetServer(MasterServer server, int maxPlayers, Hashtable attributes, Hashtable playerAttributes, string privateKey, Action<string, object> callback)
		{
			EndPoint endPoint = GetEndPoint(server);
			ArrayList arrayList = new ArrayList();
			arrayList.Add(new Hashtable
			{
				{ "max_players", maxPlayers },
				{ "attributes", attributes },
				{ "private", privateKey }
			});
			arrayList.Add(playerAttributes);
			endPoint.RPC("resetServer", arrayList, (string err, object obj) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					callback(err, null);
				}
				else
				{
					callback(null, obj);
				}
			});
		}

		public void ListGames(MasterServer server, Hashtable attributes, Action<string, MasterServerGame[]> callback)
		{
			EndPoint endPoint = GetEndPoint(server);
			ArrayList arrayList = new ArrayList();
			arrayList.Add(attributes);
			endPoint.RPC("listGames", arrayList, (string err, object obj) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					Debug.LogError("List Games Error:" + err);
					callback(err, null);
				}
				else
				{
					List<MasterServerGame> list = new List<MasterServerGame>();
					ArrayList arrayList2 = (ArrayList)obj;
					foreach (object item in arrayList2)
					{
						list.Add(new MasterServerGame(item));
					}
					callback(null, list.ToArray());
				}
			});
		}

		public void FindGame(MasterServer server, Hashtable queryAttributes, Hashtable playerAttributes, Action<string, object> callback)
		{
			EndPoint endPoint = GetEndPoint(server);
			ArrayList arrayList = new ArrayList();
			arrayList.Add(queryAttributes);
			arrayList.Add(playerAttributes);
			endPoint.RPC("find", arrayList, (string err, object obj) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					callback(err, null);
				}
				else
				{
					callback(null, obj);
				}
			});
		}

		public void FindOrCreateGame(MasterServer server, Hashtable queryAttributes, int maxPlayers, Hashtable resetAttributes, Hashtable playerAttributes, Action<string, object> callback)
		{
			EndPoint endPoint = GetEndPoint(server);
			ArrayList arrayList = new ArrayList();
			arrayList.Add(queryAttributes);
			arrayList.Add(new Hashtable
			{
				{ "max_players", maxPlayers },
				{ "attributes", resetAttributes }
			});
			arrayList.Add(playerAttributes);
			endPoint.RPC("findOrCreate", arrayList, (string err, object obj) =>
			{
				if (!string.IsNullOrEmpty(err))
				{
					callback(err, null);
				}
				else
				{
					callback(null, obj);
				}
			});
		}

		public void Dispose()
		{
			foreach (EndPoint value in _masterServerEndpoints.Values)
			{
				value.Dispose();
			}
			_masterServerEndpoints.Clear();
		}
	}
}
