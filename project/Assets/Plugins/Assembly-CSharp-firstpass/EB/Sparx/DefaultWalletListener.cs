using EB.IAP;

namespace EB.Sparx
{
	public class DefaultWalletListener : WalletListener
	{
		public void OnBalanceUpdated(int balance)
		{
			Util.BroadcastMessage("OnBalanceUpdated", balance);
		}

		public void OnCreditSuceeded(int id)
		{
			Util.BroadcastMessage("OnCreditSuceeded", id);
		}

		public void OnDebitFailed(int id)
		{
			Util.BroadcastMessage("OnDebitFailed", id);
		}

		public void OnDebitSuceeded(int id)
		{
			Util.BroadcastMessage("OnDebitSuceeded", id);
		}

		public void OnOffersFetched()
		{
			Util.BroadcastMessage("OnOffersFetched");
		}

		public void OnOfferPurchaseFailed(string error)
		{
			Util.BroadcastMessage("OnOfferPurchaseFailed", error);
		}

		public void OnOfferPurchaseSuceeded(Item item)
		{
			Util.BroadcastMessage("OnOfferPurchaseSuceeded", item);
		}

		public void OnOfferPurchaseCanceled()
		{
			Util.BroadcastMessage("OnOfferPurchaseCanceled");
		}
	}
}
