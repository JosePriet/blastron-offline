using System;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class TableHelperUI : MonoBehaviour
{
	public delegate void itemEventHandler(GameObject go);

	private List<GameObject> itemList = new List<GameObject>();

	private int itemIndex;

	private UICenterOnChild c;

	private int delta = 1;

	private int numVisRowCols = 1;

	private UIGrid grid;

	private int maxVisibleItems = 1;

	private Vector3 dragDirection;

	public event itemEventHandler onItemSelected;

	public void SetDelta(int d)
	{
		delta = d;
		itemIndex = delta;
	}

	public List<GameObject> GetItemList()
	{
		return itemList;
	}

	public void SetMaxVisibleItems(int v)
	{
		numVisRowCols = v;
	}

	private void Awake()
	{
		grid = Util.FindComponent<UIGrid>(base.gameObject);
		c = Util.FindComponent<UICenterOnChild>(base.gameObject);
		dragDirection = Util.FindComponent<UIDraggablePanel>(base.gameObject).scale;
		if (c == null)
		{
			EB.Debug.Log("Missing UICenterOnChild component.", this);
		}
	}

	public void CenterOnCurrentItem()
	{
		c.Recenter(null);
	}

	public void CenterOnIndex()
	{
		if (itemIndex < 0 || itemIndex >= itemList.Count)
		{
			return;
		}
		if (c != null)
		{
			if (itemList[itemIndex] != null)
			{
				c.Recenter(itemList[itemIndex]);
			}
		}
		else
		{
			EB.Debug.Log("C is null!");
		}
	}

	public void Refresh()
	{
		grid.Reposition();
		if (itemList.Count <= 0)
		{
		}
	}

	public void Reset()
	{
		Refresh();
		itemIndex = delta;
		CenterOnIndex();
	}

	public void OnScrollLeft()
	{
		itemIndex = GetCurrentIndex();
		int num = itemIndex;
		if (dragDirection.x > 0f)
		{
			while (num > delta && Mathf.Approximately(itemList[num].transform.localPosition.x, itemList[itemIndex].transform.localPosition.x))
			{
				num--;
			}
		}
		else if (dragDirection.y > 0f)
		{
			while (num > delta && Mathf.Approximately(itemList[num].transform.localPosition.y, itemList[itemIndex].transform.localPosition.y))
			{
				num--;
			}
		}
		if (num > 0)
		{
			itemIndex = num;
			CenterOnIndex();
		}
	}

	public int GetCurrentIndex()
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			if (c != null && c.centeredObject == itemList[i])
			{
				return i;
			}
		}
		return 0;
	}

	public void CenterOnItem(GameObject item)
	{
		if (!(item != null))
		{
			return;
		}
		for (int i = 0; i < itemList.Count; i++)
		{
			if (itemList[i] == item)
			{
				itemIndex = i;
				CenterOnIndex();
				break;
			}
		}
	}

	public GameObject GetItem(int index)
	{
		if (index >= 0 && index < itemList.Count)
		{
			return itemList[index];
		}
		return null;
	}

	public void OnScrollRight()
	{
		itemIndex = GetCurrentIndex();
		int i = itemIndex;
		if (dragDirection.x > 0f)
		{
			for (; i < itemList.Count - delta - 1 && Mathf.Approximately(itemList[i].transform.localPosition.x, itemList[itemIndex].transform.localPosition.x); i++)
			{
			}
		}
		else if (dragDirection.y > 0f)
		{
			for (; i >= itemList.Count - delta - 1 && Mathf.Approximately(itemList[i].transform.localPosition.y, itemList[itemIndex].transform.localPosition.y); i++)
			{
			}
		}
		if (i < itemList.Count)
		{
			itemIndex = i;
			CenterOnIndex();
		}
	}

	public void Clear()
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			if ((bool)itemList[i])
			{
				itemList[i].transform.parent = null;
				UnityEngine.Object.Destroy(itemList[i]);
			}
		}
		itemList.Clear();
		Refresh();
	}

	public void OnItemClicked(GameObject obj)
	{
		if (onItemSelected != null)
		{
			onItemSelected(obj);
		}
	}

	public void AddItem(GameObject obj)
	{
		itemList.Add(obj);
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnItemClicked));
	}
}
