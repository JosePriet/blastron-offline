using SmoothMoves;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
	public enum SortOrderType
	{
		RemoveItem = 0,
		NewFreeItem = 1,
		FreeItem = 2,
		NewCoinsItem = 3,
		NewBucksItem = 4,
		CoinsItem = 5,
		BucksItem = 6
	}

	public CharacterDB.Category Category = CharacterDB.Category.kUnknown;

	public TextureAtlas TextureAtlas;

	public TextureAtlas FaceAtlas;

	public Material inGameMaterial;

	public Material uiMaterial;

	public Material inGameFaceMaterial;

	public Material uiFaceMaterial;

	public string CAC_UI_SpriteName;

	public Vector2 CAC_UI_SpritePivot = Vector2.zero;

	public string FriendlyName = string.Empty;

	public string Description = string.Empty;

	public int Bucks;

	public int Coins;

	public int UnlockLevel;

	public bool IsDefault;

	public bool IsHidden;

	public bool IsHiddenUnderAge;

	public bool IsRemove;

	public bool IsNew;

	public bool IsChampionshipBoss;

	public bool IsEpic;

	public int XP;

	public int numUpgradeStars = 5;

	public int OverideSortOrder = -1;

	public PartAttributes Attributes;

	public int Price
	{
		get
		{
			return Mathf.Max(Coins, Bucks);
		}
	}

	public SortOrderType SortOrder
	{
		get
		{
			if (IsRemove)
			{
				return SortOrderType.RemoveItem;
			}
			if (IsNew)
			{
				if (Bucks > 0)
				{
					return SortOrderType.NewBucksItem;
				}
				if (Coins > 0)
				{
					return SortOrderType.NewCoinsItem;
				}
				return SortOrderType.NewFreeItem;
			}
			if (Bucks > 0)
			{
				return SortOrderType.BucksItem;
			}
			if (Coins > 0)
			{
				return SortOrderType.CoinsItem;
			}
			return SortOrderType.FreeItem;
		}
	}

	public bool IsValid()
	{
		return Category != CharacterDB.Category.kUnknown;
	}

	public static int Compare(CharacterData data1, CharacterData data2)
	{
		SortOrderType sortOrder = data1.SortOrder;
		SortOrderType sortOrder2 = data2.SortOrder;
		int overideSortOrder = data1.OverideSortOrder;
		int overideSortOrder2 = data2.OverideSortOrder;
		if (overideSortOrder != overideSortOrder2)
		{
			return (overideSortOrder >= overideSortOrder2) ? 1 : (-1);
		}
		if (sortOrder == sortOrder2)
		{
			return CompareValue(sortOrder, data1, data2);
		}
		return (sortOrder >= sortOrder2) ? 1 : (-1);
	}

	public static int CompareValue(SortOrderType sort, CharacterData data1, CharacterData data2)
	{
		int price = data1.Price;
		int price2 = data2.Price;
		if (price != price2)
		{
			return (price > price2) ? 1 : (-1);
		}
		int overideSortOrder = data1.OverideSortOrder;
		int overideSortOrder2 = data2.OverideSortOrder;
		if (overideSortOrder != overideSortOrder2)
		{
			return (overideSortOrder >= overideSortOrder2) ? 1 : (-1);
		}
		int num = string.Compare(data1.FriendlyName, data2.FriendlyName);
		if (num != 0)
		{
			return num;
		}
		return string.Compare(data1.name, data2.name);
	}
}
