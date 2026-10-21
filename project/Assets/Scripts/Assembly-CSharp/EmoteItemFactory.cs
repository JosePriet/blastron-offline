using EB;
using UnityEngine;

public class EmoteItemFactory : GridItemFactory
{
	public override int GetNumItems()
	{
		return EmoteManager.Instance.Emotes.Length;
	}

	public override int GetDefaultIndex()
	{
		return 0;
	}

	public override GameObject CreateItem(int index)
	{
		GameObject gameObject = NGUITools.AddChild(targetGrid, itemPrefab);
		EmoteUiItem emoteUiItem = Util.FindComponent<EmoteUiItem>(gameObject);
		Cleanup(gameObject);
		string empty = string.Empty;
		if (index < GetNumItems())
		{
			gameObject.name = string.Format(arg2: emoteUiItem.emoteName = EmoteManager.Instance.Emotes[index], format: "[{0}]{1} {2}", arg0: index, arg1: itemPrefab.name);
			emoteUiItem.isEmpty = false;
		}
		else
		{
			gameObject.name = string.Format("[{0}]{1} {2}", index, itemPrefab.name, "(empty)");
			emoteUiItem.isEmpty = true;
		}
		return gameObject;
	}

	public override void OnInit()
	{
		base.gameObject.SendMessageUpwards("OnEmoteItemsReady", this);
	}
}
