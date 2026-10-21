using System.Collections.Generic;

namespace EB.IAP.Internal
{
	public interface Provider
	{
		string Name { get; }

		void PurchaseItem(Item item);

		void Enumerate(List<Item> items);

		void Complete(Transaction transaction);
	}
}
