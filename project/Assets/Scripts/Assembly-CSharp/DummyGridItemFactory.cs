using UnityEngine;

public abstract class DummyGridItemFactory : GridItemFactory
{
	private GameObject _dummy;

	protected override void Start()
	{
		_dummy = new GameObject("dummy", typeof(DummyGridItem));
		GameObject gameObject = new GameObject("tex", typeof(UITexture));
		gameObject.transform.parent = _dummy.transform;
		UIGrid component = targetGrid.GetComponent<UIGrid>();
		gameObject.transform.localScale = new Vector3(component.cellWidth, component.cellHeight, 1f);
		_dummy.SetActive(true);
		base.Start();
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, _dummy);
		DummyGridItem component = gameObject.GetComponent<DummyGridItem>();
		component.index = index;
		component.LoadCb = LoadObject;
		component.UnloadCb = UnloadObject;
		component.name = index.ToString();
		return gameObject;
	}

	protected virtual void LoadObject(GameObject parent, int index)
	{
	}

	protected virtual void UnloadObject(GameObject parent, int index)
	{
	}

	private void Update()
	{
		GameObject gameObject = targetGrid;
		if (!gameObject)
		{
			return;
		}
		UIGrid component = targetGrid.GetComponent<UIGrid>();
		if (!component)
		{
			return;
		}
		float x = gameObject.transform.localPosition.x;
		int num = (int)Mathf.Abs(x / component.cellWidth);
		int num2 = Mathf.Max(0, num - 1);
		int num3 = num + gridItemsHorizontal;
		for (int i = 0; i < createdItems.Count; i++)
		{
			int num4 = i / gridItemsVertical;
			DummyGridItem component2 = createdItems[i].GetComponent<DummyGridItem>();
			if ((bool)component2)
			{
				if (num4 >= num2 && num4 <= num3)
				{
					component2.Load();
				}
				else
				{
					component2.Unload();
				}
			}
		}
	}
}
