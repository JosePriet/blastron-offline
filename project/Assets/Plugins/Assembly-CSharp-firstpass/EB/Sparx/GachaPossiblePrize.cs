using System.Collections;

namespace EB.Sparx
{
	public class GachaPossiblePrize
	{
		public bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(Type);
			}
		}

		public string Type { get; private set; }

		public string Data { get; private set; }

		public string Id { get; private set; }

		public GachaPossiblePrize(Hashtable data = null)
		{
			Type = string.Empty;
			Data = string.Empty;
			Id = string.Empty;
			if (data != null)
			{
				Type = Dot.String("type", data, string.Empty);
				Data = Dot.String("data", data, string.Empty);
				Id = Dot.String("id", data, string.Empty);
			}
		}

		public override string ToString()
		{
			return string.Format("GachaPossiblePrize: id='{0}' type='{1}' data='{2}'", Id, Type, Data);
		}
	}
}
