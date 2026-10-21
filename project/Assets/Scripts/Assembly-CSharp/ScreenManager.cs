using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Collections;
using EB.Sparx;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
	public class PopupInfo
	{
		public string name;

		public GameObject popupObj;

		public bool fader;

		public bool faderPrevious;

		public bool showPrevious;

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", name, showPrevious, fader);
		}
	}

	public const float ZDepthPerLayer = 50f;

	private const float ZDepthFader = 5f;

	private static ScreenManager _this;

	private EB.Collections.Stack<PopupInfo> _popups = new EB.Collections.Stack<PopupInfo>();

	private EB.Collections.Queue<PopupInfo> _queuedPopups = new EB.Collections.Queue<PopupInfo>();

	private Dictionary<string, GameObject> _persistent;

	private Dictionary<string, GameObject> _overlays;

	private int _loadingScreenRef;

	private GameObject _loadingScreen;

	private GameObject uiPanel;

	private GameObject _blocker;

	private Fader fader;

	private GameObject _transitionblocker;

	private GameObject loginActivity;

	private Foreground foregroundBg;

	private GameObject nibbles;

	private UICamera cam;

	private Camera rootCam;

	private CameraClearFlags originalClearFlags;

	private bool _OSBackButtonEnabled;

	private bool _TitleScreenCleared;

	public static ScreenManager Instance
	{
		get
		{
			return _this;
		}
	}

	public Vector2 UiDimensions
	{
		get
		{
			Camera camera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			UIRoot uIRoot = NGUITools.FindInParents<UIRoot>(camera.gameObject);
			float num = (float)Screen.width / (float)Screen.height;
			return new Vector2(num * (float)uIRoot.manualHeight, uIRoot.manualHeight);
		}
	}

	public bool PopupOpen
	{
		get
		{
			return _popups.Count > 0;
		}
	}

	public bool IsLoadingScreenUp
	{
		get
		{
			return _loadingScreen != null;
		}
	}

	private void Awake()
	{
		_this = this;
		_persistent = new Dictionary<string, GameObject>();
		_overlays = new Dictionary<string, GameObject>();
		uiPanel = Util.GetObject(base.gameObject, "Panel");
		Object.DontDestroyOnLoad(base.gameObject);
		GameObject prefab = Resources.Load("UI/fader") as GameObject;
		_blocker = NGUITools.AddChild(uiPanel, prefab);
		_blocker.transform.localPosition = Vector3.zero;
		_blocker.transform.localScale = new Vector3(2048f, 1f, 2048f);
		_blocker.transform.localEulerAngles = new Vector3(0f, 90f, 270f);
		fader = _blocker.GetComponent<Fader>();
		_blocker.layer = base.gameObject.layer;
		BoxCollider boxCollider = _blocker.AddComponent<BoxCollider>();
		boxCollider.size = Vector3.one;
		boxCollider.center = Vector3.zero;
		EnableBgFade(false);
		_transitionblocker = new GameObject("transitionBlocker");
		Util.Parent(uiPanel, _transitionblocker);
		_transitionblocker.transform.localPosition = new Vector3(0f, 0f, -500f);
		_transitionblocker.layer = base.gameObject.layer;
		Vector2 uiDimensions = UiDimensions;
		BoxCollider boxCollider2 = _transitionblocker.AddComponent<BoxCollider>();
		boxCollider2.size = new Vector3(uiDimensions.x, uiDimensions.y, 1f);
		boxCollider2.center = Vector3.zero;
		_transitionblocker.collider.enabled = false;
		loginActivity = Instance.AddOverlayScreen("LoginActivity");
		GameObject obj = GameObject.Find("UI Root (2D)");
		cam = Util.GetObjectExactMatch(obj, "Camera").GetComponent<UICamera>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Camera");
		rootCam = objectExactMatch.GetComponent<Camera>();
		originalClearFlags = rootCam.clearFlags;
		EB.Debug.Log("LOADING FOREGROUND!");
		GameObject prefab2 = Resources.Load("UI/ForegroundBg") as GameObject;
		GameObject gameObject = NGUITools.AddChild(uiPanel, prefab2);
		gameObject.transform.localPosition = new Vector3(0f, 0f, -600f);
		gameObject.transform.localScale = new Vector3(2048f, 1f, 2048f);
		gameObject.transform.localEulerAngles = new Vector3(0f, 90f, 90f);
		gameObject.layer = base.gameObject.layer;
		BoxCollider boxCollider3 = gameObject.AddComponent<BoxCollider>();
		boxCollider3.size = new Vector3(uiDimensions.x, uiDimensions.y, 1f);
		boxCollider3.center = Vector3.zero;
		foregroundBg = gameObject.GetComponent<Foreground>();
		EB.Debug.Log("LOADING FOREGROUND!");
		GameObject prefab3 = Resources.Load("UI/Nibbles") as GameObject;
		nibbles = NGUITools.AddChild(uiPanel, prefab3);
		nibbles.transform.localPosition = new Vector3(0f, 0f, -610f);
		nibbles.layer = base.gameObject.layer;
		EnableNibbles(false);
		EnableMultiTouch(false);
	}

	public void EnableMultiTouch(bool enable)
	{
		if (cam != null)
		{
			cam.allowMultiTouch = enable;
		}
	}

	public void EnableNibbles(bool enable)
	{
		NGUITools.SetActive(nibbles, enable);
	}

	public void SetCameraClearFlags(CameraClearFlags ccf, Color color)
	{
		rootCam.clearFlags = ccf;
		rootCam.backgroundColor = color;
	}

	public void ResetCameraClearFlags()
	{
		rootCam.clearFlags = originalClearFlags;
	}

	private IEnumerator Start()
	{
		base.transform.position = new Vector3(1000f, 1000f, 1000f);
		Object.DontDestroyOnLoad(base.gameObject);
		yield break;
	}

	public EB.Collections.Stack<PopupInfo> GetPopupStack()
	{
		return _popups;
	}

	public void FadeIn(Foreground.fadeInCallback fb)
	{
		EnableTransitionBlocker(true);
		foregroundBg.FadeIn(fb);
	}

	public void FadeOut(Foreground.fadeOutCallback fb)
	{
		EnableTransitionBlocker(true);
		EnableNibbles(false);
		foregroundBg.FadeOut(fb);
	}

	public void EnableBgFade(bool state)
	{
		EB.Debug.Log("ENABLING BG " + state);
		if (state)
		{
			NGUITools.SetActive(_blocker, state);
		}
		fader.Enable(state);
	}

	public GameObject AddOverlayScreen(string name)
	{
		if (!_overlays.ContainsKey(name))
		{
			GameObject gameObject = FindPopup(name);
			_overlays[name] = gameObject;
			Object.DontDestroyOnLoad(gameObject);
			gameObject.BroadcastMessage("OnOpenScreen", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			EB.Debug.Log("SCREEN ALREADY OPEN ( OVERLAY ) " + name);
		}
		return _overlays[name];
	}

	public void RemoveOverlayScreen(string name)
	{
		EB.Debug.Log("REMOVING OVERLAY SCREN " + name);
		using (Dictionary<string, GameObject>.KeyCollection.Enumerator enumerator = _persistent.Keys.GetEnumerator())
		{
			if (!enumerator.MoveNext())
			{
				return;
			}
			string current = enumerator.Current;
			EB.Debug.Log("KEYS = " + current);
			if (current == name)
			{
				GameObject gameObject = _overlays[current];
				gameObject.transform.parent = null;
				Object.Destroy(gameObject);
				_overlays.Remove(current);
				if (!Assets.Unload("UI/" + name))
				{
					EB.Debug.Log("COULD NOT FIND OVERLAYS SCREEN ASSET TO REMOVE!?!?" + name);
				}
			}
		}
	}

	public GameObject AddPersistentScreen(string name)
	{
		GameObject gameObject = FindPopup(name);
		if (!_persistent.ContainsKey(name))
		{
			_persistent[name] = gameObject;
			gameObject.BroadcastMessage("OnOpenScreen", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			EB.Debug.Log("SCREEN ALREADY OPEN ( PERSISTENT ) " + name);
		}
		return gameObject;
	}

	public void RmvPersistentScreen(string name)
	{
		EB.Debug.Log("REMOVING PERSISTENT SCREN " + name);
		EB.Debug.Log("PERSISTENT COUNT = " + _persistent.Count);
		foreach (string key in _persistent.Keys)
		{
			EB.Debug.Log("KEYS = " + key);
			if (key == name)
			{
				RemovePersistentAsset(key);
				break;
			}
		}
	}

	public void RemovePersistentAsset(string s)
	{
		GameObject gameObject = _persistent[s];
		gameObject.transform.parent = null;
		Object.Destroy(gameObject);
		_persistent.Remove(s);
		if (!Assets.Unload("UI/" + s))
		{
			EB.Debug.Log("COULD NOT FIND PERSISTENT SCREEN ASSET TO REMOVE!?!?" + s);
		}
	}

	public void ClearPersistentScreens()
	{
		List<string> list = new List<string>();
		foreach (string key in _persistent.Keys)
		{
			list.Add(key);
		}
		for (int i = 0; i < list.Count; i++)
		{
			RemovePersistentAsset(list[i]);
		}
		_persistent.Clear();
	}

	public GameObject GetPersistentScreen(string name)
	{
		GameObject value = null;
		if (_persistent.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public GameObject GetOverlayScreen(string name)
	{
		GameObject value = null;
		if (_overlays.TryGetValue(name, out value))
		{
			return value;
		}
		return null;
	}

	public void Confirmation(bool showClose, string title, string desc, string buttonLeft, string buttonRight, Action<ConfirmationDialogUI.ButtonIds> callback = null, bool showTransition = true)
	{
		GameObject obj = ((!showTransition) ? OpenPopup("ConfirmationDialogTransitionlessUI", true, true) : OpenPopup("ConfirmationDialogUI", true, true));
		ConfirmationDialogUI confirmationDialogUI = Util.FindComponent<ConfirmationDialogUI>(obj);
		confirmationDialogUI.SetParameters(showClose, desc, title, buttonLeft, buttonRight);
		confirmationDialogUI.callback = callback;
	}

	public void BlockingUI(string title, string text)
	{
		BlockingUI component = OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText(title, text);
	}

	private GameObject AddPopup(GameObject prefab)
	{
		if (prefab == null)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(prefab) as GameObject;
		gameObject.transform.parent = uiPanel.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		gameObject.name = prefab.name;
		Object.DontDestroyOnLoad(gameObject);
		return gameObject;
	}

	public bool CanOpenQueuePopup()
	{
		if (_popups.Count > 0)
		{
			return false;
		}
		return true;
	}

	public PopupInfo QueuePopup(string name)
	{
		PopupInfo popupInfo = new PopupInfo();
		popupInfo.name = name;
		_queuedPopups.Enqueue(popupInfo);
		return popupInfo;
	}

	public GameObject FindPopup(string name)
	{
		if (uiPanel != null)
		{
			GameObject gameObject = Util.GetObjectExactMatch(uiPanel, name);
			if (gameObject == null)
			{
				GameObject prefab = Assets.Load("UI/" + name) as GameObject;
				gameObject = AddPopup(prefab);
			}
			return gameObject;
		}
		return null;
	}

	public bool RemovePopup(string name)
	{
		for (int i = 0; i < _popups.Count; i++)
		{
			if (name == _popups[i].name)
			{
				DestroyPopup(_popups[i]);
				_popups.RemoveAt(i);
				return true;
			}
		}
		return false;
	}

	public void ShowError(string text, Action callback)
	{
		GameObject gameObject = OpenPopup("ErrorUI", true, true);
		if (gameObject != null)
		{
			if (LevelData.Instance != null && LevelData.Instance.PauseGame)
			{
				LevelData.Instance.PauseGame = false;
			}
			gameObject.GetComponent<ErrorUI>().ShowError(text, callback);
			Instance.FadeOut(null);
			ForceHideLoadingScreen();
		}
	}

	public GameObject OpenPopup(string name, bool showPrevious = false, bool showFader = false, bool showPreviousFader = false, bool callOpenPopup = true)
	{
		EB.Debug.Log("OPEN POPUP " + name);
		GameObject gameObject = FindPopup(name);
		if (gameObject != null)
		{
			if (!showPrevious && _popups.Count > 0)
			{
				GameObject popupObj = _popups.Peek().popupObj;
				if (popupObj != null)
				{
					popupObj.BroadcastMessage("OnHover", false, SendMessageOptions.DontRequireReceiver);
					if (popupObj.active)
					{
						Util.SetLayerRecursive(popupObj, 0, LayerMask.NameToLayer("GUI"));
					}
				}
			}
			Vector3 localPosition = gameObject.transform.localPosition;
			localPosition.z = -50f * (float)_popups.Count;
			gameObject.transform.localPosition = localPosition;
			PopupInfo popupInfo = new PopupInfo();
			popupInfo.name = name;
			popupInfo.popupObj = gameObject;
			popupInfo.fader = showFader;
			popupInfo.showPrevious = showPrevious;
			popupInfo.faderPrevious = showPreviousFader;
			_popups.Push(popupInfo);
			PositionBlocker();
			if (showFader)
			{
				EnableBgFade(true);
			}
			Camera uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			UIAnchor[] componentsInChildren = gameObject.GetComponentsInChildren<UIAnchor>();
			UIAnchor[] array = componentsInChildren;
			foreach (UIAnchor uIAnchor in array)
			{
				uIAnchor.uiCamera = uiCamera;
			}
			UIStretch[] componentsInChildren2 = gameObject.GetComponentsInChildren<UIStretch>();
			UIStretch[] array2 = componentsInChildren2;
			foreach (UIStretch uIStretch in array2)
			{
				uIStretch.uiCamera = uiCamera;
			}
			if (callOpenPopup)
			{
				gameObject.BroadcastMessage("OnOpenPopup", SendMessageOptions.DontRequireReceiver);
			}
			gameObject.BroadcastMessage("OnFocusPopup", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			EB.Debug.LogError("Failed to open popup " + name);
		}
		return gameObject;
	}

	public bool GetTransitionBlockerState()
	{
		return _transitionblocker.collider.enabled;
	}

	public void EnableTransitionBlocker(bool bEnable)
	{
		_transitionblocker.collider.enabled = bEnable;
	}

	private void PositionBlocker()
	{
		if (_popups.Count > 0 && _popups.Peek().fader)
		{
			float num = -50f * (float)(_popups.Count - 1);
			_blocker.transform.localPosition = new Vector3(0f, 0f, num + 5f);
		}
		else if (_popups.Count > 1 && _popups.Peek().faderPrevious)
		{
			float num2 = -50f * (float)(_popups.Count - 2);
			_blocker.transform.localPosition = new Vector3(0f, 0f, num2 + 5f);
		}
	}

	private void PositionPersistentScreens()
	{
		foreach (GameObject value in _persistent.Values)
		{
			if (HasActiveCollider(value))
			{
				value.transform.localPosition = new Vector3(value.transform.localPosition.x, value.transform.localPosition.y, _blocker.transform.localPosition.z - 5f);
			}
		}
	}

	private bool HasActiveCollider(GameObject gameObject)
	{
		Collider[] componentsInChildren = gameObject.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			if (collider.enabled)
			{
				return true;
			}
		}
		return false;
	}

	public void OpenWebView(string uri, Hashtable query)
	{
		if (Hub.Instance.State == HubState.Connected)
		{
			query["stoken"] = Hub.Instance.ApiEndPoint.GetData("stoken");
			string text = Hub.Instance.ApiEndPoint.Url + uri + "?" + QueryString.Stringify(query);
			text = text.Replace("https://", "http://");
			Application.OpenURL(text);
		}
	}

	private void DestroyPopup(PopupInfo info)
	{
		GameObject popupObj = info.popupObj;
		if (popupObj != null)
		{
			popupObj.BroadcastMessage("OnClosePopup", SendMessageOptions.DontRequireReceiver);
			popupObj.transform.parent = null;
			Object.Destroy(popupObj);
		}
		if (GetPopup(info.name) == null)
		{
			Assets.Unload("UI/" + info.name);
		}
	}

	public void ClosePopup()
	{
		if (_popups.Count > 0)
		{
			DestroyPopup(_popups.Pop());
		}
		if (_popups.Count > 0)
		{
			PopupInfo popupInfo = _popups.Peek();
			if (popupInfo.popupObj.layer == 0)
			{
				_popups.Pop();
				Util.SetLayerRecursive(popupInfo.popupObj, base.gameObject.layer, 0);
				OpenPopup(popupInfo.name, popupInfo.showPrevious, popupInfo.fader);
			}
			else
			{
				popupInfo.popupObj.BroadcastMessage("OnFocusPopup", SendMessageOptions.DontRequireReceiver);
			}
		}
		PositionBlocker();
		if (_popups.Count > 0)
		{
			EnableBgFade(_popups.Peek().fader);
		}
	}

	public void PopTo(string name)
	{
		if (GetPopup(name) == null)
		{
			CloseAllPopups();
			OpenPopup(name);
		}
		else
		{
			while (_popups.Count > 0 && !(name == _popups.Peek().name))
			{
				ClosePopup();
			}
		}
	}

	public void CloseAllPopups()
	{
		while (_popups.Count > 0)
		{
			DestroyPopup(_popups.Pop());
		}
		EnableBgFade(false);
		PositionBlocker();
	}

	public GameObject GetPopup(string name)
	{
		for (int i = 0; i < _popups.Count; i++)
		{
			if (_popups[i].name == name)
			{
				return _popups[i].popupObj;
			}
		}
		return null;
	}

	public void ClosePopup(string name)
	{
		if (GetPopup(name) == null)
		{
			return;
		}
		while (_popups.Count > 0)
		{
			PopupInfo popupInfo = _popups.Peek();
			ClosePopup();
			if (popupInfo.name == name && _popups.Count > 0)
			{
				PopupInfo popupInfo2 = _popups.Peek();
				if ((bool)popupInfo2.popupObj && popupInfo2.popupObj.layer == 0)
				{
					Util.SetLayerRecursive(popupInfo2.popupObj, base.gameObject.layer);
					OpenPopup(popupInfo2.name, popupInfo2.showPrevious, popupInfo2.fader);
				}
				break;
			}
		}
	}

	private void Update()
	{
		if (_queuedPopups.Count > 0 && CanOpenQueuePopup())
		{
			PopupInfo popupInfo = _queuedPopups.Dequeue();
			OpenPopup(popupInfo.name);
		}
		ProcessInput();
	}

	private void OnDrawGizmos()
	{
		int manualHeight = GetComponent<UIRoot>().manualHeight;
		float x = (float)manualHeight * 4f / 3f;
		Gizmos.DrawWireCube(base.transform.position, new Vector3(x, manualHeight, 0.1f));
	}

	public GameObject ShowLoadingScreen(string loadScreen = null)
	{
		string text = "LoadingScreenUI";
		if (!string.IsNullOrEmpty(loadScreen))
		{
			text = loadScreen;
		}
		_loadingScreenRef++;
		if (_loadingScreen == null)
		{
			_loadingScreen = FindPopup(text);
		}
		return _loadingScreen;
	}

	public void ForceHideLoadingScreen()
	{
		while (_loadingScreenRef > 0)
		{
			HideLoadingScreen();
		}
	}

	public void HideLoadingScreen()
	{
		_loadingScreenRef--;
		if (_loadingScreenRef > 0)
		{
			return;
		}
		Object.Destroy(_loadingScreen);
		_loadingScreen = null;
		_loadingScreenRef = 0;
		if (_popups.Count > 0)
		{
			PopupInfo popupInfo = _popups.Peek();
			if (popupInfo != null)
			{
				popupInfo.popupObj.BroadcastMessage("OnLoadingScreenClosed", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void DisplayPopupStack()
	{
		string text = string.Format("[{0}] PopupStack changed to:\n", UnityEngine.Time.frameCount);
		for (int i = 0; i < _popups.Count; i++)
		{
			text += string.Format("[{0,2}] ", i);
			text = text + _popups[i].ToString() + "\n";
		}
		UnityEngine.Debug.Log(text);
	}

	private void ProcessInput()
	{
		if (!Input.GetKeyDown(KeyCode.Escape))
		{
			return;
		}
		if (!_OSBackButtonEnabled && Profile.Instance != null && Hub.Instance != null)
		{
			_OSBackButtonEnabled = _TitleScreenCleared && (Profile.Instance.IsTutorialDone("mp") || Hub.Instance.State == HubState.Idle);
		}
		if (!_OSBackButtonEnabled)
		{
			return;
		}
		bool flag = false;
		if (!_transitionblocker.collider.enabled && _popups.Count > 0)
		{
			PopupInfo popupInfo = _popups.Peek();
			if (popupInfo != null)
			{
				popupInfo.popupObj.BroadcastMessage("OnBack", base.gameObject, SendMessageOptions.DontRequireReceiver);
				flag = true;
			}
		}
		if (!flag && GameHUD.Instance != null)
		{
			GameHUD.Instance.gameObject.BroadcastMessage("OnBack", base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetOSBackButtonEnabled()
	{
		_OSBackButtonEnabled = true;
	}

	public void SetTitleScreenCleared()
	{
		_TitleScreenCleared = true;
	}
}
