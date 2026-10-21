using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class DailyOffersUIItem : MonoBehaviour
{
	public DailyOffer.MetaData itemMetadata;

	private DailyOffer m_offerData;

	private string m_rewardTexturePath;

	private GameObject m_offerPrefab;

	private UISlicedSprite m_backgroundFill;

	private GameObject m_collectedPanel;

	private UITexture m_rewardTexture;

	private UILabel m_rewardText;

	private UILabel m_dayLabel;

	private Color m_bgUnselectedColor;

	private Color m_bgSelectedColor = new Color(0.368f, 1f, 0f, 1f);

	private void Awake()
	{
		m_backgroundFill = Util.GetObjectExactMatch(base.gameObject, "Fill").GetComponent<UISlicedSprite>();
		m_collectedPanel = Util.GetObjectExactMatch(base.gameObject, "Layer 2 - CollectedPanel");
		m_rewardTexture = Util.GetObjectExactMatch(base.gameObject, "RewardTextureHolder").GetComponent<UITexture>();
		m_rewardText = Util.GetObjectExactMatch(base.gameObject, "RewardText").GetComponent<UILabel>();
		m_dayLabel = Util.GetObjectExactMatch(base.gameObject, "DayLabel").GetComponent<UILabel>();
	}

	private void Start()
	{
		m_bgUnselectedColor = m_backgroundFill.color;
	}

	public void RefreshFromMetadata()
	{
		if (null != m_offerPrefab)
		{
			Assets.Unload(m_offerPrefab);
		}
		m_dayLabel.text = itemMetadata.day.ToString();
		bool flag = itemMetadata.day <= Profile.Instance.DailyOffersCollected;
		SetCurrentTarget(itemMetadata.day == Profile.Instance.DailyOffersCollected + 1);
		SetCollected(flag);
		m_offerPrefab = Assets.Load(itemMetadata.assetPrefabPath) as GameObject;
		DailyOffer dailyOffer = null;
		DailyOffer[] components = m_offerPrefab.GetComponents<DailyOffer>();
		if (components.Length > 1)
		{
			DailyOffer[] array = components;
			foreach (DailyOffer dailyOffer2 in array)
			{
				if (dailyOffer2.type.Equals(DailyOffer.Type.kCAC))
				{
					if (Hub.Instance.InventoryManager.GetCount(dailyOffer2.offerPrefab.name) == 0 || flag)
					{
						dailyOffer = dailyOffer2;
						break;
					}
				}
				else
				{
					dailyOffer = dailyOffer2;
				}
			}
		}
		else
		{
			dailyOffer = components[0];
		}
		if (m_offerData == dailyOffer)
		{
			return;
		}
		m_offerData = dailyOffer;
		ReleaseRewardTexture();
		if (null != m_offerData.overrideUiTexture)
		{
			DisplayRewardTexture(m_offerData.overrideUiTexture);
		}
		else
		{
			switch (m_offerData.type)
			{
			case DailyOffer.Type.kWeapon:
				LoadRewardTexture(m_offerData.offerPrefab.GetComponent<Weapon>().uiTexturePath);
				break;
			case DailyOffer.Type.kCAC:
				LoadRewardTexture("cac_ui_parts/" + m_offerData.offerPrefab.GetComponent<CharacterData>().CAC_UI_SpriteName);
				break;
			}
		}
		string text = m_offerData.uiDescription;
		if (string.IsNullOrEmpty(text))
		{
			switch (m_offerData.type)
			{
			case DailyOffer.Type.kSpinToken:
				EB.Debug.LogError("Need a valid uiDescription string for spin tokens!");
				break;
			case DailyOffer.Type.kCAC:
			{
				CharacterData component2 = m_offerData.offerPrefab.GetComponent<CharacterData>();
				CharacterDB.MetaData metaData2 = CharacterDB.GetMetaData(Hash.StringHash(component2.name));
				text = Localizer.GetString(metaData2.friendlyName);
				break;
			}
			case DailyOffer.Type.kHardCurrency:
				text = Localizer.GetString("ID_UI_DAILYOFFERS_GEARS");
				break;
			case DailyOffer.Type.kSoftCurrency:
				text = Localizer.GetString("ID_UI_DAILYOFFERS_RIVETS");
				break;
			case DailyOffer.Type.kWeapon:
			{
				InventoryItem component = m_offerData.offerPrefab.GetComponent<InventoryItem>();
				InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(component.name));
				text = Localizer.GetString(metaData.friendlyName);
				break;
			}
			}
		}
		if (m_offerData.quantity > 1)
		{
			m_rewardText.text = Localizer.Format("ID_UI_DAILYOFFERS_ITEM_QUANTITY_FORMAT", text, m_offerData.quantity);
		}
		else
		{
			m_rewardText.text = text;
		}
	}

	private void LoadRewardTexture(string texturePath)
	{
		ReleaseRewardTexture();
		m_rewardTexturePath = texturePath;
		TexturePoolManager.Instance.LoadTexture(texturePath, DisplayRewardTexture);
	}

	private void DisplayRewardTexture(Texture2D t)
	{
		m_rewardTexture.mainTexture = t;
		Vector3 localScale = new Vector3(t.width, t.height, 1f);
		float num = Mathf.Max(localScale.x, localScale.y);
		if (num > 123f)
		{
			float num2 = localScale.x / localScale.y;
			if (localScale.x > 123f)
			{
				localScale.x = 123f;
				localScale.y = localScale.x / num2;
			}
			else
			{
				localScale.y = 123f;
				localScale.x = localScale.y * num2;
			}
		}
		m_rewardTexture.transform.localScale = localScale;
	}

	private void ReleaseRewardTexture()
	{
		if (!string.IsNullOrEmpty(m_rewardTexturePath))
		{
			TexturePoolManager.Instance.ReleaseTexture(m_rewardTexturePath);
			m_rewardTexturePath = null;
		}
	}

	public void OnClose()
	{
		ReleaseRewardTexture();
		m_offerPrefab = null;
		Assets.Unload(itemMetadata.assetPrefabPath);
	}

	public void SetCurrentTarget(bool bCurrent)
	{
		base.gameObject.transform.localScale = ((!bCurrent) ? new Vector3(1f, 1f, 1f) : new Vector3(1.05f, 1.05f, 1f));
		Vector3 localPosition = base.gameObject.transform.localPosition;
		localPosition.y = ((!bCurrent) ? 60f : 70f);
		base.gameObject.transform.localPosition = localPosition;
		m_backgroundFill.color = ((!bCurrent) ? m_bgUnselectedColor : m_bgSelectedColor);
		m_dayLabel.color = ((!bCurrent) ? new Color(1f, 1f, 1f, 1f) : m_bgSelectedColor);
	}

	public void SetCollected(bool bCollected)
	{
		m_collectedPanel.SetActive(bCollected);
	}

	public void OnCollect()
	{
		switch (m_offerData.type)
		{
		case DailyOffer.Type.kSoftCurrency:
			Profile.Instance.AddSoftCurrency(m_offerData.quantity);
			EB.Debug.Log("DailyOfferUIItem.OnCollect awarded " + m_offerData.quantity + " rivets");
			break;
		case DailyOffer.Type.kHardCurrency:
			Hub.Instance.WalletManager.Credit(m_offerData.quantity, "dailyOffer");
			EB.Debug.Log("DailyOfferUIItem.OnCollect awarded " + m_offerData.quantity + " gears");
			break;
		case DailyOffer.Type.kWeapon:
			EB.Debug.Log("DailyOfferUIItem.OnCollect awarded " + m_offerData.quantity + " " + m_offerData.offerPrefab.gameObject.name);
			if (InventoryDB.GetMetaData(Hash.StringHash(m_offerData.offerPrefab.name)) == null)
			{
				EB.Debug.LogError("invalid weapon specified for award: " + m_offerData.offerPrefab.name);
				break;
			}
			Hub.Instance.InventoryManager.AddItem(m_offerData.offerPrefab.name, m_offerData.quantity);
			Telemetry.Event("item/daily", m_offerData.quantity, new Hashtable { 
			{
				"name",
				m_offerData.offerPrefab.name
			} });
			break;
		case DailyOffer.Type.kCAC:
			EB.Debug.Log("DailyOfferUIItem.OnCollect awarded " + m_offerData.quantity + " " + m_offerData.offerPrefab.gameObject.name);
			if (CharacterDB.GetMetaData(Hash.StringHash(m_offerData.offerPrefab.name)) == null)
			{
				EB.Debug.LogError("invalid CAC part specified for award: " + m_offerData.offerPrefab.name);
				break;
			}
			Hub.Instance.InventoryManager.AddItem(m_offerData.offerPrefab.name, m_offerData.quantity);
			Telemetry.Event("cac/daily", m_offerData.quantity, new Hashtable { 
			{
				"name",
				m_offerData.offerPrefab.name
			} });
			break;
		case DailyOffer.Type.kSpinToken:
			EB.Debug.Log("DailyOfferUIItem.OnCollect awarded " + m_offerData.quantity + " " + GetSpinTokenName(m_offerData.spinTokenType) + " spin token");
			GachaManager.Instance.AddFreeSpin((int)m_offerData.spinTokenType);
			break;
		}
		Profile.Instance.IncrementDailyOffersCollection();
		SetCollected(true);
	}

	private string GetSpinTokenName(DailyOffer.SpinTokenType tokenType)
	{
		switch (tokenType)
		{
		case DailyOffer.SpinTokenType.kBronze:
			return "bronze";
		case DailyOffer.SpinTokenType.kSilver:
			return "silver";
		case DailyOffer.SpinTokenType.kGold:
			return "gold";
		case DailyOffer.SpinTokenType.kPlatinum:
			return "platinum";
		default:
			return "INVALID";
		}
	}
}
