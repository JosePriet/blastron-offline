using System.Collections.Generic;
using EB;
using UnityEngine;

public abstract class GridItemFactory : MonoBehaviour
{
	public GameObject itemPrefab;

	public GameObject targetGrid;

	public int gridItemsHorizontal = 1;

	public int gridItemsVertical = 1;

	protected List<GameObject> createdItems = new List<GameObject>();

	private UIPanel panel;

	public virtual void Reset()
	{
		foreach (GameObject createdItem in createdItems)
		{
			createdItem.transform.parent = null;
			Object.Destroy(createdItem);
		}
		createdItems = null;
		Init();
	}

	protected virtual void Start()
	{
		Init();
	}

	protected void Cleanup(GameObject item)
	{
		UIPanel component = item.GetComponent<UIPanel>();
		if (component != null)
		{
			Object.Destroy(component);
		}
	}

	protected virtual void Init()
	{
		createdItems = new List<GameObject>();
		panel = GetComponent<UIPanel>();
		int numItems = GetNumItems();
		if (numItems != 0)
		{
			int defaultIndex = GetDefaultIndex();
			for (int i = defaultIndex; i < numItems; i++)
			{
				AddItemWithIndex(i);
			}
			for (int j = 0; j < defaultIndex; j++)
			{
				AddItemWithIndex(j);
			}
			OnInit();
		}
	}

	public abstract int GetNumItems();

	public abstract GameObject CreateItem(int index);

	public abstract int GetDefaultIndex();

	public abstract void OnInit();

	private void AddItemWithIndex(int i)
	{
		GameObject gameObject = CreateItem(i);
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localScale = Vector3.one;
		if (gameObject == null)
		{
			EB.Debug.Log("ERROR ITEM");
		}
		GridItem component = gameObject.GetComponent<GridItem>();
		if (component == null)
		{
			EB.Debug.Log("ERROR G");
		}
		component.Load();
		createdItems.Add(gameObject);
	}
}
