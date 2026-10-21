using System.Collections.Generic;
using System.Linq;
using EB;
using UnityEngine;

public class UIGridSnap : UIGrid
{
	public class GridItemContainer
	{
		public int index;

		public Transform trans;
	}

	public int gridItemsHorizontal = 1;

	public int gridItemsVertical = 1;

	public List<GridItemContainer> itemList;

	public List<GridItemContainer> itemsByOriginalIndex;

	private int _selectedIndex;

	private List<int> currentlyLoadedTextureIndices = new List<int>();

	private List<int> currentlyVisibleIndices = new List<int>();

	private int x;

	private int y;

	private int ItemsPerPage
	{
		get
		{
			return gridItemsHorizontal * gridItemsVertical;
		}
	}

	private int NumPages
	{
		get
		{
			int itemsPerPage = ItemsPerPage;
			return (itemList.Count + ItemsPerPage - 1) / itemsPerPage;
		}
	}

	private static int SortByIndex(GridItemContainer a, GridItemContainer b)
	{
		return a.index - b.index;
	}

	private void Awake()
	{
		Init();
	}

	public void Reset()
	{
		currentlyLoadedTextureIndices = new List<int>();
		Init();
		UpdateLoadedElements();
		UpdateCollidableElements();
	}

	public void Init()
	{
		_selectedIndex = 0;
		itemList = new List<GridItemContainer>();
		itemsByOriginalIndex = new List<GridItemContainer>();
		Transform transform = base.transform;
		for (int i = 0; i < transform.childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (child.gameObject.active || !hideInactive)
			{
				GridItemContainer gridItemContainer = new GridItemContainer();
				gridItemContainer.index = i;
				gridItemContainer.trans = child;
				itemList.Add(gridItemContainer);
				itemsByOriginalIndex.Add(gridItemContainer);
				if (IsDefaultItem(gridItemContainer))
				{
					_selectedIndex = gridItemContainer.index;
				}
			}
		}
		DisableAllCollidableElements();
	}

	protected virtual bool IsDefaultItem(GridItemContainer item)
	{
		return false;
	}

	public override void Reposition()
	{
		x = 0;
		y = 0;
		DoReposition();
	}

	private void DoReposition()
	{
		if (!mStarted)
		{
			repositionNow = true;
			return;
		}
		itemList.Sort(SortByIndex);
		int num = 0;
		int i = 0;
		for (int count = itemList.Count; i < count; i++)
		{
			if (itemList[i].index == _selectedIndex)
			{
				num = i;
			}
		}
		int j = num;
		for (int count2 = itemList.Count; j < count2; j++)
		{
			Transform trans = itemList[j].trans;
			if (trans.gameObject.active || !hideInactive)
			{
				float z = trans.localPosition.z;
				trans.localPosition = ((arrangement != Arrangement.Horizontal) ? new Vector3(cellWidth * (float)y, (0f - cellHeight) * (float)x, z) : new Vector3(cellWidth * (float)x, (0f - cellHeight) * (float)y, z));
				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					y++;
				}
			}
		}
		int k = 0;
		for (int num2 = num; k < num2; k++)
		{
			Transform trans2 = itemList[k].trans;
			if (trans2.gameObject.active || !hideInactive)
			{
				float z2 = trans2.localPosition.z;
				trans2.localPosition = ((arrangement != Arrangement.Horizontal) ? new Vector3(cellWidth * (float)y, (0f - cellHeight) * (float)x, z2) : new Vector3(cellWidth * (float)x, (0f - cellHeight) * (float)y, z2));
				if (++x >= maxPerLine && maxPerLine > 0)
				{
					x = 0;
					y++;
				}
			}
		}
		UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		if (uIDraggablePanel != null)
		{
			uIDraggablePanel.UpdateScrollbars(true);
		}
		UpdateLoadedElements();
		UpdateCollidableElements();
	}

	public void RepositionUpdate(bool swipeRight)
	{
		itemList.Sort(SortByIndex);
		int num = gridItemsHorizontal * gridItemsVertical;
		if (swipeRight)
		{
			_selectedIndex -= num;
			for (int i = 0; i < num; i++)
			{
				GridItemContainer gridItemContainer = itemList[i];
				GridItemContainer gridItemContainer2 = itemList[itemList.Count - num + i];
				gridItemContainer2.index = gridItemContainer.index - num;
				gridItemContainer2.trans.localPosition = new Vector3(gridItemContainer.trans.localPosition.x - cellWidth * (float)gridItemsHorizontal, gridItemContainer.trans.localPosition.y, gridItemContainer.trans.localPosition.z);
			}
		}
		else
		{
			_selectedIndex += num;
			for (int j = 0; j < num; j++)
			{
				GridItemContainer gridItemContainer3 = itemList[j];
				GridItemContainer gridItemContainer4 = itemList[itemList.Count - num + j];
				gridItemContainer3.index = gridItemContainer4.index + num;
				gridItemContainer3.trans.localPosition = new Vector3(gridItemContainer4.trans.localPosition.x + cellWidth * (float)gridItemsHorizontal, gridItemContainer4.trans.localPosition.y, gridItemContainer3.trans.localPosition.z);
			}
		}
		UpdateLoadedElements();
		UpdateCollidableElements();
	}

	public void DisableAllCollidableElements()
	{
		foreach (GridItemContainer item in itemsByOriginalIndex)
		{
			item.trans.collider.enabled = false;
		}
		currentlyVisibleIndices = new List<int>();
	}

	public void UpdateCollidableElements()
	{
		int num = gridItemsHorizontal * gridItemsVertical;
		int range = itemList.Count / num;
		int pageIdx = WrapValue(_selectedIndex / num, range);
		List<int> list = currentlyVisibleIndices;
		List<int> requiredIndices = GetRequiredIndices(pageIdx, 0);
		IEnumerable<int> enumerable = list.Except(requiredIndices);
		foreach (int item in enumerable)
		{
			if (itemsByOriginalIndex.Count > item)
			{
				itemsByOriginalIndex[item].trans.collider.enabled = false;
			}
		}
		IEnumerable<int> enumerable2 = requiredIndices.Except(list);
		foreach (int item2 in enumerable2)
		{
			if (itemsByOriginalIndex.Count > item2)
			{
				itemsByOriginalIndex[item2].trans.collider.enabled = true;
			}
		}
		currentlyVisibleIndices = requiredIndices;
	}

	public void UpdateLoadedElements()
	{
		int itemsPerPage = ItemsPerPage;
		int numPages = NumPages;
		int pageIdx = WrapValue(_selectedIndex / itemsPerPage, numPages);
		List<int> requiredIndices = GetRequiredIndices(pageIdx, 1);
		List<int> list = currentlyLoadedTextureIndices;
		List<int> list2 = new List<int>(currentlyLoadedTextureIndices);
		IEnumerable<int> enumerable = list.Except(requiredIndices);
		foreach (int item in enumerable)
		{
			if (item >= itemsByOriginalIndex.Count)
			{
				continue;
			}
			GridItemContainer gridItemContainer = itemsByOriginalIndex[item];
			if (gridItemContainer != null)
			{
				GridItem component = gridItemContainer.trans.GetComponent<GridItem>();
				if (component != null)
				{
					gridItemContainer.trans.GetComponent<GridItem>().Unload();
					list2.Remove(item);
				}
			}
		}
		IEnumerable<int> enumerable2 = requiredIndices.Except(list);
		foreach (int item2 in enumerable2)
		{
			if (item2 < 0 || item2 >= itemsByOriginalIndex.Count)
			{
				continue;
			}
			GridItemContainer gridItemContainer2 = itemsByOriginalIndex[item2];
			if (gridItemContainer2 != null)
			{
				GridItem component2 = gridItemContainer2.trans.GetComponent<GridItem>();
				if (component2 != null)
				{
					gridItemContainer2.trans.GetComponent<GridItem>().Load();
					list2.Add(item2);
				}
			}
		}
		currentlyLoadedTextureIndices = list2;
	}

	private List<int> GetRequiredIndices(int pageIdx, int numPagesToEitherSide)
	{
		List<int> list = new List<int>();
		int num = NumPages * ItemsPerPage;
		if (NumPages < 1 + 2 * numPagesToEitherSide)
		{
			for (int i = 0; i < num; i++)
			{
				list.Add(i);
			}
		}
		else
		{
			int num2 = GetFirstIndexForPage(pageIdx - numPagesToEitherSide);
			int lastIndexForPage = GetLastIndexForPage(pageIdx + numPagesToEitherSide);
			if (num > 0)
			{
				while (num2 != lastIndexForPage)
				{
					list.Add(num2);
					num2 = (num2 + 1) % num;
				}
				list.Add(lastIndexForPage);
			}
		}
		return list;
	}

	private int GetFirstIndexForPage(int pageIdx)
	{
		int itemsPerPage = ItemsPerPage;
		int numPages = NumPages;
		pageIdx = WrapValue(pageIdx, numPages);
		return pageIdx * itemsPerPage;
	}

	private int GetLastIndexForPage(int pageIdx)
	{
		int num = gridItemsHorizontal * gridItemsVertical;
		return GetFirstIndexForPage(pageIdx) + (num - 1);
	}

	private int WrapValue(int val, int range)
	{
		if (range == 0)
		{
			range = 1;
		}
		val %= range;
		if (val < 0)
		{
			val = range + val;
		}
		return val;
	}

	public GridItemContainer GetSelectedGridItemContainer()
	{
		foreach (GridItemContainer item in itemList)
		{
			if (item.index == _selectedIndex)
			{
				return item;
			}
		}
		EB.Debug.LogError("Unable to find selected item " + _selectedIndex + ", returning default instead!");
		return new GridItemContainer();
	}
}
