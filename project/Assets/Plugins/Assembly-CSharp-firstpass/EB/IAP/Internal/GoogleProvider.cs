using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EB.IAP.Internal
{
	public class GoogleProvider : MonoBehaviour, Provider
	{
		private static Config _config;

		private AndroidJavaClass _class;

		private List<Item> _items;

		public string Name
		{
			get
			{
				return "googleapp";
			}
		}

		public static GoogleProvider Create(Config config)
		{
			_config = config;
			GameObject gameObject = new GameObject("iap_plugin_android");
			return gameObject.AddComponent<IAPGoogleProvider>();
		}

		private void Awake()
		{
			_class = new AndroidJavaClass("com.explodingbarrel.iap.MainActivity");
			_class.CallStatic("Setup", _config.PublicKey, true);
			Object.DontDestroyOnLoad(base.gameObject);
		}

		public void PurchaseItem(Item item)
		{
			string text = "inapp";
			if (item.type == ItemType.Subscription)
			{
				text = "subs";
			}
			Debug.Log("PurchaseItem: " + item.productId);
			_class.CallStatic("PurchaseItem", item.productId, text, item.developerPayload);
		}

		public void Enumerate(List<Item> items)
		{
			_items = items;
			List<string> list = ArrayUtils.Map(items, (Item item) => (item.type == ItemType.Soft) ? null : item.productId);
			string text = ArrayUtils.Join(list, ',');
			_class.CallStatic("Enumerate", text);
		}

		private void OnIAPUnsupported()
		{
			Debug.Log("OnIAPUnsupported");
			if (_items == null)
			{
				return;
			}
			foreach (Item item in _items)
			{
				item.valid = false;
			}
			if (_config.OnEnumerate != null)
			{
				_config.OnEnumerate();
			}
		}

		private void OnIAPSuppored(string skuDetails)
		{
			Debug.Log("OnIAPSuppored: " + skuDetails);
			if (_items == null || string.IsNullOrEmpty(skuDetails))
			{
				return;
			}
			ArrayList arrayList = JSON.Parse(skuDetails) as ArrayList;
			foreach (Item item in _items)
			{
				if (item.type != ItemType.Soft)
				{
					item.valid = false;
				}
				foreach (object item2 in arrayList)
				{
					if (Dot.String("productId", item2, string.Empty) == item.productId)
					{
						item.valid = true;
						item.localizedCost = Dot.String("price", item2, string.Empty);
						item.localizedTitle = Dot.String("title", item2, string.Empty);
						item.localizedDesc = Dot.String("description", item2, string.Empty);
						break;
					}
				}
			}
			if (_config.OnEnumerate != null)
			{
				_config.OnEnumerate();
			}
		}

		public void Complete(Transaction transaction)
		{
			_class.CallStatic("CompletePurchase", transaction.payload, transaction.signature);
		}

		private void OnItemPurchaseCanceled(string ignore)
		{
			Debug.Log("OnItemPurchaseCanceled:");
			if (_config.OnPurchaseCanceled != null)
			{
				_config.OnPurchaseCanceled();
			}
		}

		private void OnItemPurchaseFailed(string error)
		{
			Debug.Log("OnItemPurchaseFailed: " + error);
			if (_config.OnPurchaseFailed != null)
			{
				_config.OnPurchaseFailed(error);
			}
		}

		private void OnItemPurchased(string jsonParts)
		{
			Debug.Log("OnItemPurchased: " + jsonParts);
			string[] array = jsonParts.Split('|');
			string text = array[0];
			string signature = array[1];
			object obj = JSON.Parse(text);
			Transaction transaction = new Transaction();
			transaction.transactionId = Dot.String("orderId", obj, string.Empty);
			transaction.signature = signature;
			transaction.payload = text;
			transaction.productId = Dot.String("productId", obj, string.Empty);
			if (_config.Verify != null)
			{
				_config.Verify(transaction);
			}
			else
			{
				Complete(transaction);
			}
		}
	}
}
