using System.Collections;

namespace EB.IAP
{
	public class Item
	{
		public string productId = string.Empty;

		public float cost;

		public int cents;

		public string localizedCost = string.Empty;

		public bool valid;

		public bool show = true;

		public ItemType type;

		public int value;

		public Hashtable info;

		public string currencyCode = string.Empty;

		public string localizedTitle = string.Empty;

		public string localizedDesc = string.Empty;

		public string developerPayload = string.Empty;
	}
}
