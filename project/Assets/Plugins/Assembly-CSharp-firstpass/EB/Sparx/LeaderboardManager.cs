using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EB.Sparx
{
	public class LeaderboardManager : Manager
	{
		public enum UpdateType
		{
			Set = 0,
			Max = 1,
			Min = 2,
			Inc = 3,
			Dec = 4
		}

		private EndPoint _ep;

		public override void Initialize(Config config)
		{
			_ep = base.Hub.ApiEndPoint;
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public void GetUserPosition(int leaderboardId, string context, Id[] userIds, Action<int, string, int, int> callback)
		{
			if (base.Hub.State != HubState.Connected)
			{
				return;
			}
			Request request = _ep.Get("/leaderboards/" + leaderboardId);
			if (!string.IsNullOrEmpty(context))
			{
				request.AddQuery("context", context);
			}
			request.AddQuery("type", "userPos");
			if (userIds != null && userIds.Length > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = true;
				foreach (Id id in userIds)
				{
					if (!flag)
					{
						stringBuilder.Append("c");
					}
					stringBuilder.Append(id.Value);
					flag = false;
				}
				request.AddQuery("uids", stringBuilder.ToString());
			}
			_ep.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					int obj = Dot.Integer("userPos", result.hashtable, -1);
					int obj2 = Dot.Integer("size", result.hashtable, -1);
					callback(leaderboardId, null, obj, obj2);
				}
				else
				{
					callback(leaderboardId, result.localizedError, -1, -1);
				}
			});
		}

		public void GetScores(int leaderboardId, string context, int count, int startIndex, Id[] userIds, Action<int, string, Dictionary<Id, Hashtable>> callback)
		{
			if (base.Hub.State != HubState.Connected)
			{
				return;
			}
			Request request = _ep.Get("/leaderboards/" + leaderboardId);
			if (!string.IsNullOrEmpty(context))
			{
				request.AddQuery("context", context);
			}
			request.AddQuery("count", count);
			request.AddQuery("startIndex", startIndex);
			if (userIds != null && userIds.Length > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				bool flag = true;
				foreach (Id id in userIds)
				{
					if (!flag)
					{
						stringBuilder.Append("c");
					}
					stringBuilder.Append(id.Value);
					flag = false;
				}
				request.AddQuery("uids", stringBuilder.ToString());
			}
			_ep.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					ArrayList arrayList = Dot.Array("scores", result.hashtable, new ArrayList());
					ArrayList arrayList2 = Dot.Array("users", result.hashtable, new ArrayList());
					ArrayList arrayList3 = Dot.Array("results", result.hashtable, new ArrayList());
					Dictionary<Id, Hashtable> dictionary = new Dictionary<Id, Hashtable>(arrayList.Count);
					if (arrayList.Count > 0)
					{
						foreach (Hashtable item in arrayList)
						{
							Hashtable hashtable2 = new Hashtable(item.Keys.Count);
							foreach (object key4 in item.Keys)
							{
								hashtable2[key4] = item[key4];
							}
							string value = hashtable2["uid"].ToString();
							Id key = new Id(value);
							dictionary.Add(key, hashtable2);
						}
						if (arrayList2.Count > 0)
						{
							foreach (Hashtable item2 in arrayList2)
							{
								Id key2 = new Id(item2["uid"]);
								if (dictionary.ContainsKey(key2))
								{
									dictionary[key2].Add("name", item2["name"]);
								}
							}
						}
						if (arrayList3.Count > 0)
						{
							foreach (Hashtable item3 in arrayList3)
							{
								Id key3 = new Id(item3["uid"]);
								if (dictionary.ContainsKey(key3))
								{
									dictionary[key3].Add("name", item3["name"]);
								}
							}
						}
					}
					callback(leaderboardId, null, dictionary);
				}
				else
				{
					callback(leaderboardId, result.localizedError, null);
				}
			});
		}

		public void ReportScore(int leaderboardId, string context, UpdateType update, int score, params string[] info)
		{
			if (base.Hub.State != HubState.Connected)
			{
				if (update != UpdateType.Inc && update != UpdateType.Dec && base.Hub.GameCenterManager != null)
				{
					base.Hub.GameCenterManager.ReportScore(leaderboardId, score);
				}
				return;
			}
			Request request = _ep.Post("/leaderboards/" + leaderboardId);
			request.AddData("score", score);
			request.AddData("info", info);
			request.AddData("update", update.ToString().ToLower());
			request.AddData("nonce", Nonce.Generate());
			if (!string.IsNullOrEmpty(context))
			{
				request.AddData("context", context);
			}
			_ep.Service(request, (Response result) =>
			{
				if (result.sucessful)
				{
					score = Dot.Integer("global.score", result.hashtable, score);
					if (base.Hub.GameCenterManager != null)
					{
						base.Hub.GameCenterManager.ReportScore(leaderboardId, score);
					}
				}
			});
		}
	}
}
