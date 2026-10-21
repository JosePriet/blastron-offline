using EB;
using UnityEngine;

public class CampaignAllStarsCollectedUI : PopupScript
{
	private void Start()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "StarTotal");
		Util.GetObjectExactMatch(objectExactMatch, "Label").GetComponent<UILabel>().text = ChampionshipData.TotalStars() + " / " + ChampionshipData.MaxStars();
		SinglePlayerUI.UserInputToggle();
	}

	protected override void OnClose()
	{
		SinglePlayerUI.UserInputToggle();
		if (UnlocksManager.Instance.unlockedDivision > 0)
		{
			base.onClosed += (PopupScript ps) =>
			{
				ScreenManager.Instance.OpenPopup("CampaignDivisionCompletedUI", false, true);
			};
		}
		base.OnClose();
	}
}
