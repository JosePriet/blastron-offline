using EB;
using UnityEngine;

public class EmotesPanel : MonoBehaviour
{
	public Collider[] buttons;

	private bool isPanelActive;

	private UIPanel[] cachedPanels;

	private TableHelperUI helperTable;

	private UIGrid grid;

	private void Awake()
	{
		cachedPanels = Util.FindAllComponents<UIPanel>(base.gameObject);
		grid = base.gameObject.GetComponentInChildren<UIGrid>();
		helperTable = base.gameObject.AddComponent<TableHelperUI>();
		helperTable.SetDelta(3);
	}

	public void ToggleVisibility(GameObject caller)
	{
		ShowPanel(!isPanelActive);
	}

	public void ShowPanel(bool show)
	{
		EB.Debug.Log("TRYING TO SHOW PANEL " + show);
		isPanelActive = show;
		UIPanel[] array = cachedPanels;
		foreach (UIPanel uIPanel in array)
		{
			uIPanel.enabled = isPanelActive;
		}
		NGUITools.SetActive(grid.gameObject, show);
		if (show)
		{
			helperTable.Refresh();
			SendMessageUpwards("OnHudElementOpened", base.gameObject, SendMessageOptions.RequireReceiver);
		}
	}

	public void OnEmoteSelected(string emoteName)
	{
		EmoteManager.Instance.SpawnEmote(emoteName);
		ShowPanel(false);
	}

	public void OnScrollLeft(GameObject caller)
	{
		helperTable.OnScrollLeft();
	}

	public void OnScrollRight(GameObject caller)
	{
		helperTable.OnScrollRight();
	}

	public void OnEmoteItemsReady(EmoteItemFactory caller)
	{
		helperTable.Clear();
		UICallback[] array = Util.FindAllComponents<UICallback>(base.gameObject);
		EmoteUiItem[] componentsInChildren = base.gameObject.GetComponentsInChildren<EmoteUiItem>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			helperTable.AddItem(componentsInChildren[i].gameObject);
		}
		UICallback[] array2 = array;
		foreach (UICallback uICallback in array2)
		{
			uICallback.Target = this;
		}
		ShowPanel(false);
		helperTable.Refresh();
	}
}
