using System;
using System.Collections;
using System.Collections.Generic;
using EB.IAP;

namespace EB.Sparx
{
	public class WalletManager : SubSystem, Updatable
	{
		private WalletConfig _config;

		private WalletAPI _api;

		private SafeInt _balance;

		private EB.IAP.Manager _iapManager;

		private List<Item> _payouts;

		private List<Transaction> _verify;

		private int _lastFetchTime;

		private object _fetchTimeout;

		private int _fetchWait = 1;

		private string _externalId = string.Empty;

		private bool _enumerated;

		private bool _fetchBalance;

		public int Balance
		{
			get
			{
				return _balance;
			}
		}

		public Item[] Payouts
		{
			get
			{
				return _payouts.ToArray();
			}
		}

		public bool UpdateOffline
		{
			get
			{
				return false;
			}
		}

		public override void Initialize(Config config)
		{
			_balance = 0;
			_api = new WalletAPI(base.Hub.ApiEndPoint);
			_config = config.WalletConfig;
			_payouts = new List<Item>();
			_verify = new List<Transaction>();
			_lastFetchTime = 0;
			if (_config.Listener == null)
			{
				throw new ArgumentNullException("Missing wallet listener");
			}
		}

		public override IEnumerator LoadOfflineData()
		{
			yield break;
		}

		public override void Connect()
		{
			base.State = SubSystemState.Connecting;
			_lastFetchTime = 0;
			_payouts.Clear();
			_verify.Clear();
			if (_iapManager == null)
			{
				EB.IAP.Config config = new EB.IAP.Config();
				config.OnEnumerate = (Action)Delegate.Combine(config.OnEnumerate, new Action(OnEnumerate));
				config.Verify = (Action<Transaction>)Delegate.Combine(config.Verify, new Action<Transaction>(OnVerify));
				config.OnPurchaseFailed = (Action<string>)Delegate.Combine(config.OnPurchaseFailed, new Action<string>(OnPurchaseFailed));
				config.OnPurchaseCanceled = (Action)Delegate.Combine(config.OnPurchaseCanceled, new Action(OnPurchaseCanceled));
				config.PublicKey = _config.IAPPublicKey;
				_iapManager = new EB.IAP.Manager(config);
			}
			Hashtable hashtable = Dot.Object("wallet", base.Hub.LoginManager.LoginData, null);
			if (hashtable != null)
			{
				OnFetch(null, hashtable);
			}
			else
			{
				Fetch();
			}
			_fetchWait = Dot.Integer("offer_delay", base.Hub.LoginManager.LoginData, _fetchWait);
			FetchOffersDelayed();
		}

		public override void OnEnteredForeground()
		{
			base.OnEnteredForeground();
			if (_fetchBalance)
			{
				_fetchBalance = false;
				Fetch();
			}
		}

		private unsafe void FetchOffersDelayed()
		{
			_enumerated = false;
			Coroutines.ClearTimeout(_fetchTimeout);
			_fetchTimeout = Coroutines.SetTimeout(delegate { FetchOffers(false); }, _fetchWait * 1000);
		}

		public void GetOfferUrl(string offerName, Action<string, string> callback)
		{
			_fetchBalance = true;
			_api.FetchOfferUrl(offerName, callback);
		}

		public void Update()
		{
			if (_verify.Count > 0 && !string.IsNullOrEmpty(_externalId) && _enumerated)
			{
				Transaction transaction = _verify[0];
				_verify.RemoveAt(0);
				VerifyPayout(transaction);
			}
		}

		private void VerifyPayout(Transaction transaction)
		{
			Item payout = _payouts.Find((Item obj) => obj.productId == transaction.productId);
			if (payout == null)
			{
				Debug.LogError("Failed to verify receipt! Cant find payout");
				_verify.Add(transaction);
				return;
			}
			Hashtable hashtable = new Hashtable();
			hashtable["cents"] = payout.cents;
			hashtable["currency"] = payout.currencyCode;
			hashtable["externalTrkid"] = _externalId;
			hashtable["payoutid"] = Dot.Integer("payoutid", payout.info, 0);
			hashtable["response-data"] = transaction.payload;
			hashtable["response-signature"] = transaction.signature;
			_api.VerifyPayout(hashtable, (string err, Hashtable res) =>
			{
				OnVerifyPayout(payout, transaction, err, res);
			});
		}

		private void OnVerifyPayout(Item item, Transaction t, string err, Hashtable data)
		{
			if (!string.IsNullOrEmpty(err))
			{
				Debug.LogError("Fatal: failed to verify payout");
				_config.Listener.OnOfferPurchaseFailed(err);
				_verify.Add(t);
				FetchOffersDelayed();
			}
			else
			{
				_lastFetchTime = 0;
				Fetch();
				_iapManager.Complete(t);
				Coroutines.Run(HandlePurchsedSuceeded(item));
			}
		}

		private IEnumerator HandlePurchsedSuceeded(Item item)
		{
			if (base.Hub.InventoryManager != null)
			{
				int id = base.Hub.InventoryManager.Sync();
				yield return base.Hub.InventoryManager.Wait(id);
			}
			_config.Listener.OnOfferPurchaseSuceeded(item);
		}

		public override void Disconnect(bool isLogout)
		{
			_balance = 0;
		}

		private void AddOffers(ItemType type, ArrayList items)
		{
			foreach (object item2 in items)
			{
				Item item = new Item();
				item.productId = Dot.String("productId", item2, string.Empty);
				item.value = Dot.Integer("value", item2, 0);
				item.cost = Dot.Single("cost", item2, 0f);
				item.cents = (int)(item.cost * 100f);
				item.localizedCost = item.cost.ToString();
				item.info = Dot.Object("info", item2, new Hashtable());
				item.type = type;
				item.valid = true;
				_payouts.Add(item);
			}
		}

		public override void Async(string message, object payload)
		{
			switch (message.ToLower())
			{
			case "sync":
				Fetch();
				break;
			case "offers":
				FetchOffers();
				break;
			case "payouts":
				FetchPayouts();
				break;
			}
		}

		public void FetchOffers(bool forceFetch = false)
		{
			if (_iapManager == null)
			{
				_config.Listener.OnOffersFetched();
			}
			else if (forceFetch || Time.Now - _lastFetchTime >= 3600)
			{
				_lastFetchTime = Time.Now;
				FetchPayouts();
			}
			else
			{
				_config.Listener.OnOffersFetched();
			}
		}

		private void FetchPayouts()
		{
			if (_iapManager == null)
			{
				_config.Listener.OnOffersFetched();
			}
			else
			{
				_api.FetchPayouts(OnLoadPayouts);
			}
		}

		private void OnLoadPayouts(string err, Hashtable data)
		{
			if (!string.IsNullOrEmpty(err))
			{
				Debug.LogError("Failed to get payout: " + err);
				_config.Listener.OnOffersFetched();
				return;
			}
			_externalId = Dot.String("externalTrkid", data, _externalId);
			_payouts.Clear();
			_enumerated = false;
			object obj = Dot.Find("data.payoutSets.0", data);
			if (obj != null)
			{
				ArrayList arrayList = Dot.Array("payouts", obj, new ArrayList());
				foreach (Hashtable item2 in arrayList)
				{
					Item item = new Item();
					item.info = item2;
					item.cost = Dot.Single("cost", item2, 0f);
					item.value = Dot.Integer("numOfIGC", item2, 0);
					item.localizedCost = Dot.String("coststring", item2, string.Empty);
					item.productId = Dot.String("thirdPartyId", item2, string.Empty);
					item.type = ItemType.Consumable;
					item.show = Dot.Bool("show", item2, false);
					item.developerPayload = _externalId + "@=>@" + Dot.String("payoutid", item2, string.Empty);
					_payouts.Add(item);
				}
			}
			ArrayList arrayList2 = Dot.Array("offers.soft", data, null);
			if (arrayList2 != null)
			{
				AddOffers(ItemType.Soft, arrayList2);
			}
			_iapManager.Enumerate(_payouts);
		}

		public void PurchaseOffer(Item item)
		{
			if (_iapManager != null)
			{
				_iapManager.PurchaseItem(item);
			}
			else
			{
				_config.Listener.OnOfferPurchaseFailed("ID_SPARX_ERROR_MUST_BE_LOGGED_IN");
			}
		}

		public void Fetch()
		{
			Debug.Log("TODO-moko: SparxWalletManager > Fetch > Async Calling Fetch");
			_api.Fetch(OnFetch);
		}

		public int Credit(int value, string reason)
		{
			return _api.Credit(value, reason, OnCredit);
		}

		public int Debit(int value, string reason)
		{
			return _api.Debit(value, reason, OnDebit);
		}

		private void OnCredit(int id, string error, Hashtable data)
		{
			if (!string.IsNullOrEmpty(error))
			{
				FatalError(error);
				return;
			}
			StoreBalance(data);
			_config.Listener.OnCreditSuceeded(id);
		}

		private void OnDebit(int id, string error, Hashtable data)
		{
			if (!string.IsNullOrEmpty(error))
			{
				_config.Listener.OnDebitFailed(id);
				return;
			}
			StoreBalance(data);
			_config.Listener.OnDebitSuceeded(id);
		}

		public void StoreBalance(Hashtable data)
		{
			Debug.Log("TODO-moko: SparxWalletManager > StoreBalance > data = " + JSON.Stringify(data));
			_balance = Dot.Integer("balance", data, Balance);
			_config.Listener.OnBalanceUpdated(Balance);
		}

		private void OnFetch(string error, Hashtable data)
		{
			if (!string.IsNullOrEmpty(error))
			{
				FatalError(error);
				return;
			}
			StoreBalance(data);
			if (base.State == SubSystemState.Connecting)
			{
				base.State = SubSystemState.Connected;
			}
		}

		private void OnEnumerate()
		{
			Debug.Log("IAP OnEnumerate");
			_payouts.RemoveAll((Item obj) => !obj.valid);
			_enumerated = true;
			_config.Listener.OnOffersFetched();
		}

		private void OnVerify(Transaction transaction)
		{
			Debug.Log("IAP Verify");
			_verify.Add(transaction);
		}

		private void OnPurchaseFailed(string error)
		{
			Debug.Log("Purchase failed! " + error);
			_config.Listener.OnOfferPurchaseFailed(error);
		}

		private void OnPurchaseCanceled()
		{
			Debug.Log("OnPurchaseCancled");
			_config.Listener.OnOfferPurchaseCanceled();
		}
	}
}
