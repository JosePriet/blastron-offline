using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class MyStuffUI : MonoBehaviour
{
	private bool _transitioning = true;

	public void OnOpenPopup()
	{
		UIUtils.SetLabelContents(Util.GetObjectExactMatch(base.gameObject, "SPCount"), AttributesManager.instance.currentSkillPoints.ToString());
		ScreenManager.Instance.FadeOut(null);
		_transitioning = true;
		EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, () =>
		{
			_transitioning = false;
			EB.Sequence.Component.Activate(base.gameObject, "MyStuffUI", typeof(SequenceEvent_OnScreenOpened));
		});
	}

	private void Start()
	{
		Util.GetObjectExactMatch(base.gameObject, "BackButton").GetComponent<ButtonScript>().onButtonClick += OnBack;
		Util.GetObjectExactMatch(base.gameObject, "BoostButton").GetComponent<MenuButtonScript>().onButtonClick += OnBoosts;
		Util.GetObjectExactMatch(base.gameObject, "WeaponButton").GetComponent<MenuButtonScript>().onButtonClick += OnWeapons;
		Util.GetObjectExactMatch(base.gameObject, "MyStatsButton").GetComponent<MenuButtonScript>().onButtonClick += OnMyStats;
		Util.GetObjectExactMatch(base.gameObject, "PartsButton").GetComponent<MenuButtonScript>().onButtonClick += OnParts;
	}

	private void OnWeapons(GameObject g)
	{
		if (!_transitioning)
		{
			_transitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToWeapons);
		}
	}

	public void OnExitToWeapons()
	{
		GameObject obj = ScreenManager.Instance.OpenPopup("StoreWeaponUI");
		StoreWeaponUI storeWeaponUI = Util.FindComponent<StoreWeaponUI>(obj);
		storeWeaponUI.startInStore = false;
	}

	private void OnMembership(GameObject g)
	{
		EB.Debug.Log("OnMembership is not yet implemented", this);
	}

	private void OnBoosts(GameObject g)
	{
		if (!_transitioning)
		{
			_transitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToBoost);
		}
	}

	public void OnExitToBoost()
	{
		ScreenManager.Instance.OpenPopup("StoreBoostUI");
	}

	public void OnExitToPrevious()
	{
		ScreenManager.Instance.ClosePopup();
	}

	private void OnExitToCoreUpgrade()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.OpenPopup("CoreUpgradeUI");
		});
	}

	private void OnBack(GameObject g)
	{
		if (!_transitioning)
		{
			_transitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToPrevious);
		}
	}

	private void OnMyStats(GameObject g)
	{
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToCoreUpgrade);
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
		if (RequireLogin() && !_transitioning)
		{
			_transitioning = true;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnExitToParts);
		}
	}

	public void OnExitToParts()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			GameObject gameObject = ScreenManager.Instance.OpenPopup("MyRobotUI");
			gameObject.GetComponent<MyRobotUI>().showOwned = true;
		});
	}
}
