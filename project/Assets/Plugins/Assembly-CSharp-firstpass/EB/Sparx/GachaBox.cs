using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EB.Sparx
{
	public class GachaBox
	{
		public bool IsValid
		{
			get
			{
				return !string.IsNullOrEmpty(Name);
			}
		}

		public string Name { get; private set; }

		public int Index { get; private set; }

		public string Token { get; private set; }

		public int Multiplier { get; private set; }

		public GachaBoxSpendInfo SoftCurrency { get; private set; }

		public GachaBoxSpendInfo HardCurrency { get; private set; }

		public GachaBoxSpendInfo Tokens { get; private set; }

		public List<GachaPossiblePrize> PossiblePrizes { get; private set; }

		public bool HasBanner { get; private set; }

		public Dictionary<string, string> BannerText { get; private set; }

		public Color BannerColor { get; private set; }

		public int MinLevel { get; private set; }

		public Color Tint { get; private set; }

		public Dictionary<string, string> DisplayNames { get; private set; }

		public GachaBox(int index, Hashtable data)
		{
			Name = string.Empty;
			Index = index;
			Token = string.Empty;
			Multiplier = 0;
			PossiblePrizes = new List<GachaPossiblePrize>();
			HasBanner = false;
			BannerText = new Dictionary<string, string>();
			BannerColor = Color.white;
			MinLevel = 0;
			Tint = Color.white;
			DisplayNames = new Dictionary<string, string>();
			if (data != null)
			{
				Name = Dot.String("name", data, string.Empty);
				Token = Dot.String("token", data, string.Empty);
				Multiplier = Dot.Integer("multiplier", data, 0);
				ArrayList arrayList = Dot.Array("possiblePrizes", data, null);
				if (arrayList != null)
				{
					foreach (object item in arrayList)
					{
						GachaPossiblePrize gachaPossiblePrize = new GachaPossiblePrize(item as Hashtable);
						if (gachaPossiblePrize.IsValid)
						{
							PossiblePrizes.Add(gachaPossiblePrize);
						}
					}
				}
				SoftCurrency = new GachaBoxSpendInfo(Dot.Object("sc", data, null));
				HardCurrency = new GachaBoxSpendInfo(Dot.Object("hc", data, null));
				Tokens = new GachaBoxSpendInfo(Dot.Object("tokenc", data, null));
				MinLevel = Dot.Integer("minLevel", data, 0);
				Hashtable hashtable = Dot.Object("highlightcolor", data, null);
				Color white = Color.white;
				if (hashtable != null)
				{
					white.r = (float)Dot.Integer("r", hashtable, 255) / 255f;
					white.g = (float)Dot.Integer("g", hashtable, 255) / 255f;
					white.b = (float)Dot.Integer("b", hashtable, 255) / 255f;
					white.a = Dot.Single("a", hashtable, 1f);
				}
				Tint = white;
				Hashtable hashtable2 = Dot.Object("banner", data, null);
				if (hashtable2 != null)
				{
					foreach (DictionaryEntry item2 in hashtable2)
					{
						if (!BannerText.ContainsKey((string)item2.Key))
						{
							string value = (string)item2.Value;
							if (!string.IsNullOrEmpty(value))
							{
								HasBanner = true;
							}
							BannerText.Add((string)item2.Key, value);
						}
					}
				}
				Hashtable hashtable3 = Dot.Object("bannercolor", data, null);
				Color white2 = Color.white;
				if (hashtable3 != null)
				{
					white2.r = (float)Dot.Integer("r", hashtable3, 255) / 255f;
					white2.g = (float)Dot.Integer("g", hashtable3, 255) / 255f;
					white2.b = (float)Dot.Integer("b", hashtable3, 255) / 255f;
					white2.a = Dot.Single("a", hashtable3, 1f);
				}
				BannerColor = white2;
				Hashtable hashtable4 = Dot.Object("displayname", data, null);
				if (hashtable4 == null)
				{
					return;
				}
				{
					foreach (DictionaryEntry item3 in hashtable4)
					{
						if (!DisplayNames.ContainsKey((string)item3.Key))
						{
							DisplayNames.Add((string)item3.Key, (string)item3.Value);
						}
					}
					return;
				}
			}
			SoftCurrency = new GachaBoxSpendInfo();
			HardCurrency = new GachaBoxSpendInfo();
			Tokens = new GachaBoxSpendInfo();
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
			return string.Format("Name:{0} Index:{1} Multiplier:{2} SC:{3} HC:{4} Token:{5} PossiblePrizes:{6}", Name, Index, Multiplier, SoftCurrency, HardCurrency, Tokens, stringBuilder);
		}

		public string LocalizedDisplayName(Language locale)
		{
			string oldLanguageCode = Localizer.GetOldLanguageCode(locale);
			string value = string.Empty;
			DisplayNames.TryGetValue(oldLanguageCode, out value);
			return value;
		}

		public string LocalizedBannerText(Language locale)
		{
			string oldLanguageCode = Localizer.GetOldLanguageCode(locale);
			string value = string.Empty;
			BannerText.TryGetValue(oldLanguageCode, out value);
			return value;
		}
	}
}
