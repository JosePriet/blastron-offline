using System.Collections;

namespace EB.Sparx
{
	public class DefaultInventoryListener : InventoryListener
	{
		public void OnInventorySynced(int requestId)
		{
			Util.BroadcastMessage("OnInventorySynced");
		}

		public void OnInventoryItemsAdded(int requestId, Hashtable items)
		{
			Util.BroadcastMessage("OnInventoryItemsAdded", items);
		}

		public void OnInventoryPurchaseFailed(int requestId)
		{
			Util.BroadcastMessage("OnInventoryPurchaseFailed");
		}
	}
}
