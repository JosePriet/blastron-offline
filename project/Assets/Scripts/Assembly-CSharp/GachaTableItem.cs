using System.Collections.Generic;
using EB;
using EB.Collections;
using EB.Sparx;
using UnityEngine;

public class GachaTableItem : MonoBehaviour
{
	public enum Selection
	{
		SelectedHard = 0,
		SelectedSoft = 1,
		SelectedFree = 2,
		Unselected = 3
	}

	public List<UIWidget> tintableElements;

	public List<GameObject> tokenPrefabs;

	public GachaUI gachaUI;

	public Color highlightedTileBgColor;

	public Color highlightedInnerBgColor;

	public Color highlightedBgOverlayColor;

	public Color highlightedStarburstColor;

	private Selection _selected;

	private GameObject inactiveLayout;

	private GameObject lockedLayout;

	private GameObject singleButtonLayout;

	private GameObject dualButtonLayout;

	private GameObject diamondInactiveLayout;

	private GameObject activeLayout;

	private GameObject tokenInstance;

	private GameObject banner;

	private UIWidget tileBg;

	private LerpColorAnimation tileBgLerp;

	private UIWidget innerBg;

	private LerpColorAnimation innerBgLerp;

	private UIWidget bgOverlay;

	private LerpColorAnimation bgOverlayLerp;

	private LerpColorAnimation[] starburstColorLerps;

	private GameObject hardCurrencyBtn;

	private ButtonScript hardBtnScript;

	private UISprite hardCurrencyBtnHighlight;

	private GameObject freeCurrencyBtn;

	private ButtonScript freeBtnScript;

	private GameObject softCurrencyBtn;

	private UISprite softCurrencyBtnHighlight;

	private ButtonScript softBtnScript;

	private RotateAtSpeed glowRotator;

	private RotateAtSpeed tokenRotator;

	private GameObject spinCounter;

	private UILabel spinCounterLabel;

	private UILabel multiplierLabel;

	private UIWidget freeBtnWidget;

	private UILabel bannerLabel;

	private GameObject diamondGraphicContainer;

	private GameObject diamondIconInstance;

	private Color unhighlightedTileBgColor;

	private Color unhighlightedInnerBgColor;

	private Color unhighlightedBgOverlayColor;

	private Color btnDefaultColor = Color.black;

	private bool isInteractive = true;

	private SafeInt numFreeSpins = 0;

	private UITutorialArrow tutorialArrow;

	private Vector3 defaultMultiplierLabelSize = Vector3.zero;

	private Quaternion initialTokenRotation;

	private bool isTokenRotating;

	public GachaBox Box { get; private set; }

	public bool IsFree
	{
		get
		{
			return (int)numFreeSpins > 0 && (int)numFreeSpins >= Box.Tokens.Cost;
		}
	}

	public bool IsLocked
	{
		get
		{
			return Profile.Instance.Level < Box.MinLevel;
		}
	}

	public bool CanPaySoft
	{
		get
		{
			return Box != null && Box.SoftCurrency.Cost > 0;
		}
	}

	public bool CanPayHard
	{
		get
		{
			return Box != null && Box.HardCurrency.Cost > 0;
		}
	}

	public bool IsDiamond
	{
		get
		{
			return Box != null && Box.Token == "token_diamond";
		}
	}

	public Selection Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			EB.Debug.Log("GachaTableItem > Selected > " + value);
			_selected = value;
			bool flag = _selected != Selection.Unselected;
			Rotate(flag);
			if (flag && gachaUI != null)
			{
				gachaUI.SetSelectedTable(this);
			}
			tileBgLerp.startPoint = tileBg.color;
			tileBgLerp.endPoint = ((!flag) ? unhighlightedTileBgColor : highlightedTileBgColor);
			tileBgLerp.animationDuration = Tuning.Instance.Gacha_Table_ColorBlendTime;
			tileBgLerp.BeginAnimation();
			innerBgLerp.startPoint = innerBg.color;
			innerBgLerp.endPoint = ((!flag) ? unhighlightedInnerBgColor : highlightedInnerBgColor);
			innerBgLerp.animationDuration = Tuning.Instance.Gacha_Table_ColorBlendTime;
			innerBgLerp.BeginAnimation();
			bgOverlayLerp.startPoint = bgOverlay.color;
			bgOverlayLerp.endPoint = ((!flag) ? unhighlightedBgOverlayColor : highlightedBgOverlayColor);
			bgOverlayLerp.animationDuration = Tuning.Instance.Gacha_Table_ColorBlendTime;
			bgOverlayLerp.BeginAnimation();
			if (starburstColorLerps != null)
			{
				LerpColorAnimation[] array = starburstColorLerps;
				foreach (LerpColorAnimation lerpColorAnimation in array)
				{
					lerpColorAnimation.startPoint = lerpColorAnimation.gameObject.GetComponent<UIWidget>().color;
					lerpColorAnimation.endPoint = ((!flag) ? Box.Tint : highlightedStarburstColor);
					lerpColorAnimation.animationDuration = Tuning.Instance.Gacha_Table_ColorBlendTime;
					lerpColorAnimation.BeginAnimation();
				}
			}
			if (flag)
			{
				if (softCurrencyBtnHighlight != null)
				{
					softCurrencyBtnHighlight.enabled = _selected == Selection.SelectedSoft;
				}
				if (hardCurrencyBtnHighlight != null)
				{
					hardCurrencyBtnHighlight.enabled = _selected == Selection.SelectedHard;
				}
			}
		}
	}

	public void SetData(GachaBox box, int freeSpins)
	{
		numFreeSpins = freeSpins;
		spinCounter.SetActive((int)numFreeSpins > 0);
		spinCounterLabel.text = numFreeSpins.ToString();
		Box = box;
		if (Box != null)
		{
			tutorialArrow.ReregisterArrow("gacha_spin_" + Box.Index);
		}
		ShowActiveLayout();
		LoadToken();
		TintElements();
		UpdateActiveDisplay();
		Selected = Selection.Unselected;
		SetInteractive(true);
	}

	public void SetInteractive(bool interactive)
	{
		isInteractive = interactive;
		if (hardBtnScript != null)
		{
			hardBtnScript.SetEnabled(isInteractive);
		}
		if (softBtnScript != null)
		{
			softBtnScript.SetEnabled(isInteractive);
		}
		if (freeBtnScript != null)
		{
			freeBtnScript.SetEnabled(isInteractive);
			Color color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
			freeBtnWidget.color = ((!isInteractive) ? color : btnDefaultColor);
		}
	}

	public void ScaleToken(bool scaleUp, Action completionCb)
	{
		GameObject tokenContainer = Util.GetObjectExactMatch(activeLayout, "TokenContainer");
		LerpScaleAnimation lerp = Util.FindComponent<LerpScaleAnimation>(tokenContainer);
		lerp.animationDuration = ((!scaleUp) ? Tuning.Instance.Gacha_Token_ScaleDownTime : Tuning.Instance.Gacha_Token_ScaleUpTime);
		lerp.startPoint = ((!scaleUp) ? Vector3.one : Vector3.zero);
		lerp.endPoint = ((!scaleUp) ? Vector3.zero : Vector3.one);
		lerp.transform.localScale = lerp.startPoint;
		tokenContainer.SetActive(true);
		Action callback = null;
		callback = SafeAction.Wrap(this, () =>
		{
			tokenContainer.SetActive(scaleUp);
			if (completionCb != null)
			{
				completionCb();
			}
			lerp.animationComplete -= callback;
		});
		lerp.animationComplete += callback;
		lerp.BeginAnimation();
	}

	public GameObject GetCurrentTokenPrefab()
	{
		return tokenPrefabs.Find((GameObject item) => item.name == Box.Token);
	}

	public void PrepareForSpin()
	{
		Rotate(false);
	}

	public void HandlePostSpin()
	{
		Rotate(true);
	}

	public GameObject GetTokenInstance()
	{
		return tokenInstance;
	}

	private void Awake()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Shared");
		inactiveLayout = Util.GetObjectExactMatch(base.gameObject, "InactiveLayout");
		singleButtonLayout = Util.GetObjectExactMatch(base.gameObject, "SingleButtonLayout");
		dualButtonLayout = Util.GetObjectExactMatch(base.gameObject, "DualButtonLayout");
		lockedLayout = Util.GetObjectExactMatch(base.gameObject, "LockedLayout");
		diamondInactiveLayout = Util.GetObjectExactMatch(base.gameObject, "DiamondInactiveLayout");
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "TiledSprite");
		UIWidget uIWidget = Util.FindComponent<UIWidget>(objectExactMatch2);
		unhighlightedTileBgColor = uIWidget.color;
		tileBg = uIWidget;
		tileBgLerp = Util.FindComponent<LerpColorAnimation>(objectExactMatch2);
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch, "InnerBackground");
		uIWidget = Util.FindComponent<UIWidget>(objectExactMatch3);
		unhighlightedInnerBgColor = uIWidget.color;
		innerBg = uIWidget;
		innerBgLerp = Util.FindComponent<LerpColorAnimation>(objectExactMatch3);
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch, "BackgroundOverlay");
		uIWidget = Util.FindComponent<UIWidget>(objectExactMatch4);
		unhighlightedBgOverlayColor = uIWidget.color;
		bgOverlay = uIWidget;
		bgOverlayLerp = Util.FindComponent<LerpColorAnimation>(objectExactMatch4);
		spinCounter = Util.GetObjectExactMatch(objectExactMatch, "Count");
		spinCounterLabel = Util.FindComponent<UILabel>(spinCounter);
		tutorialArrow = Util.FindComponent<UITutorialArrow>(singleButtonLayout);
		banner = Util.GetObjectExactMatch(objectExactMatch, "Banner");
		bannerLabel = Util.GetObjectExactMatch(banner, "String").GetComponent<UILabel>();
	}

	private void Update()
	{
		if (_selected == Selection.Unselected || !isTokenRotating)
		{
			return;
		}
		EB.Collections.Stack<ScreenManager.PopupInfo> popupStack = ScreenManager.Instance.GetPopupStack();
		ScreenManager.PopupInfo popupInfo = popupStack.Peek();
		bool flag = true;
		if (popupInfo.name != "GachaUI" && popupInfo.fader)
		{
			flag = false;
		}
		if (tokenRotator != null)
		{
			tokenRotator.enabled = flag;
			if (!flag)
			{
				tokenRotator.transform.localRotation = initialTokenRotation;
			}
		}
		else
		{
			UnityEngine.Debug.LogError("[GachaTableItem::Update] tokenRotator should not be null at this point");
		}
	}

	private void LoadToken()
	{
		if (Box == null)
		{
			return;
		}
		if (tokenInstance != null)
		{
			Object.Destroy(tokenInstance);
			tokenInstance = null;
		}
		if (diamondIconInstance != null)
		{
			Object.Destroy(diamondIconInstance);
			diamondIconInstance = null;
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(activeLayout, "TokenContainer");
		if (!(objectExactMatch != null))
		{
			return;
		}
		GameObject currentTokenPrefab = GetCurrentTokenPrefab();
		if (currentTokenPrefab != null)
		{
			tokenInstance = NGUITools.AddChild(objectExactMatch, currentTokenPrefab);
			tokenInstance.transform.localScale = currentTokenPrefab.transform.localScale;
			tokenInstance.transform.localRotation = currentTokenPrefab.transform.localRotation;
			initialTokenRotation = tokenInstance.transform.localRotation;
			if (IsDiamond)
			{
				diamondGraphicContainer = Util.GetObjectExactMatch(activeLayout, "GraphicContainer");
				if (diamondGraphicContainer != null)
				{
					diamondIconInstance = NGUITools.AddChild(diamondGraphicContainer, currentTokenPrefab);
					diamondIconInstance.transform.localScale = new Vector3(30f, 10.843f, 30f);
					diamondIconInstance.transform.localRotation = currentTokenPrefab.transform.localRotation;
					diamondIconInstance.GetComponent<RotateAtSpeed>().enabled = false;
				}
			}
		}
		else
		{
			EB.Debug.LogError("GachaTableItem > LoadToken > tokenPrefab not found for token: '" + Box.Token + "'");
		}
	}

	private void TintElements()
	{
		if (Box == null)
		{
			return;
		}
		foreach (UIWidget tintableElement in tintableElements)
		{
			tintableElement.color = Box.Tint;
		}
	}

	private void UpdateActiveDisplay()
	{
		if (Box != null)
		{
			freeCurrencyBtn = Util.GetObjectExactMatch(activeLayout, "ButtonFree");
			if (freeCurrencyBtn != null)
			{
				freeCurrencyBtn.SetActive(IsFree);
				ButtonScript buttonScript = Util.FindComponent<ButtonScript>(freeCurrencyBtn);
				buttonScript.onButtonClick += (GameObject go) =>
				{
					EB.Debug.Log("GachaTableItem > freeCurrencyBtn > click - isInteractive:" + isInteractive + " IsFree:" + IsFree);
					if (isInteractive && IsFree)
					{
						Selected = Selection.SelectedFree;
					}
				};
				freeBtnScript = buttonScript;
				GameObject objectExactMatch = Util.GetObjectExactMatch(freeBtnScript.gameObject, "BackgroundArt");
				freeBtnWidget = Util.FindComponent<UIWidget>(objectExactMatch);
				if (btnDefaultColor == Color.black)
				{
					btnDefaultColor = freeBtnWidget.color;
				}
			}
			hardCurrencyBtn = Util.GetObjectExactMatch(activeLayout, "ButtonHard");
			if (hardCurrencyBtn != null)
			{
				hardCurrencyBtn.SetActive(!IsFree && CanPayHard);
				UILabel uILabel = Util.FindComponent<UILabel>(hardCurrencyBtn);
				if (uILabel != null)
				{
					UIUtils.AssignTextAndResize(uILabel, Box.HardCurrency.Cost.ToString(), uILabel.transform.localScale);
				}
				ButtonScript buttonScript2 = Util.FindComponent<ButtonScript>(hardCurrencyBtn);
				buttonScript2.onButtonClick += (GameObject go) =>
				{
					EB.Debug.Log("GachaTableItem > hardCurrencyBtn > click - isInteractive:" + isInteractive + " CanPayHard:" + CanPayHard);
					if (isInteractive && CanPayHard)
					{
						Selected = Selection.SelectedHard;
					}
				};
				hardBtnScript = buttonScript2;
				GameObject objectExactMatch2 = Util.GetObjectExactMatch(hardCurrencyBtn, "Highlight");
				if (objectExactMatch2 != null)
				{
					hardCurrencyBtnHighlight = objectExactMatch2.GetComponent<UISprite>();
				}
			}
			softCurrencyBtn = Util.GetObjectExactMatch(activeLayout, "ButtonSoft");
			if (softCurrencyBtn != null)
			{
				softCurrencyBtn.SetActive(!IsFree && CanPaySoft);
				UILabel uILabel2 = Util.FindComponent<UILabel>(softCurrencyBtn);
				if (uILabel2 != null)
				{
					UIUtils.AssignTextAndResize(uILabel2, Box.SoftCurrency.ToString(), uILabel2.transform.localScale);
				}
				ButtonScript buttonScript3 = Util.FindComponent<ButtonScript>(softCurrencyBtn);
				buttonScript3.onButtonClick += (GameObject go) =>
				{
					EB.Debug.Log("GachaTableItem > softCurrencyBtn > click - isInteractive:" + isInteractive + " CanPaySoft:" + CanPaySoft);
					if (isInteractive && CanPaySoft)
					{
						Selected = Selection.SelectedSoft;
					}
				};
				softBtnScript = buttonScript3;
				GameObject objectExactMatch3 = Util.GetObjectExactMatch(softCurrencyBtn, "Highlight");
				if (objectExactMatch3 != null)
				{
					softCurrencyBtnHighlight = objectExactMatch3.GetComponent<UISprite>();
				}
			}
			if (softCurrencyBtnHighlight != null)
			{
				softCurrencyBtnHighlight.enabled = false;
			}
			if (hardCurrencyBtnHighlight != null)
			{
				hardCurrencyBtnHighlight.enabled = false;
			}
			if (IsLocked)
			{
				GameObject objectExactMatch4 = Util.GetObjectExactMatch(activeLayout, "ItemUnlocksAt");
				UILabel uILabel3 = Util.FindComponent<UILabel>(objectExactMatch4);
				uILabel3.text = Localizer.Format("ID_UI_GACHA_UNLOCKS_AT", Box.MinLevel);
			}
			GameObject objectExactMatch5 = Util.GetObjectExactMatch(activeLayout, "ItemName");
			UIUtils.SetLabelContents(objectExactMatch5, Box.LocalizedDisplayName(Setup.Locale));
			string text = Localizer.Format("ID_UI_GACHA_MULTIPLIER", Box.Multiplier);
			if (multiplierLabel != null)
			{
				UIUtils.AssignTextAndResize(multiplierLabel, text, defaultMultiplierLabelSize);
			}
			if (!IsLocked && Box.HasBanner)
			{
				banner.SetActive(true);
				bannerLabel.text = Box.LocalizedBannerText(Setup.Locale);
				UIUtils.ApplyTints(banner, "BG", Box.BannerColor);
			}
			else
			{
				banner.SetActive(false);
			}
			GameObject objectExactMatch6 = Util.GetObjectExactMatch(activeLayout, "Starbursts");
			starburstColorLerps = Util.FindAllComponents<LerpColorAnimation>(objectExactMatch6);
			GameObject objectExactMatch7 = Util.GetObjectExactMatch(objectExactMatch6, "TokenStarburstRotator");
			glowRotator = Util.FindComponent<RotateAtSpeed>(objectExactMatch7);
			tokenRotator = Util.FindComponent<RotateAtSpeed>(tokenInstance);
		}
		else
		{
			UnityEngine.Debug.LogError("[GachaTableItem::UpdateActiveDisplay]: " + base.name + " has no GachaBox");
		}
	}

	private void ShowActiveLayout()
	{
		if (Box == null)
		{
			inactiveLayout.SetActive(true);
			singleButtonLayout.SetActive(false);
			dualButtonLayout.SetActive(false);
			lockedLayout.SetActive(false);
			diamondInactiveLayout.SetActive(false);
		}
		else
		{
			lockedLayout.SetActive(IsLocked);
			singleButtonLayout.SetActive(!IsLocked && ((CanPayHard && !CanPaySoft) || (!CanPayHard && CanPaySoft) || IsFree));
			dualButtonLayout.SetActive(!IsLocked && CanPayHard && CanPaySoft && !IsFree);
			diamondInactiveLayout.SetActive(!IsLocked && !CanPayHard && !CanPaySoft && !IsFree && IsDiamond);
			inactiveLayout.SetActive(!IsLocked && !CanPayHard && !CanPaySoft && !IsFree && !IsDiamond);
		}
		GameObject[] array = new GameObject[5] { inactiveLayout, singleButtonLayout, dualButtonLayout, lockedLayout, diamondInactiveLayout };
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.activeSelf)
			{
				activeLayout = gameObject;
				break;
			}
		}
		GameObject objectExactMatch = Util.GetObjectExactMatch(activeLayout, "ItemMultiplier");
		if (objectExactMatch != null)
		{
			multiplierLabel = Util.FindComponent<UILabel>(objectExactMatch);
			if (defaultMultiplierLabelSize == Vector3.zero)
			{
				defaultMultiplierLabelSize = multiplierLabel.transform.localScale;
			}
		}
		else
		{
			multiplierLabel = null;
			defaultMultiplierLabelSize = Vector3.zero;
		}
	}

	private void Rotate(bool enable)
	{
		isTokenRotating = enable;
		if (glowRotator != null)
		{
			glowRotator.enabled = enable;
		}
		if (tokenRotator != null)
		{
			tokenRotator.enabled = enable;
			if (!enable)
			{
				tokenRotator.transform.localRotation = initialTokenRotation;
			}
		}
	}
}
