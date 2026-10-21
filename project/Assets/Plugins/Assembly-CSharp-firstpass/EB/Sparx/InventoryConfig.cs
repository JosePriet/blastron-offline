using System.Collections;

namespace EB.Sparx
{
	public class InventoryConfig
	{
		public InventoryListener Listener;

		public bool Enabled = true;

		public Hashtable StartingInventory = new Hashtable();
	}
}
