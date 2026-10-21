using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class ChallengeManager : MonoBehaviour, ISaveLoadInterface
{
	public enum Category
	{
		kCollect = 0,
		kPlay = 1,
		kKill = 2
	}

	[Serializable]
	public class ChallengeMetaData
	{
		[Serializable(Tag = "n")]
		public string challengeName;

		[Serializable(Tag = "t")]
		public string challengeTitle;
	}

	private class ChallengeStatus
	{
		public string name;

		public bool done;
	}

	private bool _saveRequired;

	private List<ChallengeMetaData>[] _challengeMetaData;

	private List<ChallengeStatus>[] _challengeStatusLists;

	private GameObject _sequenceRoot;

	public static bool DebugOn;

	public static ChallengeManager Instance { get; private set; }

	public string Key
	{
		get
		{
			return "chall";
		}
	}

	public bool PostLoadSaveRequired
	{
		get
		{
			return _saveRequired;
		}
	}

	public GameObject SequenceRoot
	{
		get
		{
			if (_sequenceRoot == null)
			{
				_sequenceRoot = new GameObject("challenges");
				Object.DontDestroyOnLoad(_sequenceRoot);
			}
			return _sequenceRoot;
		}
	}

	private void Awake()
	{
		Instance = this;
	}

	private IEnumerator Start()
	{
		InitFromMetaData();
		yield break;
	}

	private void LaunchSequence(string path, string name, Transform parent)
	{
		Object obj = Assets.Load(path);
		if (obj != null)
		{
			EB.Debug.Log(string.Format("Activating {0}", path));
			GameObject gameObject = Object.Instantiate(obj) as GameObject;
			gameObject.name = name;
			gameObject.transform.parent = parent;
		}
	}

	private void InitFromMetaData()
	{
		TextAsset textAsset = Assets.Load<TextAsset>("challengedb");
		if (textAsset == null)
		{
			EB.Debug.LogError("I (ChallengeManager) didn't find challengedb.txt - did you bundle?");
			return;
		}
		object obj = JSON.Parse(textAsset.text);
		if (obj != null)
		{
			_challengeMetaData = null;
			if (!Serialization.Deserialize(obj, ref _challengeMetaData))
			{
				EB.Debug.LogError("Challenge metadata was not deserialized");
				return;
			}
			_challengeStatusLists = new List<ChallengeStatus>[_challengeMetaData.Length];
			int num = 0;
			List<ChallengeMetaData>[] challengeMetaData = _challengeMetaData;
			foreach (List<ChallengeMetaData> list in challengeMetaData)
			{
				_challengeStatusLists[num] = new List<ChallengeStatus>(list.Count);
				int num2 = 0;
				foreach (ChallengeMetaData item in list)
				{
					_challengeStatusLists[num].Add(new ChallengeStatus
					{
						name = item.challengeName
					});
					num2++;
				}
				num++;
			}
		}
		else
		{
			EB.Debug.LogError("Challenge metadata was not parsed");
		}
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		UnityEngine.Debug.LogError(">>>>> Saving Challenges");
		int listCounter = 0;
		List<ChallengeStatus>[] challengeStatusLists = _challengeStatusLists;
		foreach (List<ChallengeStatus> challengeStatusList in challengeStatusLists)
		{
			List<string> completed = new List<string>();
			foreach (ChallengeStatus c in challengeStatusList)
			{
				if (c.done)
				{
					UnityEngine.Debug.Log(string.Format("On save, I think {0} in cat {1} is complete", c.name, listCounter));
					completed.Add(c.name);
				}
			}
			data["cat_" + listCounter] = new ArrayList(completed);
			listCounter++;
		}
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		Object.Destroy(_sequenceRoot);
		for (int i = 0; i < _challengeMetaData.Length; i++)
		{
			ArrayList completed = Dot.Array("cat_" + i, data, new ArrayList());
			if (completed == null)
			{
				continue;
			}
			foreach (string n in completed)
			{
				UnityEngine.Debug.Log(string.Format("On loading, I think {0} in cat {1} is complete", n, i));
				bool reset;
				MarkDone(i, n, out reset, false);
			}
		}
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		LaunchMasterSequence();
		string currentColl = VariableManager.Instance.GetString("s_CurrentChallenge_Coll", string.Empty);
		string currentPlay = VariableManager.Instance.GetString("s_CurrentChallenge_Play", string.Empty);
		string currentKill = VariableManager.Instance.GetString("s_CurrentChallenge_Kill", string.Empty);
		EB.Debug.Log(string.Format(">>>>> On Load: currentColl {0} currentPlay {1} currentKill {2}", currentColl, currentPlay, currentKill));
		yield break;
	}

	private void LaunchMasterSequence()
	{
		LaunchSequence("sequence_challengemaster", "master", SequenceRoot.transform);
	}

	public bool MarkDone(string n, bool save)
	{
		bool reset = false;
		for (int i = 0; i < _challengeStatusLists.Length && !MarkDone(i, n, out reset, save); i++)
		{
		}
		return reset;
	}

	private bool MarkDone(int cat, string n, out bool reset, bool save)
	{
		reset = false;
		int num = _challengeStatusLists[cat].FindIndex((ChallengeStatus chalStat) => chalStat.name == n);
		if (num >= 0)
		{
			int num2 = _challengeStatusLists[cat].FindIndex((ChallengeStatus chalStat) => !chalStat.done);
			reset = num2 != num;
			MarkDone(cat, num);
			if (save)
			{
				Hub.Instance.SaveLoadManager.Save(this);
			}
			return true;
		}
		return false;
	}

	private void MarkDone(int cat, int idx)
	{
		_challengeStatusLists[cat][idx].done = true;
	}

	public bool IsDone(string n)
	{
		bool found = false;
		for (int i = 0; i < _challengeStatusLists.Length; i++)
		{
			if (IsDone(i, n, out found))
			{
				return true;
			}
			if (found)
			{
				break;
			}
		}
		return false;
	}

	private bool IsDone(int cat, string n, out bool found)
	{
		ChallengeStatus challengeStatus = _challengeStatusLists[cat].Find((ChallengeStatus this_s) => this_s.name == n);
		found = false;
		if (challengeStatus != null)
		{
			found = true;
			return challengeStatus.done;
		}
		return false;
	}

	public void Complete(string n, bool save)
	{
		for (int i = 0; i < _challengeStatusLists.Length && !Complete(i, n, save); i++)
		{
		}
	}

	public bool Complete(int cat, string n, bool save)
	{
		int num = _challengeStatusLists[cat].FindIndex((ChallengeStatus chalStat) => chalStat.name == n);
		if (num >= 0)
		{
			MarkDone(cat, num);
			if (save)
			{
				Hub.Instance.SaveLoadManager.Save(this);
			}
			return true;
		}
		return false;
	}

	public void ClearAll()
	{
		for (int i = 0; i < _challengeStatusLists.Length; i++)
		{
			UncompleteChallengeList(i);
		}
		VariableManager.Instance.SetVariable("s_CurrentChallenge_Coll", string.Empty);
		VariableManager.Instance.SetVariable("s_CurrentChallenge_Play", string.Empty);
		VariableManager.Instance.SetVariable("s_CurrentChallenge_Kill", string.Empty);
		Hub.Instance.SaveLoadManager.DebugClear(this);
		Object.Destroy(_sequenceRoot);
		LaunchMasterSequence();
	}

	public void ClearCategory(int i)
	{
		UncompleteChallengeList(i);
		Hub.Instance.SaveLoadManager.Save(this);
	}

	private void UncompleteChallengeList(int i)
	{
		foreach (ChallengeStatus item in _challengeStatusLists[i])
		{
			item.done = false;
		}
	}

	private int NextActiveChallenge(int category)
	{
		return _challengeStatusLists[category].FindIndex((ChallengeStatus c) => !c.done);
	}
}
