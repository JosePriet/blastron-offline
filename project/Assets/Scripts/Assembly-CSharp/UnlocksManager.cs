using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class UnlocksManager : MonoBehaviour, ISaveLoadInterface
{
	public static string showUnlockSuffix = "_show";

	private ArrayList _unlocks = new ArrayList();

	private ArrayList _defaults = new ArrayList();

	private int _unlockVersion = 1;

	private bool _canSave;

	public int unlockedDivision;

	public bool _hasCollectedAllStars;

	private SafeBool _unlockAll = false;

	public static UnlocksManager Instance { get; private set; }

	public int Version
	{
		get
		{
			return _unlockVersion;
		}
	}

	public string Key
	{
		get
		{
			return "unlocks";
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
		Hub.Instance.SaveLoadManager.Register(this);
	}

	public static void UnlockAll()
	{
		if (Instance != null)
		{
			Instance._unlockAll = true;
			Instance._unlockVersion++;
		}
	}

	public bool IsUnlocked(string id)
	{
		return (bool)_unlockAll || _unlocks.Contains(id) || _defaults.Contains(id);
	}

	public ArrayList GetUnlocks()
	{
		return _unlocks;
	}

	public bool AreAnyUnlocked(params string[] ids)
	{
		foreach (string id in ids)
		{
			if (IsUnlocked(id))
			{
				return true;
			}
		}
		return false;
	}

	public bool AreAllUnlocked(params string[] ids)
	{
		foreach (string id in ids)
		{
			if (!IsUnlocked(id))
			{
				return false;
			}
		}
		return true;
	}

	public void UnlockDefault(string id)
	{
		if (!_defaults.Contains(id))
		{
			_defaults.Add(id);
		}
	}

	public Coroutine Unlock(string id)
	{
		if (!IsUnlocked(id))
		{
			_unlocks.Add(id);
			_unlockVersion++;
			if (_canSave)
			{
				return Hub.Instance.SaveLoadManager.Save(this);
			}
			EB.Debug.LogError("Can't save until we have loaded!");
		}
		return null;
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		data["values"] = new ArrayList(_unlocks);
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		ArrayList tmp = Dot.Array("values", data, new ArrayList());
		if (userId == Id.Offline)
		{
			_unlocks.Clear();
		}
		foreach (object item in tmp)
		{
			string str = item.ToString();
			if (!IsUnlocked(str))
			{
				EB.Debug.LogWarning("adding unlock " + str);
				_unlocks.Add(str);
			}
		}
		_unlockVersion++;
		_canSave = true;
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		yield break;
	}
}
