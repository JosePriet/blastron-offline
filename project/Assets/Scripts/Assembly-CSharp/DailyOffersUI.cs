using System.Collections.Generic;
using EB;
using SmoothMoves;
using UnityEngine;

public class DailyOffersUI : PopupScript
{
	public GameObject itemPrefab;

	public GameObject cacGridItemPrefab;

	private DailyOffersUIItem[] m_items = new DailyOffersUIItem[7];

	private int m_idxCurrentOffer;

	private GameObject m_itemHolder;

	private GameObject m_collectButton;

	private GameObject m_collectLabelObj;

	private UISlicedSprite m_collectBackground;

	private Color m_collectBackgroundActiveColor = new Color(0.4f, 1f, 0f, 1f);

	private Color m_collectBackgroundInactiveColor = Color.red;

	private GameObject m_collectInLabelObj;

	private GameObject m_collectTimerLabelObj;

	private UILabel m_collectTimerLabel;

	private GameObject m_robotHead;

	private UISlider m_progressBar;

	private UISprite[] m_progressNodes = new UISprite[7];

	private Color m_progressNodeActiveColor = new Color(0.4f, 1f, 0f, 1f);

	private Color m_progressNodeInactiveColor = new Color(0.214f, 0.214f, 0.214f, 1f);

	public TextureAtlas headAtlas;

	public string headTextureName;

	public Vector3 headScale;

	public TextureAtlas faceAtlas;

	public string faceTextureName;

	public Vector3 faceScale;

	public Vector3 faceOffset;

	public string tutorText;

	private List<string> m_texturePaths = new List<string>();

	private List<GameObject> cacAnchors = new List<GameObject>();

	private List<CharacterDB.MetaData> cacMetadata = new List<CharacterDB.MetaData>();

	protected override void OnAwake()
	{
		base.OnAwake();
		m_collectButton = Util.GetObjectExactMatch(base.gameObject, "ButtonCollect");
		m_itemHolder = Util.GetObjectExactMatch(base.gameObject, "ItemHolder");
		m_collectLabelObj = Util.GetObjectExactMatch(m_collectButton, "Collect");
		m_collectBackground = Util.GetObjectExactMatch(m_collectButton, "Background").GetComponent<UISlicedSprite>();
		m_collectInLabelObj = Util.GetObjectExactMatch(m_collectButton, "CollectIn");
		m_collectTimerLabelObj = Util.GetObjectExactMatch(m_collectButton, "CollectInTimer");
		m_collectTimerLabel = m_collectTimerLabelObj.GetComponent<UILabel>();
		m_robotHead = Util.GetObjectExactMatch(base.gameObject, "RobotHead");
		m_progressBar = Util.GetObjectExactMatch(base.gameObject, "ProgressBar").GetComponent<UISlider>();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ProgressNodes");
		for (int i = 1; i < 8; i++)
		{
			string text = "Node" + i;
			m_progressNodes[i - 1] = Util.GetObjectExactMatch(objectExactMatch, text).GetComponent<UISprite>();
		}
		UITexture headTexture = m_robotHead.GetComponent<UITexture>();
		int part = CharacterManager.Instance.LocalPlayer.recipe.GetPart(CharacterDB.Category.kHead);
		CharacterDB.MetaData metaDataOrDefault = CharacterDB.GetMetaDataOrDefault(part, CharacterDB.Category.kHead);
		if (metaDataOrDefault != null)
		{
			string text2 = "cac_ui_parts/" + metaDataOrDefault.cacTexturePath;
			m_texturePaths.Add(text2);
			TexturePoolManager.Instance.LoadTexture(text2, SafeAction.Wrap(this, (Texture2D t) =>
			{
				headTexture.mainTexture = t;
				headTexture.transform.localScale = new Vector3(64f, 64f, 1f);
			}));
		}
		ButtonScript component = m_collectButton.GetComponent<ButtonScript>();
		component.onButtonClick += OnCollect;
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "RobotParts");
		cacAnchors.AddRange(Util.GetObjects(objectExactMatch2, "Week"));
		cacAnchors.Sort(CompareAnchors);
	}

	public static int CompareAnchors(GameObject a1, GameObject a2)
	{
		return a1.name.CompareTo(a2.name);
	}

	protected override void OnStart()
	{
		base.OnStart();
		for (int i = 0; i < 7; i++)
		{
			GameObject gameObject = Object.Instantiate(itemPrefab) as GameObject;
			gameObject.transform.parent = m_itemHolder.transform;
			gameObject.name = itemPrefab.name + (i + 1);
			gameObject.transform.localPosition = new Vector3(getXPos(i), 60f, -5f);
			gameObject.transform.localScale = Vector3.one;
			m_items[i] = gameObject.GetComponent<DailyOffersUIItem>();
		}
		LoadDailyOffers();
		LoadCacItems();
		RefreshCacState();
		ScreenManager.Instance.FadeOut(null);
	}

	private void ReleaseResources()
	{
		foreach (string texturePath in m_texturePaths)
		{
			TexturePoolManager.Instance.ReleaseTexture(texturePath);
		}
	}

	protected override void OnTransitionOutComplete()
	{
		for (int i = 0; i < 7; i++)
		{
			DailyOffersUIItem dailyOffersUIItem = m_items[i];
			dailyOffersUIItem.OnClose();
		}
		ReleaseResources();
		Profile.Instance.DailyOffersSeen = true;
		base.OnTransitionOutComplete();
	}

	private float getXPos(int idx)
	{
		return -426 + idx * 142;
	}

	private void LoadDailyOffers()
	{
		m_idxCurrentOffer = Profile.Instance.DailyOffersCollected % 7;
		int num = Profile.Instance.DailyOffersCollected / 7 * 7;
		for (int i = 0; i < 7; i++)
		{
			DailyOffer.MetaData itemAt = DailyOffersDB.GetItemAt(num);
			m_items[i].itemMetadata = itemAt;
			m_items[i].RefreshFromMetadata();
			num++;
		}
		UpdateUIElementPositioning();
	}

	private void LoadTutorialBot()
	{
		if (faceAtlas == null || headAtlas == null)
		{
			EB.Debug.LogError("no head or face atlas specified, no cookie for you sir!");
			return;
		}
		if (!string.IsNullOrEmpty(headTextureName))
		{
			Material material = new Material(headAtlas.material);
			material.shader = Shader.Find("Unlit/Transparent Colored");
			UITexture component = Util.GetObjectExactMatch(base.gameObject, "HeadTex").GetComponent<UITexture>();
			component.material = material;
			string textureGUIDFromName = headAtlas.GetTextureGUIDFromName(headTextureName);
			int textureIndex = headAtlas.GetTextureIndex(textureGUIDFromName);
			if (textureIndex >= 0)
			{
				Rect uvRect = headAtlas.uvs[textureIndex];
				component.uvRect = uvRect;
				if (headScale != Vector3.zero)
				{
					component.transform.localScale = headScale;
				}
			}
			else
			{
				EB.Debug.LogError(string.Format("could not find texture '{0}' in head atlas '{1}'", headTextureName, headAtlas.name));
			}
		}
		if (!string.IsNullOrEmpty(faceTextureName))
		{
			string textureGUIDFromName2 = faceAtlas.GetTextureGUIDFromName(faceTextureName);
			int textureIndex2 = faceAtlas.GetTextureIndex(textureGUIDFromName2);
			if (textureIndex2 >= 0)
			{
				Material material2 = new Material(faceAtlas.material);
				material2.shader = Shader.Find("Unlit/Transparent Colored");
				UITexture component2 = Util.GetObjectExactMatch(base.gameObject, "FaceTex").GetComponent<UITexture>();
				component2.material = material2;
				Rect uvRect2 = faceAtlas.uvs[textureIndex2];
				component2.uvRect = uvRect2;
				if (faceScale != Vector3.zero)
				{
					component2.transform.localScale = faceScale;
				}
				component2.transform.localPosition = component2.transform.localPosition + faceOffset;
			}
			else
			{
				EB.Debug.LogError(string.Format("could not find texture '{0}' in face atlas '{1}'", faceTextureName, faceAtlas.name));
			}
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "TutorBot");
		UILabel component3 = Util.GetObjectExactMatch(objectExactMatch, "Advice").GetComponent<UILabel>();
		if (tutorText.StartsWith("ID_"))
		{
			component3.text = Localizer.GetString(tutorText);
		}
		else
		{
			component3.text = tutorText;
		}
		component3 = Util.GetObjectExactMatch(objectExactMatch, "CollectHeader").GetComponent<UILabel>();
		component3.text = string.Format("[{0}]{1}: [{2}]{3}", NGUITools.EncodeColor(new Color(0.35f, 1f, 0f, 1f)), Localizer.GetString("ID_UI_DAILYOFFERS_COLLECTIBLE"), NGUITools.EncodeColor(new Color(0.93f, 0.8f, 0f, 1f)), Localizer.GetString("ID_UI_DAILYOFFERS_PARTSNAME"));
	}

	private void LoadCacItems()
	{
		cacMetadata.Add(CharacterDB.GetMetaData(Hash.StringHash("mechbot_legs_1")));
		cacMetadata.Add(CharacterDB.GetMetaData(Hash.StringHash("mechbot_torso_1")));
		cacMetadata.Add(CharacterDB.GetMetaData(Hash.StringHash("mechbot_head_1")));
		for (int i = 0; i < cacMetadata.Count; i++)
		{
			GameObject obj = cacAnchors[i];
			UILabel uILabel = Util.FindComponent<UILabel>(obj);
			UITexture texture = Util.FindComponent<UITexture>(obj);
			string text = "cac_ui_parts/" + cacMetadata[i].cacTexturePath;
			m_texturePaths.Add(text);
			TexturePoolManager.Instance.LoadTexture(text, SafeAction.Wrap(this, (Texture2D t) =>
			{
				texture.mainTexture = t;
				texture.transform.localScale = new Vector3(128f, 128f, 1f);
			}));
		}
	}

	private void UpdateUIElementPositioning()
	{
		Vector3 localPosition = m_collectButton.transform.localPosition;
		localPosition.x = getXPos(m_idxCurrentOffer);
		m_collectButton.transform.localPosition = localPosition;
		if (m_idxCurrentOffer > 0)
		{
			m_robotHead.SetActive(true);
			localPosition = m_robotHead.transform.localPosition;
			localPosition.x = getXPos(m_idxCurrentOffer - 1);
			m_robotHead.transform.localPosition = localPosition;
		}
		else
		{
			m_robotHead.SetActive(false);
		}
		m_progressBar.sliderValue = (float)Mathf.Max(0, m_idxCurrentOffer - 1) / 6f;
		for (int i = 0; i < 7; i++)
		{
			m_progressNodes[i].color = ((m_idxCurrentOffer <= i) ? m_progressNodeInactiveColor : m_progressNodeActiveColor);
			if (i == 6)
			{
				m_progressNodes[i].color = Color.white;
			}
		}
	}

	public void OnCollect(GameObject caller)
	{
		if (IsCollectible())
		{
			m_items[m_idxCurrentOffer].OnCollect();
			m_items[m_idxCurrentOffer].SetCurrentTarget(false);
			m_idxCurrentOffer++;
			if (m_idxCurrentOffer < 7)
			{
				m_items[m_idxCurrentOffer].SetCurrentTarget(true);
				UpdateUIElementPositioning();
			}
			else if (Profile.Instance.DailyOffersCollected < 21)
			{
				LoadDailyOffers();
				RefreshCacState();
			}
			else
			{
				UpdateUIElementPositioning();
				m_collectButton.SetActive(false);
			}
			Profile.Instance.Save();
		}
	}

	private void RefreshCacState()
	{
		for (int i = 0; i < cacMetadata.Count; i++)
		{
			GameObject obj = cacAnchors[i];
			UITexture uITexture = Util.FindComponent<UITexture>(obj);
			UILabel uILabel = Util.FindComponent<UILabel>(obj);
			if (cacMetadata[i].IsOwned)
			{
				uILabel.text = Localizer.GetString("ID_UI_DAILYOFFERS_COLLECTED");
				uITexture.color = new Color(0.35f, 0.35f, 0.35f, 1f);
			}
			else
			{
				uILabel.text = Localizer.Format("ID_UI_DAILYOFFERS_WEEK", i + 1);
				uITexture.color = Color.white;
			}
		}
	}

	private bool IsCollectible()
	{
		return Profile.Instance.DailyOffersNextTime <= EB.Time.Now;
	}

	private void Update()
	{
		if (!IsCollectible())
		{
			m_collectLabelObj.SetActive(false);
			m_collectInLabelObj.SetActive(true);
			m_collectTimerLabelObj.SetActive(true);
			m_collectBackground.color = m_collectBackgroundInactiveColor;
			m_collectTimerLabel.text = Localizer.FormatTime(Profile.Instance.DailyOffersNextTime - EB.Time.Now, true);
		}
		else
		{
			m_collectLabelObj.SetActive(true);
			m_collectInLabelObj.SetActive(false);
			m_collectTimerLabelObj.SetActive(false);
			m_collectBackground.color = m_collectBackgroundActiveColor;
		}
	}
}
