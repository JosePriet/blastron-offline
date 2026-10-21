using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class ChampionshipData : MonoBehaviour, ISaveLoadInterface
{
	private List<Division> _divisions = new List<Division>();

	private int[][] _stageStars;

	private int _currentDivision;

	private int[] _currentStages;

	private int[][] _highscorePoints;

	private int[][] _highscoreKills;

	private bool[][] _played;

	private ChampionshipPrefab _selectedPrefab;

	public int _selectedDivision = -1;

	private Division.Stage _selectedStage;

	public bool _rewardWeapon;

	public bool _rewardItem;

	public ChampionshipRewardTiers.Tier _tierToReward;

	private ChampionshipRewardTiers _rewardTiers;

	private int _lastDivUnlocked;

	private readonly string cdiv = "cdvi_";

	private int _MaxStars;

	public Division.Stage SelectedStage
	{
		get
		{
			return _selectedStage;
		}
		set
		{
			_selectedStage = value;
			EB.Debug.Log("SELECTED STAGE IS NOW " + _selectedStage);
		}
	}

	public static ChampionshipRewardTiers RewardTiers
	{
		get
		{
			return Instance._rewardTiers;
		}
	}

	public static ChampionshipData Instance { get; private set; }

	public static int CurrentDivision
	{
		get
		{
			return Instance._currentDivision;
		}
	}

	public static int CurrentDivIndex
	{
		get
		{
			return Instance._CurrentDivIndex;
		}
	}

	private int _CurrentDivIndex
	{
		get
		{
			return _currentDivision;
		}
	}

	public string Key
	{
		get
		{
			return "champ";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return false;
		}
	}

	private void Awake()
	{
		Instance = this;
		StartCoroutine(LoadData());
	}

	private void Start()
	{
		Hub.Instance.SaveLoadManager.Register(this);
	}

	private IEnumerator LoadData()
	{
		yield return Assets.LoadBundle("championship");
		UnityEngine.Object[] prefabs = Assets.LoadAll("divisions", typeof(GameObject));
		UnityEngine.Object[] array = prefabs;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject go = (GameObject)array[i];
			Division div = go.GetComponent<Division>();
			if (div != null)
			{
				_divisions.Add(div);
			}
		}
		GameObject tiersPrefab = Assets.Load("ChampionshipRewardTiers", typeof(GameObject)) as GameObject;
		_rewardTiers = tiersPrefab.GetComponent<ChampionshipRewardTiers>();
		_rewardTiers.Init();
		ResetData();
	}

	private void ResetData()
	{
		_stageStars = new int[_divisions.Count][];
		_highscorePoints = new int[_divisions.Count][];
		_highscoreKills = new int[_divisions.Count][];
		_played = new bool[_divisions.Count][];
		_MaxStars = 0;
		for (int i = 0; i < _divisions.Count; i++)
		{
			_stageStars[i] = new int[_divisions[i]._stages.Length];
			_highscorePoints[i] = new int[_divisions[i]._stages.Length];
			_highscoreKills[i] = new int[_divisions[i]._stages.Length];
			_played[i] = new bool[_divisions[i]._stages.Length];
			for (int j = 0; j < _stageStars[i].Length; j++)
			{
				_stageStars[i][j] = -1;
				_highscorePoints[i][j] = 0;
				_highscoreKills[i][j] = 0;
				_played[i][j] = false;
			}
			_MaxStars += _divisions[i]._stages.Length * 3;
		}
		_currentStages = new int[_divisions.Count];
		for (int k = 0; k < _currentStages.Length; k++)
		{
			_currentStages[k] = 0;
		}
		UnlocksManager.Instance.Unlock(cdiv + "0");
	}

	public static void LoadTuningData()
	{
		TuningManager tuningManager = Hub.Instance.TuningManager;
		for (int i = 0; i < Instance._divisions.Count; i++)
		{
			Division instance = Instance._divisions[i];
			string text = "championship.Division_" + (i + 1);
			tuningManager.Apply(instance, text);
		}
	}

	public static int TotalStars()
	{
		return Instance._TotalStars();
	}

	public static int MaxStars()
	{
		return Instance._MaxStars;
	}

	private int _TotalStars()
	{
		int num = 0;
		for (int i = 0; i < _divisions.Count; i++)
		{
			num += _Stars(i);
		}
		return num;
	}

	public static int TotalStars(int div)
	{
		return Instance._Stars(div);
	}

	private int _Stars(int div)
	{
		int num = 0;
		int[] array = _stageStars[div];
		for (int i = 0; i < array.Length && array[i] != -1; i++)
		{
			num += array[i] + 1;
		}
		return num;
	}

	public static int StarState(int division, string endsWith)
	{
		return Instance._StarState(division, endsWith);
	}

	public static int StarState(int division, Division.Stage stage)
	{
		return Instance._StarState(division, stage);
	}

	private int _StarState(int division, Division.Stage searchStage)
	{
		if (division >= 0 && division < _divisions.Count)
		{
			int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage stage) => stage.Equals(searchStage));
			if (num != -1)
			{
				return _stageStars[division][num];
			}
		}
		return -2;
	}

	public static int StageIndexFromName(int division, string endsWith)
	{
		return Instance._StageIndexFromName(division, endsWith);
	}

	private int _StageIndexFromName(int division, string endsWith)
	{
		return Array.FindIndex(_divisions[division]._stages, (Division.Stage stage) => stage._level.EndsWith(endsWith));
	}

	private int _StarState(int division, string endsWith)
	{
		int num = StageIndexFromName(division, endsWith);
		if (num != -1)
		{
			return _stageStars[division][num];
		}
		return -2;
	}

	public bool UpdateStarState(int division, Division.Stage stage, int points, int kills)
	{
		bool result = false;
		int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage curStage) => curStage.Equals(stage));
		if (num != -1)
		{
			int starsFor = stage.GetStarsFor(points, kills);
			int num2 = _stageStars[division][num];
			EB.Debug.Log(string.Format("Championship: achievedStars: {0}, curStars: {1}", starsFor, num2));
			if (starsFor > num2)
			{
				_stageStars[division][num] = starsFor;
				result = true;
				int num3 = TotalStars();
				int num4 = num3 + (num2 - starsFor);
				if (num3 != num4)
				{
					ChampionshipRewardTiers.Tier nextTierFor = _rewardTiers.GetNextTierFor(num4);
					ChampionshipRewardTiers.Tier nextTierFor2 = _rewardTiers.GetNextTierFor(num3);
					if (nextTierFor != null && nextTierFor2 != null && nextTierFor != nextTierFor2)
					{
						_rewardTiers.GrantRewardForTier(nextTierFor);
						_rewardWeapon = true;
						_tierToReward = nextTierFor;
					}
				}
				UnityEngine.Debug.LogWarning("ID " + num + " L " + _divisions[division]._stages.Length);
				if (num >= 0 && num < _divisions[division]._stages.Length)
				{
					string text = ((Hub.Instance.FacebookManager.LocalUser == null) ? null : Hub.Instance.FacebookManager.LocalUser.FacebookId);
					int score = num | (division << 16);
					Hub.Instance.LeaderboardManager.ReportScore(6, (-1).ToString(), LeaderboardManager.UpdateType.Max, score);
				}
				if (division == _currentDivision && num == _currentStages[_currentDivision])
				{
					int value = num + 1;
					value = Mathf.Clamp(value, 0, _divisions[division]._stages.Length - 1);
					_currentStages[_currentDivision] = value;
				}
				if (_currentDivision == division && num == _divisions[division]._stages.Length - 1)
				{
					if (_currentDivision < _divisions.Count - 1)
					{
						_currentDivision++;
						UnlocksManager.Instance.unlockedDivision = _currentDivision;
						string id = cdiv + _currentDivision;
						UnlocksManager.Instance.Unlock(id);
						_lastDivUnlocked = _currentDivision;
					}
					else
					{
						UnlocksManager.Instance.unlockedDivision = 3;
					}
				}
			}
		}
		return result;
	}

	public static void SetStarState(int division, Division.Stage stage, int newValue)
	{
		Instance._SetStarState(division, stage, newValue);
	}

	private void _SetStarState(int division, Division.Stage stage, int newValue)
	{
		int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage curStage) => curStage.Equals(stage));
		if (num != -1)
		{
			_stageStars[division][num] = Mathf.Clamp(newValue, -1, 2);
		}
	}

	public void GetHighScore(int division, Division.Stage stage, out int points, out int kills)
	{
		int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage obj) => obj.Equals(stage));
		if (num != -1)
		{
			kills = _highscoreKills[division][num];
			points = _highscorePoints[division][num];
		}
		else
		{
			kills = 0;
			points = 0;
		}
	}

	public bool SetHighScore(int division, Division.Stage stage, int points, int kills)
	{
		bool flag = false;
		int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage obj) => obj.Equals(stage));
		if (num != -1)
		{
			flag = ((stage._mode != GameAttributes.GameModeType.KillCount) ? (points > _highscorePoints[division][num]) : (kills > _highscoreKills[division][num]));
			if (flag)
			{
				_highscoreKills[division][num] = kills;
				_highscorePoints[division][num] = points;
			}
		}
		return flag;
	}

	public bool IsFirstTime(int division, Division.Stage stage)
	{
		int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage obj) => obj.Equals(stage));
		if (_played[division][num])
		{
			return false;
		}
		int points = 0;
		int kills = 0;
		GetHighScore(division, stage, out points, out kills);
		if (points == 0 && kills == 0)
		{
			return true;
		}
		return false;
	}

	public void SetPlayed(int division, Division.Stage stage, bool played)
	{
		int num = Array.FindIndex(_divisions[division]._stages, (Division.Stage obj) => obj.Equals(stage));
		if (num != -1)
		{
			_played[division][num] = played;
		}
	}

	public static Division.Stage CurrentForDiv(int div)
	{
		return Instance._CurrentForDiv(div);
	}

	private Division.Stage _CurrentForDiv(int div)
	{
		if (div >= 0 && div < _currentStages.Length)
		{
			return _divisions[div]._stages[_currentStages[div]];
		}
		return null;
	}

	public static Division CurrentDiv()
	{
		return Instance._CurrentDiv();
	}

	private Division _CurrentDiv()
	{
		return _divisions[_currentDivision];
	}

	public Division GetDivision(int idx)
	{
		if (idx >= 0 && idx < _divisions.Count)
		{
			return _divisions[idx];
		}
		EB.Debug.LogError("invalid division index: " + idx);
		return null;
	}

	public bool IsStageUnlocked(int divIdx, Division.Stage stage)
	{
		if (divIdx > _currentDivision)
		{
			return false;
		}
		return IsStageUnlocked(divIdx, _divisions[divIdx].FindIndexForStage(stage));
	}

	public bool IsStageUnlocked(int divIdx, int stageIdx)
	{
		if (stageIdx == 0)
		{
			return true;
		}
		if (stageIdx > 0)
		{
			return _stageStars[divIdx][stageIdx - 1] >= 0;
		}
		return false;
	}

	public bool IsFinishedChampionship()
	{
		return _divisions.Count - 1 == _currentDivision && _divisions[_currentDivision]._stages.Length - 1 == _currentStages[_currentDivision];
	}

	public bool IsFinishedChampionship(int div, int stage)
	{
		return _divisions.Count - 1 == div && _divisions[div]._stages.Length - 1 == stage;
	}

	private bool SetPrefab(int div, Division.Stage stage)
	{
		if (div != _selectedDivision || !stage.Equals(SelectedStage))
		{
			if (_selectedPrefab != null)
			{
				Assets.Unload(_selectedPrefab.gameObject);
				_selectedPrefab = null;
			}
			string text = null;
			switch (div)
			{
			case 0:
				text = "prefabs/Div_01_Bronze/";
				break;
			case 1:
				text = "prefabs/Div_02_Silver/";
				break;
			case 2:
				text = "prefabs/Div_03_Gold/";
				break;
			default:
				EB.Debug.LogError("invalid division specified: " + div + ", cannot load prefab '" + stage._championshipPrefab + "' for stage " + stage._level);
				break;
			}
			GameObject gameObject = Assets.Load(text + stage._championshipPrefab) as GameObject;
			if (null != gameObject)
			{
				_selectedPrefab = gameObject.GetComponent<ChampionshipPrefab>();
				SelectedStage = stage;
				_selectedDivision = div;
			}
		}
		return _selectedPrefab != null;
	}

	public ChampionshipPrefab GetSelectedPrefab()
	{
		return _selectedPrefab;
	}

	public void SetupGame(int division, Division.Stage stage)
	{
		if (!SetPrefab(division, stage))
		{
			EB.Debug.LogError("failed to load championship prefab " + SelectedStage._championshipPrefab);
		}
	}

	public bool PlayGame()
	{
		if (_selectedPrefab != null)
		{
			_selectedPrefab.Play(SelectedStage._level, SelectedStage._mode);
			return true;
		}
		ScreenManager.Instance.ShowError("[PLAY] Nice try bro, prefab not set up properly!", null);
		EB.Debug.LogError("failed to load championship prefab " + SelectedStage._championshipPrefab);
		return false;
	}

	public Division.Stage GetSelectedStage()
	{
		if (SelectedStage == null)
		{
			EB.Debug.LogWarning("no currently selected stage, null will be returned");
		}
		return SelectedStage;
	}

	public int GetSelectedDivision()
	{
		return _selectedDivision;
	}

	public void ResetProgress(int division)
	{
		if (division < 0)
		{
			for (int i = 0; i < _divisions.Count; i++)
			{
				ResetProgressForDivision(i);
			}
		}
		else
		{
			ResetProgressForDivision(division);
		}
	}

	private void ResetProgressForDivision(int div)
	{
		for (int i = 0; i < _divisions[div]._stages.Length; i++)
		{
			_stageStars[div][i] = -1;
			_highscorePoints[div][i] = 0;
			_highscoreKills[div][i] = 0;
			_played[div][i] = false;
			_currentStages[div] = 0;
		}
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		data["currdiv"] = _currentDivision;
		for (int i = 0; i < _divisions.Count; i++)
		{
			data["stageStars_" + i] = _stageStars[i];
			data["highScorePoints_" + i] = _highscorePoints[i];
			data["highScoreKills_" + i] = _highscoreKills[i];
			data["played_" + i] = _played[i];
		}
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		ResetData();
		_currentDivision = Dot.Integer("currdiv", data, 0);
		_currentDivision = Mathf.Clamp(_currentDivision, 0, _divisions.Count - 1);
		for (int i = 0; i < _divisions.Count; i++)
		{
			object arrayData = data["stageStars_" + i];
			if (arrayData != null)
			{
				ArrayList tmp = (ArrayList)arrayData;
				for (int j = 0; j < tmp.Count; j++)
				{
					int.TryParse(tmp[j].ToString(), out _stageStars[i][j]);
				}
			}
			arrayData = data["highScorePoints_" + i];
			if (arrayData != null)
			{
				ArrayList tmp2 = (ArrayList)arrayData;
				for (int k = 0; k < tmp2.Count; k++)
				{
					int.TryParse(tmp2[k].ToString(), out _highscorePoints[i][k]);
				}
			}
			arrayData = data["highScoreKills_" + i];
			if (arrayData != null)
			{
				ArrayList tmp3 = (ArrayList)arrayData;
				for (int l = 0; l < tmp3.Count; l++)
				{
					int.TryParse(tmp3[l].ToString(), out _highscoreKills[i][l]);
				}
			}
			arrayData = data["played_" + i];
			if (arrayData != null)
			{
				ArrayList tmp4 = (ArrayList)arrayData;
				for (int m = 0; m < tmp4.Count; m++)
				{
					bool.TryParse(tmp4[m].ToString(), out _played[i][m]);
				}
			}
		}
		yield break;
	}

	public static int DivisionsUnlocked()
	{
		return Instance._DivisionsUnlocked();
	}

	private int _DivisionsUnlocked()
	{
		return _lastDivUnlocked;
	}

	public IEnumerator PostLoad(Id userId)
	{
		for (int j = 0; j < _divisions.Count; j++)
		{
			int[] stars = _stageStars[j];
			_currentStages[j] = 0;
			for (int i = 0; i < stars.Length; i++)
			{
				_currentStages[j] = i;
				if (stars[i] == -1)
				{
					break;
				}
			}
			EB.Debug.Log("PostLoad - _currentStages[" + j + "]: " + _currentStages[j]);
		}
		int divs = _divisions.Count;
		for (int k = 0; k < divs; k++)
		{
			int checkDiv = divs - 1 - k;
			if (UnlocksManager.Instance.IsUnlocked(cdiv + checkDiv))
			{
				_lastDivUnlocked = checkDiv;
				break;
			}
		}
		yield break;
	}

	public void UnlockAll()
	{
		_currentDivision = _divisions.Count - 1;
		for (int i = 0; i < _currentStages.Length; i++)
		{
			_currentStages[i] = _divisions[i]._stages.Length - 1;
		}
		for (int j = 0; j < _divisions.Count; j++)
		{
			for (int k = 0; k < _divisions[j]._stages.Length; k++)
			{
				_stageStars[j][k] = 0;
			}
		}
		_lastDivUnlocked = _divisions.Count - 1;
	}

	public void AddTelemetryEventData(Hashtable eventData)
	{
		int num = -1;
		string value = string.Empty;
		if (_selectedDivision != -1)
		{
			Division division = GetDivision(_selectedDivision);
			if (division != null)
			{
				num = division.FindIndexForStage(_selectedStage);
				if (num >= 0)
				{
					value = _selectedStage._level;
				}
			}
		}
		eventData["division"] = _selectedDivision;
		eventData["stage"] = num;
		eventData["stageName"] = value;
		eventData["finished_championship"] = IsFinishedChampionship();
		eventData["currentDivsion"] = _currentDivision;
		eventData["currentStage"] = ((_currentDivision >= _divisions.Count) ? (-2) : _currentStages[_currentDivision]);
	}
}
