using EB;
using EB.Sparx;
using UnityEngine;

public class InventoryUiItem : MonoBehaviour
{
	public InventoryDB.MetaData metaData;

	private bool isDrag;

	public void Awake()
	{
		isDrag = false;
	}

	public void UpdateCount()
	{
		if (metaData != null)
		{
			string labelText = Hub.Instance.InventoryManager.GetCount(metaData.name).ToString();
			if (metaData.IsFree || Hub.Instance.InventoryManager.EverythingFree)
			{
				labelText = "∞";
			}
			UIUtils.SetLabelContents(base.gameObject, "Count", labelText);
		}
	}

	private void OnInventoryUpdated()
	{
		UpdateCount();
	}

	private void OnDrag(Vector2 delta)
	{
		isDrag = true;
	}

	private void OnPress(bool down)
	{
		if (!down && !isDrag && metaData != null)
		{
			SendMessageUpwards("OnItemSelected", metaData);
		}
		isDrag = false;
	}
}
