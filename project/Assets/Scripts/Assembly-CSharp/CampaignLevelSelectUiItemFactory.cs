using EB;
using UnityEngine;

public class CampaignLevelSelectUiItemFactory : GridItemFactory
{
	public GameAttributes.CampaignGameModeType campaignGameModeType;

	public override void OnInit()
	{
	}

	public override int GetNumItems()
	{
		return Setup.Instance.Levels.Length;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = Object.Instantiate(itemPrefab) as GameObject;
		CampaignLevelSelectInteractive campaignLevelSelectInteractive = Util.FindComponent<CampaignLevelSelectInteractive>(gameObject);
		gameObject.transform.parent = targetGrid.transform;
		if (index < GetNumItems())
		{
			string arg = Setup.Instance.Levels[index];
			string localizedValue = GameAttributes.GetLocalizedValue(GameAttributes.AttributeType.Level, base.name);
			campaignLevelSelectInteractive.campaignIndex = index;
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, campaignLevelSelectInteractive.campaignIndex, arg);
			UIUtils.SetLabelContents(gameObject, "LabelItemName", localizedValue);
			bool active = false;
			if (campaignGameModeType == GameAttributes.CampaignGameModeType.Challenge)
			{
				active = Random.Range(0f, 1f) < 0.5f;
			}
			Util.GetObjectExactMatch(gameObject, "TaskComplete").SetActive(active);
		}
		else
		{
			campaignLevelSelectInteractive.campaignIndex = -1;
			gameObject.name = string.Format("{0}([{1}] {2})", itemPrefab.name, index, "empty");
			UIUtils.SetLabelContents(gameObject, "LabelItemName", string.Empty);
			Util.GetObjectExactMatch(gameObject, "TaskComplete").SetActive(false);
		}
		return gameObject;
	}
}
