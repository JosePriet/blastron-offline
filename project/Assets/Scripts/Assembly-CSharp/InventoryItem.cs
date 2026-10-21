using UnityEngine;

public class InventoryItem : MonoBehaviour
{
	public enum Category
	{
		Weapon = 0,
		Boost = 1,
		Count = 2
	}

	protected Category category;

	public bool IsDefault;

	public int Rivets;

	public int Gears;

	public int UnlockRivets;

	public int UnlockGears;

	public int StoreQuantity = 1;

	public int LevelRequired = 1;

	public int StartingCount;

	public int SortOrderOverride = -1;

	public string FriendlyName;

	public string Description;

	public int campaignPlusWeight;

	public int campaignPlusCount;

	public string uiTexturePath;

	public bool isHidden;

	public int kabamItemId;

	public bool isChampionshipTierReward;

	public int RevengeSaleQuantity;

	public string itemLevelPrefabName;

	public virtual Category ItemCategory
	{
		get
		{
			return category;
		}
	}

	public bool IsFree
	{
		get
		{
			return Rivets == 0 && Gears == 0;
		}
	}

	public virtual bool IsValid()
	{
		return false;
	}
}
