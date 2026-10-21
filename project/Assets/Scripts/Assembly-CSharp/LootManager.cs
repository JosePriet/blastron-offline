using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using Microsoft.Xna.Framework;
using UnityEngine;

public class LootManager : MonoBehaviour
{
	private class LootPool
	{
		private List<GameObject> _items = new List<GameObject>();

		public LootPool(GameObject prefab, int val, Transform parent)
		{
			for (int i = 0; i < 5; i++)
			{
				GameObject gameObject = Object.Instantiate(prefab) as GameObject;
				gameObject.GetComponent<Loot>().Value = val;
				gameObject.transform.parent = parent;
				_items.Add(gameObject);
			}
		}

		public void Spawn(int count, Vector3 pos, uint attackerID)
		{
			foreach (GameObject item in NextFree(count))
			{
				item.transform.position = pos;
				item.SetActive(true);
				item.SendMessage("SuckTo", attackerID);
			}
		}

		private IEnumerable NextFree(int count)
		{
			for (int i = 0; i < _items.Count; i++)
			{
				if (count <= 0)
				{
					break;
				}
				if (_items[i] != null && !_items[i].active)
				{
					count--;
					yield return _items[i];
				}
			}
		}

		public void Reset()
		{
			foreach (GameObject item in _items)
			{
				if (item != null)
				{
					item.GetComponent<Loot>().Reset();
				}
			}
		}

		public void Clear()
		{
			foreach (GameObject item in _items)
			{
				Object.Destroy(item);
			}
		}
	}

	private const int numDenominations = 3;

	private const int numPerDenomination = 5;

	private static LootManager _instance;

	public GameObject[] _lootXpPrefabs;

	public GameObject[] _lootRivetsPrefabs;

	private ReplicationView _view;

	private static bool _initialized;

	private int[,] _damageLootTable = new int[6, 3]
	{
		{ 100, 1, 2 },
		{ 250, 2, 4 },
		{ 500, 4, 10 },
		{ 1000, 8, 20 },
		{ 2000, 13, 35 },
		{ 3000, 20, 50 }
	};

	private int[] _lootValues = new int[3] { 10, 5, 1 };

	public bool EnableLoot;

	private LootPool[] _lootXpPools = new LootPool[3];

	private LootPool[] _lootRivetsPools = new LootPool[3];

	private AudioEmitter _audioEmitter;

	private bool _initializedRPCs;

	private bool _spawnedLoot;

	private float _spawnedLootAt;

	public static LootManager Instance
	{
		get
		{
			if (!_initialized)
			{
				_initialized = true;
			}
			return _instance;
		}
	}

	private bool SpawnedLoot
	{
		get
		{
			return _spawnedLoot;
		}
		set
		{
			if (value && !_spawnedLoot)
			{
				_spawnedLootAt = UnityEngine.Time.time;
			}
			_spawnedLoot = value;
		}
	}

	private void Start()
	{
		_instance = this;
		_audioEmitter = GetComponent<AudioEmitter>();
	}

	public void Init()
	{
		if (!_initializedRPCs)
		{
			Manager.RegisterRPCs(this);
			_initializedRPCs = true;
		}
		InitPools(_lootXpPools, _lootXpPrefabs);
		InitPools(_lootRivetsPools, _lootRivetsPrefabs);
		EnableLoot = true;
	}

	private void InitPools(LootPool[] lootPools, GameObject[] prefabs)
	{
		for (int i = 0; i < 3; i++)
		{
			if (prefabs[i] != null)
			{
				lootPools[i] = new LootPool(prefabs[i], _lootValues[i], base.transform);
			}
			else
			{
				EB.Debug.LogError(string.Format("Loot denomination {0} is missing a prefab. Will not be able to spawn correct loot amount.", i));
			}
		}
	}

	public void SpawnLoot(FVector2 pos, ref Misc.HitInfo hitInfo)
	{
		if (EnableLoot)
		{
			int i;
			for (i = 0; i < _damageLootTable.GetLength(0) - 1 && (float)_damageLootTable[i, 0] < hitInfo._damage; i++)
			{
			}
			Vector3 pos2 = pos.ToVector3();
			pos2.z = -20f;
			SpawnLoot(pos2, hitInfo._dir.ToVector3(), _damageLootTable[i, 1], _damageLootTable[i, 2], hitInfo._attackerID);
		}
	}

	private void Update()
	{
		if (SpawnedLoot && UnityEngine.Time.time > _spawnedLootAt + Tuning.Instance.Loot_PreSuckPeriod)
		{
			_audioEmitter.Play();
			SpawnedLoot = false;
		}
	}

	[RPC]
	private void SpawnLoot(Vector3 pos, Vector3 dir, int xp, int rivets, uint attackerID)
	{
		if (EnableLoot && attackerID < uint.MaxValue)
		{
			SpawnLoot(pos, xp, _lootXpPools, attackerID);
			SpawnLoot(pos, rivets, _lootRivetsPools, attackerID);
			SpawnedLoot = true;
		}
	}

	private void SpawnLoot(Vector3 pos, int val, LootPool[] pools, uint attackerID)
	{
		if (!EnableLoot)
		{
			return;
		}
		int num = _lootValues.Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = val / _lootValues[i];
			if (pools[i] != null)
			{
				pools[i].Spawn(num2, pos, attackerID);
			}
			val -= num2 * _lootValues[i];
		}
	}

	public void ShutDown()
	{
		LootPool[] lootXpPools = _lootXpPools;
		foreach (LootPool lootPool in lootXpPools)
		{
			lootPool.Clear();
		}
		LootPool[] lootRivetsPools = _lootRivetsPools;
		foreach (LootPool lootPool2 in lootRivetsPools)
		{
			lootPool2.Clear();
		}
	}
}
