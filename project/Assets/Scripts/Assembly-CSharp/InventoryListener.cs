using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sparx;
using UnityEngine;

public class InventoryListener : MonoBehaviour, EB.Sparx.InventoryListener
{
	public enum PurchaseResult
	{
		Ok = 0,
		Failed = 1
	}

	public const float minTime = 0.6f;

	private Dictionary<SafeInt, SafeInt> _gachaTableTokens = new Dictionary<SafeInt, SafeInt>();

	private Dictionary<int, Action<PurchaseResult>> _callbacks = new Dictionary<int, Action<PurchaseResult>>();

	public static InventoryListener Instance { get; set; }

	public int GetNumFreeSpins(int gachaId)
	{
		SafeInt key = gachaId;
		if (_gachaTableTokens.ContainsKey(key))
		{
			return _gachaTableTokens[key];
		}
		return 0;
	}

	private void Awake()
	{
		Instance = this;
	}

	private void SendInventoryUpdated()
	{
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		bool flag = GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kTutorialOverride);
		if (mode == GameAttributes.ModeType.MP && flag)
		{
			Hub.Instance.InventoryManager.SinglePlayerClearUsedInventory();
		}
		ScreenManager.Instance.BroadcastMessage("OnInventoryUpdated", SendMessageOptions.DontRequireReceiver);
	}

	public bool UseInGame(string item, int quantity)
	{
		InventoryManager inventoryManager = Hub.Instance.InventoryManager;
		if (inventoryManager.EverythingFree)
		{
			return true;
		}
		if (inventoryManager.UseInGame(item, quantity))
		{
			if (!inventoryManager.IsOverriden)
			{
				GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
				GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
				bool flag = championshipMode == GameAttributes.ChampionshipType.kOn;
				Hashtable hashtable = new Hashtable();
				hashtable["mp"] = mode == GameAttributes.ModeType.MP;
				hashtable["champ"] = flag;
				hashtable["remain"] = inventoryManager.GetCount(inventoryManager.SafeName(item));
				if (flag)
				{
					ChampionshipData.Instance.AddTelemetryEventData(hashtable);
				}
				else
				{
					hashtable["stageName"] = GameAttributes.GetValue(GameAttributes.AttributeType.Level, Hub.Instance.GameManager.Game.Attributes);
				}
				Telemetry.Event("use/" + item, quantity, hashtable);
			}
			SendInventoryUpdated();
			return true;
		}
		return false;
	}

	public void OnInventorySynced(int requestId)
	{
		Action<PurchaseResult> value;
		if (_callbacks.TryGetValue(requestId, out value))
		{
			_callbacks.Remove(requestId);
			value(PurchaseResult.Ok);
		}
		SendInventoryUpdated();
	}

	public void OnInventoryItemsAdded(int requestId, Hashtable items)
	{
		Action<PurchaseResult> value;
		if (_callbacks.TryGetValue(requestId, out value))
		{
			_callbacks.Remove(requestId);
			value(PurchaseResult.Ok);
		}
		SendInventoryUpdated();
	}

	public void OnInventoryPurchaseFailed(int requestId)
	{
		Action<PurchaseResult> value;
		if (_callbacks.TryGetValue(requestId, out value))
		{
			_callbacks.Remove(requestId);
			value(PurchaseResult.Failed);
		}
	}

	public static Coroutine PurchaseSoft(Hashtable items, string src, int cost, Action<PurchaseResult> callback)
	{
		return Coroutines.Run(Instance._PurchaseSoft(items, src, cost, callback));
	}

	public static Coroutine PurchaseHard(Hashtable items, string src, int cost, Action<PurchaseResult> callback)
	{
		return Coroutines.Run(Instance._PurchaseHard(items, src, cost, callback));
	}

	private IEnumerator _PurchaseSoft(Hashtable items, string src, int cost, Action<PurchaseResult> callback)
	{
		InventoryManager manager = Hub.Instance.InventoryManager;
		float wait = UnityEngine.Time.realtimeSinceStartup + 0.6f;
		PurchaseResult result = PurchaseResult.Failed;
		if (Profile.Instance.RemoveSoftCurrency(cost))
		{
			int transId = manager.AddItems(items);
			bool done = false;
			_callbacks[transId] = (PurchaseResult r) =>
			{
				result = r;
				done = true;
			};
			while (!done)
			{
				yield return 1;
			}
			if (result == PurchaseResult.Failed)
			{
				EB.Debug.LogError("Purchase failed!");
				Profile.Instance.AddSoftCurrency(cost);
			}
			else if (cost > 0)
			{
				yield return Profile.Instance.Save();
				Telemetry.Event("spend_soft", cost);
				Telemetry.Event("spend_soft_src/" + src, cost);
				foreach (DictionaryEntry entry in items)
				{
					Telemetry.Event("buy/" + entry.Key, (int)entry.Value);
					Telemetry.Event("buy_soft/" + entry.Key, (int)entry.Value);
					if (EB.Replication.Manager.IsConnected)
					{
						Telemetry.Event("buy_soft_game/" + entry.Key, (int)entry.Value);
					}
					else
					{
						Telemetry.Event("buy_soft_ui/" + entry.Key, (int)entry.Value);
					}
				}
				Telemetry.Flush();
			}
			SendInventoryUpdated();
		}
		while (UnityEngine.Time.realtimeSinceStartup < wait)
		{
			yield return 1;
		}
		callback(result);
	}

	private IEnumerator _PurchaseHard(Hashtable items, string src, int cost, Action<PurchaseResult> callback)
	{
		InventoryManager manager = Hub.Instance.InventoryManager;
		float wait = UnityEngine.Time.realtimeSinceStartup + 0.6f;
		PurchaseResult result = PurchaseResult.Failed;
		int transId = manager.PurchaseItems(items, cost);
		bool done = false;
		_callbacks[transId] = (PurchaseResult r) =>
		{
			result = r;
			done = true;
		};
		while (!done)
		{
			yield return 1;
		}
		if (result == PurchaseResult.Ok)
		{
			if (cost > 0)
			{
				Telemetry.Event("spend_hard", cost);
				Telemetry.Event("spend_hard_src/" + src, cost);
				foreach (DictionaryEntry entry in items)
				{
					Telemetry.Event("buy/" + entry.Key, (int)entry.Value);
					Telemetry.Event("buy_hard/" + entry.Key, (int)entry.Value);
					if (EB.Replication.Manager.IsConnected)
					{
						Telemetry.Event("buy_hard_game/" + entry.Key, (int)entry.Value);
					}
					else
					{
						Telemetry.Event("buy_hard_ui/" + entry.Key, (int)entry.Value);
					}
				}
				Telemetry.Flush();
			}
			Profile.Instance.SpentHardCurrency(cost);
		}
		while (UnityEngine.Time.realtimeSinceStartup < wait)
		{
			yield return 1;
		}
		callback(result);
	}
}
