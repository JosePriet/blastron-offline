using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class CampaignData : MonoBehaviour, ISaveLoadInterface
{
	private List<SinglePlayerPrefab> _missions = new List<SinglePlayerPrefab>();

	private Dictionary<int, int> _highScores;

	private Dictionary<SinglePlayerPrefab, SinglePlayerRewards> _rewards;

	private static CampaignData _this;

	public static bool debugCampaignPlus;

	private string completeString = "campaign+";

	private bool _saveRequired;

	public static CampaignData Instance
	{
		get
		{
			return _this;
		}
	}

	public List<SinglePlayerPrefab> Missions
	{
		get
		{
			return _missions;
		}
	}

	public int CurrentMission { get; set; }

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
			return "highscore";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return _saveRequired;
		}
	}

	private void Awake()
	{
		_this = this;
		_highScores = new Dictionary<int, int>();
		_rewards = new Dictionary<SinglePlayerPrefab, SinglePlayerRewards>();
		StartCoroutine(LoadData());
	}

	private void Start()
	{
		Hub.Instance.SaveLoadManager.Register(this);
	}

	private IEnumerator LoadData()
	{
		yield return Assets.LoadBundle("singleplayer");
		Object[] prefabs = Assets.LoadAll("SinglePlayer_Prefabs", typeof(GameObject));
		Object[] array = prefabs;
		for (int i = 0; i < array.Length; i++)
		{
			GameObject go = (GameObject)array[i];
			SinglePlayerPrefab sp = go.GetComponent<SinglePlayerPrefab>();
			if (sp != null)
			{
				_missions.Add(sp);
				SinglePlayerRewards spr = go.GetComponent<SinglePlayerRewards>();
				if (spr != null)
				{
					_rewards.Add(sp, spr);
				}
			}
		}
		_missions.Sort((SinglePlayerPrefab p1, SinglePlayerPrefab p2) => string.Compare(p1.name, p2.name));
	}

	public string[] CampaignDataStrings()
	{
		List<string> list = new List<string>();
		foreach (SinglePlayerPrefab mission in _missions)
		{
			list.Add(mission.name);
		}
		return list.ToArray();
	}

	public SinglePlayerRewards GetRewards(SinglePlayerPrefab mission)
	{
		SinglePlayerRewards value = null;
		if (mission != null)
		{
			_rewards.TryGetValue(mission, out value);
		}
		return value;
	}

	public SinglePlayerRewards GetCurrentMissionRewards()
	{
		return GetRewards(CurrentMissionPrefab());
	}

	public int GetHighScore(SinglePlayerPrefab mission)
	{
		int value = 0;
		if (mission != null)
		{
			_highScores.TryGetValue(Hash.StringHash(mission.name), out value);
		}
		return value;
	}

	public bool SetHighScore(SinglePlayerPrefab mission, int highScore)
	{
		if (mission != null)
		{
			int highScore2 = GetHighScore(mission);
			if (highScore > highScore2)
			{
				_highScores[Hash.StringHash(mission.name)] = highScore;
				Hub.Instance.SaveLoadManager.Save(this);
				return true;
			}
		}
		return false;
	}

	public void UnlockNextMission()
	{
		int num = CurrentMission + 1;
		if (num < Missions.Count)
		{
			UnlocksManager.Instance.Unlock(Missions[num].name);
		}
		else
		{
			UnlocksManager.Instance.Unlock(completeString);
		}
	}

	public bool IsNextMissionUnlocked()
	{
		int num = CurrentMission + 1;
		if (num < Missions.Count)
		{
			return UnlocksManager.Instance.IsUnlocked(Missions[num].name);
		}
		return false;
	}

	public bool IsLastMission()
	{
		return CurrentMission + 1 == Missions.Count;
	}

	public bool IsCampaignPlus()
	{
		return UnlocksManager.Instance.IsUnlocked(completeString) || debugCampaignPlus;
	}

	public SinglePlayerPrefab CurrentMissionPrefab()
	{
		return _missions[CurrentMission];
	}

	public SinglePlayerPrefab MissionPrefabAt(int index)
	{
		if (index < _missions.Count)
		{
			return _missions[index];
		}
		return null;
	}

	public void SetCurrentMissionByName(string name)
	{
		int num = _missions.FindIndex((SinglePlayerPrefab obj) => obj.name == name);
		if (num != -1)
		{
			CurrentMission = num;
		}
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		Hashtable highScores = new Hashtable(_highScores);
		data["highScores"] = highScores;
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		if (data["highScores"] == null)
		{
			yield break;
		}
		Hashtable highScores = Dot.Object("highScores", data, new Hashtable());
		if (highScores == null)
		{
			yield break;
		}
		foreach (DictionaryEntry highScore in highScores)
		{
			int missionHash = 0;
			Serialization.Deserialize(highScore.Key, ref missionHash);
			int val = 0;
			Serialization.Deserialize(highScore.Value, ref val);
			_highScores[missionHash] = val;
		}
	}

	public IEnumerator PostLoad(Id userId)
	{
		yield break;
	}
}
