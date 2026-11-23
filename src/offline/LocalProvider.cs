using System.Collections.Generic;
using EB.IAP;
using EB.IAP.Internal;

namespace EB.Sparx
{
	// Replaces Google Play billing. Every gear pack is free: a purchase is handed
	// straight to the wallet's verify step, which the local server credits.
	public class LocalProvider : Provider
	{
		private EB.IAP.Config _config;

		internal static bool IsReceipt(string value)
		{
			if (value == null || !value.StartsWith("offline.") || value.Length != 40) return false;
			for (int i = 8; i < value.Length; i++)
			{
				char c = value[i];
				if (!(c >= '0' && c <= '9') && !(c >= 'a' && c <= 'f')) return false;
			}
			return true;
		}

		public string Name => "offline";

		public LocalProvider(EB.IAP.Config config)
		{
			_config = config;
		}

		public void Enumerate(List<Item> items)
		{
			foreach (Item item in items)
			{
				item.valid = true;
				if (item.type != ItemType.Soft)
				{
					item.localizedCost = "FREE";
				}
			}
			Coroutines.NextFrame(delegate
			{
				if (_config.OnEnumerate != null)
				{
					_config.OnEnumerate();
				}
			});
		}

		public void PurchaseItem(Item item)
		{
			Transaction transaction = new Transaction();
			transaction.transactionId = "offline." + System.Guid.NewGuid().ToString("N");
			transaction.productId = item.productId;
			transaction.payload = transaction.transactionId;
			transaction.signature = string.Empty;
			Coroutines.NextFrame(delegate
			{
				if (_config.Verify != null)
				{
					_config.Verify(transaction);
				}
			});
		}

		public void Complete(Transaction transaction)
		{
		}
	}
}
