using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace EB.Sparx
{
	public class GachaSet
	{
		public bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Version);
			}
		}

		public string Version { get; private set; }

		public string Name { get; private set; }

		public List<string> AttractImages { get; private set; }

		public List<GachaPossiblePrize> PossiblePrizes { get; private set; }

		public List<GachaBox> Boxes { get; private set; }

		public GachaSet(Hashtable data)
		{
			Version = string.Empty;
			Name = string.Empty;
			AttractImages = new List<string>();
			PossiblePrizes = new List<GachaPossiblePrize>();
			Boxes = new List<GachaBox>();
			if (data == null)
			{
				return;
			}
			Version = Dot.String("version", data, string.Empty);
			Name = Dot.String("name", data, string.Empty);
			ArrayList arrayList = Dot.Array("attracts", data, null);
			if (arrayList != null)
			{
				foreach (object item in arrayList)
				{
					if (item is string)
					{
						AttractImages.Add(item as string);
					}
				}
			}
			ArrayList arrayList2 = Dot.Array("possiblePrizes", data, null);
			if (arrayList2 != null)
			{
				foreach (object item2 in arrayList2)
				{
					GachaPossiblePrize gachaPossiblePrize = new GachaPossiblePrize(item2 as Hashtable);
					if (gachaPossiblePrize.IsValid)
					{
						PossiblePrizes.Add(gachaPossiblePrize);
					}
				}
			}
			ArrayList arrayList3 = Dot.Array("boxes", data, null);
			if (arrayList3 == null)
			{
				return;
			}
			for (int i = 0; i < arrayList3.Count; i++)
			{
				GachaBox gachaBox = new GachaBox(i, arrayList3[i] as Hashtable);
				if (gachaBox.IsValid)
				{
					Boxes.Add(gachaBox);
				}
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (GachaPossiblePrize possiblePrize in PossiblePrizes)
			{
				stringBuilder.Append(possiblePrize.Id);
				stringBuilder.Append("->");
				stringBuilder.Append(possiblePrize.Type);
				stringBuilder.Append("->");
				stringBuilder.Append(possiblePrize.Data);
				stringBuilder.Append(",");
			}
			return string.Format("Name:{0} Boxes:{3} Attracts:{1} PossiblePrizes:{2}", Name, string.Join(",", AttractImages.ToArray()), stringBuilder, Boxes.Count);
		}

		public GachaBox GetBox(string boxName)
		{
			GachaBox result = null;
			foreach (GachaBox box in Boxes)
			{
				if (box.Name == boxName)
				{
					result = box;
					break;
				}
			}
			return result;
		}
	}
}
