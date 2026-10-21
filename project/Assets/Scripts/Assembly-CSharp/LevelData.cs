using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Common.ConvexHull;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Terrain;
using UnityEngine;

public class LevelData : MonoBehaviour
{
	[System.Serializable]
	public class CollisionRegionInfo
	{
		public int[] _polyOffsets;

		public float[] _polyElements;

		public CollisionRegionInfo(int[] offsets, float[] elements)
		{
			_polyOffsets = offsets;
			_polyElements = elements;
		}

		public List<Vertices> ToVertexLists()
		{
			List<Vertices> list = new List<Vertices>(_polyOffsets.Length);
			for (int i = 0; i < _polyOffsets.Length; i++)
			{
				int num = _polyOffsets[i];
				Vertices vertices = new Vertices();
				int num2 = ((i >= _polyOffsets.Length - 1) ? (_polyElements.Length - num) : (_polyOffsets[i + 1] - num));
				for (int j = num; j < num + num2; j += 2)
				{
					vertices.Add(new FVector2(_polyElements[j], _polyElements[j + 1]));
				}
				list.Add(vertices);
			}
			return list;
		}
	}

	[System.Serializable]
	public class SpawnPoint
	{
		public bool _primary;

		public Vector2 _pos;

		public string _name;
	}

	private struct Hole
	{
		public FVector2 pt;

		public float radius;
	}

	private struct TimeInfo
	{
		public string _tex;

		public double _time;

		public override string ToString()
		{
			return string.Format("Event : {0} took {1}", _tex, _time);
		}
	}

	public delegate void GameObjectQueryCallback(GameObject gameObject);

	public delegate void PlayerQueryCallback(Player player);

	public const float Px2World = 0.1f;

	public const float World2Px = 10f;

	public const float DefaultGameplayWidth = 102.4f;

	public const int _maxSpawnPoints = 8;

	public const float _backgroundDistance = 100f;

	private const float cGameplayWidth = 102.4f;

	private const float maxViewHeight = 76.8f;

	private const int _maxTimeInfos = 20;

	public const int FatRayIntersectionMask_Player = 1;

	public const int FatRayIntersectionMask_Mine = 2;

	[HideInInspector]
	public int collisionRegionsInX;

	[HideInInspector]
	public int collisionRegionsInY;

	[HideInInspector]
	public CollisionRegionInfo[] _collisionRegions;

	[HideInInspector]
	public int _cellsInX;

	[HideInInspector]
	public int _cellsInY;

	[HideInInspector]
	public float _cellDim;

	[HideInInspector]
	public int[] _navCells;

	private float _gameplayWidth = 102.4f;

	public float _worldScale = 1.1f;

	private float _backgroundAspect = 2f;

	private float _backgroundScale = 1.1f;

	private Texture2D visualTexture;

	public Texture2D background;

	[HideInInspector]
	public string collisionTextureGUID;

	[HideInInspector]
	public string visualTextureGUID;

	private Material _subtraction;

	private RenderTexture _rt;

	public SpawnPoint[] spawnPoints = new SpawnPoint[8]
	{
		new SpawnPoint
		{
			_primary = true,
			_pos = new Vector2(0.02f, 1f)
		},
		new SpawnPoint
		{
			_primary = true,
			_pos = new Vector2(0.98f, 1f)
		},
		new SpawnPoint
		{
			_primary = true,
			_pos = new Vector2(0.02f, 1f)
		},
		new SpawnPoint
		{
			_primary = true,
			_pos = new Vector2(0.98f, 1f)
		},
		new SpawnPoint
		{
			_pos = new Vector2(0.02f, 1f)
		},
		new SpawnPoint
		{
			_pos = new Vector2(0.98f, 1f)
		},
		new SpawnPoint
		{
			_pos = new Vector2(0.02f, 1f)
		},
		new SpawnPoint
		{
			_pos = new Vector2(0.98f, 1f)
		}
	};

	[HideInInspector]
	public int _activeSpawnPoints = 4;

	private GameObject _backgroundObject;

	private static LevelData _this;

	private Body _body;

	private GameObject _player;

	private Dictionary<uint, Player> _playerMap = new Dictionary<uint, Player>();

	private float _viewWidth;

	private float _viewHeight;

	private FSWorldComponent _world;

	public bool enableRespawn;

	public bool enableLocalPlayerRespawn;

	private bool isChampionship;

	private float _bgHeight;

	private float _bgWidth;

	private bool _started;

	private int NumOfPlayers;

	private Queue<Hole> _holes = new Queue<Hole>();

	private TerrainBody[] _terrainBodies;

	private float _boolInterval = 0.2f;

	private List<Hole> _holeBuffer = new List<Hole>();

	private List<Vertices> _revisedVerts;

	private int _myPlayerIndex;

	private List<SpawnPoint> _sp_SpawnPoints;

	private Navigation.NavWorld _navWorld;

	private List<Vertices> _intermediateHulls;

	private List<AABB> _BOOL_CANDIDATES = new List<AABB>();

	private List<TimeInfo> _timeInfos = new List<TimeInfo>();

	private static Player _currentPlayer;

	private List<GameObject> _hitReceivers = new List<GameObject>();

	private bool _paused;

	public float CellDim
	{
		get
		{
			return _cellDim;
		}
	}

	public static LevelData Instance
	{
		get
		{
			return _this;
		}
	}

	public float ScreenScale
	{
		get
		{
			return Camera.mainCamera.orthographicSize * 2f / (float)Screen.height;
		}
	}

	public float GameplayWidth
	{
		get
		{
			return _gameplayWidth;
		}
		set
		{
			_gameplayWidth = value;
		}
	}

	public float GameplayHeight
	{
		get
		{
			return GameplayWidth / GameplayAspect;
		}
	}

	public float WorldWidth
	{
		get
		{
			return GameplayWidth * _worldScale;
		}
	}

	public float WorldHeight
	{
		get
		{
			return WorldWidth / GameplayAspect;
		}
	}

	public Vector2 GameplayMin
	{
		get
		{
			return new Vector2(0f, 0f);
		}
	}

	public Vector2 GameplayMax
	{
		get
		{
			return new Vector2(GameplayWidth, GameplayHeight);
		}
	}

	public float ViewWidth
	{
		get
		{
			return _viewWidth;
		}
	}

	public float ViewHeight
	{
		get
		{
			return _viewHeight;
		}
	}

	private float GameplayAspect
	{
		get
		{
			return 2f;
		}
	}

	public bool IsReady { get; private set; }

	private GameObject ActiveMatchSequenceObject { get; set; }

	public static bool DrawingTerrain { get; set; }

	public Navigation.NavWorld NavWorld
	{
		get
		{
			return _navWorld;
		}
	}

	public static Player CurrentPlayer
	{
		get
		{
			if (_currentPlayer == null)
			{
				if (Instance != null)
				{
					return Instance.GetPlayer();
				}
				return null;
			}
			return _currentPlayer;
		}
		set
		{
			_currentPlayer = value;
		}
	}

	public bool PauseGame
	{
		get
		{
			return _paused;
		}
		set
		{
			_paused = value;
			UnityEngine.Time.timeScale = ((!_paused) ? 1 : 0);
			AudioListener.pause = _paused;
		}
	}

	public Vector2 GameplayNormalizedToGameplay(Vector2 v)
	{
		return new Vector2(v.x * GameplayWidth + GameplayMin.x, v.y * GameplayHeight + GameplayMin.y);
	}

	public Vector2 GameplayToGameplayNormalized(Vector2 v)
	{
		return new Vector2((v.x - GameplayMin.x) / GameplayWidth, (v.y - GameplayMin.y) / GameplayHeight);
	}

	private void Awake()
	{
		_this = this;
		enableRespawn = true;
		Arsenal.Load();
		new GameObject("scoring", typeof(Scoring));
		if (base.renderer != null)
		{
			visualTexture = base.renderer.material.mainTexture as Texture2D;
		}
		_gameplayWidth = 102.4f;
		_viewWidth = (float)Screen.width * 0.1f;
		_viewHeight = (float)Screen.height * 0.1f;
		Camera.mainCamera.GetComponent<CameraController>().OrthoMax = Mathf.Min(ViewHeight * 0.5f, 38.4f);
		_world = base.gameObject.AddComponent<FSWorldComponent>();
		_world.Paused = true;
		FSWorldComponent.PhysicsWorld.Gravity = new FVector2(0f, Tuning.Instance.World_Gravity);
		base.gameObject.AddComponent<NetworkView>().observed = null;
		_backgroundObject = CreateBackground();
		_backgroundObject.transform.parent = Camera.mainCamera.transform;
		_backgroundObject.transform.localPosition = Vector3.zero + Vector3.forward * 100f;
		BuildCollisionFromSerialized();
		BuildNavWorldFromSerialized();
		base.transform.position = Vector3.zero;
		base.transform.rotation = Quaternion.identity;
		EB.Replication.Manager.RegisterRPCs(this);
		EB.Replication.Manager.RegisterResourceFolder("Prefab_Sequences/GameModes");
		EB.Replication.Manager.RegisterResource("Player");
		EB.Replication.Manager.RegisterResource("PlayerAI");
		EB.Replication.Manager.RegisterResource("CrateManager");
		DrawingTerrain = true;
		isChampionship = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn;
	}

	private void Start()
	{
		if (visualTexture != null)
		{
			Shader shader = Assets.Load<Shader>("Shaders/Subtraction");
			if (shader.isSupported)
			{
				_subtraction = new Material(shader);
				_subtraction.SetVector("_Params", new Vector4(visualTexture.width, visualTexture.height));
				_rt = new RenderTexture(visualTexture.width, visualTexture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
				if (!_rt.Create())
				{
					EB.Debug.LogError("Failed to created render texture, falling back to cpu");
					_subtraction = null;
				}
				else
				{
					_rt.wrapMode = TextureWrapMode.Clamp;
					Graphics.Blit(visualTexture, _rt);
				}
			}
			if (_subtraction == null)
			{
				visualTexture = (Texture2D)UnityEngine.Object.Instantiate(visualTexture);
			}
		}
		CreateLevel();
	}

	private void OnDestroy()
	{
		if (visualTexture != null && visualTexture.name.Contains("Clone"))
		{
			UnityEngine.Object.Destroy(visualTexture);
		}
		UnityEngine.Object.Destroy(_rt);
		UnityEngine.Object.Destroy(_subtraction);
		Arsenal.Unload();
	}

	public void CreateLevel()
	{
		if (base.renderer != null)
		{
			Material material = new Material(Shader.Find("Custom/TransparentFast"));
			if ((bool)_rt)
			{
				material.mainTexture = _rt;
			}
			else if ((bool)visualTexture)
			{
				material.mainTexture = visualTexture;
			}
			GetComponent<MeshRenderer>().sharedMaterial = material;
		}
	}

	public GameObject CreateBackground()
	{
		_bgHeight = 1f;
		_bgWidth = _bgHeight * _backgroundAspect;
		Material material = new Material(Shader.Find("Custom/OpaqueFast"));
		material.renderQueue = 0;
		material.mainTexture = background;
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[4]
		{
			new Vector3((0f - _bgWidth) * 0.5f, (0f - _bgHeight) * 0.5f, 0f),
			new Vector3(_bgWidth * 0.5f, (0f - _bgHeight) * 0.5f, 0f),
			new Vector3(_bgWidth * 0.5f, _bgHeight * 0.5f, 0f),
			new Vector3((0f - _bgWidth) * 0.5f, _bgHeight * 0.5f, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		mesh.triangles = new int[6] { 0, 3, 2, 2, 1, 0 };
		GameObject gameObject = new GameObject("background_" + DateTime.Now.Ticks, typeof(MeshRenderer), typeof(MeshFilter));
		gameObject.GetComponent<MeshFilter>().mesh = mesh;
		gameObject.renderer.sharedMaterial = material;
		return gameObject;
	}

	public void RegisterPlayer(Player p)
	{
		ReplicationView component = p.GetComponent<ReplicationView>();
		if (component != null)
		{
			_playerMap[component.instantiatorId] = p;
		}
	}

	public void UnregisterPlayer(Player p)
	{
		ReplicationView component = p.GetComponent<ReplicationView>();
		if (component != null)
		{
			_playerMap.Remove(component.instantiatorId);
		}
	}

	public IEnumerable GetPlayers()
	{
		foreach (Player value in _playerMap.Values)
		{
			yield return value;
		}
	}

	public Player GetPlayer(uint playerId)
	{
		Player value;
		_playerMap.TryGetValue(playerId, out value);
		return value;
	}

	public bool PlayerIsAI(uint playerId)
	{
		Player value;
		if (_playerMap.TryGetValue(playerId, out value))
		{
			return value.IsAI;
		}
		return false;
	}

	public Player GetAiPlayer(string tutorialId)
	{
		foreach (Player value in _playerMap.Values)
		{
			if (value.tutorialId == tutorialId)
			{
				return value;
			}
		}
		return null;
	}

	private void TryToStartGame()
	{
		if (_started)
		{
			return;
		}
		Game game = Hub.Instance.GameManager.Game;
		if (game != null)
		{
			if (NumOfPlayers != game.Players.Count && NumOfPlayers != 0)
			{
				game.LeaveGame(string.Empty);
			}
			NumOfPlayers = game.Players.Count;
			bool flag = GameAttributes.IsTutorialGameMode();
			if (game.Players.Count == _playerMap.Count || (flag && _playerMap.Count == 1))
			{
				_started = true;
				StartGame();
			}
		}
	}

	public void BlastHole(FVector2 pt, float radius)
	{
		if (DebugSettings.LevelDestruction)
		{
			Hole item = new Hole
			{
				pt = pt,
				radius = radius
			};
			_holes.Enqueue(item);
			_holeBuffer.Add(item);
		}
	}

	private void RenderNextHole()
	{
		Hole hole = _holes.Dequeue();
		Vector2 normalizedPoint = new Vector2(hole.pt.X / GameplayWidth, hole.pt.Y / GameplayHeight);
		if (visualTexture != null)
		{
			if ((bool)_subtraction)
			{
				RenderHoleGPU(visualTexture, hole.radius, normalizedPoint);
			}
			else
			{
				RenderHole(visualTexture, hole.radius, normalizedPoint);
			}
		}
		_navWorld.Clear(hole.pt, hole.radius);
	}

	private void RenderHoleGPU(Texture2D texture, float radius, Vector2 normalizedPoint)
	{
		DateTime now = DateTime.Now;
		float num = radius / GameplayHeight;
		Vector4 vector = new Vector4(normalizedPoint.x * (float)texture.width / (float)texture.height, normalizedPoint.y, num, texture.width / texture.height);
		if (texture.texelSize.y < 0f)
		{
			vector.y = (float)texture.width - vector.y;
		}
		float num2 = num * (float)texture.height / (float)texture.width;
		float num3 = num;
		float left = Mathf.Clamp01(normalizedPoint.x - num2);
		float right = Mathf.Clamp01(normalizedPoint.x + num2);
		float top = Mathf.Clamp01(normalizedPoint.y - num3);
		float bottom = Mathf.Clamp01(normalizedPoint.y + num3);
		Rect rect = Rect.MinMaxRect(left, top, right, bottom);
		RenderTexture.active = _rt;
		_subtraction.mainTexture = _rt;
		_subtraction.SetVector("_Params", vector);
		Graphics.Blit(_rt, _rt, _subtraction);
	}

	public static void DrawQuad(bool invertY, Rect clip)
	{
		GL.Begin(7);
		float y;
		float y2;
		if (invertY)
		{
			y = clip.yMax;
			y2 = clip.yMin;
		}
		else
		{
			y2 = clip.yMax;
			y = clip.yMin;
		}
		float xMin = clip.xMin;
		float xMax = clip.xMax;
		GL.TexCoord2(xMin, y);
		GL.Vertex3(xMin, clip.yMin, 0.1f);
		GL.TexCoord2(xMax, y);
		GL.Vertex3(xMax, clip.yMin, 0.1f);
		GL.TexCoord2(xMax, y2);
		GL.Vertex3(xMax, clip.yMax, 0.1f);
		GL.TexCoord2(xMin, y2);
		GL.Vertex3(xMin, clip.yMax, 0.1f);
		GL.End();
	}

	private void RenderHole(Texture2D texture, float radius, Vector2 normalizedPoint)
	{
		DateTime now = DateTime.Now;
		Vector2 vector = new Vector2(normalizedPoint.x * (float)texture.width, normalizedPoint.y * (float)texture.height);
		float num = radius / GameplayHeight * (float)texture.height;
		float num2 = num * num;
		int num3 = (int)Mathf.Max(0f, vector.x - num);
		int num4 = (int)Mathf.Min(texture.width - 1, vector.x + num);
		int num5 = (int)Mathf.Max(0f, vector.y - num);
		int num6 = (int)Mathf.Min(texture.height - 1, vector.y + num);
		for (int i = num5; i <= num6; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				float sqrMagnitude = (new Vector2(j, i) - vector).sqrMagnitude;
				if (sqrMagnitude <= num2)
				{
					Color pixel = texture.GetPixel(j, i);
					pixel.a = 0f;
					texture.SetPixel(j, i, pixel);
				}
			}
		}
		texture.Apply();
	}

	private List<Vertices> CalculateHulls(List<Vertices> polys)
	{
		List<Vertices> list = new List<Vertices>(polys.Count);
		foreach (Vertices poly in polys)
		{
			list.Add(GiftWrap.GetConvexHull(poly));
		}
		return list;
	}

	private List<AABB> CalculateBoxes(List<Vertices> polys)
	{
		List<AABB> list = new List<AABB>(polys.Count);
		foreach (Vertices poly in polys)
		{
			list.Add(Misc.ComputeAABB(poly));
		}
		return list;
	}

	private IEnumerator Booler()
	{
		while (true)
		{
			if (_holeBuffer.Count > 0)
			{
				_world.Paused = true;
				foreach (Hole h in _holeBuffer)
				{
					Rebool(h.pt, h.radius);
				}
				TerrainBody[] terrainBodies = _terrainBodies;
				foreach (TerrainBody tb in terrainBodies)
				{
					if (tb != null)
					{
						tb.CreateBody();
					}
				}
				_holeBuffer.Clear();
				_world.Paused = false;
			}
			yield return new WaitForSeconds(_boolInterval);
		}
	}

	private void Rebool(FVector2 pt, float radius)
	{
		default(Misc.IntermediateTimer).Start();
		int numberOfEdges = Mathf.RoundToInt(Mathf.Clamp(radius, 8f, 20f));
		Vertices vertices = PolygonTools.CreateCircle(radius, numberOfEdges);
		vertices.Translate(pt);
		AABB aabb = Misc.ComputeAABB(vertices);
		TerrainBody[] terrainBodies = _terrainBodies;
		foreach (TerrainBody terrainBody in terrainBodies)
		{
			if (terrainBody != null)
			{
				terrainBody.Bool(vertices, aabb);
			}
		}
	}

	private void OnNetworkLoadedLevel()
	{
		ScreenManager.Instance.EnableTransitionBlocker(false);
		EBUI_TransitionManager.Instance.Clear();
		_myPlayerIndex = EB.Replication.Manager.LocalPlayerIndex;
		_world.Paused = false;
		DetermineSpawnPoints();
		SpawnPlayer();
		if (!GameAttributes.IsTutorialGameMode() && EB.Replication.Manager.IsHost)
		{
			SpawnAI();
		}
		LootManager.Instance.Init();
		StartCoroutine(Booler());
	}

	public void TerminateGame()
	{
		if (ActiveMatchSequenceObject != null)
		{
			Sequence component = ActiveMatchSequenceObject.GetComponent<Sequence>();
			component.KillSequence();
			UnityEngine.Object.Destroy(ActiveMatchSequenceObject);
		}
		GameHUD.Instance.Visible = false;
		StopWorld();
	}

	public void StartGame()
	{
		Profile.Instance.ClearSessionData();
		LootManager.Instance.EnableLoot = true;
		if (EB.Replication.Manager.IsHost)
		{
			GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode();
			string text = "Prefab_Sequences/GameModes/sequence_";
			text = ((gameMode != GameAttributes.GameModeType.DeathMatch || !GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kTutorialOverride)) ? (text + GameAttributes.GetValue(GameAttributes.AttributeType.GameMode).ToLower()) : (text + "tutorialdeathmatch"));
			UnityEngine.Object prefab = Assets.Load(text);
			ActiveMatchSequenceObject = EB.Replication.Manager.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
			UnityEngine.Object prefab2 = Assets.Load("CrateManager");
			EB.Replication.Manager.Instantiate(prefab2, Vector3.zero, Quaternion.identity);
		}
		string value = GameAttributes.GetValue(GameAttributes.AttributeType.Mode);
		if (GameAttributes.GetMode(value) == GameAttributes.ModeType.SP)
		{
			StartCoroutine(StartSP());
		}
		if (CrateManager.Instance != null)
		{
			Game game = Hub.Instance.GameManager.Game;
			if (game.IsPrivate)
			{
				CrateManager.Instance.UsePrivateSpawnData();
			}
			else if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn)
			{
				CrateManager.Instance.UseChampionshipSpawnData();
			}
			else if (game.Network.IsLocal)
			{
				CrateManager.Instance.UseCampaignSpawnData();
			}
			else
			{
				CrateManager.Instance.UseDefaultSpawnData();
			}
		}
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.HideLoadingScreen();
			if (_this != null)
			{
				EB.Sequence.Component.Activate(base.gameObject, "GameStart", typeof(SequenceEvent_GameEvent));
			}
			ScreenManager.Instance.FadeOut(null);
			IsReady = true;
		});
		WeaponMonitor.VisEnable(GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kDetonationVis));
	}

	private IEnumerator StartSP()
	{
		bool isLoggedIn = Hub.Instance.State == HubState.Connected;
		LootManager.Instance.EnableLoot = isLoggedIn;
		yield return new WaitForFixedUpdate();
		Hashtable gameAttribs = Hub.Instance.GameManager.Game.Attributes;
		string prefabName = gameAttribs["sp_prefab"] as string;
		if (!string.IsNullOrEmpty(prefabName))
		{
			EB.Sequence.Component.Activate(base.gameObject, prefabName, typeof(SequenceEvent_CampaignStartedEvent));
		}
		else
		{
			EB.Debug.LogError("missing sp_prefab name!");
		}
	}

	public GameObject GetPlayerObject()
	{
		return _player;
	}

	public Player GetPlayer()
	{
		return (!(_player != null)) ? null : _player.GetComponent<Player>();
	}

	public int GetPlayerCount()
	{
		return _playerMap.Count;
	}

	private void DetermineSpawnPoints()
	{
		_sp_SpawnPoints = new List<SpawnPoint>();
		List<SpawnPoint> list = new List<SpawnPoint>();
		SpawnPoint[] array = spawnPoints;
		foreach (SpawnPoint item in array)
		{
			list.Add(item);
		}
		List<EB.Sparx.Player> players = EB.Replication.Manager.Players;
		foreach (EB.Sparx.Player item2 in players)
		{
			int num = UnityEngine.Random.Range(0, list.Count);
			if (num < list.Count)
			{
				_sp_SpawnPoints.Add(list[num]);
				list.RemoveAt(num);
			}
		}
	}

	public void SpawnPlayer()
	{
		SpawnPoint spawnPoint = spawnPoints[_myPlayerIndex % spawnPoints.Length];
		string value = GameAttributes.GetValue(GameAttributes.AttributeType.Mode);
		if (GameAttributes.GetMode(value) == GameAttributes.ModeType.SP)
		{
			if (CampaignData.Instance.IsCampaignPlus() && _sp_SpawnPoints.Count > 0 && !GameAttributes.IsTutorialGameMode() && !isChampionship)
			{
				spawnPoint = _sp_SpawnPoints[0];
				_sp_SpawnPoints.RemoveAt(0);
			}
			else
			{
				Hashtable attributes = Hub.Instance.GameManager.Game.Attributes;
				string text = attributes["sp_spawnpoint"] as string;
				SpawnPoint[] array = spawnPoints;
				foreach (SpawnPoint spawnPoint2 in array)
				{
					if (!string.IsNullOrEmpty(text) && spawnPoint2._name == text)
					{
						spawnPoint = spawnPoint2;
						break;
					}
				}
			}
		}
		if (_player == null)
		{
			_player = (GameObject)EB.Replication.Manager.Instantiate(Resources.Load("Player"), GameplayNormalizedToGameplay(spawnPoint._pos), Quaternion.identity);
			GameObject player = _player;
			player.name = player.name + "_" + _myPlayerIndex;
		}
		UnityEngine.Object[] array2 = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		for (int j = 0; j < array2.Length; j++)
		{
			GameObject gameObject = (GameObject)array2[j];
			gameObject.SendMessage("OnPlayerReady", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SpawnAI()
	{
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		if (mode == GameAttributes.ModeType.SP && GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOff)
		{
			AISimple.SetLoadout(CampaignData.Instance.CurrentMissionPrefab().Inventory);
		}
		List<EB.Sparx.Player> players = EB.Replication.Manager.Players;
		foreach (EB.Sparx.Player item in players)
		{
			if (Dot.Bool("ai", item.Attributes, false))
			{
				string spawnPointName = Dot.String("ai_sp", item.Attributes, string.Empty);
				SpawnAI(item, spawnPointName);
			}
		}
	}

	public void AIFireAtPlayer(int aiIndex)
	{
		int num = 0;
		foreach (Player value in _playerMap.Values)
		{
			if (value.IsAI && num == aiIndex)
			{
				AISimple component = value.GetComponent<AISimple>();
				if (component != null)
				{
					component.StatelessFireAtPlayer();
				}
			}
		}
	}

	public void SpawnAI(int playerIndex, string spawnPointName, string tutorialId, Vector3 tutorialArrowOffset, float healthPercent)
	{
		int num = 0;
		List<EB.Sparx.Player> players = EB.Replication.Manager.Players;
		foreach (EB.Sparx.Player item in players)
		{
			if (!Dot.Bool("ai", item.Attributes, false))
			{
				continue;
			}
			if (num == playerIndex)
			{
				SpawnPoint spawnPoint = spawnPoints[item.Index % spawnPoints.Length];
				if (!string.IsNullOrEmpty(spawnPointName))
				{
					SpawnPoint[] array = spawnPoints;
					foreach (SpawnPoint spawnPoint2 in array)
					{
						if (spawnPoint2._name == spawnPointName)
						{
							spawnPoint = spawnPoint2;
							break;
						}
					}
				}
				GameObject gameObject = EB.Replication.Manager.InstantiateAs(item.PlayerId, Resources.Load("PlayerAI"), GameplayNormalizedToGameplay(spawnPoint._pos), Quaternion.identity) as GameObject;
				if (gameObject != null)
				{
					Player component = gameObject.GetComponent<Player>();
					if ((bool)component)
					{
						component.tutorialId = tutorialId;
						component.SendMessage("DoRespawnFX");
					}
					UITutorialArrow uITutorialArrow = gameObject.AddComponent<UITutorialArrow>();
					uITutorialArrow.tutorialId = tutorialId;
					uITutorialArrow.offset = tutorialArrowOffset;
					uITutorialArrow.arrowType = BigArrow.ArrowType.World;
					Health component2 = gameObject.GetComponent<Health>();
					if (component2 != null)
					{
						component2.SetByPostInit(healthPercent);
					}
				}
				break;
			}
			num++;
		}
	}

	public void SpawnAI(EB.Sparx.Player player, string spawnPointName)
	{
		SpawnPoint spawnPoint = spawnPoints[player.Index % spawnPoints.Length];
		if (CampaignData.Instance.IsCampaignPlus() && _sp_SpawnPoints.Count > 0 && !GameAttributes.IsTutorialGameMode() && !isChampionship)
		{
			spawnPoint = _sp_SpawnPoints[0];
			_sp_SpawnPoints.RemoveAt(0);
		}
		else if (!string.IsNullOrEmpty(spawnPointName))
		{
			SpawnPoint[] array = spawnPoints;
			foreach (SpawnPoint spawnPoint2 in array)
			{
				if (spawnPoint2._name == spawnPointName)
				{
					spawnPoint = spawnPoint2;
					break;
				}
			}
		}
		GameObject gameObject = EB.Replication.Manager.InstantiateAs(player.PlayerId, Resources.Load("PlayerAI"), GameplayNormalizedToGameplay(spawnPoint._pos), Quaternion.identity) as GameObject;
		if (GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) == GameAttributes.ModeType.MP)
		{
			AISimple component = gameObject.GetComponent<AISimple>();
			if (component != null)
			{
				component.IsBot = true;
			}
		}
	}

	[RPC]
	public void SetPlayerRespawn(bool enable, bool enableLocal)
	{
		enableRespawn = enable;
		enableLocalPlayerRespawn = enableLocal;
	}

	[RPC]
	public void OnPlayerDied(uint playerId, Vector3 spawnPos)
	{
		if (enableRespawn || (enableLocalPlayerRespawn && playerId == Hub.Instance.GameManager.Game.LocalPlayer.PlayerId))
		{
			Player player = GetPlayer(playerId);
			player.SendMessage("OnRespawn", GameplayNormalizedToGameplay(spawnPos));
			if (playerId == EB.Replication.Manager.LocalPlayerId)
			{
				UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
				for (int i = 0; i < array.Length; i++)
				{
					GameObject gameObject = (GameObject)array[i];
					gameObject.SendMessage("OnPlayerReady", SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		EB.Sequence.Component.Activate(base.gameObject, playerId, typeof(SequenceEvent_PlayerDied));
	}

	[RPC]
	private void StartGap(uint playerId, float length)
	{
		Player player = Instance.GetPlayer(playerId);
		CameraController component = Camera.mainCamera.GetComponent<CameraController>();
		if (player != null)
		{
			component.ZoomPlayer(player.gameObject);
		}
		else
		{
			EB.Debug.LogError("Cant get player " + playerId);
		}
		StartCoroutine(_GapMessage(playerId, length));
	}

	private IEnumerator _GapMessage(uint playerId, float length)
	{
		GameHUD hud = GameHUD.Instance;
		if (!(hud != null))
		{
			yield break;
		}
		while (length > 0f)
		{
			yield return 1;
			length -= UnityEngine.Time.deltaTime;
		}
		if (!(hud != null))
		{
			yield break;
		}
		if (playerId == EB.Replication.Manager.LocalPlayerId)
		{
			hud.ShowMessage(Localizer.GetString("ID_GAME_MESSAGE_YOUR_TURN"), Tuning.Instance.Timer_ShowYourTurn);
		}
		else if (Tuning.Instance.Timer_ShowOthersTurn)
		{
			Player player = GetPlayer(playerId);
			if (player != null)
			{
				string message = Localizer.Format("ID_GAME_MESSAGE_OTHER_TURN", player.PlayerName);
				hud.ShowMessage(message, Tuning.Instance.Timer_ShowYourTurn);
			}
		}
		else
		{
			hud.ClearMessage();
		}
	}

	[RPC]
	private void EndGap(uint playerId)
	{
		CameraController component = Camera.mainCamera.GetComponent<CameraController>();
		component.UnzoomPlayer();
	}

	[RPC]
	private void NextTurn(uint playerId, float length)
	{
		GameHUD instance = GameHUD.Instance;
		Player player = GetPlayer();
		Player player2 = (CurrentPlayer = GetPlayer(playerId));
		if (player2 != null)
		{
			if (playerId == EB.Replication.Manager.LocalPlayerId)
			{
				player.SendMessage("EnableInput", true);
				EB.Replication.Manager.TransferSceneOwnership();
				if (instance != null)
				{
					instance.UpdateCurrentPlayerName(player.PlayerName, playerId);
				}
				player.SpawnProjectile();
				player.ResetIdle();
				Camera.mainCamera.SendMessage("PlayerMoved", player.gameObject);
			}
			else
			{
				player.SendMessage("EnableInput", false);
				if (instance != null)
				{
					instance.UpdateCurrentPlayerName(player2.PlayerName, playerId);
				}
				player2.SpawnProjectile();
				player2.ResetIdle();
			}
			if (player2.IsAI)
			{
				player2.SendMessage("EnableInput", true);
			}
		}
		if (EB.Replication.Manager.IsHost)
		{
			CrateManager.Instance.NextTurn(playerId);
		}
		if (instance != null)
		{
			instance.ShowTurnTimer(length);
		}
	}

	[RPC]
	private void SetTurnTimer(float length)
	{
		GameHUD.Instance.ShowTurnTimer(length);
	}

	[RPC]
	private void EndTurn(uint currentPlayerID, uint nextPlayerID)
	{
		Player player = GetPlayer();
		Player player2 = GetPlayer(currentPlayerID);
		if (player2 != null)
		{
			player2.SendMessage("EnableInput", false, SendMessageOptions.DontRequireReceiver);
		}
		player.SendMessage("EnableInput", false, SendMessageOptions.DontRequireReceiver);
		GameHUD instance = GameHUD.Instance;
		if (instance != null)
		{
			instance.ClearMessage();
			instance.ClearTimer();
		}
		foreach (Player value in _playerMap.Values)
		{
			value.EndTurn(currentPlayerID, nextPlayerID);
		}
		ScreenManager.Instance.BroadcastMessage("EndTurn", currentPlayerID, SendMessageOptions.DontRequireReceiver);
	}

	[RPC]
	private void TurnHudVisible()
	{
		GameHUD.Instance.Visible = true;
	}

	[RPC]
	private void EndGame(float percentTimeElapsed)
	{
		PauseGame = false;
		StopWorld();
		Setup.Instance.gameObject.GetComponent<HUD>().OnLeaveGame();
		foreach (Player value in _playerMap.Values)
		{
			value.SendMessage("EndGame");
			value.ReleasePlayer();
		}
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		Scoring.Instance.SortEndGamePlayers();
		bool flag = false;
		List<EB.Sparx.Player> endGamePlayers = Scoring.Instance.GetEndGamePlayers();
		GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
		if (gameMode == GameAttributes.GameModeType.SuddenDeath)
		{
			EB.Sparx.Player player = endGamePlayers[0];
			if (player != null && player.PlayerId == game.LocalPlayer.PlayerId)
			{
				flag = true;
			}
		}
		else if (Scoring.Instance.GetRankByPlayer(Hub.Instance.GameManager.Game.LocalPlayer.PlayerId) == 1)
		{
			flag = true;
		}
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		bool flag2 = false;
		if (flag)
		{
			if (mode == GameAttributes.ModeType.SP)
			{
				if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn)
				{
					Division.Stage selectedStage = ChampionshipData.Instance.GetSelectedStage();
					int selectedDivision = ChampionshipData.Instance.GetSelectedDivision();
					if (selectedStage != null)
					{
						int num = ChampionshipData.StarState(selectedDivision, selectedStage);
						flag2 = num == -1;
						if (flag2)
						{
							Profile.Instance.ConsecutiveChampionshipLosses = 0;
						}
						ChampionshipData.Instance.SetPlayed(selectedDivision, selectedStage, true);
						int killsByPlayer = Scoring.Instance.GetKillsByPlayer(GetPlayer().PlayerId);
						int scoreByPlayer = Scoring.Instance.GetScoreByPlayer(GetPlayer().PlayerId);
						ChampionshipData.Instance.UpdateStarState(selectedDivision, selectedStage, scoreByPlayer, killsByPlayer);
						EB.Debug.Log(string.Format("Championship - received score, {0} kills, {1} points", killsByPlayer, scoreByPlayer));
						if (ChampionshipData.Instance.SetHighScore(selectedDivision, selectedStage, scoreByPlayer, killsByPlayer))
						{
							EB.Debug.Log("Championship - high score recorded!");
							Profile.Instance.SessionHighScoreAchieved = true;
						}
					}
					else
					{
						EB.Debug.LogError("Cannot update Championship stars or high scores because of null Championship stage!");
					}
				}
				else
				{
					CampaignData instance = CampaignData.Instance;
					if (instance.SetHighScore(instance.Missions[instance.CurrentMission], Scoring.Instance.GetScoreByPlayer(GetPlayer().PlayerId)))
					{
						Profile.Instance.SessionHighScoreAchieved = true;
					}
				}
			}
		}
		else if (mode == GameAttributes.ModeType.SP && GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn)
		{
			Division.Stage selectedStage2 = ChampionshipData.Instance.GetSelectedStage();
			int selectedDivision2 = ChampionshipData.Instance.GetSelectedDivision();
			if (selectedStage2 != null)
			{
				flag2 = ChampionshipData.Instance.IsFirstTime(selectedDivision2, selectedStage2);
				ChampionshipData.Instance.SetPlayed(selectedDivision2, selectedStage2, true);
			}
			if (Profile.Instance.IsTutorialDone("promo_popups"))
			{
				Profile.Instance.ConsecutiveChampionshipLosses++;
			}
		}
		RewardsManager.Instance.DidLevel = false;
		int level = Profile.Instance.Level;
		if (mode != GameAttributes.ModeType.SP)
		{
			Profile.Instance.FinalizeSessionData();
			if (GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kTutorialOverride))
			{
				SinglePlayerRewards currentMissionRewards = CampaignData.Instance.GetCurrentMissionRewards();
				if (currentMissionRewards != null)
				{
					RewardsManager.Instance.RewardXP = currentMissionRewards.PracticeRewardXp;
					RewardsManager.Instance.RewardRivets = currentMissionRewards.PracticeRewardRivets;
				}
			}
			else
			{
				RewardsManager.Instance.CalculateEndGameRewards(percentTimeElapsed);
			}
		}
		else
		{
			RewardsManager.Instance.RewardXP = 0;
			RewardsManager.Instance.RewardRivets = 0;
			RewardsManager.Instance.GrantChampionshipRewards(null, null);
			if (GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship)) == GameAttributes.ChampionshipType.kOn)
			{
				RewardsManager.Instance.ChampionshipFirstTimeAttempt = flag2;
				if (flag)
				{
					Division.Stage selectedStage3 = ChampionshipData.Instance.GetSelectedStage();
					if (selectedStage3 != null)
					{
						int rivets = 0;
						int xp = 0;
						selectedStage3.GetAwards(Scoring.Instance.GetScoreByPlayer(GetPlayer().PlayerId), Scoring.Instance.GetKillsByPlayer(GetPlayer().PlayerId), out rivets, out xp);
						EB.Debug.Log(string.Format("Championship - awarding {0} xp, {1} rivets", xp, rivets));
						RewardsManager.Instance.RewardRivets = rivets;
						RewardsManager.Instance.RewardXP = xp;
						ChampionshipPrefab.Reward[] array = null;
						ChampionshipPrefab.Reward uiOverride = null;
						if (flag2)
						{
							array = selectedStage3._firstTimeRewards;
							if (selectedStage3.HasFirstTimeRewardUiOverride())
							{
								uiOverride = selectedStage3._firstTimeRewardUiOverride;
							}
						}
						else
						{
							array = selectedStage3._regularRewards;
							if (selectedStage3.HasRegularRewardUiOverride())
							{
								uiOverride = selectedStage3._regularRewardUiOverride;
							}
						}
						RewardsManager.Instance.GrantChampionshipRewards(array, uiOverride);
						Profile.Instance.FinalizeSessionData();
					}
					else
					{
						EB.Debug.LogError("Cannot reward Championship XP and rivets because of null Championship stage!");
					}
				}
				else
				{
					Profile.Instance.RewardSessionGachaSpins();
					Profile.Instance.ClearSessionData();
				}
			}
			else
			{
				bool flag3 = Hub.Instance.State == HubState.Connected;
				bool flag4 = !CampaignData.Instance.IsCampaignPlus() && flag;
				bool flag5 = CampaignData.Instance.IsCampaignPlus() && Profile.Instance.SessionHighScoreAchieved;
				if (flag4)
				{
					if (flag3 && (!CampaignData.Instance.IsNextMissionUnlocked() || CampaignData.Instance.IsLastMission()))
					{
						SinglePlayerRewards currentMissionRewards2 = CampaignData.Instance.GetCurrentMissionRewards();
						if (currentMissionRewards2 != null)
						{
							UnityEngine.Debug.Log(">>>>> Awarding campaign xp " + currentMissionRewards2.PracticeRewardXp);
							RewardsManager.Instance.RewardXP = currentMissionRewards2.PracticeRewardXp;
							RewardsManager.Instance.RewardRivets = currentMissionRewards2.PracticeRewardRivets;
						}
					}
					Profile.Instance.ClearSessionData();
				}
				else if (flag5 && flag3)
				{
					Profile.Instance.FinalizeSessionData();
					SinglePlayerRewards currentMissionRewards3 = CampaignData.Instance.GetCurrentMissionRewards();
					if (currentMissionRewards3 != null)
					{
						RewardsManager.Instance.RewardXP = currentMissionRewards3.PlusRewardXp;
						RewardsManager.Instance.RewardRivets = currentMissionRewards3.PlusRewardRivets;
					}
				}
				else
				{
					Profile.Instance.ClearSessionData();
				}
			}
		}
		Profile.Instance.AddXP(RewardsManager.Instance.RewardXP);
		Profile.Instance.AddSoftCurrency(RewardsManager.Instance.RewardRivets);
		Profile.Instance.Save();
		StatManager.Instance.IncrementStat(StatManager.Stat.PromoGamesPlayed);
		int xP = Profile.Instance.XP;
		int level2 = Profile.Instance.Level;
		RewardsManager.Instance.DidLevel = level != level2;
		List<string> nextScreenStack = new List<string>();
		if (RewardsManager.Instance.DidLevel)
		{
			RewardsManager.Instance.GrantLevelUpRewards();
		}
		if (mode == GameAttributes.ModeType.MP && !Hub.Instance.GameManager.Game.IsPrivate)
		{
			Scoring.Instance.UpdateHoF();
		}
		Hub.Instance.SaveLoadManager.Save();
		ScreenManager.Instance.FadeIn(() =>
		{
			nextScreenStack.Add("ResultsUI");
			Setup.Instance.nextScreenStack = nextScreenStack;
			Setup.Instance.ReturnToFrontendScreen();
		});
	}

	[RPC]
	private void Celebrate()
	{
		Scoring.Instance.SortEndGamePlayers();
		if (GameHUD.Instance != null)
		{
			GameHUD.Instance.HideElementsForCeleb();
		}
		int num = 1;
		List<EB.Sparx.Player> endGamePlayers = Scoring.Instance.GetEndGamePlayers();
		if (endGamePlayers != null)
		{
			GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
			if (gameMode == GameAttributes.GameModeType.TeamDeathMatch && endGamePlayers.Count > 1)
			{
				num = ((Scoring.Instance.GetTeamId(endGamePlayers[1].PlayerId) != Scoring.Instance.GetTeamId(endGamePlayers[0].PlayerId)) ? 1 : 2);
			}
			if (endGamePlayers.Count >= num)
			{
				for (int i = 0; i < num; i++)
				{
					Player player = Instance.GetPlayer(endGamePlayers[i].PlayerId);
					StartCoroutine(DisplayCelebMessage(player));
					player.SendMessage("PlayEndGameCelebAnim");
				}
			}
		}
		CameraController component = Camera.main.GetComponent<CameraController>();
		if (component != null)
		{
			component.OutroSequence(num);
		}
	}

	private IEnumerator DisplayCelebMessage(Player player)
	{
		yield return new WaitForSeconds(Tuning.Instance.Outro_CelebMsg_StartDelay);
		if (GameHUD.Instance != null)
		{
			string playerName = player.GetComponent<Player>().PlayerName;
			GameHUD.Instance.ShowMessage2(Localizer.Format("ID_GAME_MESSAGE_CELEB", playerName));
		}
		else
		{
			UnityEngine.Debug.LogWarning("GameHUD is null!");
		}
	}

	private void Update()
	{
		if (_holes.Count > 0)
		{
			RenderNextHole();
		}
		if (!_started)
		{
			TryToStartGame();
		}
	}

	private void LateUpdate()
	{
		if (base.renderer != null)
		{
			if (DrawingTerrain && !base.renderer.enabled)
			{
				base.renderer.enabled = true;
			}
			else if (!DrawingTerrain && base.renderer.enabled)
			{
				base.renderer.enabled = false;
			}
		}
		UpdateBG();
	}

	private void UpdateBG()
	{
		CameraController component = Camera.main.GetComponent<CameraController>();
		Vector2 vector = component.transform.position;
		float num = Camera.main.aspect * Camera.main.orthographicSize;
		Vector3 localScale = Camera.main.orthographicSize * 2f * Vector3.one * _backgroundScale;
		_backgroundObject.transform.localScale = localScale;
		float num2 = 0f * _backgroundScale;
		float num3 = (1f - component.Zoom01) * (1f - num2) + num2;
		float num4 = component.ViewLimitMax.x - component.ViewLimitMin.x - 2f * num;
		float num5 = component.ViewLimitMax.y - component.ViewLimitMin.y - 2f * Camera.mainCamera.orthographicSize;
		float num6 = (vector.x - (component.ViewLimitMin.x + num)) / num4;
		float num7 = (vector.y - (component.ViewLimitMin.y + Camera.mainCamera.orthographicSize)) / num5;
		num6 -= 0.5f;
		num7 -= 0.5f;
		float num8 = (0f - (_bgWidth * localScale.x - num * 2f)) * num6;
		float num9 = (0f - (_bgHeight * localScale.y - Camera.mainCamera.orthographicSize * 2f)) * num7;
		Vector3 localPosition = _backgroundObject.transform.localPosition;
		localPosition.x = num8 * num3;
		localPosition.y = num9 * num3;
		_backgroundObject.transform.localPosition = localPosition;
	}

	public void DownProbe(FVector2 p0, FVector2 p1, Tools.RayCastCallback fnCB)
	{
		TerrainBody[] terrainBodies = _terrainBodies;
		foreach (TerrainBody terrainBody in terrainBodies)
		{
			List<Vertices> polys = terrainBody.Polys;
			foreach (Vertices item in polys)
			{
				Tools.DownProbe(p0, p1, item, fnCB);
			}
		}
	}

	public bool PointInPolygons(FVector2 p)
	{
		TerrainBody[] terrainBodies = _terrainBodies;
		foreach (TerrainBody terrainBody in terrainBodies)
		{
			if (terrainBody == null)
			{
				continue;
			}
			List<Vertices> polys = terrainBody.Polys;
			foreach (Vertices item in polys)
			{
				if (item.PointInPolygonAngle(ref p))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void BuildCollisionFromSerialized()
	{
		float num = GameplayWidth / (float)collisionRegionsInX;
		float num2 = GameplayHeight / (float)collisionRegionsInY;
		_terrainBodies = new TerrainBody[collisionRegionsInX * collisionRegionsInY];
		int num3 = 0;
		CollisionRegionInfo[] collisionRegions = _collisionRegions;
		foreach (CollisionRegionInfo collisionRegionInfo in collisionRegions)
		{
			AABB aabb = default(AABB);
			aabb.LowerBound = new FVector2((float)(num3 % collisionRegionsInX) * num, (float)(num3 / collisionRegionsInX) * num2);
			aabb.UpperBound = aabb.LowerBound + new FVector2(num, num2);
			_terrainBodies[num3++] = new TerrainBody(collisionRegionInfo.ToVertexLists(), aabb);
		}
	}

	private void BuildNavWorldFromSerialized()
	{
		_navWorld = new Navigation.NavWorld(_navCells, _cellsInX, _cellsInY, _cellDim);
	}

	public Navigation.Node Path(FVector2 A, FVector2 B, out Navigation.PathingResult eRes)
	{
		return Navigation.Search(A, B, _navWorld, out eRes);
	}

	public bool FreeAccessiblePosition(FVector2 fromPos, FVector2 min, FVector2 max, out FVector2 pos)
	{
		return _navWorld.FreeAccessiblePosition(fromPos, min, max, out pos);
	}

	private bool CheckSpawnPoint(Vector2 position, float minDistance)
	{
		foreach (Player value in _playerMap.Values)
		{
			float x = value.transform.position.x;
			float num = Mathf.Abs(x - position.x);
			if (num < minDistance)
			{
				return false;
			}
		}
		return true;
	}

	public Vector3 DetermineRespawn(Player respawning)
	{
		if (GameAttributes.IsTutorialGameMode())
		{
			Hashtable attributes = Hub.Instance.GameManager.Game.Attributes;
			string text = attributes["sp_spawnpoint"] as string;
			if (!string.IsNullOrEmpty(text))
			{
				SpawnPoint[] array = spawnPoints;
				foreach (SpawnPoint spawnPoint in array)
				{
					if (!string.IsNullOrEmpty(text) && spawnPoint._name == text)
					{
						return spawnPoint._pos;
					}
				}
			}
		}
		float respawn_MinDistance = Tuning.Instance.Respawn_MinDistance;
		List<Vector3> list = new List<Vector3>();
		SpawnPoint[] array2 = spawnPoints;
		foreach (SpawnPoint spawnPoint2 in array2)
		{
			if (CheckSpawnPoint(GameplayNormalizedToGameplay(spawnPoint2._pos), respawn_MinDistance))
			{
				list.Add(spawnPoint2._pos);
			}
		}
		if (list.Count > 0)
		{
			return list[UnityEngine.Random.Range(0, list.Count)];
		}
		List<Player> list2 = new List<Player>();
		foreach (Player value in _playerMap.Values)
		{
			if (value != respawning)
			{
				list2.Add(value);
			}
		}
		float num = float.MinValue;
		Vector3 result = spawnPoints[0]._pos;
		SpawnPoint[] array3 = spawnPoints;
		foreach (SpawnPoint spawnPoint3 in array3)
		{
			Vector2 pos = spawnPoint3._pos;
			float num2 = 0f;
			foreach (Player item in list2)
			{
				num2 += Vector2.Distance(item.transform.position, GameplayNormalizedToGameplay(pos));
			}
			if (num2 > num)
			{
				num = num2;
				result = spawnPoint3._pos;
			}
		}
		return result;
	}

	private void StopWorld()
	{
		StopAllCoroutines();
		FSWorldComponent.Destroy();
		ProjectileIndicatorManager.Instance.CleanUp();
		WeaponMonitor.Instance.CleanUp();
		LootManager.Instance.ShutDown();
		EffectVolume.ResetAll();
		Camera.mainCamera.cullingMask = 0;
	}

	private void OnDrawGizmos()
	{
		Vector3 zero = Vector3.zero;
		zero = Vector3.back * 50f;
		zero += Vector3.back;
		if (_BOOL_CANDIDATES != null)
		{
			for (int i = 0; i < _BOOL_CANDIDATES.Count; i++)
			{
				DrawBox(_BOOL_CANDIDATES[i], Color.yellow, zero);
				zero += Vector3.back;
			}
		}
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireCube(Camera.mainCamera.transform.position, new Vector3(ViewWidth, ViewHeight, 10f));
	}

	private static void DrawBox(AABB aabb, Color color, Vector3 offset)
	{
		Vector3 vector = new Vector3(aabb.LowerBound.X, aabb.LowerBound.Y, offset.z);
		Vector3 vector2 = new Vector3(aabb.UpperBound.X, aabb.LowerBound.Y, offset.z);
		Vector3 vector3 = new Vector3(aabb.UpperBound.X, aabb.UpperBound.Y, offset.z);
		Vector3 vector4 = new Vector3(aabb.LowerBound.X, aabb.UpperBound.Y, offset.z);
		UnityEngine.Debug.DrawLine(vector, vector2, color);
		UnityEngine.Debug.DrawLine(vector2, vector3, color);
		UnityEngine.Debug.DrawLine(vector3, vector4, color);
		UnityEngine.Debug.DrawLine(vector4, vector, color);
	}

	private void AddTimeEntry(string tex, double time)
	{
		if (_timeInfos.Count == 20)
		{
			_timeInfos.RemoveAt(0);
		}
		_timeInfos.Add(new TimeInfo
		{
			_tex = tex,
			_time = time
		});
	}

	public void FatRayGameObjectQuery(GameObjectQueryCallback callback, float range, ref FVector2 origin, ref FVector2 ray, int collisionMask)
	{
		if ((collisionMask & 1) != 0)
		{
			foreach (Player value in _playerMap.Values)
			{
				if (FatRayFixtureQuery(value.QueryFixture, range, ref origin, ref ray))
				{
					callback(value.gameObject);
				}
			}
		}
		if ((collisionMask & 2) == 0)
		{
			return;
		}
		GameObject root = WeaponMonitor.Instance.Root;
		foreach (UnityEngine.Transform item in root.transform)
		{
			RangeWeapon component = item.GetComponent<RangeWeapon>();
			if (component != null && component.MyBody != null && FatRayFixtureQuery(component.MyBody.FixtureList[0], range, ref origin, ref ray))
			{
				callback(item.gameObject);
			}
		}
	}

	private bool FatRayFixtureQuery(Fixture f, float range, ref FVector2 origin, ref FVector2 dir)
	{
		AABB aabb;
		f.GetAABB(out aabb, 0);
		FVector2 fVector = aabb.Center + f.Body.Position;
		FVector2 fVector2 = fVector - origin;
		if (FVector2.Dot(fVector2, dir) > 0f)
		{
			FVector2 value = default(FVector2);
			value.X = dir.Y;
			value.Y = 0f - dir.X;
			float num = Mathf.Abs(FVector2.Dot(value, fVector2));
			return num < range;
		}
		return false;
	}

	public void AABBPlayerQuery(PlayerQueryCallback callback, AABB aabb)
	{
		foreach (Player value in _playerMap.Values)
		{
			AABB aabb2;
			value.QueryFixture.GetAABB(out aabb2, 0);
			FVector2 fVector = FSHelper.Vector3ToFVector2(value.transform.position);
			aabb2.LowerBound += fVector;
			aabb2.UpperBound += fVector;
			if (AABB.TestOverlap(aabb2, aabb))
			{
				callback(value);
			}
		}
	}

	public void PlayerRangeQuery(Misc.FixtureRangeQueryCallback callback, FVector2 pos, float range)
	{
		float num = range * range;
		foreach (Player value in _playerMap.Values)
		{
			float num2 = FVector2.DistanceSquared(FSHelper.Vector3ToFVector2(value.transform.position), pos);
			if (num2 < num)
			{
				callback(value.QueryFixture);
			}
		}
	}

	public void DestructibleRangeQuery(Misc.FixtureRangeQueryCallback callback, FVector2 pos, float range)
	{
		float num = range * range;
		foreach (GameObject hitReceiver in _hitReceivers)
		{
			if (!(hitReceiver != null))
			{
				continue;
			}
			FSBodyComponent component = hitReceiver.GetComponent<FSBodyComponent>();
			if (component != null)
			{
				float num2 = FVector2.DistanceSquared(FSHelper.Vector3ToFVector2(hitReceiver.transform.position), pos);
				if (num2 < num)
				{
					callback(component.PhysicsBody.FixtureList[0]);
				}
			}
		}
	}

	public void RegisterHitReceiver(GameObject g)
	{
		if (_hitReceivers.Contains(g))
		{
			EB.Debug.LogWarning(string.Format("THIS OBJECT {0} HAS ALREADY BEEN REGISTERED! DOES IT HAVE TWO FSSPECIAL SCRIPTS?", g.name));
		}
		_hitReceivers.Add(g);
	}

	public void UnregisterHitReceiver(GameObject g)
	{
		_hitReceivers.Remove(g);
	}
}
