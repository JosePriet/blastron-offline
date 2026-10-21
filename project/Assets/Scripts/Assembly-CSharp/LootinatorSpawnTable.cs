using System;
using System.Collections.Generic;
using UnityEngine;

public class LootinatorSpawnTable : MonoBehaviour
{
	[Serializable]
	public class NumPicksEntry
	{
		public int numBonusPicks;

		public float weight;
	}

	[Serializable]
	public class LootEntry
	{
		public enum LootEntryType
		{
			Weapon = 0,
			CAC = 1
		}

		public LootEntryType type;

		public string name;

		public int quantity;

		public float weight;
	}

	public int baseNumPicks;

	public List<NumPicksEntry> pickEntries;

	public float totalPickEntryWeight;

	public List<LootEntry> lootEntries;

	public float totalLootEntryWeight;

	public List<LootEntry> GetRandomPicks()
	{
		List<LootEntry> list = new List<LootEntry>();
		int num = baseNumPicks;
		float num2 = UnityEngine.Random.Range(0f, totalPickEntryWeight);
		float num3 = 0f;
		foreach (NumPicksEntry pickEntry in pickEntries)
		{
			num3 += pickEntry.weight;
			if (num2 <= num3)
			{
				num += pickEntry.numBonusPicks;
				break;
			}
		}
		for (int i = 0; i < num; i++)
		{
			num2 = UnityEngine.Random.Range(0f, totalLootEntryWeight);
			num3 = 0f;
			LootEntry lootEntry = null;
			foreach (LootEntry lootEntry2 in lootEntries)
			{
				num3 += lootEntry2.weight;
				if (num2 <= num3)
				{
					list.Add(lootEntry2);
					lootEntry = lootEntry2;
					break;
				}
			}
			if (lootEntry != null && lootEntries.Count > 1)
			{
				lootEntries.Remove(lootEntry);
				totalLootEntryWeight -= lootEntry.weight;
			}
		}
		return list;
	}

	public int GetNumForBestPossiblePick()
	{
		int num = baseNumPicks;
		foreach (NumPicksEntry pickEntry in pickEntries)
		{
			int num2 = baseNumPicks + pickEntry.numBonusPicks;
			if (num < num2)
			{
				num = num2;
			}
		}
		return num;
	}
}
