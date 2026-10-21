using System.Collections.Generic;
using EB;
using UnityEngine;

public class DailyOffersDB : MonoBehaviour
{
	private static List<DailyOffer.MetaData> _metaData;

	static DailyOffersDB()
	{
		_metaData = new List<DailyOffer.MetaData>();
	}

	public static void LoadMetaData()
	{
		List<DailyOffer.MetaData> result = null;
		TextAsset textAsset = Assets.Load<TextAsset>("dailyoffersdb");
		object value = JSON.Parse(textAsset.text);
		if (Serialization.Deserialize(value, ref result))
		{
			foreach (DailyOffer.MetaData item in result)
			{
				_metaData.Add(item);
			}
		}
		Assets.Unload(textAsset);
		UnityEngine.Debug.Log("Loaded " + _metaData.Count + " daily offer items");
	}

	public static List<DailyOffer.MetaData> GetAllMetaData()
	{
		return _metaData;
	}

	public static DailyOffer.MetaData GetItemAt(int index)
	{
		if (index >= 0 && index < _metaData.Count)
		{
			return _metaData[index];
		}
		return null;
	}
}
