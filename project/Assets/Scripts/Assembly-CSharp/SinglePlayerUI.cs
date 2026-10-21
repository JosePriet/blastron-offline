using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class SinglePlayerUI : MonoBehaviour
{
	[System.Serializable]
	public class Settings
	{
		public bool TaperRoute = true;

		public bool ScrollRoute = true;

		public bool DrawCurrentStageRouteOnly = true;

		public bool AlphaFade = true;

		public bool AlphaFadeBothEnds;
	}

	private delegate void OrbitDelegate();

	private const float BUTTON_YDIFF = 40f;

	private const float _asteroidPanDuration = 3f;

	public GameObject _worldMapPrefab;

	public Material _skybox;

	public AudioEvent _territorySelect;

	public AudioEvent _worldOrbit;

	public AudioEvent _activateLevelPanel;

	public AudioEvent _panAwayFromAsteroid;

	public AudioEvent _asteroidIntro;

	public Settings _settings;

	public GameObject _stageinfoPrefab;

	public Material _routeMaterial;

	private GameObject _stageInfoObj;

	private StageInfoPanelUI _stageInfo;

	private AudioEmitter _audioEmitter;

	private string unlockSoonTexturePath = string.Empty;

	private GameObject asteroid;

	private List<Mesh> _routeMeshes = new List<Mesh>();

	private readonly Color[] _stateColours = new Color[5]
	{
		new Color(1f, 1f, 1f),
		Color.white,
		Color.white,
		Color.white,
		Color.white
	};

	private Camera _worldCamera;

	private GameObject _worldMap;

	private GameObject _territories;

	private WorldCam _worldCam;

	private int _worldLayer;

	private Transform _divsParent;

	private bool _enableUserInput;

	private UIPanelAlpha _rootPanelAlpha;

	private Transform _worldControlsRoot;

	private Transform _haloT;

	private int _currentDivision;

	private GameObject[] _divisionButtons = new GameObject[3];

	private Vector3[] _divisionButtonPositions = new Vector3[3];

	private Dictionary<string, StageItemControl> _stageItems = new Dictionary<string, StageItemControl>();

	private UILabel _starLabel;

	private Division.Stage _currentStage;

	public static bool ShownAsteroid;

	private bool _motorOrbitInProgress;

	private bool _withFader;

	private bool _fadeControls;

	private bool _fadeFader;

	public static SinglePlayerUI Instance { get; private set; }

	public static float GlobalAlpha { get; private set; }

	public static Camera WorldCam
	{
		get
		{
			return Instance._worldCamera;
		}
	}

	private void Awake()
	{
		Instance = this;
		if (ScreenManager.Instance.GetPopup("ConfirmationDialogUI") != null)
		{
			ScreenManager.Instance.ClosePopup("ConfirmationDialogUI");
		}
		_worldLayer = LayerMask.NameToLayer("World");
		MakeCameraAndLight();
		SetupControls();
		SetupStageInfoPanel();
		_enableUserInput = true;
		_audioEmitter = base.gameObject.AddComponent<AudioEmitter>();
		_audioEmitter.m_PlayMode = AudioEmitter.PlayMode.Stack;
		FadeControls(true, false, true, 0f);
		Messenger.BroadcastToAllListeners("StopMusic");
	}

	private void MakeCameraAndLight()
	{
		Transform transform = base.transform.FindChild("WorldView");
		_worldCamera = transform.camera;
		_worldCamera.orthographicSize = 1f;
		_worldCam = transform.GetComponent<WorldCam>();
		_worldCam.Radius = 2000f;
	}

	private void SetupControls()
	{
		Transform transform = base.transform.FindChild("ControlsRootPanel");
		_rootPanelAlpha = transform.GetComponent<UIPanelAlpha>();
		_rootPanelAlpha.alpha = 0f;
		_worldControlsRoot = base.transform.FindChild("WorldControlRoot");
		_divsParent = transform.FindChild("Divisions");
		for (int i = 0; i < 3; i++)
		{
			bool flag = i <= ChampionshipData.DivisionsUnlocked();
			GameObject objectExactMatch = Util.GetObjectExactMatch(_divsParent.gameObject, "d" + i);
			_divisionButtons[i] = objectExactMatch;
			_divisionButtonPositions[i] = objectExactMatch.transform.localPosition;
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Lock");
			objectExactMatch2.SetActive(!flag);
			UISprite component = Util.GetObjectExactMatch(objectExactMatch, "Background").GetComponent<UISprite>();
			component.color = ((!flag) ? new Color(0.25f, 0.25f, 0.25f, 1f) : Color.white);
		}
		_divisionButtons[0].GetComponent<ButtonScript>().onButtonClick += OnDiv1;
		_divisionButtons[1].GetComponent<ButtonScript>().onButtonClick += OnDiv2;
		_divisionButtons[2].GetComponent<ButtonScript>().onButtonClick += OnDiv3;
		Util.GetObjectExactMatch(base.gameObject, "EnergyButton").GetComponent<ButtonScript>().onButtonClick += OnEnergy;
		Util.GetObjectExactMatch(base.gameObject, "BackButton").GetComponent<ButtonScript>().onButtonClick += OnBack;
		Util.GetObjectExactMatch(base.gameObject, "FindMe").GetComponent<ButtonScript>().onButtonClick += OnRotateTo;
		_starLabel = Util.GetObjectExactMatch(Util.GetObjectExactMatch(base.gameObject, "StarTotal"), "Label").GetComponent<UILabel>();
		_starLabel.text = Localizer.Format("ID_UI_STATUS_BAR_XP", ChampionshipData.TotalStars(), ChampionshipData.MaxStars());
		UpdateUnlocksIn();
	}

	private void UpdateUnlocksIn()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "UnlockPanel");
		int num = ChampionshipData.TotalStars();
		ChampionshipRewardTiers.Tier nextTierFor = ChampionshipData.RewardTiers.GetNextTierFor(num);
		if (nextTierFor != null)
		{
			UILabel component = Util.GetObjectExactMatch(objectExactMatch, "Label").GetComponent<UILabel>();
			UITexture unlocksInTexture = Util.GetObjectExactMatch(objectExactMatch, "Texture").GetComponent<UITexture>();
			ReleaseUnlocksInTexture();
			InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(nextTierFor.WeaponName));
			unlockSoonTexturePath = metaData.uiTexturePath;
			TexturePoolManager.Instance.LoadTexture(unlockSoonTexturePath, SafeAction.Wrap(this, (Texture2D loadedTexture) =>
			{
				unlocksInTexture.mainTexture = loadedTexture;
			}));
			component.text = Localizer.Format("ID_UI_SINGLEPLAYER_UNLOCKS_IN", nextTierFor.NumStars - num);
		}
		else
		{
			objectExactMatch.transform.localPosition += new Vector3(5000f, 5000f, 0f);
			UIPanel panel = Util.FindComponentUpwards<UIPanel>(objectExactMatch);
			bool previouslyStaticWidgets = panel.widgetsAreStatic;
			panel.widgetsAreStatic = false;
			Coroutines.NextFrame(SafeAction.Wrap(this, () =>
			{
				panel.widgetsAreStatic = previouslyStaticWidgets;
			}));
		}
	}

	private void SetupStageInfoPanel()
	{
		_stageInfoObj = UnityEngine.Object.Instantiate(Resources.Load("UI/StageInfoPanelUI")) as GameObject;
		_stageInfoObj.transform.parent = Util.GetObject(ScreenManager.Instance.gameObject, "Panel").transform;
		_stageInfoObj.transform.localPosition = new Vector3(0f, 0f, -20f);
		_stageInfoObj.transform.localRotation = Quaternion.identity;
		_stageInfoObj.transform.localScale = Vector3.one;
		_stageInfo = _stageInfoObj.GetComponent<StageInfoPanelUI>();
	}

	private void OnOpenPopup()
	{
		ScreenManager.Instance.AddPersistentScreen("StatusBar");
		ScreenManager.Instance.RmvPersistentScreen("BackgroundUI");
		ScreenManager.Instance.EnableTransitionBlocker(false);
	}

	private IEnumerator Start()
	{
		ScreenManager.Instance.ResetCameraClearFlags();
		if (ChampionshipData.TotalStars() == ChampionshipData.MaxStars() && !UnlocksManager.Instance._hasCollectedAllStars)
		{
			ScreenManager.Instance.OpenPopup("CampaignAllStarsCollectedUI", false, true);
			UnlocksManager.Instance._hasCollectedAllStars = true;
		}
		else if (UnlocksManager.Instance.unlockedDivision > 0)
		{
			ScreenManager.Instance.OpenPopup("CampaignDivisionCompletedUI", false, true);
		}
		if (_worldMapPrefab != null)
		{
			_worldMap = UnityEngine.Object.Instantiate(_worldMapPrefab) as GameObject;
			_worldMap.transform.parent = base.transform;
			_worldMap.transform.localPosition = Vector3.zero;
			_worldMap.transform.localScale = new Vector3(550f, 550f, 550f);
			_haloT = _worldMap.transform.Find("halo");
			if (_haloT != null)
			{
				_haloT.transform.parent = base.transform;
				_haloT.localPosition += Vector3.forward * 50f;
				_haloT.localScale *= 0.99f;
			}
		}
		InitializeTerritories();
		_withFader = false;
		ScreenManager.Instance.FadeOut(null);
		if (ShownAsteroid)
		{
			yield return new WaitForEndOfFrame();
			_worldCam.LookAtWorld(OnWorldCamFinishPanning, 0f);
			FadeControls(true, true, true, 2f);
			yield break;
		}
		_audioEmitter.Play(_asteroidIntro);
		_worldCam.SlowPan(3f);
		yield return new WaitForSeconds(4f);
		_audioEmitter.Play(_panAwayFromAsteroid);
		ShownAsteroid = true;
		_worldCam.LookAtWorld(OnWorldCamFinishPanning, 2f);
	}

	private void OnWorldCamFinishPanning()
	{
		asteroid = Util.GetObjectExactMatch(base.gameObject, "world_asteroid");
		if (asteroid != null)
		{
			Util.SetActiveRecursive(asteroid, false);
		}
		_enableUserInput = true;
		FadeControls(true, true, _withFader, 0.5f);
		if (ChampionshipData.Instance._selectedDivision != -1)
		{
			int selectedDivision = ChampionshipData.Instance._selectedDivision;
			ChampionshipData.Instance._selectedDivision = -1;
			SetStageForDivision(selectedDivision);
		}
		else if (_currentStage != null)
		{
			SetStageForDivision(_currentDivision);
		}
		else
		{
			SetStageForDivision(ChampionshipData.CurrentDivision);
		}
	}

	private void SetStageForDivision(int division)
	{
		UnityEngine.Debug.Log("Setup stage for div " + division);
		if (_currentDivision != division)
		{
			RecessDivisionButton(_currentDivision);
		}
		FocusDivisionButton(division);
		_currentDivision = division;
		foreach (StageItemControl value in _stageItems.Values)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		_stageItems.Clear();
		SetupTerritories();
		SetupStageUI();
		UpdateRoute();
		_starLabel.text = Localizer.Format("ID_UI_STATUS_BAR_XP", ChampionshipData.TotalStars(), ChampionshipData.MaxStars());
		UpdateUnlocksIn();
		_motorOrbitInProgress = true;
		Division.Stage stage;
		if (ChampionshipData.Instance.SelectedStage != null)
		{
			stage = ChampionshipData.Instance.SelectedStage;
			UnityEngine.Debug.Log(string.Concat("Selected stage is ", stage, " whose level is ", stage._level));
			ChampionshipData.Instance.SelectedStage = null;
			_worldCam.OrbitTo(TerritoryPosition(stage), () =>
			{
				_motorOrbitInProgress = false;
			});
		}
		else if (_currentStage != null)
		{
			stage = _currentStage;
			_worldCam.OrbitTo(TerritoryPosition(stage), () =>
			{
				_motorOrbitInProgress = false;
			});
		}
		else
		{
			stage = ChampionshipData.CurrentForDiv(_currentDivision);
			_worldCam.OrbitTo(CurrentTerritoryPosition(_currentDivision), () =>
			{
				_motorOrbitInProgress = false;
			});
		}
		Flash(stage, -1f);
	}

	private void RecessDivisionButton(int idx)
	{
		GameObject gameObject = _divisionButtons[idx];
		Vector3 dest = _divisionButtonPositions[idx];
		float num = Mathf.Abs(gameObject.transform.localPosition.y - dest.y);
		AnimatePosition.Do(gameObject, EZAnimation.ANIM_MODE.To, dest, EZAnimation.sinusOut, 0.2f * (num / 40f), 0f, null, null);
	}

	private void FocusDivisionButton(int idx)
	{
		GameObject gameObject = _divisionButtons[idx];
		Vector3 dest = _divisionButtonPositions[idx] + new Vector3(0f, 40f, 0f);
		float num = Mathf.Abs(gameObject.transform.localPosition.y - dest.y);
		AnimatePosition.Do(gameObject, EZAnimation.ANIM_MODE.To, dest, EZAnimation.sinusOut, 0.2f * (num / 40f), 0f, null, null);
	}

	private void OnRotateTo(GameObject go)
	{
		if (!_motorOrbitInProgress)
		{
			if (_currentDivision != ChampionshipData.CurrentDivIndex)
			{
				SetStageForDivision(ChampionshipData.CurrentDivIndex);
				return;
			}
			Division.Stage stage = ChampionshipData.CurrentForDiv(ChampionshipData.CurrentDivIndex);
			OnRotateToStage(stage);
		}
	}

	private void OnStageTouched(Division.Stage stage)
	{
		OnRotateToStage(stage);
	}

	private void OnRotateToStage(Division.Stage stage)
	{
		_motorOrbitInProgress = true;
		_audioEmitter.Play(_territorySelect);
		_audioEmitter.Play(_activateLevelPanel);
		_worldCam.OrbitTo(StageToPos(stage), () =>
		{
			_currentStage = stage;
			_stageInfo.SetParameters(_currentDivision, stage);
			_stageInfo.Show();
			_motorOrbitInProgress = false;
		});
	}

	private Vector3 StageToPos(Division.Stage stage)
	{
		StageItemControl value;
		if (_stageItems.TryGetValue(stage._level, out value))
		{
			return value.Territory.Position;
		}
		return Vector3.zero;
	}

	private void InitializeTerritories()
	{
		_territories = Util.GetObject(_worldMap, "world_territories");
		foreach (Transform item in _territories.transform)
		{
			item.gameObject.AddComponent<TerritoryControl>();
		}
	}

	private void SetupTerritories()
	{
		if (!(_territories != null))
		{
			return;
		}
		foreach (Transform item in _territories.transform)
		{
			int stageIdx = ChampionshipData.StageIndexFromName(_currentDivision, item.name);
			bool dim = !ChampionshipData.Instance.IsStageUnlocked(_currentDivision, stageIdx);
			TerritoryControl component = item.GetComponent<TerritoryControl>();
			component.Dim(dim);
			component.DeactivateBorderFlash();
		}
	}

	private Transform GetTerritoryMatchingName(string stageName)
	{
		if (_territories != null)
		{
			foreach (Transform item in _territories.transform)
			{
				if (stageName.EndsWith(item.name))
				{
					return item;
				}
			}
		}
		return null;
	}

	private void SetupStageUI()
	{
		Division division = ChampionshipData.Instance.GetDivision(_currentDivision);
		Division.Stage[] stages = division._stages;
		foreach (Division.Stage stage in stages)
		{
			Transform territoryMatchingName = GetTerritoryMatchingName(stage._level);
			if (territoryMatchingName != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_stageinfoPrefab) as GameObject;
				gameObject.transform.parent = _worldControlsRoot;
				StageItemControl component = gameObject.GetComponent<StageItemControl>();
				component.Stage = stage;
				component.Division = _currentDivision;
				component.FriendFacebookId = null;
				component.Refresh();
				component.Territory = territoryMatchingName.GetComponent<TerritoryControl>();
				Util.Parent(base.gameObject, component.gameObject);
				_stageItems.Add(stage._level, component);
			}
			else
			{
				EB.Debug.LogError("I did NOT find a mesh for a territory named : " + stage._level);
			}
		}
		if (Hub.Instance.FacebookManager.HasAccessToken)
		{
			Id[] array = new Id[Hub.Instance.FacebookManager.AppFriends.Length];
			for (int j = 0; j < Hub.Instance.FacebookManager.AppFriends.Length; j++)
			{
				array[j] = Hub.Instance.FacebookManager.AppFriends[j].UserId;
			}
			UnityEngine.Debug.LogWarning("Friends length " + array.Length);
			Hub.Instance.LeaderboardManager.GetScores(6, (-1).ToString(), array.Length, 0, array, OnFBFriendsProgressFetched);
		}
	}

	private void OnFBFriendsProgressFetched(int leaderboardId, string error, Dictionary<Id, Hashtable> records)
	{
		if (!string.IsNullOrEmpty(error))
		{
			return;
		}
		Division division = ChampionshipData.Instance.GetDivision(_currentDivision);
		Id id;
		foreach (Id key in records.Keys)
		{
			id = key;
			int num = Dot.Integer("score", records[id], 0);
			int num2 = num >> 16;
			int num3 = (num & 0xFFFF) + 1;
			if (num3 == division._stages.Length)
			{
				num2++;
			}
			if (num2 != _currentDivision || ChampionshipData.Instance.IsFinishedChampionship(num2, num3) || num3 >= division._stages.Length || num3 < 0)
			{
				continue;
			}
			StageItemControl stageItemControl = _stageItems[division._stages[num3]._level];
			if (stageItemControl != null)
			{
				FacebookUser facebookUser = Array.Find(Hub.Instance.FacebookManager.AppFriends, (FacebookUser curUser) => curUser.UserId.Equals(id));
				if (facebookUser != null && !string.IsNullOrEmpty(facebookUser.FacebookId))
				{
					stageItemControl.FriendFacebookId = facebookUser.FacebookId;
					stageItemControl.Refresh();
					UnityEngine.Debug.LogWarning("Progress Name: " + facebookUser.FirstName + " " + facebookUser.LastName + " is at " + num + " division " + _currentDivision);
				}
			}
		}
	}

	private Vector3 CurrentTerritoryPosition(int div)
	{
		Division.Stage stage = ChampionshipData.CurrentForDiv(div);
		return TerritoryPosition(stage);
	}

	private Vector3 TerritoryPosition(Division.Stage stage)
	{
		if (stage == null)
		{
			UnityEngine.Debug.LogError("STAGE IS NULL?! WHY?");
		}
		if (stage._level == null)
		{
			UnityEngine.Debug.LogError("STAGE._LEVEL IS NULL");
		}
		StageItemControl value;
		if (_stageItems.TryGetValue(stage._level, out value))
		{
			return value.Territory.Position;
		}
		return Vector3.zero;
	}

	private void OnBack(GameObject go)
	{
		if (_stageInfo.IsVisible())
		{
			_stageInfo.onClose(base.gameObject);
			return;
		}
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.ClosePopup();
			ChampionshipData.Instance.SelectedStage = null;
			ChampionshipData.Instance._selectedDivision = -1;
			bool Argument = true;
			Messenger<bool>.BroadcastToAllListeners("Restart", ref Argument);
		});
	}

	private void OnDestroy()
	{
		ReleaseUnlocksInTexture();
		if (_stageInfoObj != null)
		{
			UnityEngine.Object.Destroy(_stageInfoObj);
		}
	}

	private void ReleaseUnlocksInTexture()
	{
		if (!string.IsNullOrEmpty(unlockSoonTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(unlockSoonTexturePath);
			unlockSoonTexturePath = string.Empty;
		}
	}

	private void OnEnergy(GameObject go)
	{
		if (Profile.Instance.Energy == Profile.Instance.EnergyMax)
		{
			ScreenManager.Instance.OpenPopup("EnergyFullUI", true, true);
		}
		else
		{
			ScreenManager.Instance.OpenPopup("GetMoreEnergyUI", true, true);
		}
	}

	public void OnExitToStore()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.ClosePopup();
			ChampionshipData.Instance.SelectedStage = null;
			ChampionshipData.Instance._selectedDivision = -1;
			bool Argument = true;
			Messenger<bool>.BroadcastToAllListeners("Restart", ref Argument);
			GameObject gameObject = ScreenManager.Instance.OpenPopup("StoreUI");
			StoreUI component = gameObject.GetComponent<StoreUI>();
			component.SetReturnScreen("SinglePlayerUI");
		});
	}

	private Vector3 ToScreen(Vector3 world)
	{
		return _worldCamera.WorldToScreenPoint(world);
	}

	private void Update()
	{
		if (_worldMap == null)
		{
			return;
		}
		UpdateTerritoryInfos();
		if (_routeMeshes == null || ChampionshipData.Instance.IsFinishedChampionship())
		{
			return;
		}
		if (_settings.DrawCurrentStageRouteOnly)
		{
			Division division = ChampionshipData.Instance.GetDivision(_currentDivision);
			int num = division.FindIndexForStage(ChampionshipData.CurrentForDiv(_currentDivision));
			if (num > 0 && num <= _routeMeshes.Count && _currentDivision == ChampionshipData.CurrentDivision)
			{
				Mesh mesh = _routeMeshes[num - 1];
				Graphics.DrawMesh(mesh, _territories.transform.localToWorldMatrix, _routeMaterial, _worldLayer);
			}
			return;
		}
		foreach (Mesh routeMesh in _routeMeshes)
		{
			Graphics.DrawMesh(routeMesh, _territories.transform.localToWorldMatrix, _routeMaterial, _worldLayer);
		}
	}

	private void UpdateTerritoryInfos()
	{
		Matrix4x4 localToWorldMatrix = _territories.transform.localToWorldMatrix;
		foreach (StageItemControl value2 in _stageItems.Values)
		{
			Vector3 position = ToScreen(localToWorldMatrix.MultiplyPoint3x4(value2.Territory.Position));
			value2.transform.position = UICamera.currentCamera.ScreenToWorldPoint(position);
			float num = 0f - Vector3.Dot(_worldCamera.transform.forward, value2.Territory.Position * 2f);
			float value = 1f;
			if (num < 0.707f)
			{
				value = ((!(num > 0.259f)) ? 0f : ((num - 0.259f) / 0.448f));
			}
			value2.Alpha = Mathf.Clamp01(value);
			float num2 = Mathf.Clamp((num + 1f) / 2f, 0.966f, 1f);
			num2 = (num2 - 0.966f) / 0.03399998f * 0.29999995f + 1f;
			value2.transform.localScale = new Vector3(num2, num2, num2);
		}
	}

	private void UpdateRoute()
	{
		foreach (Mesh routeMesh in _routeMeshes)
		{
			UnityEngine.Object.Destroy(routeMesh);
		}
		_routeMeshes.Clear();
		Division division = ChampionshipData.Instance.GetDivision(_currentDivision);
		float num = 0.0625f;
		int num2 = 32;
		int[] array = new int[3 * num2];
		int num3 = -1;
		for (int i = 0; i < 16; i++)
		{
			array[++num3] = i * 2;
			array[++num3] = i * 2 + 1;
			array[++num3] = i * 2 + 2;
			array[++num3] = i * 2 + 1;
			array[++num3] = i * 2 + 3;
			array[++num3] = i * 2 + 2;
		}
		for (int j = 0; j < division._stages.Length - 1; j++)
		{
			float num4 = 0f;
			Division.Stage stage = division._stages[j];
			Division.Stage stage2 = division._stages[j + 1];
			int num5 = 34;
			Vector3[] array2 = new Vector3[num5];
			Vector2[] array3 = new Vector2[num5];
			Color[] array4 = new Color[num5];
			StageItemControl value;
			StageItemControl value2;
			if (!_stageItems.TryGetValue(stage._level, out value) || !_stageItems.TryGetValue(stage2._level, out value2))
			{
				continue;
			}
			Vector3 position = value.Territory.Position;
			Vector3 position2 = value2.Territory.Position;
			Vector3 vector = position2 - position;
			Vector3 rhs = position + vector * 0.5f;
			vector.Normalize();
			rhs.Normalize();
			Vector3 vector2 = Vector3.Cross(vector, rhs) * 0.05f;
			Vector2 zero = Vector2.zero;
			Vector2 up = Vector2.up;
			float num6 = Vector3.Distance(position, position2);
			Vector3 vector3 = position;
			float num7 = 0f;
			for (int k = 0; k < 17; k++)
			{
				float num8 = 2f * num4 - 1f;
				float num9 = 0f - num8 * num8 + 1f;
				Vector3 vector4 = Vector3.Lerp(position, position2, num4).normalized * (0.5f + num9 * 0.2f * num6);
				int num10 = k * 2;
				Vector3 vector5 = vector2 * 0.5f;
				if (_settings.TaperRoute)
				{
					vector5 *= num4;
				}
				array2[num10] = vector4 - vector5;
				array2[num10 + 1] = vector4 + vector5;
				num7 = (up.x = (zero.x = num7 + (vector4 - vector3).magnitude * 20f));
				vector3 = vector4;
				array3[num10] = zero;
				array3[num10 + 1] = up;
				Color white = Color.white;
				white.a = 1f;
				if (_settings.AlphaFade)
				{
					white.a = num4;
				}
				else if (_settings.AlphaFadeBothEnds)
				{
					white.a = num9;
				}
				array4[num10] = white;
				array4[num10 + 1] = white;
				num4 += num;
			}
			Mesh mesh = new Mesh();
			mesh.vertices = array2;
			mesh.triangles = array;
			mesh.uv = array3;
			mesh.colors = array4;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			_routeMeshes.Add(mesh);
		}
	}

	public static bool UserInputEnabled()
	{
		if (Instance != null)
		{
			return Instance._enableUserInput;
		}
		return false;
	}

	public static void UserInputToggle()
	{
		if (Instance != null)
		{
			Instance._enableUserInput = !Instance._enableUserInput;
		}
	}

	public static void EnableUserInput(bool enable)
	{
		if (Instance != null)
		{
			Instance._enableUserInput = enable;
		}
	}

	public void OnDiv1(GameObject go)
	{
		ChampionshipData.Instance.SelectedStage = null;
		_currentStage = null;
		SetStageForDivision(0);
	}

	public void OnDiv2(GameObject go)
	{
		if (1 <= ChampionshipData.DivisionsUnlocked())
		{
			ChampionshipData.Instance.SelectedStage = null;
			_currentStage = null;
			SetStageForDivision(1);
		}
	}

	public void OnDiv3(GameObject go)
	{
		if (2 <= ChampionshipData.DivisionsUnlocked())
		{
			ChampionshipData.Instance.SelectedStage = null;
			_currentStage = null;
			SetStageForDivision(2);
		}
	}

	private void Flash(Division.Stage s, float duration)
	{
		Flash(_stageItems[s._level], duration);
	}

	private void Flash(StageItemControl s, float duration)
	{
		if (s != null)
		{
			s.Territory.ActivateBorderFlash(duration, 1f, new Color(1f, 0.5f, 0f, 1f));
		}
	}

	public static void FadeControls(bool fadeIn, bool controls, bool fader, float period)
	{
		Instance._FadeControls(fadeIn, controls, fader, period);
	}

	private void _FadeControls(bool fadeIn, bool controls, bool fader, float period)
	{
		StopCoroutine("DoFadeControls");
		object[] value = new object[2] { fadeIn, period };
		_fadeControls = controls;
		_fadeFader = fader;
		StartCoroutine("DoFadeControls", value);
	}

	private IEnumerator DoFadeControls(object[] param)
	{
		bool fadeIn = (bool)param[0];
		float fadePeriod = (float)param[1];
		float dir = 1f;
		float a1 = 1f;
		if (!fadeIn)
		{
			dir = -1f;
			a1 = 0f;
		}
		if (fadePeriod <= 0f)
		{
			SetAlpha(a1);
		}
		float dt = 1f / fadePeriod;
		float a2 = _rootPanelAlpha.alpha;
		for (float t = 0f; t <= 1f; t += dt * UnityEngine.Time.deltaTime)
		{
			yield return new WaitForEndOfFrame();
			SetAlpha(a2 + t * (a1 - a2));
		}
		SetAlpha(a1);
	}

	private void SetAlpha(float f)
	{
		if (_fadeControls)
		{
			_rootPanelAlpha.alpha = f;
			GlobalAlpha = f;
		}
		if (_fadeFader)
		{
			_worldCam.FaderAlpha = ToFaderAlpha(f);
		}
	}

	public float ToFaderAlpha(float alpha)
	{
		return 1f - (0.5f * alpha + 0.5f);
	}

	public static void LookAway()
	{
		Instance._worldCam.LookAway(null, 2f);
	}

	public static void LookAtWorld()
	{
		Instance._withFader = true;
		Instance._worldCam.LookAtWorld(Instance.OnWorldCamFinishPanning, 2f);
	}

	private void LateUpdate()
	{
		Vector3 forward = _worldCamera.transform.forward;
		_haloT.localPosition = forward * 50f;
		_haloT.LookAt(base.transform);
	}

	public IEnumerator Cleanup()
	{
		_territories = null;
		foreach (StageItemControl s in _stageItems.Values)
		{
			UnityEngine.Object.Destroy(s);
		}
		_stageItems.Clear();
		UnityEngine.Object.Destroy(_worldMap);
		_worldMap = null;
		yield return new WaitForFixedUpdate();
		TexturePoolManager.Instance.ClearPool();
		yield return Assets.UnloadUnusedAssets();
	}
}
