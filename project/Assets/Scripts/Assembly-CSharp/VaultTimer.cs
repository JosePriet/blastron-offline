using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class VaultTimer : MonoBehaviour, ISaveLoadInterface
{
	private static VaultTimer _this;

	public int _lastGrantTime;

	private string _nextGrant;

	private bool _saveRequired;

	public static VaultTimer Instance
	{
		get
		{
			return _this;
		}
	}

	public int GrantTimeLeft
	{
		get
		{
			return Tuning.Instance.Vault_TimerIntervalSeconds - (EB.Time.Now - _lastGrantTime);
		}
	}

	public bool FreeBundleAvailable
	{
		get
		{
			return GrantTimeLeft <= 0;
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
			return "vaultTimer";
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
	}

	private void Start()
	{
		Hub.Instance.SaveLoadManager.Register(this);
	}

	public void ForceVaultTimerReady()
	{
		_lastGrantTime = EB.Time.Now - Tuning.Instance.Vault_TimerIntervalSeconds;
	}

	public void ResetVaultTimer()
	{
		_lastGrantTime = EB.Time.Now;
		UnityEngine.Debug.LogWarning("VaultTimer > ResetVaultTimer > Reset Daily Spin lastGrantTime to " + _lastGrantTime);
		Hub.Instance.SaveLoadManager.Save(this);
	}

	public IEnumerator Save(Id userId, Hashtable data)
	{
		UnityEngine.Debug.Log(string.Concat("VaultTimer > Save > Saving Data for ", userId, " = ", JSON.Stringify(data)));
		data["lastGrantTime"] = _lastGrantTime;
		data["nextGrant"] = _nextGrant;
		EB.Debug.LogWarning("Saving last grant time! " + _lastGrantTime);
		yield break;
	}

	public IEnumerator Load(Id userId, Hashtable data)
	{
		if (data["lastGrantTime"] != null)
		{
			UnityEngine.Debug.Log(string.Concat("VaultTimer > Load > Loading Data for ", userId, " = ", JSON.Stringify(data)));
			_lastGrantTime = Dot.Integer("lastGrantTime", data, 0);
			_nextGrant = Dot.String("nextGrant", data, string.Empty);
		}
		yield break;
	}

	public IEnumerator PostLoad(Id userId)
	{
		yield break;
	}
}
