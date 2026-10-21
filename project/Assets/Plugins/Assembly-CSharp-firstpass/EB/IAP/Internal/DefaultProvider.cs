using System.Collections.Generic;

namespace EB.IAP.Internal
{
	internal class DefaultProvider : Provider
	{
		private Config _config;

		public string Name
		{
			get
			{
				return "default";
			}
		}

		private DefaultProvider(Config config)
		{
			_config = config;
		}

		public static DefaultProvider Create(Config config)
		{
			return new DefaultProvider(config);
		}

		public void PurchaseItem(Item item)
		{
			if (_config.OnPurchaseFailed != null)
			{
				_config.OnPurchaseFailed("Not supported");
			}
		}

		public void Enumerate(List<Item> items)
		{
			foreach (Item item in items)
			{
				item.valid = true;
			}
			if (_config.OnEnumerate != null)
			{
				_config.OnEnumerate();
			}
		}

		public void Complete(Transaction transaction)
		{
		}
	}
}
