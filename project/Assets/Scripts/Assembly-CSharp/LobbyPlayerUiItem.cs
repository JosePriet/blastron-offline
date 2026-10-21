using EB;
using UnityEngine;

public class LobbyPlayerUiItem : MonoBehaviour
{
	public string playerName
	{
		set
		{
			UIUtils.SetLabelContents(base.gameObject, "PlayerName", value);
		}
	}

	public int level
	{
		set
		{
			UIUtils.SetLabelContents(base.gameObject, "PlayerLevelValue", value);
		}
	}

	public int teamId
	{
		set
		{
		}
	}

	public bool isPresent
	{
		set
		{
			Util.GetObjectExactMatch(base.gameObject, "ActivePlayer").SetActive(value);
			Util.GetObjectExactMatch(base.gameObject, "InactivePlayer").SetActive(!value);
		}
	}

	public void OnClick()
	{
	}
}
