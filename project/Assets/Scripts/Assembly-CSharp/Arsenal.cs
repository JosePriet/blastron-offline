using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sparx;
using UnityEngine;

public static class Arsenal
{
	private delegate void FoundObject(GameObject obj, int index);

	private static List<InventoryItem>[] _itemLists;

	private static int _emptyWeaponIndex;

	public static int EmptyWeaponIndex
	{
		get
		{
			return _emptyWeaponIndex;
		}
	}

	public static void Load()
	{
		if (_itemLists == null)
		{
			_itemLists = new List<InventoryItem>[2];
			LoadArsenal();
			LoadBoosts();
		}
	}

	public static void Unload()
	{
	}

	public static Weapon WeaponAt(int index)
	{
		return _itemLists[0][index] as Weapon;
	}

	public static BoostItem BoostAt(int index)
	{
		return _itemLists[1][index] as BoostItem;
	}

	public static InventoryItem InventoryItemWithName(InventoryItem.Category eCat, string name)
	{
		return _itemLists[(int)eCat].Find((InventoryItem ii) => ii.name == name);
	}

	public static int NumItems(InventoryItem.Category eCat)
	{
		return _itemLists[(int)eCat].Count;
	}

	public static string ItemName(InventoryItem.Category eCat, int index)
	{
		return _itemLists[(int)eCat][index].name;
	}

	public static int GetWeaponIndex(string name)
	{
		name = Hub.Instance.InventoryManager.SafeName(name);
		for (int i = 0; i < _itemLists[0].Count; i++)
		{
			if (Hub.Instance.InventoryManager.SafeName(_itemLists[0][i].name) == name)
			{
				return i;
			}
		}
		return -1;
	}

	private static void LoadArsenal()
	{
		_itemLists[0] = new List<InventoryItem>();
		LoadDebugItems("Inventory/Weapons", (GameObject obj, int index) =>
		{
			EB.Replication.Manager.RegisterPrefab(obj);
			Weapon component = obj.GetComponent<Weapon>();
			if (component != null)
			{
				component.Init();
				_itemLists[0].Add(component);
				if (component.name.Equals("Empty"))
				{
					_emptyWeaponIndex = index;
				}
			}
		});
	}

	private static void LoadBoosts()
	{
		_itemLists[1] = new List<InventoryItem>();
		LoadDebugItems("Inventory/Boosts", (GameObject obj, int index) =>
		{
			EB.Replication.Manager.RegisterPrefab(obj);
			BoostItem component = obj.GetComponent<BoostItem>();
			if (component != null)
			{
				_itemLists[1].Add(component);
			}
		});
	}

	private static void LoadDebugItems(string path, FoundObject fnCallback)
	{
		Object[] array = Assets.LoadAll(path, typeof(GameObject));
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				fnCallback(array[i] as GameObject, i);
			}
		}
	}
}
