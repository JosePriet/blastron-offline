using System.Collections.Generic;
using EB.IAP.Internal;

namespace EB.IAP
{
	public class Manager
	{
		private Provider _provider;

		public string ProviderName
		{
			get
			{
				return _provider.Name;
			}
		}

		public Manager(Config config)
		{
			_provider = ProviderFactory.Create(config);
		}

		public void PurchaseItem(Item item)
		{
			_provider.PurchaseItem(item);
		}

		public void Enumerate(List<Item> items)
		{
			_provider.Enumerate(items);
		}

		public void Complete(Transaction t)
		{
			_provider.Complete(t);
		}
	}
}
