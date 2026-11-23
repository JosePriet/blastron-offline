using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EB.Sparx
{
	// Stands in for the shut-down Sparx/Kabam API server. Every request the game
	// used to send to api.rivets.sparx.io is answered here, on the device, and the
	// account (name, inventory, wallet, save data, gacha tokens) is kept in
	// <persistentDataPath>/offline_server.json.
	public class LocalEndPoint : EndPoint
	{
		private const string kUid = "1";

		private const string kFileName = "offline_server.json";

		private List<Request> _queue = new List<Request>();

		// Shared: the master-server code also creates endpoints through the factory.
		private static Hashtable _state;

		private static bool _primaryValid;

		private Hashtable _data = new Hashtable();

		private int _nextId = 1;

		private int _doneId;

		public LocalEndPoint(string url, EndPointOptions options)
			: base(url, options)
		{
			// The push socket and master-server ping have no server to reach.
			Hub.Instance.Config.UsePush = false;
			Hub.Instance.Config.GameManagerConfig.PingEnabled = false;
		}

		public override void AddData(string key, string value)
		{
			_data[key] = value;
		}

		public override string GetData(string key)
		{
			object obj = _data[key];
			if (obj != null)
			{
				return obj.ToString();
			}
			return string.Empty;
		}

		// Online matchmaking RPCs have no server to reach.
		public override void RPC(string name, ArrayList args, Action<string, object> callback)
		{
			Coroutines.NextFrame(delegate
			{
				callback("ID_SPARX_ERROR_NOT_CONNECTED", null);
			});
		}

		public override int Service(Request request, Action<Response> callback)
		{
			request.userData = callback;
			request.id = _nextId++;
			_queue.Add(request);
			return request.id;
		}

		public override bool IsDone(int requestId)
		{
			return _doneId >= requestId;
		}

		public override void Update()
		{
			while (_queue.Count > 0)
			{
				Request request = _queue[0];
				_queue.RemoveAt(0);
				Response response;
				try
				{
					Load();
					response = Handle(request);
				}
				catch (IOException)
				{
					response = StorageError(request);
				}
				catch (UnauthorizedAccessException)
				{
					response = StorageError(request);
				}
				_doneId = request.id;
				Action<Response> callback = request.userData as Action<Response>;
				if (callback != null)
				{
					callback(response);
				}
			}
		}

		private string StatePath
		{
			get { return Path.Combine(UnityEngine.Application.persistentDataPath, kFileName); }
		}

		private void Load()
		{
			if (_state != null)
			{
				return;
			}
			string primary = StatePath;
			string backup = primary + ".bak";
			FinishPendingSave(primary);
			_state = ReadState(primary);
			_primaryValid = _state != null;
			if (!_primaryValid)
			{
				_state = ReadState(backup);
				if (_state == null && (File.Exists(primary) || File.Exists(backup)))
				{
					throw new IOException("Unreadable offline account");
				}
			}
			if (_state == null) _state = new Hashtable();
			if (Obj("wallet") == null)
			{
				_state["wallet"] = new Hashtable { { "balance", 0 } };
			}
			if (Obj("ds") == null)
			{
				_state["ds"] = new Hashtable();
			}
			if (Obj("tokens") == null)
			{
				// New accounts start with the vault tutorial's spin token.
				_state["tokens"] = new Hashtable { { OfflineCatalog.TutorialToken, 1 } };
			}
		}

		private static Hashtable ReadState(string path)
		{
			if (!File.Exists(path)) return null;
			string json = Encoding.GetString(File.ReadAllBytes(path));
			try
			{
				return JSON.Parse(json) as Hashtable;
			}
			catch (FormatException)
			{
				return null;
			}
			catch (IndexOutOfRangeException)
			{
				return null;
			}
			catch (ArgumentOutOfRangeException)
			{
				return null;
			}
			catch (OverflowException)
			{
				return null;
			}
		}

		private Response StorageError(Request request)
		{
			// Another endpoint shares this account. Reload its last commit on the next request.
			_state = null;
			UnityEngine.Debug.LogError("LocalEndPoint: offline account storage unavailable");
			return Fail(request, "ID_SPARX_ERROR_UNKNOWN");
		}

		// The stripped Unity runtime lacks File.Move. A zero-length marker commits a
		// complete staging file. Interrupted copies can resume before the next read.
		private static void FinishPendingSave(string path)
		{
			string marker = path + ".commit";
			if (!File.Exists(marker)) return;
			byte[] committed = File.ReadAllBytes(path + ".tmp");
			File.WriteAllBytes(path, committed);
			File.Delete(marker);
		}

		private void Save()
		{
			string path = StatePath;
			FinishPendingSave(path);
			string json = JSON.Stringify(_state);
			if (json == null) throw new IOException("Could not serialize offline account");
			File.WriteAllBytes(path + ".tmp", Encoding.GetBytes(json));
			if (_primaryValid) File.WriteAllBytes(path + ".bak", File.ReadAllBytes(path));
			using (File.Create(path + ".commit")) {}
			_primaryValid = true;
			// The complete staging file remains authoritative if copying to the legacy path
			// fails. Leave the marker in place for a later save or process restart.
			try
			{
				FinishPendingSave(path);
			}
			catch (IOException)
			{
				UnityEngine.Debug.LogError("LocalEndPoint: account copy pending");
			}
			catch (UnauthorizedAccessException)
			{
				UnityEngine.Debug.LogError("LocalEndPoint: account copy pending");
			}
		}

		private Hashtable Obj(string key)
		{
			return _state[key] as Hashtable;
		}

		private int Balance
		{
			get { return Dot.Integer("balance", Obj("wallet"), 0); }
			set { Obj("wallet")["balance"] = value; }
		}

		private Hashtable Copy(object obj)
		{
			return JSON.Parse(JSON.Stringify(obj)) as Hashtable;
		}

		private Response Ok(Request request, object result)
		{
			Response response = new Response(request);
			response.sucessful = true;
			// Round-trip through JSON so the game gets fresh objects of the same
			// types a real server reply would have produced.
			result = (result == null) ? null : JSON.Parse(JSON.Stringify(result));
			response.result = result;
			if (result is Hashtable)
			{
				response.hashtable = (Hashtable)result;
			}
			else if (result is ArrayList)
			{
				response.arrayList = (ArrayList)result;
			}
			else if (result is double)
			{
				response.number = (double)result;
			}
			else if (result is string)
			{
				response.str = (string)result;
			}
			return response;
		}

		private Response Fail(Request request, string error)
		{
			Response response = new Response(request);
			response.sucessful = false;
			response.error = error;
			response.fatal = false;
			return response;
		}

		private Hashtable LoginResult()
		{
			Hashtable user = new Hashtable();
			user["uid"] = kUid;
			string name = _state["name"] as string;
			if (!string.IsNullOrEmpty(name))
			{
				user["name"] = name;
			}
			Hashtable result = new Hashtable();
			result["stoken"] = "offline";
			result["user"] = user;
			result["data"] = new Hashtable();
			return result;
		}

		private Hashtable InventoryResult()
		{
			Hashtable inventory = Obj("inventory");
			return inventory ?? new Hashtable();
		}

		private void AddItems(Hashtable items, int sign)
		{
			Hashtable inventory = Obj("inventory");
			if (inventory == null)
			{
				inventory = new Hashtable();
				_state["inventory"] = inventory;
			}
			if (items == null)
			{
				return;
			}
			foreach (DictionaryEntry item in items)
			{
				string key = item.Key.ToString();
				int count = Dot.Integer(key, inventory, 0) + sign * Dot.Integer(key, items, 0);
				inventory[key] = Math.Max(0, count);
			}
		}

		private Response Handle(Request request)
		{
			Time.Now = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
			string path = request.uri.Path;
			Hashtable data = Copy(request.data) ?? new Hashtable();
			switch (path)
			{
			case "/kabam/register":
				return Ok(request, new Hashtable
				{
					{ "naid", kUid },
					{ "country", "US" },
					{ "salt", "offline" },
					{ "fbAppId", string.Empty }
				});
			case "/kabam/guest":
			case "/kabam/login":
				return Ok(request, LoginResult());
			case "/kabam/name":
				_state["name"] = Dot.String("name", data, string.Empty);
				Save();
				return Ok(request, LoginResult());
			case "/kabam/support":
				return Ok(request, new Hashtable { { "url", string.Empty } });
			case "/tuning":
				return Ok(request, new ArrayList());
			case "/telemetry":
			case "/util/ping":
			case "/push/apple":
			case "/gamecenter/id":
			case "/tournaments/report":
				return Ok(request, new Hashtable());
			case "/master/list":
				return Ok(request, new ArrayList
				{
					new Hashtable
					{
						{ "name", "offline" },
						{ "endpoint", "ws://127.0.0.1:1" },
						{ "token", string.Empty },
						{ "pingPort", 0 }
					}
				});
			case "/push/token":
				return Ok(request, new Hashtable());
			case "/tournaments/active":
			case "/messenger/motd":
				return Ok(request, null);
			case "/messenger/count":
				return Ok(request, 0);
			case "/inventory":
				// null tells the game this is a new account, so it grants the
				// starting inventory from its own data.
				return Ok(request, Obj("inventory"));
			case "/inventory/add":
				AddItems(Dot.Object("items", data, null), 1);
				Save();
				return Ok(request, InventoryResult());
			case "/inventory/use":
				AddItems(Dot.Object("items", data, null), -1);
				Save();
				return Ok(request, InventoryResult());
			case "/inventory/purchase":
			{
				int cost = Dot.Integer("cost", data, 0);
				if (cost > Balance)
				{
					return Fail(request, "nsf");
				}
				Balance -= cost;
				AddItems(Dot.Object("items", data, null), 1);
				Save();
				Hashtable result = Copy(InventoryResult());
				result["__wallet"] = Obj("wallet");
				return Ok(request, result);
			}
			case "/wallet/balance":
				return Ok(request, Obj("wallet"));
			case "/wallet/credit":
				Balance += Dot.Integer("value", data, 0);
				Save();
				return Ok(request, Obj("wallet"));
			case "/wallet/debit":
			{
				int value = Dot.Integer("value", data, 0);
				if (value > Balance)
				{
					return Fail(request, "nsf");
				}
				Balance -= value;
				Save();
				return Ok(request, Obj("wallet"));
			}
			case "/store/payouts":
			case "/store/offers":
				return Ok(request, OfflineCatalog.Payouts());
			case "/store/verify-payout":
			{
				// Gear packs are free offline: credit the pack's gears.
				int payout = Dot.Integer("data.payoutid", data, 0);
				int credit = OfflineCatalog.PayoutValue(payout);
				string receipt = Dot.String("data.response-data", data, string.Empty);
				if (credit <= 0 || !LocalProvider.IsReceipt(receipt))
				{
					return Fail(request, "ID_SPARX_ERROR_UNKNOWN");
				}
				Hashtable receipts = Obj("receipts");
				if (receipts == null) _state["receipts"] = receipts = new Hashtable();
				if (receipts.ContainsKey(receipt))
				{
					return Convert.ToInt32(receipts[receipt]) == payout
						? Ok(request, Obj("wallet")) : Fail(request, "ID_SPARX_ERROR_UNKNOWN");
				}
				receipts[receipt] = payout;
				Balance += credit;
				Save();
				return Ok(request, Obj("wallet"));
			}
			case "/gacha/getTokens":
			{
				ArrayList tokens = new ArrayList();
				foreach (DictionaryEntry entry in Obj("tokens"))
				{
					tokens.Add(new Hashtable
					{
						{ "token", entry.Key },
						{ "count", entry.Value }
					});
				}
				return Ok(request, tokens);
			}
			case "/gacha/addtoken":
			{
				string token = Dot.String("token", data, string.Empty);
				int count = Dot.Integer(token, Obj("tokens"), 0) + 1;
				Obj("tokens")[token] = count;
				Save();
				return Ok(request, count);
			}
			case "/gacha/getSet":
				{
				Hashtable set = OfflineCatalog.GachaSet(Dot.String("group", data, string.Empty));
				return set == null ? Fail(request, "ID_SPARX_ERROR_UNKNOWN") : Ok(request, set);
			}
			case "/gacha/getTimestamp":
				return Ok(request, new Hashtable { { "version", OfflineCatalog.GachaVersion } });
			case "/gacha/pick":
				return Pick(request, data);
			}
			if (path.StartsWith("/ds/"))
			{
				string[] route = path.Split('/');
				if (route[2] != kUid || route.Length > 4 || (request.isPost && route.Length != 3))
				{
					return Fail(request, "ID_SPARX_ERROR_UNKNOWN");
				}
				Hashtable ds = Obj("ds");
				if (request.isPost)
				{
					Hashtable values = Dot.Object("data", data, null);
					if (values != null)
					{
						foreach (DictionaryEntry entry in values)
						{
							ds[entry.Key] = entry.Value;
						}
					}
					Save();
					return Ok(request, new Hashtable());
				}
				if (route.Length == 4 && route[3].Length > 0)
				{
					Hashtable fragment = new Hashtable();
					if (ds.ContainsKey(route[3])) fragment[route[3]] = ds[route[3]];
					return Ok(request, fragment);
				}
				return Ok(request, ds);
			}
			return Fail(request, "ID_SPARX_ERROR_NOT_CONNECTED");
		}

		private Response Pick(Request request, Hashtable data)
		{
			string token;
			int softCost;
			int hardCost;
			string itemKey;
			int itemCount;
			Hashtable result = OfflineCatalog.Pick(Dot.String("group", data, string.Empty), Dot.String("box", data, string.Empty), out token, out softCost, out hardCost, out itemKey, out itemCount);
			if (result == null)
			{
				return Fail(request, "ID_SPARX_ERROR_UNKNOWN");
			}
			if (Dot.String("payment", data, string.Empty) == "token")
			{
				int tokens = Dot.Integer(token, Obj("tokens"), 0);
				if (tokens < 1)
				{
					return Fail(request, "nsf");
				}
				Obj("tokens")[token] = tokens - 1;
				result["balance"] = tokens - 1;
			}
			else if (hardCost > 0)
			{
				if (hardCost > Balance)
				{
					return Fail(request, "nsf");
				}
				Balance -= hardCost;
			}
			else
			{
				// The game already took the rivets; this confirms the amount.
				result["softToPay"] = softCost;
			}
			if (itemKey != null)
			{
				AddItems(new Hashtable { { itemKey, itemCount } }, 1);
			}
			Save();
			return Ok(request, result);
		}
	}
}
