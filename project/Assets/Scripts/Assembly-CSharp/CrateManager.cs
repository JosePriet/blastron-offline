using System;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sparx;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

public class CrateManager : MonoBehaviour
{
	[System.Serializable]
	[EB.Serializable(Tag = "cts")]
	public class ChanceToSpawn
	{
		[EB.Serializable(Tag = "cis")]
		public int cratesInScene;

		[EB.Serializable(Tag = "c")]
		public float chanceToSpawn;
	}

	[System.Serializable]
	[EB.Serializable(Tag = "isd")]
	public class ItemSpawningData
	{
		[EB.Serializable(Tag = "w")]
		public float itemWeight;

		[NoSerialize]
		public GameObject itemPrefab;

		[HideInInspector]
		[EB.Serializable(Tag = "n")]
		public string itemPrefabName;
	}

	[System.Serializable]
	[EB.Serializable(Tag = "tysd")]
	public class TypeSpawningData
	{
		[EB.Serializable(Tag = "t")]
		public Crate.Type crateType;

		[EB.Serializable(Tag = "w")]
		public float typeWeight;

		[EB.Serializable(Tag = "d")]
		public ItemSpawningData[] itemData;
	}

	[System.Serializable]
	public class ChestPrefab
	{
		public GameObject Prefab;

		public float Chance = 1f;
	}

	private static CrateManager _instance;

	private Transform[] _spawnPoints;

	private Transform[] _chestSpawnPoints;

	public int initialSpawnMin;

	public int initialSpawnMax;

	public AudioEvent _weaponCrateAudioEvent;

	public AudioEvent _healthPackAudioEvent;

	public AudioEvent _keyAudioEvent;

	public AudioEvent _boostAudioEvent;

	public AudioEvent _chestAudioEvent;

	private AudioEvent _vaultAudioEvent;

	public GameObject spawnChanceData;

	private ChanceToSpawn[] _turnSpawnData;

	public TypeSpawningData[] _spawnData;

	public GameObject campaignSpawnData;

	public GameObject privateSpawnData;

	public GameObject championshipSpawnData;

	private TypeSpawningData[] _campaignSpawnData;

	private TypeSpawningData[] _privateSpawnData;

	private TypeSpawningData[] _championshipSpawnData;

	private TypeSpawningData[] _activeSpawnData;

	private Dictionary<ViewId, int> _occupiedSpawnPoints;

	private static bool _initialized;

	private ReplicationView _view;

	public GameObject keyPrefab;

	private bool _keySpawned;

	public ChestPrefab[] chestPrefabs = new ChestPrefab[0];

	private bool _chestSpawned;

	private bool _respawnVault;

	private int _vaultIndex;

	internal List<GameObject> _cratesInScene = new List<GameObject>();

	public static CrateManager Instance
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

	public static bool DebugOn { get; set; }

	private static int SortByCratesInScene(ChanceToSpawn a, ChanceToSpawn b)
	{
		return a.cratesInScene - b.cratesInScene;
	}

	private void Awake()
	{
		_instance = this;
		_occupiedSpawnPoints = new Dictionary<ViewId, int>();
		if (_spawnData != null)
		{
			EB.Replication.Manager.RegisterPrefab(keyPrefab);
			TypeSpawningData[] spawnData = _spawnData;
			foreach (TypeSpawningData typeSpawningData in spawnData)
			{
				if (typeSpawningData.itemData == null)
				{
					continue;
				}
				ItemSpawningData[] itemData = typeSpawningData.itemData;
				foreach (ItemSpawningData itemSpawningData in itemData)
				{
					if (itemSpawningData.itemPrefab != null)
					{
						EB.Replication.Manager.RegisterPrefab(itemSpawningData.itemPrefab);
					}
				}
			}
		}
		if (privateSpawnData != null)
		{
			CrateSpawnData component = privateSpawnData.GetComponent<CrateSpawnData>();
			if (component != null)
			{
				_privateSpawnData = component._spawnData;
				TypeSpawningData[] array = _privateSpawnData;
				foreach (TypeSpawningData typeSpawningData2 in array)
				{
					if (typeSpawningData2.itemData == null)
					{
						continue;
					}
					ItemSpawningData[] itemData2 = typeSpawningData2.itemData;
					foreach (ItemSpawningData itemSpawningData2 in itemData2)
					{
						if (itemSpawningData2.itemPrefab != null)
						{
							EB.Replication.Manager.RegisterPrefab(itemSpawningData2.itemPrefab);
						}
					}
				}
			}
		}
		if (campaignSpawnData != null)
		{
			CrateSpawnData component2 = campaignSpawnData.GetComponent<CrateSpawnData>();
			if (component2 != null)
			{
				_campaignSpawnData = component2._spawnData;
				TypeSpawningData[] array2 = _campaignSpawnData;
				foreach (TypeSpawningData typeSpawningData3 in array2)
				{
					if (typeSpawningData3.itemData == null)
					{
						continue;
					}
					ItemSpawningData[] itemData3 = typeSpawningData3.itemData;
					foreach (ItemSpawningData itemSpawningData3 in itemData3)
					{
						if (itemSpawningData3.itemPrefab != null)
						{
							EB.Replication.Manager.RegisterPrefab(itemSpawningData3.itemPrefab);
						}
					}
				}
			}
		}
		if (championshipSpawnData != null)
		{
			CrateSpawnData component3 = championshipSpawnData.GetComponent<CrateSpawnData>();
			if (component3 != null)
			{
				_championshipSpawnData = component3._spawnData;
				TypeSpawningData[] array3 = _championshipSpawnData;
				foreach (TypeSpawningData typeSpawningData4 in array3)
				{
					if (typeSpawningData4.itemData == null)
					{
						continue;
					}
					ItemSpawningData[] itemData4 = typeSpawningData4.itemData;
					foreach (ItemSpawningData itemSpawningData4 in itemData4)
					{
						if (itemSpawningData4.itemPrefab != null)
						{
							EB.Replication.Manager.RegisterPrefab(itemSpawningData4.itemPrefab);
						}
					}
				}
			}
		}
		if (spawnChanceData != null)
		{
			CrateSpawnChanceData component4 = spawnChanceData.GetComponent<CrateSpawnChanceData>();
			if (component4 != null)
			{
				TuningManager tuningManager = Hub.Instance.TuningManager;
				tuningManager.Apply(component4, "cratespawnchancedata." + spawnChanceData.name);
				_turnSpawnData = component4._turnSpawnData;
				if (_turnSpawnData != null)
				{
					Array.Sort(_turnSpawnData, SortByCratesInScene);
				}
				else
				{
					_turnSpawnData = new ChanceToSpawn[0];
				}
			}
		}
		_view = base.gameObject.GetComponent<ReplicationView>() ?? base.gameObject.AddComponent<ReplicationView>();
		_view.observed = this;
		_view.stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed;
		_view.migrationSynchronization = NetworkMigrationSynchronization.Host;
		Init();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init()
	{
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode();
		bool flag = gameMode == GameAttributes.GameModeType.Tutorial;
		UseDefaultSpawnData();
		_respawnVault = false;
		GameObject gameObject = GameObject.Find("Locators");
		if (!(gameObject != null))
		{
			return;
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(gameObject, "Crates");
		if (objectExactMatch != null)
		{
			int childCount = objectExactMatch.transform.GetChildCount();
			_spawnPoints = new Transform[childCount];
			for (int i = 0; i < childCount; i++)
			{
				_spawnPoints[i] = objectExactMatch.transform.GetChild(i);
				EB.Replication.Manager.RegisterPrefab(_spawnPoints[i]);
			}
			if (!flag)
			{
				initialSpawnMin = Mathf.Clamp(initialSpawnMin, 0, childCount);
				initialSpawnMax = Mathf.Clamp(initialSpawnMax, 0, childCount);
				int num = UnityEngine.Random.Range(initialSpawnMin, initialSpawnMax);
				for (int j = 0; j < num; j++)
				{
					SpawnRandomCrate();
				}
			}
		}
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(gameObject, "Chests");
		if (objectExactMatch2 != null)
		{
			int childCount2 = objectExactMatch2.transform.GetChildCount();
			_chestSpawnPoints = new Transform[childCount2];
			for (int k = 0; k < childCount2; k++)
			{
				_chestSpawnPoints[k] = objectExactMatch2.transform.GetChild(k);
				EB.Replication.Manager.RegisterPrefab(_chestSpawnPoints[k]);
			}
			if (!flag)
			{
				SpawnChest();
			}
		}
	}

	private float GetChanceToSpawnMultiplierByPlayer(int numPlayers)
	{
		switch (numPlayers)
		{
		case 2:
			return 0.5f;
		case 3:
			return 0.75f;
		default:
			return 1f;
		}
	}

	private void ApplyTuning(string key)
	{
		key = "cratespawndata." + key;
		TuningManager tuningManager = Hub.Instance.TuningManager;
		GameObject gameObject = new GameObject(key);
		CrateSpawnData crateSpawnData = gameObject.AddComponent<CrateSpawnData>();
		crateSpawnData._spawnData = _activeSpawnData;
		tuningManager.Apply(crateSpawnData, key);
		_activeSpawnData = crateSpawnData._spawnData;
		UnityEngine.Object.Destroy(gameObject);
	}

	public void UseCampaignSpawnData()
	{
		if (_campaignSpawnData != null)
		{
			_activeSpawnData = _campaignSpawnData;
			ApplyTuning(campaignSpawnData.name);
		}
		else
		{
			UseDefaultSpawnData();
		}
	}

	public void UseChampionshipSpawnData()
	{
		if (_championshipSpawnData != null)
		{
			_activeSpawnData = _championshipSpawnData;
			ApplyTuning(championshipSpawnData.name);
		}
		else
		{
			UseDefaultSpawnData();
		}
	}

	public void UseDefaultSpawnData()
	{
		_activeSpawnData = _spawnData;
		ApplyTuning("DefaultCrateSpawnData");
	}

	public void UsePrivateSpawnData()
	{
		if (_privateSpawnData != null)
		{
			_activeSpawnData = _privateSpawnData;
			ApplyTuning(privateSpawnData.name);
		}
		else
		{
			UseDefaultSpawnData();
		}
	}

	private void SpawnKey()
	{
		if (_spawnPoints != null && EB.Replication.Manager.IsHost && (_activeSpawnData == _spawnData || _activeSpawnData == _championshipSpawnData))
		{
			int spawnIndex = GetSpawnIndex(keyPrefab);
			if (spawnIndex >= 0)
			{
				_view.RPC("DoSpawnKey", RPCMode.All, _spawnPoints[spawnIndex].position, _spawnPoints[spawnIndex].rotation, spawnIndex);
			}
		}
	}

	private int FindRandomChest()
	{
		float num = 0f;
		ChestPrefab[] array = chestPrefabs;
		foreach (ChestPrefab chestPrefab in array)
		{
			num += chestPrefab.Chance;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		num = 0f;
		for (int j = 0; j < chestPrefabs.Length; j++)
		{
			num += chestPrefabs[j].Chance;
			if (num2 <= num)
			{
				return j;
			}
		}
		return UnityEngine.Random.Range(0, chestPrefabs.Length);
	}

	private void SpawnChest()
	{
		if (!EB.Replication.Manager.IsHost || (_activeSpawnData != _spawnData && _activeSpawnData != _championshipSpawnData))
		{
			return;
		}
		float num = UnityEngine.Random.Range(0f, 1f);
		float num2 = 1f;
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			int count = game.Players.Count;
			num2 = GetChanceToSpawnMultiplierByPlayer(count);
		}
		float num3 = ((GameAttributes.ChampionshipType.kOn != GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship))) ? Tuning.Instance.Chest_ChanceToSpawn : Tuning.Instance.Chest_ChanceToSpawnChampionship);
		if (_chestSpawned || !(num < num3 * num2) || _chestSpawnPoints == null)
		{
			return;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < _chestSpawnPoints.Length; i++)
		{
			list.Add(i);
		}
		int num4 = FindRandomChest();
		GameObject prefab = chestPrefabs[num4].Prefab;
		if (prefab == null)
		{
			EB.Debug.LogError("null chest prefabs at index " + num4);
			return;
		}
		Crate component = prefab.GetComponent<Crate>();
		float width = 0f;
		float num5 = 0f;
		if ((bool)component)
		{
			width = component.widthBuffer;
			num5 = component.depthBuffer;
		}
		int num6 = 0;
		while (num6 < list.Count)
		{
			int num7 = list[num6];
			float num8 = TestForTerrain(_chestSpawnPoints[num7]);
			if (num8 > 0f)
			{
				if (TestForPlayers(_chestSpawnPoints[num7], width, num8 + num5))
				{
					EB.Debug.Log("Rejecting spawn point because of player contact!");
					list.Remove(num7);
				}
				else
				{
					num6++;
				}
			}
			else
			{
				EB.Debug.Log("Rejecting spawn point because there's no terrain!");
				list.Remove(num7);
			}
		}
		if (list.Count > 0)
		{
			int num9 = list[UnityEngine.Random.Range(0, list.Count)];
			_view.RPC("DoSpawnChest", RPCMode.All, _chestSpawnPoints[num9].position, _spawnPoints[num9].rotation, num9, num4);
		}
		else
		{
			EB.Debug.Log("Unable to spawn a chest, no valid spawn points");
		}
	}

	public void SpawnKey(Transform transform)
	{
		if (EB.Replication.Manager.IsHost)
		{
			_view.RPC("DoSpawnKey", RPCMode.All, transform.position, transform.rotation, -1);
		}
	}

	public void SpawnRandomCrate()
	{
		if (_spawnPoints != null && EB.Replication.Manager.IsHost)
		{
			int typeIndexToSpawn = GetTypeIndexToSpawn();
			int itemIndexToSpawn = GetItemIndexToSpawn(typeIndexToSpawn);
			GameObject crateToSpawn = GetCrateToSpawn(typeIndexToSpawn, itemIndexToSpawn);
			int spawnIndex = GetSpawnIndex(crateToSpawn);
			if (spawnIndex >= 0)
			{
				_view.RPC("SpawnCrate", RPCMode.All, typeIndexToSpawn, itemIndexToSpawn, _spawnPoints[spawnIndex].position, _spawnPoints[spawnIndex].rotation, spawnIndex);
			}
		}
	}

	public void SpawnSP()
	{
		if (_spawnPoints != null && EB.Replication.Manager.IsHost)
		{
			int num = 6;
			int num2 = 0;
			GameObject crateToSpawn = GetCrateToSpawn(num, num2);
			int spawnIndex = GetSpawnIndex(crateToSpawn);
			if (spawnIndex >= 0)
			{
				_view.RPC("SpawnCrate", RPCMode.All, num, num2, _spawnPoints[spawnIndex].position, _spawnPoints[spawnIndex].rotation, spawnIndex);
			}
		}
	}

	public void SpawnCoin()
	{
		int num = 5;
		int itemIndexToSpawn = GetItemIndexToSpawn(num);
		GameObject crateToSpawn = GetCrateToSpawn(num, itemIndexToSpawn);
		int spawnIndex = GetSpawnIndex(crateToSpawn);
		if (spawnIndex >= 0)
		{
			_view.RPC("SpawnCrate", RPCMode.All, num, itemIndexToSpawn, _spawnPoints[spawnIndex].position, _spawnPoints[spawnIndex].rotation, spawnIndex);
		}
	}

	private void RespawnVault()
	{
		if (!_respawnVault)
		{
			return;
		}
		_respawnVault = false;
		if (_spawnPoints != null && EB.Replication.Manager.IsHost)
		{
			int num = 5;
			int vaultIndex = _vaultIndex;
			GameObject crateToSpawn = GetCrateToSpawn(num, vaultIndex);
			int spawnIndex = GetSpawnIndex(crateToSpawn);
			if (spawnIndex >= 0)
			{
				_view.RPC("SpawnCrate", RPCMode.All, num, vaultIndex, _spawnPoints[spawnIndex].position, _spawnPoints[spawnIndex].rotation, spawnIndex);
			}
		}
	}

	public void NextTurn(uint playerId)
	{
		if (!_chestSpawned)
		{
			SpawnChest();
		}
		else if (!_keySpawned)
		{
			SpawnKey();
		}
		if (_respawnVault)
		{
			RespawnVault();
		}
		int count = _occupiedSpawnPoints.Keys.Count;
		for (int i = 0; i < _turnSpawnData.Length; i++)
		{
			ChanceToSpawn chanceToSpawn = _turnSpawnData[i];
			if (chanceToSpawn.cratesInScene >= count)
			{
				float num = UnityEngine.Random.Range(0f, 1f);
				float num2 = 1f;
				Game game = Hub.Instance.GameManager.Game;
				if (game != null)
				{
					int count2 = game.Players.Count;
					num2 = GetChanceToSpawnMultiplierByPlayer(count2);
				}
				if (num < chanceToSpawn.chanceToSpawn * num2)
				{
					SpawnRandomCrate();
				}
				break;
			}
		}
		base.gameObject.BroadcastMessage("OnPlayerTurnStart", playerId, SendMessageOptions.DontRequireReceiver);
	}

	public void SequenceSpawnCrate(GameObject prefab, Crate.Type crateType, string spawnPoint, string tutorialId, Vector3 tutorialArrowOffset)
	{
		if (!(prefab != null))
		{
			return;
		}
		Transform transform = null;
		if (crateType == Crate.Type.kChest)
		{
			Transform[] chestSpawnPoints = _chestSpawnPoints;
			foreach (Transform transform2 in chestSpawnPoints)
			{
				if (transform2.name == spawnPoint)
				{
					transform = transform2;
					break;
				}
			}
		}
		else
		{
			for (int j = 0; j < _spawnPoints.Length; j++)
			{
				Transform transform3 = _spawnPoints[j];
				if (transform3.name == spawnPoint)
				{
					transform = transform3;
					break;
				}
			}
		}
		if (transform != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, transform.position, transform.rotation) as GameObject;
			if (gameObject != null)
			{
				UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
				uITutorialArrow.tutorialId = tutorialId;
				uITutorialArrow.offset = tutorialArrowOffset;
				uITutorialArrow.arrowType = BigArrow.ArrowType.World;
			}
		}
	}

	[RPC]
	private void DoSpawnKey(Vector3 position, Quaternion rotation, int spawnIndex)
	{
		if (keyPrefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(keyPrefab, position, rotation) as GameObject;
			ReplicationView replicationView = gameObject.GetComponent<ReplicationView>();
			if (replicationView == null)
			{
				replicationView = gameObject.AddComponent<ReplicationView>();
			}
			_occupiedSpawnPoints[replicationView.viewId] = spawnIndex;
			_keySpawned = true;
		}
	}

	[RPC]
	private void DoSpawnChest(Vector3 position, Quaternion rotation, int spawnIndex, int chestIndex)
	{
		GameObject prefab = chestPrefabs[chestIndex].Prefab;
		if (prefab != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(prefab, position, rotation) as GameObject;
			ReplicationView component = gameObject.GetComponent<ReplicationView>();
			if (component == null)
			{
				component = gameObject.AddComponent<ReplicationView>();
			}
			_cratesInScene.Add(gameObject);
			_chestSpawned = true;
		}
	}

	[RPC]
	private void SpawnCrate(int crateType, int crateIndex, Vector3 position, Quaternion rotation, int spawnIndex)
	{
		GameObject crateToSpawn = GetCrateToSpawn(crateType, crateIndex);
		if (crateToSpawn != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(crateToSpawn, position, rotation) as GameObject;
			ReplicationView replicationView = gameObject.GetComponent<ReplicationView>();
			if (replicationView == null)
			{
				replicationView = gameObject.AddComponent<ReplicationView>();
			}
			_cratesInScene.Add(gameObject);
			_occupiedSpawnPoints[replicationView.viewId] = spawnIndex;
			if (crateType == 5)
			{
				_vaultIndex = crateIndex;
				gameObject.transform.parent = base.gameObject.transform;
			}
		}
	}

	public void VaultPickup(bool slotAvailable, string itemName, string playerName)
	{
		_view.RPC("DoVaultPickup", RPCMode.AllBuffered, slotAvailable, itemName, playerName);
	}

	[RPC]
	private void DoVaultPickup(bool slotAvailable, string itemName, string playerName)
	{
		GameHUD component = Setup.Instance.GetComponent<HUD>().HudObject.GetComponent<GameHUD>();
		if (slotAvailable)
		{
			string pickupMessage = Crate.GetPickupMessage(playerName, itemName, 1);
			component.ShowMessage2(pickupMessage, Tuning.Instance.Timer_CratePickup);
			return;
		}
		Player player = LevelData.Instance.GetPlayer();
		if (player.PlayerName == playerName)
		{
			component.ShowMessage2(Localizer.Format("ID_GAME_MESSAGE_VAULT_FULL_LOCAL"), Tuning.Instance.Timer_CratePickup);
			return;
		}
		component.ShowMessage2(Localizer.Format("ID_GAME_MESSAGE_VAULT_FULL_REMOTE", playerName), Tuning.Instance.Timer_CratePickup);
	}

	private int GetSpawnIndex(GameObject cratePrefab)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < _spawnPoints.Length; i++)
		{
			bool flag = false;
			foreach (int value in _occupiedSpawnPoints.Values)
			{
				if (value == i)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				list.Add(i);
			}
		}
		Crate component = cratePrefab.GetComponent<Crate>();
		float width = 0f;
		float num = 0f;
		if ((bool)component)
		{
			width = component.widthBuffer;
			num = component.depthBuffer;
		}
		int num2 = 0;
		while (num2 < list.Count)
		{
			int num3 = list[num2];
			float num4 = TestForTerrain(_spawnPoints[num3]);
			if (num4 > 0f)
			{
				if (TestForPlayers(_spawnPoints[num3], width, num4 + num))
				{
					EB.Debug.Log("Rejecting spawn point because of player contact!");
					list.Remove(num3);
				}
				else
				{
					num2++;
				}
			}
			else
			{
				EB.Debug.Log("Rejecting spawn point because there's no terrain!");
				list.Remove(num3);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		return -1;
	}

	private float TestForTerrain(Transform spawnPoint)
	{
		FVector2 fVector = FSHelper.Vector3ToFVector2(spawnPoint.position);
		FVector2 fVector2 = FSHelper.Vector3ToFVector2(spawnPoint.up) * -1f;
		float num = 1000f;
		World physicsWorld = FSWorldComponent.PhysicsWorld;
		bool hit = false;
		FVector2 anonHitPoint = FVector2.Zero;
		physicsWorld.RayCast((Fixture fixture, FVector2 point, FVector2 normal, float fraction) =>
		{
			if (fixture.Body.UserTag != "Player")
			{
				hit = true;
				anonHitPoint = point;
				return fraction;
			}
			return -1f;
		}, fVector, fVector + fVector2 * num);
		if (hit)
		{
			return (anonHitPoint - fVector).Length();
		}
		return -1f;
	}

	private bool TestForPlayers(Transform spawnPoint, float width, float length)
	{
		FVector2 min = new FVector2(spawnPoint.position.x - width, spawnPoint.position.y - length);
		FVector2 max = new FVector2(spawnPoint.position.x + width, spawnPoint.position.y);
		bool playerHit = false;
		AABB aabb = new AABB(min, max);
		FSWorldComponent.PhysicsWorld.QueryAABB((Fixture fixture) =>
		{
			if (fixture.Body.UserTag == "Player")
			{
				playerHit = true;
				return false;
			}
			return true;
		}, ref aabb);
		return playerHit;
	}

	private int GetTypeIndexToSpawn()
	{
		float num = 0f;
		TypeSpawningData[] activeSpawnData = _activeSpawnData;
		foreach (TypeSpawningData typeSpawningData in activeSpawnData)
		{
			num += typeSpawningData.typeWeight;
		}
		float num2 = UnityEngine.Random.Range(0f, num);
		float num3 = 0f;
		for (int j = 0; j < _activeSpawnData.Length; j++)
		{
			TypeSpawningData typeSpawningData2 = _activeSpawnData[j];
			num3 += typeSpawningData2.typeWeight;
			if (num3 > num2)
			{
				return (int)_activeSpawnData[j].crateType;
			}
		}
		EB.Debug.LogWarning("Something strange happened here, none of the types were picked? Random = " + num2);
		return -1;
	}

	private int GetItemIndexToSpawn(int typeIndex)
	{
		int num = -1;
		for (int i = 0; i < _activeSpawnData.Length; i++)
		{
			if (_activeSpawnData[i].crateType == (Crate.Type)typeIndex)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			TypeSpawningData typeSpawningData = _activeSpawnData[num];
			ItemSpawningData[] itemData = typeSpawningData.itemData;
			float num2 = 0f;
			ItemSpawningData[] array = itemData;
			foreach (ItemSpawningData itemSpawningData in array)
			{
				num2 += itemSpawningData.itemWeight;
			}
			float num3 = UnityEngine.Random.Range(0f, num2);
			float num4 = 0f;
			for (int k = 0; k < itemData.Length; k++)
			{
				ItemSpawningData itemSpawningData2 = itemData[k];
				num4 += itemSpawningData2.itemWeight;
				if (num4 > num3)
				{
					return k;
				}
			}
			EB.Debug.LogWarning("Something strange happened here, none of the items in type " + typeIndex + " were picked? Random = " + num3);
			return -1;
		}
		EB.Debug.LogError("Attempting to use an out of bounds typeIndex " + typeIndex + ", there are " + _activeSpawnData.Length + " types in the data!");
		return -1;
	}

	private GameObject GetCrateToSpawn(int type, int index)
	{
		int num = -1;
		for (int i = 0; i < _activeSpawnData.Length; i++)
		{
			if (_activeSpawnData[i].crateType == (Crate.Type)type)
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			TypeSpawningData typeSpawningData = _activeSpawnData[num];
			if (index < typeSpawningData.itemData.Length && index >= 0)
			{
				return typeSpawningData.itemData[index].itemPrefab;
			}
		}
		return null;
	}

	public void DestroyCrate(GameObject obj)
	{
		ReplicationView component = obj.GetComponent<ReplicationView>();
		if (component != null)
		{
			_occupiedSpawnPoints.Remove(component.viewId);
			if (component.isMine)
			{
				EB.Replication.Manager.Destroy(obj);
			}
		}
		else
		{
			UnityEngine.Object.Destroy(obj);
		}
	}

	public void KeyDestroyed()
	{
		_keySpawned = false;
	}

	public void ChestDestroyed()
	{
		_chestSpawned = false;
	}

	public void VaultDestroyed()
	{
		_respawnVault = true;
	}

	private void OnSerializeView(BitStream stream)
	{
		stream.Serialize(ref _keySpawned);
		stream.Serialize(ref _chestSpawned);
		stream.Serialize(ref _respawnVault);
		if (stream.isReading)
		{
			_occupiedSpawnPoints.Clear();
			while (stream.DataAvailable)
			{
				ViewId data = default(ViewId);
				int data2 = 0;
				stream.Serialize(ref data);
				stream.Serialize(ref data2);
				_occupiedSpawnPoints[data] = data2;
			}
			return;
		}
		foreach (KeyValuePair<ViewId, int> occupiedSpawnPoint in _occupiedSpawnPoints)
		{
			ViewId data3 = occupiedSpawnPoint.Key;
			int data4 = occupiedSpawnPoint.Value;
			stream.Serialize(ref data3);
			stream.Serialize(ref data4);
		}
	}
}
