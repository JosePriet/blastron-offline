using System.Collections;

namespace EB.Sparx
{
	public class GachaPickResult
	{
		public string RewardType { get; private set; }

		public string RewardData { get; private set; }

		public int RewardCount { get; private set; }

		public int SoftCurrentToPay { get; private set; }

		public int XpToGive { get; private set; }

		public int PaymentBalance { get; private set; }

		public GachaPickResult(Hashtable data = null)
		{
			if (data != null)
			{
				RewardType = Dot.String("type", data, string.Empty);
				RewardData = Dot.String("data", data, string.Empty);
				RewardCount = Dot.Integer("quantity", data, 0);
				PaymentBalance = Dot.Integer("balance", data, 0);
				if (RewardCount == 0)
				{
					RewardCount = Dot.Integer("count", data, 0);
				}
				SoftCurrentToPay = Dot.Integer("softToPay", data, 0);
				XpToGive = Dot.Integer("xpToGive", data, 0);
			}
			else
			{
				RewardType = string.Empty;
				RewardData = string.Empty;
				RewardCount = 0;
				SoftCurrentToPay = 0;
				XpToGive = 0;
				PaymentBalance = 0;
			}
		}

		public override string ToString()
		{
			return string.Format("RewardType:{0} RewardData:{1} RewardCount:{2} SCToPay:{3} XpToGive:{4}", RewardType, RewardData, RewardCount, SoftCurrentToPay, XpToGive);
		}
	}
}
