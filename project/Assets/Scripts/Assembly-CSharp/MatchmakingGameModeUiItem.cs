using EB;
using UnityEngine;

public class MatchmakingGameModeUiItem : MonoBehaviour
{
	public float dragThreshold;

	public GameAttributes.GameModeType gameModeId = GameAttributes.GameModeType.Any;

	private string _itemName;

	private string _itemIcon;

	private bool _itemSelected;

	private bool _isEmpty = true;

	private bool isDrag;

	public string itemName
	{
		get
		{
			return _itemName;
		}
		set
		{
			_itemName = value;
			UIUtils.SetLabelContents(base.gameObject, "ItemName", value);
		}
	}

	public string itemIcon
	{
		get
		{
			return _itemIcon;
		}
		set
		{
			_itemIcon = value;
			Util.GetObjectExactMatch(base.gameObject, "ItemIcon").GetComponent<UISlicedSprite>().spriteName = value;
		}
	}

	public bool itemSelected
	{
		get
		{
			return _itemSelected;
		}
		set
		{
			_itemSelected = value;
			Util.GetObjectExactMatch(base.gameObject, "ItemHighlight").SetActive(value);
		}
	}

	public bool isEmpty
	{
		get
		{
			return _isEmpty;
		}
		set
		{
			_isEmpty = value;
			Util.GetObjectExactMatch(base.gameObject, "ItemIcon").SetActive(!value);
		}
	}

	private void Awake()
	{
		isDrag = false;
	}

	private void OnDrag(Vector2 delta)
	{
		if (delta.sqrMagnitude > dragThreshold * dragThreshold)
		{
			isDrag = true;
		}
	}

	private void OnPress(bool down)
	{
		if (!down && !isDrag && !isEmpty)
		{
			SendMessageUpwards("OnGameModeSelected", gameModeId);
		}
		isDrag = false;
	}
}
