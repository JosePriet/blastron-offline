using System.Collections.Generic;
using EB;
using EB.Sparx;
using SmoothMoves;
using UnityEngine;

public class GetMoreEnergyUI : PopupScript
{
	private int _energyToAdd;

	public CharacterPrefab _mickE;

	public CacDisplay _display;

	public UILabel _numOfTickets;

	private BoneAnimation _smAnim;

	private bool sendClose = true;

	private void Start()
	{
		_display.Recipe = _mickE.ToRecipe();
		_display.SpawnPlayer(() =>
		{
			_smAnim = _display.gameObject.GetComponentInChildren<BoneAnimation>();
		});
		_numOfTickets.text = Profile.Instance.Energy.ToString();
		GameObject gameObject = null;
		GameObject gameObject2 = null;
		Util.GetObjectExactMatch(base.gameObject, "OnIncreasedEnergy").SetActive(false);
		Util.GetObjectExactMatch(base.gameObject, "OnEnergyDefault").SetActive(false);
		Util.GetObjectExactMatch(base.gameObject, "OnFreeRefill").SetActive(false);
		ButtonScript buttonScript;
		if (Profile.Instance.EnergyMax == Tuning.Instance.EnergyUpgradedMax)
		{
			gameObject = Util.GetObjectExactMatch(base.gameObject, "OnIncreasedEnergy");
			gameObject.SetActive(true);
		}
		else if (!Profile.Instance.HasRefilledEnergyBefore)
		{
			gameObject = Util.GetObjectExactMatch(base.gameObject, "OnFreeRefill");
			gameObject.SetActive(true);
		}
		else
		{
			gameObject = Util.GetObjectExactMatch(base.gameObject, "OnEnergyDefault");
			gameObject.SetActive(true);
			gameObject2 = Util.GetObjectExactMatch(gameObject, "IncreaseLimitButton");
			buttonScript = Util.FindComponent<ButtonScript>(gameObject2);
			Util.FindComponent<UILabel>(Util.GetObjectExactMatch(gameObject2, "Label")).text = Tuning.Instance.EnergyUpgradeCost.ToString();
			buttonScript.onButtonClick += OnIncreaseLimit;
			Util.FindComponent<UILabel>(Util.GetObjectExactMatch(base.gameObject, "LabelEnergyLimit")).text = Localizer.Format("ID_UI_ENERGY_INCREASE_LIMIT", Tuning.Instance.EnergyUpgradedMax.ToString());
		}
		gameObject2 = Util.GetObjectExactMatch(gameObject, "BuyEnergyButton");
		if (Profile.Instance.HasRefilledEnergyBefore)
		{
			Util.FindComponent<UILabel>(Util.GetObjectExactMatch(gameObject2, "Label")).text = Tuning.Instance.EnergyRefillCost.ToString();
		}
		buttonScript = Util.FindComponent<ButtonScript>(gameObject2);
		buttonScript.onButtonClick += OnBuy;
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "MultiplayerButton");
		objectExactMatch.SetActive(true);
		objectExactMatch.GetComponent<ButtonScript>().onButtonClick += OnMultiplayer;
		SinglePlayerUI.UserInputToggle();
		base.OnStart();
	}

	private void OnBuy(GameObject caller)
	{
		OpenBlockingUI();
		int value = (Profile.Instance.HasRefilledEnergyBefore ? Tuning.Instance.EnergyRefillCost : 0);
		WalletListener.Instance.Debit(value, "Refill Energy", SafeAction.Wrap(this, (bool result) =>
		{
			if (result)
			{
				sendClose = false;
				Telemetry.Event("energy_screen/refill");
				Profile.Instance.FullyRestoreEnergy();
				base.onClosed += HandleonRefill;
				OnClose();
				if (!Profile.Instance.HasRefilledEnergyBefore)
				{
					Profile.Instance.HasRefilledEnergyBefore = true;
					Profile.Instance.Save();
				}
			}
			else
			{
				Telemetry.Event("energy_screen/refill_insufficent");
				InsufficientCurrency(CurrencyType.Hard, Tuning.Instance.EnergyRefillCost);
			}
		}));
	}

	private void OnMultiplayer(GameObject caller)
	{
		List<string> list = new List<string>();
		list.Add("HomeUI");
		list.Add("MatchmakingGameModeSelectUI");
		Setup.Instance.nextScreenStack = list;
		Game game = Hub.Instance.GameManager.Game;
		Telemetry.Event("energy_screen/mutiplayer");
		if (game != null)
		{
			game.End();
			Hub.Instance.GameManager.LeaveGame();
		}
		else
		{
			ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
			Setup.Instance.ReturnToFrontendScreen();
		}
	}

	private void HandleonRefill(PopupScript ps)
	{
		ScreenManager.Instance.OpenPopup("EnergyFullUI", true, true);
	}

	private void OnIncreaseLimit(GameObject caller)
	{
		OpenBlockingUI();
		WalletListener.Instance.Debit(Tuning.Instance.EnergyUpgradeCost, "Increase Energy Limit", SafeAction.Wrap(this, (bool result) =>
		{
			if (result)
			{
				sendClose = false;
				Telemetry.Event("energy_screen/increase_limit");
				Profile.Instance.EnergyMax = Tuning.Instance.EnergyUpgradedMax;
				Profile.Instance.FullyRestoreEnergy();
				base.onClosed += HandleonIncreased;
				OnClose();
			}
			else
			{
				Telemetry.Event("energy_screen/increase_limit_insufficent");
				InsufficientCurrency(CurrencyType.Hard, Tuning.Instance.EnergyUpgradeCost);
			}
		}));
	}

	private void HandleonIncreased(PopupScript ps)
	{
		ScreenManager.Instance.OpenPopup("EnergyLimitIncreaseUI", true, true);
	}

	private void InsufficientCurrency(CurrencyType currencyType, int cost)
	{
		ScreenManager.Instance.ClosePopup("BlockingUI");
		GameObject gameObject = ScreenManager.Instance.OpenPopup("CurrencyInsufficientUI", true, true);
		if (gameObject != null)
		{
			CurrencyInsufficientUI currencyInsufficientUI = Util.FindComponent<CurrencyInsufficientUI>(gameObject);
			currencyInsufficientUI.SetInfo(currencyType, cost);
		}
	}

	private void OpenBlockingUI()
	{
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
	}

	protected override void OnClose()
	{
		SinglePlayerUI.UserInputToggle();
		if (sendClose)
		{
			Telemetry.Event("energy_screen/close");
		}
		ScreenManager.Instance.ClosePopup("BlockingUI");
		base.OnClose();
	}

	private void Update()
	{
		if (_smAnim != null && !_smAnim.IsPlaying("bot_idle_mick-E"))
		{
			_smAnim.Play("bot_idle_mick-E");
		}
	}
}
