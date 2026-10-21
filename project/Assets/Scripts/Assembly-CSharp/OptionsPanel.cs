using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Collections;
using UnityEngine;

public class OptionsPanel : MonoBehaviour
{
	private const float FadeSpeed = 2f;

	private string[] overlapScreens = new string[2] { "TutorialDialogUI", "StoryPopupUI" };

	public GameHUD hud;

	public UISprite optionsIcon;

	public GameObject pauseBlocker;

	public Transform pauseScreenDepth;

	private List<string> _overlapScreens;

	private bool _visible;

	private bool _isMultiplayer;

	private UIPanelAlpha _uiPanelAlpha;

	private UIPanelAlpha _pauseBlockerPanelAlpha;

	private StaticPanelController _staticPanelController;

	private GameObject _audioCross;

	private float _originalPauseScreenDepth;

	protected bool soundEnabled
	{
		get
		{
			return Options.sound != 0;
		}
		set
		{
			Options.sound = (value ? 1 : 0);
		}
	}

	protected bool pauseEnabled
	{
		get
		{
			return LevelData.Instance.PauseGame;
		}
		set
		{
			LevelData.Instance.PauseGame = value;
		}
	}

	private void Awake()
	{
		_overlapScreens = new List<string>(ArrayUtils.ToList<string>(overlapScreens));
		_originalPauseScreenDepth = pauseScreenDepth.localPosition.z;
		_staticPanelController = Util.FindComponentUpwards<StaticPanelController>(base.gameObject);
		_uiPanelAlpha = GetComponent<UIPanelAlpha>();
		_uiPanelAlpha.alpha = 0f;
		_uiPanelAlpha.mLevel = 1;
		_pauseBlockerPanelAlpha = Util.FindComponent<UIPanelAlpha>(pauseBlocker);
		_pauseBlockerPanelAlpha.alpha = 0f;
		_pauseBlockerPanelAlpha.mLevel = 1;
		_audioCross = Util.GetObjectExactMatch(base.gameObject, "Cross");
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		_isMultiplayer = mode == GameAttributes.ModeType.MP;
		UpdateCross();
		UpdatePause();
	}

	private void Update()
	{
		if (!_isMultiplayer)
		{
			if (_visible != pauseEnabled)
			{
				_visible = pauseEnabled;
				StopAllCoroutines();
				StartCoroutine((!_visible) ? "FadeOut" : "FadeIn");
				UpdatePause();
			}
			UpdatePauseBlockerDepth();
		}
	}

	public void OnLeaveGame()
	{
		hud.OnLeaveGame();
	}

	private void UpdateCross()
	{
		if (_audioCross != null)
		{
			NGUITools.SetActive(_audioCross, !soundEnabled);
		}
	}

	private void UpdatePause()
	{
		if (_isMultiplayer)
		{
			pauseBlocker.SetActive(false);
			return;
		}
		string text = ((!pauseEnabled) ? "hud_pause_icon" : "hud_play_icon");
		if (optionsIcon != null && optionsIcon.spriteName != text)
		{
			optionsIcon.spriteName = text;
			optionsIcon.MakePixelPerfect();
			optionsIcon.transform.localPosition = Vector3.zero;
		}
		if (_staticPanelController != null)
		{
			_staticPanelController.ToggleStaticWidgets();
		}
	}

	private void onToggleAudio()
	{
		soundEnabled = !soundEnabled;
		UpdateCross();
	}

	private void OnToggle()
	{
		if (GameHUD.Instance.Visible)
		{
			if (!_isMultiplayer)
			{
				pauseEnabled = !pauseEnabled;
				UpdatePause();
			}
			_visible = !_visible;
			StopAllCoroutines();
			StartCoroutine((!_visible) ? "FadeOut" : "FadeIn");
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!_isMultiplayer && pause && !_visible)
		{
			OnToggle();
		}
	}

	private void OnBack(GameObject go)
	{
		if (_isMultiplayer)
		{
			OnLeaveGame();
		}
		else
		{
			OnToggle();
		}
	}

	private IEnumerator FadeIn()
	{
		if (!_isMultiplayer)
		{
			pauseBlocker.SetActive(true);
		}
		float lastFrameStart = UnityEngine.Time.realtimeSinceStartup;
		while (_uiPanelAlpha.alpha < 1f)
		{
			float now = UnityEngine.Time.realtimeSinceStartup;
			float delta = (now - lastFrameStart) * 2f;
			lastFrameStart = now;
			_uiPanelAlpha.alpha += delta;
			_pauseBlockerPanelAlpha.alpha += delta;
			yield return 1;
		}
		_uiPanelAlpha.alpha = 1f;
		_pauseBlockerPanelAlpha.alpha = 1f;
	}

	private IEnumerator FadeOut()
	{
		float lastFrameStart = UnityEngine.Time.realtimeSinceStartup;
		while (_uiPanelAlpha.alpha > 0f)
		{
			float now = UnityEngine.Time.realtimeSinceStartup;
			float delta = (now - lastFrameStart) * 2f;
			lastFrameStart = now;
			_uiPanelAlpha.alpha -= delta;
			_pauseBlockerPanelAlpha.alpha -= delta;
			yield return 1;
		}
		_uiPanelAlpha.alpha = 0f;
		_pauseBlockerPanelAlpha.alpha = 0f;
		if (!_isMultiplayer)
		{
			pauseBlocker.SetActive(false);
		}
	}

	private void UpdatePauseBlockerDepth()
	{
		EB.Collections.Stack<ScreenManager.PopupInfo> popupStack = ScreenManager.Instance.GetPopupStack();
		float num = _originalPauseScreenDepth;
		foreach (ScreenManager.PopupInfo item in popupStack)
		{
			if (_overlapScreens.Contains(item.name))
			{
				num -= 50f;
			}
		}
		if (!Mathf.Approximately(num, pauseScreenDepth.localPosition.z))
		{
			pauseScreenDepth.localPosition = new Vector3(0f, 0f, num);
		}
	}
}
