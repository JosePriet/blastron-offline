using System;
using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class PrivateGameUI : PopupScript
{
	public Color disabledButtonTint;

	public Hashtable Attributes = new Hashtable();

	private string gameName = string.Empty;

	private Color defaultButtonTint;

	private UISlicedSprite buttonBackground;

	private bool buttonEnabled = true;

	protected override void OnStart()
	{
		base.OnStart();
		Attributes = Misc.CreateDefaultAttributes();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonPlay");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnJoin;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Background");
		buttonBackground = Util.FindComponent<UISlicedSprite>(objectExactMatch2);
		defaultButtonTint = buttonBackground.color;
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
		UIInput uIInput = Util.FindComponent<UIInput>(objectExactMatch3);
		uIInput.onSubmit = (UIInput.OnSubmit)Delegate.Combine(uIInput.onSubmit, new UIInput.OnSubmit(OnSubmitName));
		GameHUD._settings._eVisFlags = GameHUD.Settings.Visibility.kChat | GameHUD.Settings.Visibility.kBoosts;
		UpdatePlayButton();
	}

	private string SafeName(string src)
	{
		char[] array = "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();
		src = src.ToLower();
		int num = 0;
		while (num < src.Length)
		{
			if (Array.IndexOf(array, src[num]) < 0)
			{
				src = src.Remove(num, 1);
			}
			else
			{
				num++;
			}
		}
		return src;
	}

	private void OnJoin(GameObject caller)
	{
		if (gameName.Length < 3)
		{
			ScreenManager.Instance.Confirmation(false, "ID_UI_LOGIN_ERROR_TITLE", "ID_UI_PRIVATEGAME_ERROR_NAMESHORT", "ID_UI_COMMON_OK", string.Empty);
			return;
		}
		GameManager gm = Hub.Instance.GameManager;
		GameManager.CreateGameParams pars = default(GameManager.CreateGameParams);
		pars.MaxPlayers = 4;
		pars.PlayerAttributes = Misc.CreatePlayerAttributes();
		pars.Attributes = Attributes;
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_ONLINE_SEARCHING_TITLE", "ID_UI_ONLINE_SEARCHING_DESC");
		Coroutines.SetTimeout(() =>
		{
			gm.CreateOrJoinPrivateGame(gameName, pars);
		}, 500);
	}

	private void OnSubmitName(string text)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Input_Name");
		gameName = SafeName(Util.FindComponent<UIInput>(objectExactMatch).text);
		Util.FindComponent<UIInput>(objectExactMatch).text = gameName;
		UpdatePlayButton();
	}

	private void UpdatePlayButton()
	{
		bool flag = gameName.Length >= 1;
		if (flag != buttonEnabled)
		{
			buttonEnabled = flag;
			buttonBackground.color = ((!buttonEnabled) ? disabledButtonTint : defaultButtonTint);
			buttonBackground.collider.enabled = buttonEnabled;
		}
	}
}
