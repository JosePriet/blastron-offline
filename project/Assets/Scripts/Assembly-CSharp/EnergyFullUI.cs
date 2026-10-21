using EB;
using SmoothMoves;
using UnityEngine;

public class EnergyFullUI : PopupScript
{
	public CharacterPrefab _mickE;

	public CacDisplay _display;

	private BoneAnimation _smAnim;

	private void Start()
	{
		_display.Recipe = _mickE.ToRecipe();
		_display.SpawnPlayer(() =>
		{
			_smAnim = _display.gameObject.GetComponentInChildren<BoneAnimation>();
		});
		if (Profile.Instance.EnergyMax == Tuning.Instance.EnergyUpgradedMax)
		{
			Util.GetObjectExactMatch(base.gameObject, "Bottom").SetActive(false);
			MoveObjectsToCenter();
		}
		else
		{
			Util.FindComponent<ButtonScript>(Util.GetObjectExactMatch(base.gameObject, "IncreaseLimitButton")).onButtonClick += IncreaseEnergyLimit;
			Util.FindComponent<UILabel>(Util.GetObjectExactMatch(base.gameObject, "LabelBottom")).text = Localizer.Format("ID_UI_ENERGY_INCREASE_LIMIT", Tuning.Instance.EnergyUpgradedMax.ToString());
		}
		SinglePlayerUI.UserInputToggle();
		base.OnStart();
	}

	private void MoveObjectsToCenter()
	{
		Util.GetObjectExactMatch(base.gameObject, "divider").SetActive(false);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "LabelTop");
		Vector3 localPosition = objectExactMatch.transform.localPosition;
		localPosition.y = 0f;
		objectExactMatch.transform.localPosition = localPosition;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CreateACharacterInstance");
		localPosition = objectExactMatch.transform.localPosition;
		localPosition.y = 0f;
		objectExactMatch.transform.localPosition = localPosition;
	}

	private void IncreaseEnergyLimit(GameObject caller)
	{
		BlockingUI component = ScreenManager.Instance.OpenPopup("BlockingUI", true, true).GetComponent<BlockingUI>();
		component.SetText("ID_UI_TRANSACTION_TITLE", "ID_UI_TRANSACTION_BODY");
		WalletListener.Instance.Debit(Tuning.Instance.EnergyUpgradeCost, "Increase Energy Limit", SafeAction.Wrap(this, (bool result) =>
		{
			if (result)
			{
				Profile.Instance.EnergyMax = Tuning.Instance.EnergyUpgradedMax;
				Profile.Instance.FullyRestoreEnergy();
				base.onClosed += HandleonIncreased;
				OnClose();
			}
			else
			{
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

	protected override void OnClose()
	{
		SinglePlayerUI.UserInputToggle();
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
