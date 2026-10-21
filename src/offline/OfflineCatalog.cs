using System;
using System.Collections;
using System.Collections.Generic;

namespace EB.Sparx
{
	// Substitute for the gacha ("Lootinator") tables and store catalog that lived
	// only on the Sparx server. The originals are lost, so everything here is a
	// reconstruction: prizes and prices are derived from the game's own item data
	// (InventoryDB / CharacterDB) wherever possible, and the few fixed numbers are
	// marked INVENTED. Built lazily, after the game has loaded its item tables.
	public static class OfflineCatalog
	{
		public const string GachaVersion = "offline-1";

		public const string TutorialToken = "token_tutorial";

		private class Prize
		{
			public string type;

			public string data;

			public int quantity;
		}

		private class Box
		{
			public string name;

			public string token;

			public int minLevel;

			public int softCost;

			public int hardCost;

			public List<Prize> prizes = new List<Prize>();
		}

		private static Dictionary<string, List<Box>> _groups;

		private static ArrayList _payouts;

		private static ArrayList _softOffers;

		private static System.Random _random = new System.Random();

		// INVENTED: gear (hard currency) amounts of the six free gear packs, one
		// per UI_currency_hard_N icon shipped in the game.
		private static readonly int[] kHardPacks = new int[6] { 25, 60, 130, 280, 600, 1300 };

		// INVENTED: gear prices of the four rivet packs and their bonus.
		private static readonly int[] kSoftPackCosts = new int[4] { 10, 25, 50, 100 };

		private static readonly float[] kSoftPackBonus = new float[4] { 1f, 1.1f, 1.2f, 1.3f };

		// INVENTED: player level each tier unlocks at (bronze, silver, gold, platinum).
		private static readonly int[] kTierMinLevel = new int[4] { 1, 5, 10, 15 };

		// INVENTED: highest item levelRequired that can drop from bronze and silver.
		private const int kBronzeMaxItemLevel = 5;

		private const int kSilverMaxItemLevel = 12;

		private const float kDefaultRivetsPerGear = 100f;

		private static List<InventoryDB.MetaData> Items()
		{
			List<InventoryDB.MetaData> list = new List<InventoryDB.MetaData>();
			for (int c = 0; c < 2; c++)
			{
				foreach (InventoryDB.MetaData item in InventoryDB.GetCategoryList((InventoryItem.Category)c))
				{
					if (!item.isHidden && !item.isDefault && (item.softCost > 0 || item.hardCost > 0))
					{
						list.Add(item);
					}
				}
			}
			return list;
		}

		private static List<CharacterDB.MetaData> Parts()
		{
			List<CharacterDB.MetaData> list = new List<CharacterDB.MetaData>();
			CharacterDB.Category[] categories = new CharacterDB.Category[3]
			{
				CharacterDB.Category.kHead,
				CharacterDB.Category.kTorso,
				CharacterDB.Category.kLegs
			};
			foreach (CharacterDB.Category c in categories)
			{
				foreach (CharacterDB.MetaData part in CharacterDB.GetCategory(c))
				{
					if (!part.isHidden && !part.isDefault)
					{
						list.Add(part);
					}
				}
			}
			return list;
		}

		// Rivets per gear, from items the game prices in both currencies.
		public static float RivetsPerGear()
		{
			List<float> ratios = new List<float>();
			foreach (InventoryDB.MetaData item in Items())
			{
				if (item.softCost > 0 && item.hardCost > 0)
				{
					ratios.Add((float)item.softCost / (float)item.hardCost);
				}
			}
			if (ratios.Count == 0)
			{
				return kDefaultRivetsPerGear;
			}
			ratios.Sort();
			return ratios[ratios.Count / 2];
		}

		private static int Round(float value, int step)
		{
			return Math.Max(step, (int)Math.Round(value / step) * step);
		}

		private static void AddItems(Box box, List<InventoryDB.MetaData> items)
		{
			foreach (InventoryDB.MetaData item in items)
			{
				box.prizes.Add(new Prize
				{
					type = "item",
					data = StringUtil.SafeKey(item.name),
					quantity = Math.Max(1, item.quantity)
				});
			}
		}

		private static void AddParts(Box box, List<CharacterDB.MetaData> parts)
		{
			foreach (CharacterDB.MetaData part in parts)
			{
				box.prizes.Add(new Prize
				{
					type = "item",
					data = StringUtil.SafeKey(part.name),
					quantity = 1
				});
			}
		}

		private static float Average(List<InventoryDB.MetaData> items, bool hard)
		{
			float sum = 0f;
			int count = 0;
			foreach (InventoryDB.MetaData item in items)
			{
				int cost = hard ? item.hardCost : item.softCost;
				if (cost > 0)
				{
					sum += cost;
					count++;
				}
			}
			return (count != 0) ? (sum / count) : 0f;
		}

		private static void Build()
		{
			if (_groups != null)
			{
				return;
			}
			float rivetsPerGear = RivetsPerGear();
			List<InventoryDB.MetaData> items = Items();
			items.Sort((InventoryDB.MetaData a, InventoryDB.MetaData b) => a.levelRequired.CompareTo(b.levelRequired));
			List<CharacterDB.MetaData> parts = Parts();
			List<InventoryDB.MetaData> bronzeItems = items.FindAll((InventoryDB.MetaData i) => i.levelRequired <= kBronzeMaxItemLevel);
			List<InventoryDB.MetaData> silverItems = items.FindAll((InventoryDB.MetaData i) => i.levelRequired <= kSilverMaxItemLevel);
			if (bronzeItems.Count == 0)
			{
				bronzeItems = items.GetRange(0, Math.Max(1, items.Count / 3));
			}
			if (silverItems.Count == 0)
			{
				silverItems = items.GetRange(0, Math.Max(1, items.Count * 2 / 3));
			}
			List<InventoryDB.MetaData> byCost = new List<InventoryDB.MetaData>(items);
			byCost.Sort((InventoryDB.MetaData a, InventoryDB.MetaData b) => (b.softCost + b.hardCost * rivetsPerGear).CompareTo(a.softCost + a.hardCost * rivetsPerGear));
			List<InventoryDB.MetaData> platinumItems = byCost.GetRange(0, Math.Max(1, byCost.Count / 2));
			List<CharacterDB.MetaData> silverParts = parts.FindAll((CharacterDB.MetaData p) => p.unlockLevel <= kSilverMaxItemLevel);
			string[] names = new string[4] { "LootinatorTier0", "LootinatorTier1", "LootinatorTier2", "LootinatorTier3" };
			string[] tokens = new string[4] { "token_bronze", "token_silver", "token_gold", "token_platinum" };
			List<InventoryDB.MetaData>[] pools = new List<InventoryDB.MetaData>[4] { bronzeItems, silverItems, items, platinumItems };
			List<Box> boxes = new List<Box>();
			for (int t = 0; t < 4; t++)
			{
				Box box = new Box();
				box.name = names[t];
				box.token = tokens[t];
				box.minLevel = kTierMinLevel[t];
				AddItems(box, pools[t]);
				if (t == 1)
				{
					AddParts(box, silverParts);
				}
				if (t >= 2)
				{
					AddParts(box, parts);
				}
				int rivets;
				if (t < 3)
				{
					// Priced at the average rivet price of what the box can drop.
					box.softCost = Round(Average(pools[t], hard: false), 50);
					rivets = box.softCost;
				}
				else
				{
					box.hardCost = Round(Average(pools[t], hard: true), 1);
					rivets = Round(box.hardCost * rivetsPerGear, 50);
				}
				box.prizes.Add(new Prize { type = "sc", data = "sc", quantity = rivets });
				if (t >= 2)
				{
					box.prizes.Add(new Prize { type = "skillpoint", data = "skillpoint", quantity = 1 });
				}
				boxes.Add(box);
			}
			Box tutorial = new Box();
			tutorial.name = "tutorial";
			tutorial.token = TutorialToken;
			tutorial.minLevel = 0;
			AddItems(tutorial, bronzeItems);
			_groups = new Dictionary<string, List<Box>>();
			_groups["base"] = boxes;
			_groups["tutorial"] = new List<Box> { tutorial };
		}

		private static Hashtable PrizeTable(Prize prize, int index)
		{
			return new Hashtable
			{
				{ "id", index.ToString() },
				{ "type", prize.type },
				{ "data", prize.data }
			};
		}

		public static Hashtable GachaSet(string group)
		{
			Build();
			List<Box> boxes;
			if (!_groups.TryGetValue(group, out boxes))
			{
				boxes = new List<Box>();
			}
			ArrayList boxList = new ArrayList();
			ArrayList all = new ArrayList();
			foreach (Box box in boxes)
			{
				ArrayList prizes = new ArrayList();
				for (int i = 0; i < box.prizes.Count; i++)
				{
					prizes.Add(PrizeTable(box.prizes[i], i));
					all.Add(PrizeTable(box.prizes[i], i));
				}
				Hashtable table = new Hashtable();
				table["name"] = box.name;
				table["token"] = box.token;
				table["multiplier"] = 1;
				table["minLevel"] = box.minLevel;
				table["possiblePrizes"] = prizes;
				table["tokenc"] = new Hashtable { { "cost", 1 } };
				if (box.softCost > 0)
				{
					table["sc"] = new Hashtable { { "cost", box.softCost } };
				}
				if (box.hardCost > 0)
				{
					table["hc"] = new Hashtable { { "cost", box.hardCost } };
				}
				boxList.Add(table);
			}
			return new Hashtable
			{
				{ "name", group },
				{ "version", GachaVersion },
				{ "attracts", new ArrayList() },
				{ "possiblePrizes", all },
				{ "boxes", boxList }
			};
		}

		// Picks a prize uniformly from the box. Returns null for an unknown box.
		public static Hashtable Pick(string group, string boxName, out string token, out int softCost, out int hardCost, out string itemKey, out int itemCount)
		{
			Build();
			token = null;
			softCost = 0;
			hardCost = 0;
			itemKey = null;
			itemCount = 0;
			List<Box> boxes;
			if (!_groups.TryGetValue(group, out boxes))
			{
				return null;
			}
			Box box = boxes.Find((Box b) => b.name == boxName);
			if (box == null || box.prizes.Count == 0)
			{
				return null;
			}
			token = box.token;
			softCost = box.softCost;
			hardCost = box.hardCost;
			Prize prize = box.prizes[_random.Next(box.prizes.Count)];
			if (prize.type == "item")
			{
				itemKey = prize.data;
				itemCount = prize.quantity;
			}
			return new Hashtable
			{
				{ "type", prize.type },
				{ "data", prize.data },
				{ "quantity", prize.quantity }
			};
		}

		public static Hashtable Payouts()
		{
			if (_payouts == null)
			{
				_payouts = new ArrayList();
				for (int i = 0; i < kHardPacks.Length; i++)
				{
					Hashtable info = new Hashtable();
					info["payoutid"] = i + 1;
					info["thirdPartyId"] = "offline_gears_" + (i + 1);
					info["numOfIGC"] = kHardPacks[i];
					info["cost"] = 0;
					info["coststring"] = "FREE";
					info["show"] = true;
					info["visible"] = 1;
					info["art"] = i + 1;
					_payouts.Add(info);
				}
			}
			Hashtable data = new Hashtable();
			data["payoutSets"] = new ArrayList { new Hashtable { { "payouts", _payouts } } };
			return new Hashtable
			{
				{ "externalTrkid", "offline" },
				{ "data", data },
				{ "offers", new Hashtable { { "soft", SoftOffers() } } }
			};
		}

		public static int PayoutValue(int payoutId)
		{
			Payouts();
			foreach (Hashtable payout in _payouts)
			{
				if (Dot.Integer("payoutid", payout, 0) == payoutId)
				{
					return Dot.Integer("numOfIGC", payout, 0);
				}
			}
			return 0;
		}

		private static ArrayList SoftOffers()
		{
			if (_softOffers == null)
			{
				float rivetsPerGear = RivetsPerGear();
				_softOffers = new ArrayList();
				for (int i = 0; i < kSoftPackCosts.Length; i++)
				{
					_softOffers.Add(new Hashtable
					{
						{ "productId", "offline_rivets_" + (i + 1) },
						{ "value", Round(kSoftPackCosts[i] * rivetsPerGear * kSoftPackBonus[i], 50) },
						{ "cost", kSoftPackCosts[i] },
						{ "info", new Hashtable { { "art", i + 1 } } }
					});
				}
			}
			return _softOffers;
		}
	}
}
