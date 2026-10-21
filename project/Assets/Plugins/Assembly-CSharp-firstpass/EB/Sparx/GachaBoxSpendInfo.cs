using System.Collections;

namespace EB.Sparx
{
	public class GachaBoxSpendInfo
	{
		public int Cost { get; private set; }

		public int Xp { get; private set; }

		public GachaBoxSpendInfo(Hashtable data = null)
		{
			Cost = 0;
			Xp = 0;
			if (data != null)
			{
				Cost = Dot.Integer("cost", data, 0);
				Xp = Dot.Integer("xp", data, 0);
			}
		}

		public override string ToString()
		{
			return string.Format("Cost:{0} Xp:{1}", Cost, Xp);
		}
	}
}
