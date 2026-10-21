using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class BeginnerSaleRewardsUI : PopupScript
{
	public class beginnerSaleData
	{
		public List<InventoryDB.MetaData> ItemsMetadata = new List<InventoryDB.MetaData>();

		public List<int> beginnerSaleAmmoCount = new List<int>();
	}

	public List<GameObject> rewardContainers = new List<GameObject>();

	private bool isTextureLoading;

	private beginnerSaleData saleData;

	private int ReallyBigNumberForCheckingInfinity = 99999;

	private void Awake()
	{
		Util.GetObjectExactMatch(base.gameObject, "ButtonRight").GetComponent<ButtonScript>().onButtonClick += Close;
	}

	private void Start()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "LabelPopupTitle");
		objectExactMatch.transform.localScale = new Vector3(46f, 46f, 0f);
	}

	public void SetParams(beginnerSaleData weaponsData, StoreCurrencyUI.PurchaseOption chosenOffer)
	{
		saleData = weaponsData;
		StartCoroutine(SetValues(weaponsData, chosenOffer));
	}

	private IEnumerator SetValues(beginnerSaleData weaponsData, StoreCurrencyUI.PurchaseOption chosenOffer)
	{
		int boostCount = 0;
		for (int i = 0; i < weaponsData.ItemsMetadata.Count; i++)
		{
			if (i < rewardContainers.Count)
			{
				UILabel countLabel = Util.GetObjectExactMatch(rewardContainers[i], "Vault_Count").GetComponent<UILabel>();
				int AmmoCount = weaponsData.beginnerSaleAmmoCount[i];
				if (AmmoCount > ReallyBigNumberForCheckingInfinity)
				{
					countLabel.text = "∞";
				}
				else
				{
					countLabel.text = AmmoCount.ToString();
				}
				UILabel nameLabel = Util.GetObjectExactMatch(rewardContainers[i], "ItemName").GetComponent<UILabel>();
				nameLabel.text = Localizer.GetString(weaponsData.ItemsMetadata[i].friendlyName);
				LoadTexture(texHolder: Util.GetObjectExactMatch(rewardContainers[i], "ItemTexture").GetComponent<UITexture>(), imagePath: weaponsData.ItemsMetadata[i].uiTexturePath, pathPrefix: "inventory");
				isTextureLoading = true;
				while (isTextureLoading)
				{
					yield return new WaitForEndOfFrame();
				}
			}
		}
	}

	private void Update()
	{
	}

	private void LoadTexture(string imagePath, UITexture texHolder, string pathPrefix, string imageType = ".jpg")
	{
		string url = Loader.GetBaseURL(pathPrefix) + imagePath + imageType;
		StartCoroutine(DoLoadTexure(url, texHolder));
	}

	private IEnumerator DoLoadTexure(string url, UITexture texHolder)
	{
		WWW www = new WWW(url);
		yield return www;
		OnTextureLoaded(www.texture, texHolder);
	}

	public void OnTextureLoaded(Texture2D t, UITexture texHolder)
	{
		if (texHolder != null && t != null)
		{
			texHolder.mainTexture = t;
			texHolder.enabled = true;
			isTextureLoading = false;
		}
	}

	protected override void OnTransitionOutComplete()
	{
		for (int i = 0; i < saleData.ItemsMetadata.Count; i++)
		{
			if (!saleData.ItemsMetadata[i].name.Contains("Boost"))
			{
				UITexture component = Util.GetObjectExactMatch(rewardContainers[i], "ItemTexture").GetComponent<UITexture>();
				ReleaseTexture(component);
			}
		}
		base.OnTransitionOutComplete();
	}

	private void ReleaseTexture(UITexture texHolder)
	{
		if (texHolder != null)
		{
			if (texHolder.mainTexture != null)
			{
				Object.Destroy(texHolder.mainTexture);
			}
			texHolder.mainTexture = null;
		}
	}
}
