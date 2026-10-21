using System;
using EB;
using UnityEngine;

public class BoostSelectHudButton : MonoBehaviour
{
	private bool isDragging;

	private bool _hasActiveBoost = true;

	private bool _enabled;

	private bool isInit;

	private Player _myPlayer;

	private UISprite[] _childUiSprites;

	private bool HasActiveBoost
	{
		set
		{
			if (value != _hasActiveBoost)
			{
				_hasActiveBoost = value;
			}
		}
	}

	private bool Enabled
	{
		get
		{
			return _enabled;
		}
		set
		{
			if (isInit && value != _enabled)
			{
				base.collider.enabled = value;
				float alpha = ((!value) ? 0.25f : 1f);
				UISprite[] childUiSprites = _childUiSprites;
				foreach (UISprite uISprite in childUiSprites)
				{
					uISprite.alpha = alpha;
				}
				_enabled = value;
			}
		}
	}

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(HandleClick));
	}

	private void OnPlayerReady()
	{
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
		if (!isInit && (mode == GameAttributes.ModeType.MP || championshipMode == GameAttributes.ChampionshipType.kOn))
		{
			Init();
			return;
		}
		Enabled = true;
		Enabled = false;
	}

	private void Init()
	{
		GameObject playerObject = LevelData.Instance.GetPlayerObject();
		if (playerObject != null)
		{
			_myPlayer = playerObject.GetComponent<Player>();
		}
		_childUiSprites = Util.FindAllComponents<UISprite>(base.gameObject);
		isInit = true;
	}

	private void HandleClick(GameObject caller)
	{
		bool flag = false;
		CameraController component = Camera.main.GetComponent<CameraController>();
		if (component != null)
		{
			flag = component.IntroRunning;
		}
		if (!isDragging && !ScreenManager.Instance.PopupOpen && !flag && Enabled && !_myPlayer.CurrentMove.Moving)
		{
			GameHUD.Instance.ToggleSubscreen(HUDSubscreenManager.Subscreen.BoostInventory);
		}
	}

	private void Update()
	{
		if ((bool)_myPlayer)
		{
			Enabled = true;
			HasActiveBoost = _myPlayer.HasAnActiveBoost();
			if (BlastronGamepad.Instance.SwitchBoostPressed())
			{
				HandleClick(base.gameObject);
			}
		}
		else
		{
			Enabled = false;
		}
	}
}
