using System;
using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class ChampionshipRewardTiers : MonoBehaviour
{
	[System.Serializable]
	public class Tier
	{
		public int NumStars;

		public string WeaponName;
	}

	public Tier[] Tiers = new Tier[0];

	public void Init()
	{
		Array.Sort(Tiers, (Tier t0, Tier t1) => t0.NumStars.CompareTo(t1.NumStars));
		Tier[] tiers = Tiers;
		foreach (Tier tier in tiers)
		{
			EB.Debug.Log(string.Format("Read championship reward tier: [{0}|{1}]", tier.NumStars, tier.WeaponName));
		}
	}

	public Tier GetNextTierFor(int numStars)
	{
		Tier[] tiers = Tiers;
		foreach (Tier tier in tiers)
		{
			if (tier.NumStars > numStars)
			{
				return tier;
			}
		}
		return null;
	}

	public Tier GetTierWithWeapon(string weaponName)
	{
		Tier[] tiers = Tiers;
		foreach (Tier tier in tiers)
		{
			if (tier.WeaponName.Equals(weaponName))
			{
				return tier;
			}
		}
		EB.Debug.LogError("Could not find a tier with weapon " + weaponName);
		return null;
	}

	public void GrantRewardForTier(Tier tier)
	{
		if (string.IsNullOrEmpty(tier.WeaponName))
		{
			EB.Debug.LogWarning("null or empty reward name for tier[" + tier.NumStars + "], skipping");
			return;
		}
		InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(tier.WeaponName));
		if (metaData == null)
		{
			EB.Debug.LogError("Failed to find weapon metadata for weapon " + tier.WeaponName);
		}
		Telemetry.Event("item/campaignstars", metaData.quantity, new Hashtable { { "name", metaData.name } });
		Telemetry.Flush();
		Hub.Instance.InventoryManager.AddItem(metaData.name, metaData.quantity);
		UnlocksManager.Instance.Unlock(metaData.name);
	}
}
