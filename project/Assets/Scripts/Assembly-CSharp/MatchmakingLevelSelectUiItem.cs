using EB;
using UnityEngine;

public class MatchmakingLevelSelectUiItem : MonoBehaviour
{
	public int levelId;

	private UILabel label;

	private UITexture texture;

	private UISlicedSprite border;

	private void Awake()
	{
		label = Util.GetObjectExactMatch(base.gameObject, "ItemName").GetComponent<UILabel>();
		texture = Util.GetObjectExactMatch(base.gameObject, "ItemTexture").GetComponent<UITexture>();
		border = Util.GetObjectExactMatch(base.gameObject, "ItemHighlight").GetComponent<UISlicedSprite>();
		label.text = string.Empty;
		EnableBorder(false);
	}

	public void EnableBorder(bool enable)
	{
		NGUITools.SetActive(border.gameObject, enable);
	}

	public void SetItemName(string s)
	{
		UIUtils.SetLabelContents(base.gameObject, "ItemName", s);
	}
}
