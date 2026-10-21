using EB.IAP;

namespace EB.Sparx
{
	public interface WalletListener
	{
		void OnBalanceUpdated(int balance);

		void OnCreditSuceeded(int id);

		void OnDebitFailed(int id);

		void OnDebitSuceeded(int id);

		void OnOffersFetched();

		void OnOfferPurchaseFailed(string error);

		void OnOfferPurchaseSuceeded(Item item);

		void OnOfferPurchaseCanceled();
	}
}
