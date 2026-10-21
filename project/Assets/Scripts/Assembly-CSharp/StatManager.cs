using System;
using System.Collections;
using EB.Replication;
using EB.Sparx;
using UnityEngine;

public class StatManager : MonoBehaviour, ISaveLoadInterface
{
	public enum Stat
	{
		None = -1,
		Wins = 0,
		GamesPlayed = 1,
		Kills = 2,
		Deaths = 3,
		TotalRivets = 4,
		PromoGamesPlayed = 5,
		PromosDisplayed = 6,
		Points = 7,
		Suicides = 8,
		Upgrades = 9,
		Count = 10
	}

	private static StatManager _instance;

	private float[] _stats;

	private bool _saving;

	private bool _saveRequired;

	public static StatManager Instance
	{
		get
		{
			return _instance;
		}
	}

	public bool OnlyLoadLocal
	{
		get
		{
			return false;
		}
	}

	public string Key
	{
		get
		{
			return "stats";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return _saveRequired;
		}
	}

	public void Awake()
	{
		_instance = this;
		_stats = new float[10];
		_saving = false;
	}

	private void Start()
	{
		Hub.Instance.SaveLoadManager.Register(this);
	}

	public float GetStat(int type)
	{
		if (type == -1 || type == 10)
		{
			return 0f;
		}
		return _stats[type];
	}

	public void IncrementStat(Stat stat, float incVal = 1f)
	{
		bool flag = false;
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			bool flag2 = game.Attributes[GameAttributes.AttributeType.Championship.ToString()].Equals(GameAttributes.ChampionshipType.kOn.ToString()) && (stat == Stat.PromoGamesPlayed || stat == Stat.PromosDisplayed);
			bool flag3 = game.Attributes[GameAttributes.AttributeType.Mode.ToString()].Equals(GameAttributes.ModeType.MP.ToString()) && !EB.Replication.Manager.IsLocalGame && !game.IsPrivate;
			if (flag2 || flag3)
			{
				flag = true;
			}
		}
		else if (stat == Stat.Upgrades)
		{
			flag = true;
		}
		if (flag)
		{
			float stat2 = GetStat((int)stat);
			SetStat((int)stat, stat2 + incVal);
			UpdateStatAchievementProgress(stat);
			UpdateStatLeaderboards(stat);
		}
	}

	private void UpdateStatLeaderboards(Stat stat)
	{
		int score = Mathf.CeilToInt(GetStat((int)stat));
		switch (stat)
		{
		case Stat.Wins:
			Hub.Instance.LeaderboardManager.ReportScore(10, string.Empty, LeaderboardManager.UpdateType.Max, score);
			UpdateWinLossRatio();
			break;
		case Stat.GamesPlayed:
			Hub.Instance.LeaderboardManager.ReportScore(13, string.Empty, LeaderboardManager.UpdateType.Max, score);
			UpdateWinLossRatio();
			break;
		case Stat.Kills:
			Hub.Instance.LeaderboardManager.ReportScore(7, string.Empty, LeaderboardManager.UpdateType.Max, score);
			break;
		case Stat.Points:
			Hub.Instance.LeaderboardManager.ReportScore(8, string.Empty, LeaderboardManager.UpdateType.Max, score);
			break;
		case Stat.Suicides:
			Hub.Instance.LeaderboardManager.ReportScore(9, string.Empty, LeaderboardManager.UpdateType.Max, score);
			break;
		case Stat.Upgrades:
			Hub.Instance.LeaderboardManager.ReportScore(12, string.Empty, LeaderboardManager.UpdateType.Max, score);
			break;
		case Stat.Deaths:
		case Stat.TotalRivets:
		case Stat.PromoGamesPlayed:
		case Stat.PromosDisplayed:
			break;
		}
	}

	private void UpdateWinLossRatio()
	{
		float stat = GetStat(0);
		float stat2 = GetStat(1);
		int score = Mathf.CeilToInt(100f * (stat / stat2));
		Hub.Instance.LeaderboardManager.ReportScore(11, string.Empty, LeaderboardManager.UpdateType.Max, score);
	}

	private void UpdateStatAchievementProgress(Stat stat)
	{
		float stat2 = GetStat((int)stat);
		switch (stat)
		{
		case Stat.GamesPlayed:
		{
			double progress10 = stat2 * 10f;
			Hub.Instance.GameCenterManager.ReportProgress("PLAY_1", progress10);
			double progress11 = stat2 * 4f;
			Hub.Instance.GameCenterManager.ReportProgress("PLAY_2", progress11);
			double progress12 = stat2 * 2f;
			Hub.Instance.GameCenterManager.ReportProgress("PLAY_3", progress12);
			double progress13 = stat2;
			Hub.Instance.GameCenterManager.ReportProgress("PLAY_4", progress13);
			double progress14 = stat2 / 250f * 100f;
			Hub.Instance.GameCenterManager.ReportProgress("PLAY_5", progress14);
			break;
		}
		case Stat.Wins:
		{
			double progress5 = stat2 * 100f;
			Hub.Instance.GameCenterManager.ReportProgress("WIN_1", progress5);
			double progress6 = stat2 * 10f;
			Hub.Instance.GameCenterManager.ReportProgress("WIN_2", progress6);
			double progress7 = stat2 * 4f;
			Hub.Instance.GameCenterManager.ReportProgress("WIN_3", progress7);
			double progress8 = stat2 * 2f;
			Hub.Instance.GameCenterManager.ReportProgress("WIN_4", progress8);
			double progress9 = stat2;
			Hub.Instance.GameCenterManager.ReportProgress("WIN_5", progress9);
			break;
		}
		case Stat.Kills:
		{
			double progress = stat2 * 4f;
			Hub.Instance.GameCenterManager.ReportProgress("KILL_1", progress);
			double progress2 = stat2 * 2f;
			Hub.Instance.GameCenterManager.ReportProgress("KILL_2", progress2);
			double progress3 = stat2;
			Hub.Instance.GameCenterManager.ReportProgress("KILL_3", progress3);
			double progress4 = stat2 / 5f;
			Hub.Instance.GameCenterManager.ReportProgress("KILL_4", progress4);
			break;
		}
		}
	}

	public void SetStat(int type, float val)
	{
		Debug.Log("STAT IS SET TO " + val);
		_stats[type] = val;
		SaveStats();
	}

	public void ResetAllStats()
	{
		for (int i = 0; i < 10; i++)
		{
			ResetStat((Stat)i);
		}
		SaveStats();
	}

	public void SaveStats()
	{
		_saving = true;
	}

	private void LateUpdate()
	{
		if (_saving)
		{
			_saving = false;
			Hub.Instance.SaveLoadManager.Save(this);
		}
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		data[Stat.Wins.ToString()] = _stats[0];
		data[Stat.GamesPlayed.ToString()] = _stats[1];
		data[Stat.Kills.ToString()] = _stats[2];
		data[Stat.Deaths.ToString()] = _stats[3];
		data[Stat.TotalRivets.ToString()] = _stats[4];
		data[Stat.PromoGamesPlayed.ToString()] = _stats[5];
		data[Stat.PromosDisplayed.ToString()] = _stats[6];
		data[Stat.Points.ToString()] = _stats[7];
		data[Stat.Suicides.ToString()] = _stats[8];
		data[Stat.Upgrades.ToString()] = _stats[9];
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		for (int i = 0; i < _stats.Length; i++)
		{
			_stats[i] = 0f;
		}
		foreach (object key in data.Keys)
		{
			float value;
			if (string.IsNullOrEmpty(key.ToString()) || !float.TryParse(data[key].ToString(), out value))
			{
				continue;
			}
			foreach (int stat in Enum.GetValues(typeof(Stat)))
			{
				if (key.ToString() == ((Stat)stat).ToString())
				{
					_stats[stat] = value;
				}
			}
		}
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		yield break;
	}

	public void ResetStat(Stat statType, bool save = true)
	{
		if (statType != Stat.None && statType != Stat.Count)
		{
			_stats[(int)statType] = 0f;
		}
		if (save)
		{
			SaveStats();
		}
	}
}
