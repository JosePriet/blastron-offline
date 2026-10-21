namespace EB.IAP
{
	public class Config
	{
		public string PublicKey = string.Empty;

		public Action<Transaction> Verify;

		public Action OnEnumerate;

		public Action<string> OnPurchaseFailed;

		public Action OnPurchaseCanceled;
	}
}
