using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class Scoring : MonoBehaviour
{
	public enum ScoringMode
	{
		Points = 0,
		Kill = 1,
		Order = 2
	}

	public class DeadPlayer
	{
		public uint playerId;

		public int deathFrame;

		public DeadPlayer(uint id, int frame)
		{
			playerId = id;
			deathFrame = frame;
		}
	}

	private struct Score
	{
		public SafeInt points;

		public SafeInt kills;

		public SafeInt deaths;

		public SafeInt suicides;
	}

	private class BonusMultiplier
	{
		public SafeInt multiplier;

		public SafeFloat lastKillTime;
	}

	private ReplicationView _view;

	private Dictionary<int, Score> _scores;

	private Dictionary<uint, int> _playerToTeam;

	private List<int> _ranks;

	private List<BonusMultiplier> _multipliers;

	private List<DeadPlayer> _deadPlayers;

	private ScoringMode _mode;

	private List<EB.Sparx.Player> endGamePlayerList;

	public ScoringMode Mode
	{
		get
		{
			return _mode;
		}
	}

	public static Scoring Instance { get; set; }

	public int SessionHoF { get; private set; }

	public int SessionHoFLevel { get; private set; }

	private void Awake()
	{
		Instance = this;
		_scores = new Dictionary<int, Score>(4);
		_playerToTeam = new Dictionary<uint, int>(4);
		_ranks = new List<int>(4);
		_multipliers = new List<BonusMultiplier>(4);
		_deadPlayers = new List<DeadPlayer>();
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			int teamCount = GameAttributes.GetTeamCount();
			for (int i = 0; i < teamCount; i++)
			{
				_ranks.Add(i);
				BonusMultiplier bonusMultiplier = new BonusMultiplier();
				bonusMultiplier.multiplier = 1;
				_multipliers.Add(bonusMultiplier);
			}
			foreach (EB.Sparx.Player player in game.Players)
			{
				int value = player.Index % teamCount;
				_playerToTeam[player.PlayerId] = value;
			}
			endGamePlayerList = new List<EB.Sparx.Player>(game.Players);
		}
		SessionHoFLevel = -1;
	}

	public void Report(int xp)
	{
		int teamId = GetTeamId(EB.Replication.Manager.LocalPlayerId);
		Score value = default(Score);
		_scores.TryGetValue(teamId, out value);
		string context = Assets.CurrentScene;
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			context = GameAttributes.GetValue(GameAttributes.AttributeType.Level, game.Attributes);
		}
		Hub.Instance.LeaderboardManager.ReportScore(2, context, LeaderboardManager.UpdateType.Max, value.points);
		Hub.Instance.LeaderboardManager.ReportScore(3, context, LeaderboardManager.UpdateType.Max, value.kills);
		TournamentManager manager = Hub.Instance.GetManager<TournamentManager>();
		if (game != null && manager != null && !game.IsPrivate && manager.Active != null)
		{
			Hashtable hashtable = new Hashtable();
			hashtable["xp"] = xp;
			hashtable["points"] = value.points;
			hashtable["kills"] = value.kills;
			hashtable["deaths"] = value.deaths;
			hashtable["suicides"] = value.suicides;
			Hashtable attributes = game.Attributes;
			Hashtable attributes2 = game.LocalPlayer.Attributes;
			manager.Report(hashtable, attributes, attributes2);
		}
		StatManager.Instance.IncrementStat(StatManager.Stat.Points, (int)value.points);
	}

	public void ReportChampionship()
	{
		int teamId = GetTeamId(EB.Replication.Manager.LocalPlayerId);
		Game game = Hub.Instance.GameManager.Game;
		Score value = default(Score);
		_scores.TryGetValue(teamId, out value);
		EB.Sparx.Player player = game.GetPlayer(game.LocalPlayer.UserId);
		CharacterDB.Recipe recipe = new CharacterDB.Recipe(player.Attributes["recipe"]);
		int part = recipe.GetPart(CharacterDB.Category.kHead);
		EB.Debug.Log("OReportChampionship 4");
		GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship, game.Attributes));
		if (championshipMode == GameAttributes.ChampionshipType.kOn)
		{
			int selectedDivision = ChampionshipData.Instance.GetSelectedDivision();
			Division.Stage selectedStage = ChampionshipData.Instance.GetSelectedStage();
			Division division = ChampionshipData.Instance.GetDivision(selectedDivision);
			int num = division.FindIndexForStage(selectedStage);
			if (num >= 0)
			{
				int num2 = selectedDivision * 100 + num;
				int score = ((selectedStage._mode != GameAttributes.GameModeType.KillCount) ? value.points : value.kills);
				int num3 = ((selectedStage._mode != GameAttributes.GameModeType.KillCount) ? value.kills : value.points);
				Hub.Instance.LeaderboardManager.ReportScore(5, num2.ToString(), LeaderboardManager.UpdateType.Max, score, num3.ToString(), part.ToString());
			}
		}
		EB.Debug.Log("OReportChampionship 5");
	}

	public bool CheckTeams()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			Hashtable hashtable = new Hashtable();
			foreach (KeyValuePair<uint, int> item in _playerToTeam)
			{
				EB.Sparx.Player player = game.GetPlayer(item.Key);
				if (player != null)
				{
					hashtable[item.Value] = 1;
				}
			}
			if (hashtable.Count > 1)
			{
				return true;
			}
			UnityEngine.Debug.Log("Not enough teams to play");
		}
		return false;
	}

	[RPC]
	public void SetScoringMode(int mode)
	{
		_mode = (ScoringMode)mode;
		SendUpdate();
	}

	private void OnNetworkLoadedLevel()
	{
		_view = base.gameObject.GetComponent<ReplicationView>() ?? base.gameObject.AddComponent<ReplicationView>();
		_view.observed = this;
		_view.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
		_view.migrationSynchronization = NetworkMigrationSynchronization.Host;
	}

	[RPC]
	public void AddScore(uint playerId, int score)
	{
		if (_view.isMine)
		{
			Score value = default(Score);
			int teamId = GetTeamId(playerId);
			_scores.TryGetValue(teamId, out value);
			value.points = Mathf.Max(0, value.points + score);
			_scores[teamId] = value;
			if (score > 0)
			{
				EB.Sequence.Component.Activate(null, playerId, typeof(SequenceEvent_PlayerScored));
			}
			SendUpdate();
		}
	}

	[RPC]
	private void AddKill(uint attackerId, int killCount, int points, uint victimId)
	{
		if (!_view.isMine)
		{
			return;
		}
		Score value = default(Score);
		int teamId = GetTeamId(victimId);
		bool flag = false;
		if (teamId != -1)
		{
			_scores.TryGetValue(teamId, out value);
			++value.deaths;
			if (attackerId == uint.MaxValue || attackerId == victimId)
			{
				flag = true;
				++value.suicides;
				value.points = Mathf.Max(0, value.points + points);
			}
			_scores[teamId] = value;
		}
		Score value2 = default(Score);
		int teamId2 = GetTeamId(attackerId);
		if (teamId2 != -1 && !flag)
		{
			_scores.TryGetValue(teamId2, out value2);
			int num = points;
			num *= GetAndUpdateMultiplier(attackerId, false);
			value2.kills += (SafeInt)Mathf.Abs(killCount);
			value2.points = Mathf.Max(0, value2.points + num);
			_scores[teamId2] = value2;
			if (killCount > 0)
			{
				EB.Sequence.Component.Activate(null, attackerId, typeof(SequenceEvent_PlayerScored));
			}
		}
		SendUpdate();
	}

	[RPC]
	private void AddDeadPlayer(uint playerId)
	{
		_deadPlayers.Add(new DeadPlayer(playerId, UnityEngine.Time.frameCount));
		Game game = Hub.Instance.GameManager.Game;
		if (_deadPlayers.Count >= game.Players.Count - 1)
		{
			EB.Sequence.Component.Activate(null, null, typeof(SequenceEvent_SuddenDeathGameEnd));
		}
		else if (playerId == game.LocalPlayer.PlayerId && GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) == GameAttributes.ModeType.SP)
		{
			EB.Sequence.Component.Activate(null, null, typeof(SequenceEvent_SuddenDeathGameEnd));
		}
	}

	[RPC]
	private void DisplayMultiplier(int multiplier)
	{
		GameHUD component = Setup.Instance.GetComponent<HUD>().HudObject.GetComponent<GameHUD>();
		component.ShowMessage(Localizer.Format("ID_GAME_MESSAGE_MULTIPLIER", multiplier), Tuning.Instance.Timer_Multiplier);
	}

	private int GetAndUpdateMultiplier(uint playerId, bool suicide)
	{
		if (suicide)
		{
			return 1;
		}
		int teamId = GetTeamId(playerId);
		if (teamId < _multipliers.Count)
		{
			BonusMultiplier bonusMultiplier = _multipliers[teamId];
			float time = UnityEngine.Time.time;
			float num = time - bonusMultiplier.lastKillTime;
			if (num < Tuning.Instance.Scoring_Bonus_Multiplier_Timer)
			{
				++bonusMultiplier.multiplier;
				bonusMultiplier.multiplier = Mathf.Min(bonusMultiplier.multiplier, Tuning.Instance.Scoring_Bonus_Multiplier_Max);
				int num2 = bonusMultiplier.multiplier;
				_view.RPC("DisplayMultiplier", RPCMode.All, num2);
			}
			else
			{
				bonusMultiplier.multiplier = 1;
			}
			bonusMultiplier.lastKillTime = time;
			return bonusMultiplier.multiplier;
		}
		EB.Debug.LogWarning("Invalid team!");
		return 1;
	}

	public int GetTeamId(uint playerId)
	{
		int value;
		if (_playerToTeam.TryGetValue(playerId, out value))
		{
			return value;
		}
		return -1;
	}

	public void PlayerDied(uint playerId, uint attacker)
	{
		if (_mode == ScoringMode.Order)
		{
			_view.RPC("AddDeadPlayer", RPCMode.Server, playerId);
		}
		int teamId = GetTeamId(playerId);
		int teamId2 = GetTeamId(attacker);
		if (teamId == teamId2 || attacker == uint.MaxValue)
		{
			_view.RPC("AddKill", RPCMode.Server, attacker, -1, Tuning.Instance.Scoring_Points_Suicide, playerId);
		}
		else
		{
			_view.RPC("AddKill", RPCMode.Server, attacker, 1, Tuning.Instance.Scoring_Points_Kill, playerId);
		}
	}

	private void Print()
	{
		foreach (KeyValuePair<int, Score> score in _scores)
		{
			EB.Debug.Log(string.Concat("Score: Team: ", score.Key, " point: ", score.Value.points, " kills ", score.Value.kills));
		}
	}

	public List<DeadPlayer> GetDeathOrder()
	{
		return _deadPlayers;
	}

	public bool GetIsDead(uint playerId)
	{
		foreach (DeadPlayer deadPlayer in _deadPlayers)
		{
			if (deadPlayer.playerId == playerId)
			{
				return true;
			}
		}
		return false;
	}

	public int GetScoreByTeam(int teamId)
	{
		Score value;
		_scores.TryGetValue(teamId, out value);
		ScoringMode mode = _mode;
		if (mode == ScoringMode.Kill)
		{
			return value.kills - value.suicides;
		}
		return value.points;
	}

	public int GetScoreByPlayer(uint playerId)
	{
		return GetScoreByTeam(GetTeamId(playerId));
	}

	public int GetRankByPlayer(uint playerId)
	{
		int teamId = GetTeamId(playerId);
		return GetRankByTeam(teamId);
	}

	public int GetRankByTeam(int teamId)
	{
		int num = _ranks.IndexOf(teamId);
		return num + 1;
	}

	public int GetKillsByTeam(int teamId)
	{
		Score value;
		_scores.TryGetValue(teamId, out value);
		return value.kills;
	}

	public int GetPointsByTeam(int teamId)
	{
		Score value;
		_scores.TryGetValue(teamId, out value);
		return value.points;
	}

	public int GetKillsByPlayer(uint playerId)
	{
		return GetKillsByTeam(GetTeamId(playerId));
	}

	public int GetSuicidesByTeam(int teamId)
	{
		Score value;
		_scores.TryGetValue(teamId, out value);
		return value.suicides;
	}

	public int GetSuicidesByPlayer(uint playerId)
	{
		return GetSuicidesByTeam(GetTeamId(playerId));
	}

	public int GetDeathsByTeam(int teamId)
	{
		Score value;
		_scores.TryGetValue(teamId, out value);
		return value.deaths;
	}

	public int GetDeathsByPlayer(uint playerId)
	{
		return GetDeathsByTeam(GetTeamId(playerId));
	}

	private void SendUpdate()
	{
		_ranks.Sort((int team1, int team2) =>
		{
			int scoreByTeam = GetScoreByTeam(team1);
			int scoreByTeam2 = GetScoreByTeam(team2);
			return (scoreByTeam == scoreByTeam2) ? ((team1 >= team2) ? 1 : (-1)) : ((scoreByTeam <= scoreByTeam2) ? 1 : (-1));
		});
		ScreenManager.Instance.BroadcastMessage("OnScoringUpdated", SendMessageOptions.DontRequireReceiver);
	}

	private void OnSerializeView(BitStream bs)
	{
		if (bs.isReading)
		{
			_scores.Clear();
			int data = 0;
			bs.Serialize(ref data);
			for (int i = 0; i < data; i++)
			{
				int data2 = 0;
				Score value = default(Score);
				bs.Serialize(ref data2);
				bs.Serialize(ref value.points);
				bs.Serialize(ref value.kills);
				bs.Serialize(ref value.deaths);
				bs.Serialize(ref value.suicides);
				_scores[data2] = value;
			}
			_deadPlayers.Clear();
			int data3 = 0;
			bs.Serialize(ref data3);
			for (int j = 0; j < data3; j++)
			{
				uint data4 = 0u;
				int data5 = 0;
				bs.Serialize(ref data4);
				bs.Serialize(ref data5);
				_deadPlayers.Add(new DeadPlayer(data4, data5));
			}
			SendUpdate();
			return;
		}
		int data6 = _scores.Count;
		bs.Serialize(ref data6);
		foreach (KeyValuePair<int, Score> score in _scores)
		{
			int data7 = score.Key;
			Score value2 = score.Value;
			bs.Serialize(ref data7);
			bs.Serialize(ref value2.points);
			bs.Serialize(ref value2.kills);
			bs.Serialize(ref value2.deaths);
			bs.Serialize(ref value2.suicides);
		}
		int data8 = _deadPlayers.Count;
		bs.Serialize(ref data8);
		foreach (DeadPlayer deadPlayer in _deadPlayers)
		{
			uint data9 = deadPlayer.playerId;
			int data10 = deadPlayer.deathFrame;
			bs.Serialize(ref data9);
			bs.Serialize(ref data10);
		}
	}

	public void SortEndGamePlayers()
	{
		endGamePlayerList = new List<EB.Sparx.Player>(EB.Replication.Manager.Players);
		endGamePlayerList.Sort(SortPlayersEndGame);
	}

	public List<EB.Sparx.Player> GetEndGamePlayers()
	{
		return endGamePlayerList;
	}

	private int SortPlayersEndGame(EB.Sparx.Player a, EB.Sparx.Player b)
	{
		Scoring instance = Instance;
		int teamId = instance.GetTeamId(a.PlayerId);
		int teamId2 = instance.GetTeamId(b.PlayerId);
		if (teamId == teamId2)
		{
			return 0;
		}
		Game game = Hub.Instance.GameManager.Game;
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		if (gameMode == GameAttributes.GameModeType.SuddenDeath)
		{
			List<DeadPlayer> deathOrder = Instance.GetDeathOrder();
			int num = 0;
			int num2 = 0;
			foreach (DeadPlayer item in deathOrder)
			{
				if (item.playerId == a.PlayerId)
				{
					num = item.deathFrame;
				}
				else if (item.playerId == b.PlayerId)
				{
					num2 = item.deathFrame;
				}
			}
			if (num == num2)
			{
				if (GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) != GameAttributes.ModeType.MP && num == 0)
				{
					if (a.PlayerId == game.LocalPlayer.PlayerId)
					{
						return 1;
					}
					if (b.PlayerId == game.LocalPlayer.PlayerId)
					{
						return -1;
					}
				}
				int scoreByTeam = instance.GetScoreByTeam(teamId);
				int scoreByTeam2 = instance.GetScoreByTeam(teamId2);
				if (scoreByTeam == scoreByTeam2)
				{
					int num3 = UnityEngine.Random.Range(0, 2);
					if (num3 > 0)
					{
						return 1;
					}
					return -1;
				}
				return scoreByTeam2 - scoreByTeam;
			}
			if (num == 0)
			{
				return -1;
			}
			if (num2 == 0)
			{
				return 1;
			}
			return num2 - num;
		}
		int scoreByTeam3 = instance.GetScoreByTeam(teamId);
		int scoreByTeam4 = instance.GetScoreByTeam(teamId2);
		int num4 = 0;
		if (scoreByTeam3 == scoreByTeam4)
		{
			switch (gameMode)
			{
			case GameAttributes.GameModeType.KillCount:
				if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship, game.Attributes)) == GameAttributes.ChampionshipType.kOn)
				{
					if (a.PlayerId == EB.Replication.Manager.LocalPlayer.PlayerId)
					{
						num4 = -1;
					}
					else if (b.PlayerId == EB.Replication.Manager.LocalPlayer.PlayerId)
					{
						num4 = 1;
					}
				}
				else
				{
					scoreByTeam3 = GetPointsByTeam(teamId);
					scoreByTeam4 = GetPointsByTeam(teamId2);
					num4 = scoreByTeam4 - scoreByTeam3;
				}
				break;
			case GameAttributes.GameModeType.DeathMatch:
			case GameAttributes.GameModeType.TeamDeathMatch:
				scoreByTeam3 = GetKillsByTeam(teamId);
				scoreByTeam4 = GetKillsByTeam(teamId2);
				num4 = scoreByTeam4 - scoreByTeam3;
				break;
			}
			if (num4 == 0)
			{
				if (UnityEngine.Random.Range(0, 2) > 0)
				{
					return 1;
				}
				return -1;
			}
			return num4;
		}
		return scoreByTeam4 - scoreByTeam3;
	}

	public void UpdateHoF()
	{
		EB.Sparx.Player localPlayer = EB.Replication.Manager.LocalPlayer;
		List<EB.Sparx.Player> endGamePlayers = GetEndGamePlayers();
		int[] array = new int[endGamePlayers.Count];
		EB.Debug.Log(">>>> I got me players = " + endGamePlayers.Count);
		int num = Profile.Instance.HoFLevel();
		int num2 = 0;
		if (endGamePlayers.Count > 1)
		{
			for (int i = 0; i < endGamePlayers.Count; i++)
			{
				if (endGamePlayers[i].Attributes.ContainsKey("hofp"))
				{
					Type type = endGamePlayers[i].Attributes["hofp"].GetType();
					if (type == typeof(double))
					{
						double num3 = (double)endGamePlayers[i].Attributes["hofp"];
						array[i] = (int)num3;
					}
					else if (type == typeof(int))
					{
						array[i] = (int)endGamePlayers[i].Attributes["hofp"];
					}
				}
			}
			int[] array2 = array;
			foreach (int num4 in array2)
			{
				EB.Debug.Log("POINTS " + num4);
			}
			if (HoFData.Instance != null)
			{
				int place = GetRankByTeam(GetTeamId(localPlayer.PlayerId)) - 1;
				num2 = HoFData.Instance.Points(Profile.Instance.HoFLevel(), place, GetScoreByPlayer(localPlayer.PlayerId), array);
				Profile.Instance.HoFPoints += num2;
			}
		}
		int num5 = Profile.Instance.HoFLevel();
		SessionHoFLevel = -1;
		if (num5 > num)
		{
			SessionHoFLevel = num5;
		}
		SessionHoF = num2;
		EB.Debug.Log(string.Format("HoF points awarded for place {0} points {1} : {2}", GetRankByPlayer(localPlayer.PlayerId) - 1, GetScoreByPlayer(localPlayer.PlayerId), num2));
		string text = Dot.String("name", localPlayer.Attributes, string.Empty);
		string text2 = Dot.String("recipe.p[0]", localPlayer.Attributes, string.Empty);
		Hub.Instance.LeaderboardManager.ReportScore(4, string.Empty, LeaderboardManager.UpdateType.Set, Profile.Instance.HoFPoints, text, text2);
	}
}
