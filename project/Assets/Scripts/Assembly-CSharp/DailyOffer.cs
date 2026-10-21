using EB;
using UnityEngine;

public class DailyOffer : MonoBehaviour
{
	[Serializable]
	public class MetaData
	{
		[Serializable(Tag = "d")]
		public int day;

		[Serializable(Tag = "p")]
		public string assetPrefabPath;
	}

	public enum Type
	{
		kSoftCurrency = 0,
		kHardCurrency = 1,
		kWeapon = 2,
		kCAC = 3,
		kSpinToken = 4
	}

	public enum SpinTokenType
	{
		kBronze = 0,
		kSilver = 1,
		kGold = 2,
		kPlatinum = 3
	}

	public int stage;

	public Type type;

	public int quantity;

	public string uiDescription;

	public SpinTokenType spinTokenType;

	public GameObject offerPrefab;

	public Texture2D overrideUiTexture;

	public bool isValid()
	{
		bool result = true;
		if (string.IsNullOrEmpty(uiDescription) && (type == Type.kCAC || type == Type.kSpinToken))
		{
			EB.Debug.LogError(base.gameObject.name + " missing UI description text");
			result = false;
		}
		if (quantity <= 0)
		{
			EB.Debug.LogError(base.gameObject.name + " invalid quantity, must be positive non-zero");
			result = false;
		}
		if ((type == Type.kSoftCurrency || type == Type.kHardCurrency || type == Type.kSpinToken) && null == overrideUiTexture)
		{
			EB.Debug.LogError(base.gameObject.name + " missing UI texture for currency reward");
			result = false;
		}
		if ((type == Type.kWeapon || type == Type.kCAC) && null == offerPrefab)
		{
			EB.Debug.LogError(base.gameObject.name + " missing offer prefab");
			result = false;
		}
		return result;
	}
}
