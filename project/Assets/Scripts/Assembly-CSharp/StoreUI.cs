using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class StoreUI : MonoBehaviour
{
	private bool _enableInput = true;

	private string _exitDestinationScreen;

	private bool _isTransitioning;

	public void OnOpenPopup()
	{
		Util.GetObjectExactMatch(base.gameObject, "BackButton").GetComponent<ButtonScript>().onButtonClick += OnBack;
		Util.GetObjectExactMatch(base.gameObject, "LootinatorButton").GetComponent<MenuButtonScript>().onButtonClick += OnLootinator;
		Util.GetObjectExactMatch(base.gameObject, "WeaponButton").GetComponent<MenuButtonScript>().onButtonClick += OnWeapons;
		Util.GetObjectExactMatch(base.gameObject, "CurrencyButton").GetComponent<MenuButtonScript>().onButtonClick += OnCurrency;
		Util.GetObjectExactMatch(base.gameObject, "PartsButton").GetComponent<MenuButtonScript>().onButtonClick += OnParts;
		_isTransitioning = true;
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
		ScreenManager.Instance.FadeOut(null);
	}

	private void OnTransitionInComplete()
	{
		if (_isTransitioning && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts_storeintro") && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts_myrobotintro") && !Profile.Instance.IsTutorialDone("sequence_tutorial_parts"))
		{
			Object original = Assets.Load("Prefab_Sequences/sequence_tutorial_parts_storeintro");
			Object.Instantiate(original);
		}
		else
		{
			EB.Sequence.Component.Activate(base.gameObject, "StoreUI", typeof(SequenceEvent_OnScreenOpened));
		}
		_isTransitioning = false;
	}

	private void OnWeapons(GameObject g)
	{
		if (_enableInput && !_isTransitioning)
		{
			_isTransitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToWeapons);
		}
	}

	public void OnExitToWeapons()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreWeaponUI");
		StoreWeaponUI storeWeaponUI = Util.FindComponent<StoreWeaponUI>(obj);
		storeWeaponUI.startInStore = true;
	}

	private void OnMembership(GameObject g)
	{
		if (_enableInput)
		{
			EB.Debug.Log("OnMembership is not yet implemented", this);
		}
	}

	private void OnLootinator(GameObject g)
	{
		if (_enableInput && !_isTransitioning)
		{
			_isTransitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToLootinator);
		}
	}

	public void OnExitToLootinator()
	{
		ScreenManager.Instance.OpenPopup("GachaUI");
	}

	public void OnExitToPrevious()
	{
		ScreenManager.Instance.ClosePopup();
		if (_exitDestinationScreen == null)
		{
			return;
		}
		if (_exitDestinationScreen == "SinglePlayerUI")
		{
			ScreenManager.Instance.FadeIn(() =>
			{
				ScreenManager.Instance.OpenPopup(_exitDestinationScreen);
			});
		}
		else
		{
			ScreenManager.Instance.OpenPopup(_exitDestinationScreen);
		}
	}

	public void SetReturnScreen(string returnScreen)
	{
		_exitDestinationScreen = returnScreen;
	}

	private void OnBack(GameObject g)
	{
		if (_enableInput && !_isTransitioning)
		{
			_isTransitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToPrevious);
		}
	}

	private void OnCurrency(GameObject g)
	{
		if (_enableInput && !_isTransitioning && RequireLogin())
		{
			_isTransitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToCurrency);
		}
	}

	public void OnExitToCurrency()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreCurrencyUI", false, true);
		StoreCurrencyUI storeCurrencyUI = Util.FindComponent<StoreCurrencyUI>(obj);
		storeCurrencyUI.currencyType = CurrencyType.Hard;
	}

	private bool RequireLogin()
	{
		switch (Hub.Instance.State)
		{
		case HubState.Connected:
			return true;
		case HubState.Idle:
			Setup.Instance.Login(false);
			break;
		}
		return false;
	}

	private void OnParts(GameObject g)
	{
		if (_enableInput && !_isTransitioning && RequireLogin())
		{
			_isTransitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToParts);
		}
	}

	public void OnExitToParts()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			GameObject gameObject = ScreenManager.Instance.OpenPopup("MyRobotUI");
			gameObject.GetComponent<MyRobotUI>().showOwned = false;
		});
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void OnFrontEndTutorial()
	{
		if (RequireLogin())
		{
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToParts);
		}
	}
}
