using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class ChampionshipLeaderboards : MonoBehaviour
{
	public class PlayerData
	{
		public Id sparxId;

		public string personName;

		public string name;

		public int recipe;

		public int score;

		public int altScore;

		public int position;

		public string fbId;

		public bool isTied;
	}

	public delegate void OnLeaderboardReadDelegate(PlayerData[] playerData, string error);

	public delegate void OnUserPositionReadDelegate(int userPos, int size, string error);

	private static ChampionshipLeaderboards _instance;

	public int CachedRank;

	public List<PlayerData> CachedLeaderboardResults = new List<PlayerData>();

	public static ChampionshipLeaderboards Instance
	{
		get
		{
			return _instance;
		}
	}

	public bool IsReady()
	{
		return Hub.Instance.FacebookManager.HasAccessToken;
	}

	public static string GetContextString(int divisionIndex, int stageIndex)
	{
		return (divisionIndex * 100 + stageIndex).ToString();
	}

	public void GetFriendScores(int divisionIndex, int stageIndex, int count, int startIndex, OnLeaderboardReadDelegate callback)
	{
		InternalGetScores(divisionIndex, stageIndex, count, startIndex, GetFriends(), callback);
	}

	public void GetTopScores(int divisionIndex, int stageIndex, int count, OnLeaderboardReadDelegate callback)
	{
		InternalGetScores(divisionIndex, stageIndex, count * Tuning.Instance.ChampionshipFacebookGlobalLeaderboardCountMultiplier, 0, null, callback);
	}

	public void GetScoresAbovePlayer(int divisionIndex, int stageIndex, int count, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPlayer(divisionIndex, stageIndex, count, 0, null, callback);
	}

	public void GetScoresBelowPlayer(int divisionIndex, int stageIndex, int count, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPlayer(divisionIndex, stageIndex, 0, count, null, callback);
	}

	public void GetScoresAroundPlayer(int divisionIndex, int stageIndex, int top, int bottom, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPlayer(divisionIndex, stageIndex, top, bottom, null, callback);
	}

	public void GetFriendScoresAroundPlayer(int divisionIndex, int stageIndex, int top, int bottom, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPlayer(divisionIndex, stageIndex, top, bottom, GetFriends(), callback);
	}

	public void GetFriendScoresAbovePlayer(int divisionIndex, int stageIndex, int count, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPlayer(divisionIndex, stageIndex, count, 0, GetFriends(), callback);
	}

	public void GetFriendScoresAbovePosition(int divisionIndex, int stageIndex, int count, int startIndex, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPosition(divisionIndex, stageIndex, count, 0, startIndex, GetFriends(), callback);
	}

	public void GetFriendScoresBelowPlayer(int divisionIndex, int stageIndex, int count, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPlayer(divisionIndex, stageIndex, 0, count, GetFriends(), callback);
	}

	public void GetFriendScoresBelowPosition(int divisionIndex, int stageIndex, int count, int startIndex, OnLeaderboardReadDelegate callback)
	{
		InternalGetScoresAroundPosition(divisionIndex, stageIndex, 0, count, startIndex, GetFriends(), callback);
	}

	public void GetUserPosition(int divisionIndex, int stageIndex, OnUserPositionReadDelegate callback)
	{
		InternalGetUserPosition(divisionIndex, stageIndex, null, callback);
	}

	public void GetUserPositionAmongFriends(int divisionIndex, int stageIndex, OnUserPositionReadDelegate callback)
	{
		InternalGetUserPosition(divisionIndex, stageIndex, GetFriends(), callback);
	}

	private void Awake()
	{
		_instance = this;
	}

	private Id[] GetFriends()
	{
		List<Id> list = new List<Id>();
		for (int i = 0; i < Hub.Instance.FacebookManager.AppFriends.Length; i++)
		{
			list.Add(Hub.Instance.FacebookManager.AppFriends[i].UserId);
		}
		list.Add(Hub.Instance.FacebookManager.LocalUser.UserId);
		return list.ToArray();
	}

	private void InternalGetScores(int divisionIndex, int stageIndex, int count, int startIndex, Id[] userIds, OnLeaderboardReadDelegate callback)
	{
		string contextString = GetContextString(divisionIndex, stageIndex);
		Hub.Instance.LeaderboardManager.GetScores(5, contextString, count, startIndex, userIds, (int leaderboardId, string error, Dictionary<Id, Hashtable> data) =>
		{
			InternalLeaderboardReadDelegate(leaderboardId, error, data, callback);
		});
	}

	private void InternalGetScoresAroundPlayer(int divisionIndex, int stageIndex, int countAbove, int countBelow, Id[] userIds, OnLeaderboardReadDelegate callback)
	{
		InternalGetUserPosition(divisionIndex, stageIndex, userIds, (int userPos, int size, string error) =>
		{
			if (!string.IsNullOrEmpty(error))
			{
				EB.Debug.LogError("failed to read user position for championship score leaderboard");
				callback(null, error);
			}
			else
			{
				int num = Mathf.Min(0, userPos - countAbove - 1);
				InternalGetScoresAroundPosition(divisionIndex, stageIndex, countAbove, countBelow, userPos, userIds, callback);
			}
		});
	}

	private void InternalGetScoresAroundPosition(int divisionIndex, int stageIndex, int countAbove, int countBelow, int startIndex, Id[] userIds, OnLeaderboardReadDelegate callback)
	{
		int num = Mathf.Max(0, startIndex - countAbove - 1);
		int count = countAbove + countBelow + 1;
		string contextString = GetContextString(divisionIndex, stageIndex);
		Hub.Instance.LeaderboardManager.GetScores(5, contextString, count, startIndex, userIds, (int leaderboardId, string error, Dictionary<Id, Hashtable> playerData) =>
		{
			if (!string.IsNullOrEmpty(error))
			{
				EB.Debug.LogError("failed to read records above user for championship score leaderboard");
				callback(null, error);
			}
			else
			{
				InternalLeaderboardReadDelegate(leaderboardId, null, playerData, callback);
			}
		});
	}

	private void InternalGetUserPosition(int divisionIndex, int stageIndex, Id[] userIds, OnUserPositionReadDelegate callback)
	{
		string contextString = GetContextString(divisionIndex, stageIndex);
		Hub.Instance.LeaderboardManager.GetUserPosition(5, contextString, userIds, (int leaderboardId, string error, int userPos, int size) =>
		{
			callback(userPos, size, error);
		});
	}

	private void InternalLeaderboardReadDelegate(int leaderboardId, string error, Dictionary<Id, Hashtable> data, OnLeaderboardReadDelegate userCallback)
	{
		if (!string.IsNullOrEmpty(error))
		{
			userCallback(null, error);
			return;
		}
		List<PlayerData> tmp = new List<PlayerData>();
		List<long> list = new List<long>();
		foreach (Id key in data.Keys)
		{
			if (key.Valid)
			{
				list.Add(key.Value);
				ArrayList arrayList = Dot.Array("info", data[key], new ArrayList());
				int result = 0;
				int result2 = 0;
				if (arrayList.Count >= 1)
				{
					int.TryParse(arrayList[0].ToString(), out result);
				}
				if (arrayList.Count >= 2 && arrayList[1] != null)
				{
					int.TryParse(arrayList[1].ToString(), out result2);
				}
				PlayerData playerData = new PlayerData();
				playerData.name = Dot.String("name", data[key], string.Empty);
				playerData.recipe = result2;
				int score = Dot.Integer("score", data[key], 0);
				playerData.sparxId = key;
				playerData.fbId = null;
				playerData.score = score;
				playerData.altScore = result;
				tmp.Add(playerData);
			}
		}
		tmp.Sort((PlayerData p1, PlayerData p2) =>
		{
			int num = p1.score.CompareTo(p2.score);
			if (num == 0)
			{
				num = p1.altScore.CompareTo(p2.altScore);
				if (num == 0)
				{
					num = -1;
				}
			}
			return num * -1;
		});
		PlayerData pd;
		foreach (PlayerData item in tmp)
		{
			pd = item;
			FacebookUser facebookUser = Array.Find(Hub.Instance.FacebookManager.AppFriends, (FacebookUser curUser) => curUser.UserId == pd.sparxId);
			pd.personName = pd.name;
			if (facebookUser != null)
			{
				list.Remove(facebookUser.UserId.Value);
				if (!string.IsNullOrEmpty(facebookUser.FirstName + facebookUser.LastName))
				{
					pd.personName = facebookUser.FirstName + " " + facebookUser.LastName;
				}
				pd.fbId = facebookUser.FacebookId;
				EB.Debug.Log(string.Format("LB: {0} received uid:{1}, name: {2}, score:{3}, altScore: {4}, fbId: {5}", leaderboardId, pd.sparxId.ToString(), pd.name, pd.score, pd.altScore, pd.fbId));
			}
		}
		if (list.Count == 0)
		{
			userCallback(tmp.ToArray(), error);
			return;
		}
		Hub.Instance.FacebookManager.FetchSparxUsers(list, (string errStr, List<FacebookUser> fbUsers) =>
		{
			if (!string.IsNullOrEmpty(errStr) || fbUsers == null)
			{
				userCallback(null, errStr);
			}
			else
			{
				foreach (PlayerData pd2 in tmp)
				{
					FacebookUser facebookUser2 = fbUsers.Find((FacebookUser fbUser) => (fbUser.UserId.Value == pd2.sparxId.Value) ? true : false);
					if (facebookUser2 != null)
					{
						pd2.fbId = facebookUser2.FacebookId;
						if (!string.IsNullOrEmpty(facebookUser2.FirstName + facebookUser2.LastName))
						{
							pd2.personName = facebookUser2.FirstName + " " + facebookUser2.LastName;
						}
						EB.Debug.Log(string.Format("LB: {0} received uid:{1}, name: {2}, score:{3}, altScore: {4}, fbId: {5}", leaderboardId, pd2.sparxId.ToString(), pd2.name, pd2.score, pd2.altScore, pd2.fbId));
					}
				}
				userCallback(tmp.ToArray(), error);
			}
		});
	}
}
