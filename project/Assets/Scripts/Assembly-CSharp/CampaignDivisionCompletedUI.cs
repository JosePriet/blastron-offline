using EB;

public class CampaignDivisionCompletedUI : PopupScript
{
	private string stringID()
	{
		if (UnlocksManager.Instance.unlockedDivision == 1)
		{
			return "ID_UI_CAMPAIGN_BRONZE_COMPLETE";
		}
		if (UnlocksManager.Instance.unlockedDivision == 2)
		{
			return "ID_UI_CAMPAIGN_SILVER_COMPLETE";
		}
		if (UnlocksManager.Instance.unlockedDivision == 3)
		{
			return "ID_UI_CAMPAIGN_GOLD_COMPLETE";
		}
		return "not valid division";
	}

	private string division()
	{
		if (UnlocksManager.Instance.unlockedDivision == 1)
		{
			return "bronze";
		}
		if (UnlocksManager.Instance.unlockedDivision == 2)
		{
			return "silver";
		}
		if (UnlocksManager.Instance.unlockedDivision == 3)
		{
			return "gold";
		}
		return "not valid division";
	}

	private void Start()
	{
		UILabel component = Util.GetObjectExactMatch(base.gameObject, "LabelCongrats").GetComponent<UILabel>();
		component.text = Localizer.GetString(stringID());
		UISprite component2 = Util.GetObjectExactMatch(base.gameObject, "BeltSprite").GetComponent<UISprite>();
		component2.spriteName = "champ_belt_" + division().ToLower();
		base.OnStart();
	}

	protected override void OnClose()
	{
		UnlocksManager.Instance.unlockedDivision = 0;
		base.OnClose();
	}
}
