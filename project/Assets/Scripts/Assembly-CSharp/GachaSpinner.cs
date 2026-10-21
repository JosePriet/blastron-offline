using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Sparx;
using UnityEngine;

public class GachaSpinner : MonoBehaviour
{
	public enum GachaAnimationState
	{
		Closed = 0,
		Open = 1,
		Hovering = 2,
		StopHovering = 3,
		Slotting = 4
	}

	private class DisplayResult
	{
		public Texture2D texture;

		public GachaManager.GachaResult result;
	}

	private const string CacBgTexPath = "currency_ui/gacha_cac_bg.jpg";

	public AudioEvent startSpinAudioEvent;

	public AudioEvent prizeAwardedAudioEvent;

	public AudioEvent teleportAudioEvent;

	public AudioEvent finalTeleportAudioEvent;

	public AudioEvent slotAudioEvent;

	public ParticleSystem teleportOpenVfx;

	public ParticleSystem teleportSpinningVfx;

	private GameObject itemCountContainer;

	private GameObject tokenContainer;

	private Animation teleporterAnim;

	private Animation rewardAnim;

	private GameObject rewardDetails;

	private UILabel itemAwardedHint;

	private UILabel itemNameLabel;

	private UILabel itemCountLabel;

	private UIWidget itemCountBg;

	private UITexture itemTexture;

	private UITexture cacTextureBg;

	private AudioEmitter audioEmitter;

	private LerpAlphaAnimation gachaSpinHintLerp;

	private LerpAnimation tokenLerpAnim;

	private UIWidget gachaSpinHint;

	private GachaAnimationState animState;

	private int animationPendingLoads;

	private List<string> loadedTextures = new List<string>();

	private bool waitingForAnimEvent;

	private Vector3 rewardAnimStartPos;

	private Vector3 tokenContainerOriginalPos;

	private Vector3 itemTextureOriginalSize;

	public void Open()
	{
		if (animState == GachaAnimationState.Closed)
		{
			AnimationState animationState = teleporterAnim["gacha_spinner_open"];
			teleporterAnim.Play("gacha_spinner_open");
			animState = GachaAnimationState.Open;
			gachaSpinHintLerp.startPoint = gachaSpinHint.alpha;
			gachaSpinHintLerp.endPoint = 0f;
			gachaSpinHintLerp.BeginAnimation();
			if (teleportOpenVfx != null)
			{
				teleportOpenVfx.Play();
			}
		}
	}

	public void Close()
	{
		EB.Debug.Log("GachaSpinner > Close > animState: " + animState);
		if (animState != GachaAnimationState.Closed)
		{
			AnimationState animationState = teleporterAnim["gacha_spinner_close"];
			teleporterAnim.Play("gacha_spinner_close");
			animState = GachaAnimationState.Closed;
			gachaSpinHintLerp.startPoint = gachaSpinHint.alpha;
			gachaSpinHintLerp.endPoint = 1f;
			gachaSpinHintLerp.BeginAnimation();
			if (teleportOpenVfx != null)
			{
				teleportOpenVfx.Stop();
			}
			if (teleportSpinningVfx != null)
			{
				teleportSpinningVfx.Stop();
			}
		}
	}

	public void SetSpinResult(GachaManager.GachaResult result, List<GachaPossiblePrize> prespinResults, GachaTableItem tableItem, Action animationCompleteCallback)
	{
		Coroutines.Run(AnimateGachaResult(result, prespinResults, tableItem, animationCompleteCallback));
	}

	public void Exit()
	{
	}

	public void PlayAnimEvent(string eventName)
	{
		if (eventName == "slot")
		{
			audioEmitter.Play(slotAudioEvent);
			if (teleportOpenVfx != null)
			{
				teleportOpenVfx.Stop();
			}
			if (teleportSpinningVfx != null)
			{
				teleportSpinningVfx.Play();
			}
		}
		else if (eventName == "done")
		{
			waitingForAnimEvent = false;
		}
	}

	private void Awake()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Robot");
		teleporterAnim = Util.FindComponent<Animation>(objectExactMatch);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "RewardImage");
		rewardAnim = Util.FindComponent<Animation>(objectExactMatch2);
		rewardAnimStartPos = rewardAnim.transform.localPosition;
		rewardDetails = Util.GetObjectExactMatch(base.gameObject, "RewardDetails");
		itemNameLabel = Util.GetObjectExactMatch(rewardDetails, "ItemName").GetComponent<UILabel>();
		itemCountContainer = Util.GetObjectExactMatch(objectExactMatch2, "Count");
		itemCountLabel = Util.FindComponent<UILabel>(objectExactMatch2);
		GameObject objectExactMatch3 = Util.GetObjectExactMatch(objectExactMatch2, "RewardTexture");
		itemTexture = Util.FindComponent<UITexture>(objectExactMatch3);
		itemTextureOriginalSize = itemTexture.transform.localScale;
		GameObject objectExactMatch4 = Util.GetObjectExactMatch(objectExactMatch2, "CacTextureBackground");
		cacTextureBg = Util.FindComponent<UITexture>(objectExactMatch4);
		cacTextureBg.gameObject.SetActive(false);
		GameObject objectExactMatch5 = Util.GetObjectExactMatch(rewardDetails, "HintYouGot");
		itemAwardedHint = Util.FindComponent<UILabel>(objectExactMatch5);
		tokenContainer = Util.GetObjectExactMatch(base.gameObject, "TokenContainer");
		tokenLerpAnim = Util.FindComponent<LerpAnimation>(tokenContainer);
		tokenContainerOriginalPos = tokenContainer.transform.localPosition;
		GameObject objectExactMatch6 = Util.GetObjectExactMatch(itemCountContainer, "Bg");
		itemCountBg = Util.FindComponent<UIWidget>(objectExactMatch6);
		audioEmitter = Util.FindComponent<AudioEmitter>(base.gameObject);
		GameObject objectExactMatch7 = Util.GetObjectExactMatch(base.gameObject, "GachaSpinHint");
		gachaSpinHintLerp = Util.FindComponent<LerpAlphaAnimation>(objectExactMatch7);
		gachaSpinHint = Util.FindComponent<UIWidget>(objectExactMatch7);
		TexturePoolManager.Instance.LoadTexture("currency_ui/gacha_cac_bg.jpg", this, (Texture2D tex) =>
		{
			cacTextureBg.mainTexture = tex;
		});
		HideRewards();
	}

	private void Update()
	{
		if ((animState == GachaAnimationState.Open || animState == GachaAnimationState.Hovering) && !teleporterAnim.isPlaying)
		{
			teleporterAnim.Play("gacha_spinner_hover");
		}
	}

	private void OnDestroy()
	{
		TexturePoolManager.Instance.ReleaseTexture("currency_ui/gacha_cac_bg.jpg");
	}

	private void HideRewards()
	{
		itemNameLabel.text = string.Empty;
		itemCountLabel.text = string.Empty;
		itemCountContainer.SetActive(false);
		itemTexture.gameObject.SetActive(false);
		itemAwardedHint.alpha = 0f;
	}

	private void LoadDisplayTexture(DisplayResult displayResult)
	{
		loadedTextures.Add(displayResult.result.rewardTexturePath);
		TexturePoolManager.Instance.LoadTexture(displayResult.result.rewardTexturePath, (Texture2D tex) =>
		{
			displayResult.texture = tex;
			animationPendingLoads--;
		});
	}

	private void AdjustTextureDimensionsForCategory(UITexture itemTexture, GachaManager.GachaResult.RewardCategory rewardCategory)
	{
		Vector3 localScale = itemTextureOriginalSize;
		switch (rewardCategory)
		{
		case GachaManager.GachaResult.RewardCategory.Weapon:
		case GachaManager.GachaResult.RewardCategory.Boost:
		case GachaManager.GachaResult.RewardCategory.SoftCurrency:
			localScale = itemTextureOriginalSize;
			break;
		case GachaManager.GachaResult.RewardCategory.Cac:
		case GachaManager.GachaResult.RewardCategory.SkillPoints:
		case GachaManager.GachaResult.RewardCategory.GachaSpin:
			localScale = new Vector3(itemTextureOriginalSize.y * 1.5f, itemTextureOriginalSize.y * 1.5f, 1f);
			break;
		}
		itemTexture.transform.localScale = localScale;
	}

	private void UnloadTextures()
	{
		foreach (string loadedTexture in loadedTextures)
		{
			TexturePoolManager.Instance.ReleaseTexture(loadedTexture);
		}
		loadedTextures.Clear();
	}

	private IEnumerator AnimateGachaResult(GachaManager.GachaResult result, List<GachaPossiblePrize> prespinResults, GachaTableItem tableItem, Action animationCompleteCallback)
	{
		tokenContainer.SetActive(true);
		animationPendingLoads = ((prespinResults != null) ? prespinResults.Count : 0) + 1;
		DisplayResult[] displayResults = new DisplayResult[animationPendingLoads];
		if (prespinResults != null)
		{
			for (int i = 0; i < prespinResults.Count; i++)
			{
				displayResults[i] = new DisplayResult();
				GachaPossiblePrize prize = prespinResults[i];
				displayResults[i].result = new GachaManager.GachaResult(prize.Data, prize.Type);
				LoadDisplayTexture(displayResults[i]);
			}
		}
		int finalIdx = displayResults.Length - 1;
		displayResults[finalIdx] = new DisplayResult();
		displayResults[finalIdx].result = result;
		LoadDisplayTexture(displayResults[finalIdx]);
		while (animationPendingLoads > 0)
		{
			yield return null;
		}
		GameObject tokenPrefab = tableItem.GetCurrentTokenPrefab();
		GameObject tokenInstance = null;
		if (tokenPrefab != null)
		{
			tokenInstance = NGUITools.AddChild(tokenContainer, tokenPrefab);
			tokenInstance.transform.localScale = tokenPrefab.transform.localScale;
			if (tokenPrefab.name.Contains("_diamond") || tokenPrefab.name.Contains("_platinum"))
			{
				tokenInstance.transform.localScale *= 0.9f;
			}
			tokenInstance.transform.localRotation = tokenPrefab.transform.localRotation;
			RotateAtSpeed rot = Util.FindComponent<RotateAtSpeed>(tokenInstance);
			if (rot != null)
			{
				rot.enabled = false;
				Object.Destroy(rot);
			}
		}
		GameObject tableTokenInstance = tableItem.GetTokenInstance();
		Vector3 localStartPos = Vector3.zero;
		Transform t = tableTokenInstance.transform;
		while (t != null && t.gameObject.name != "GachaUI")
		{
			localStartPos += t.localPosition;
			t = t.parent;
		}
		Vector3 localEndPoint = Vector3.zero;
		t = tokenLerpAnim.transform;
		while (t != null && t.gameObject.name != "GachaUI")
		{
			localEndPoint += t.localPosition;
			t = t.parent;
		}
		Vector3 delta = localEndPoint - localStartPos;
		tokenLerpAnim.startPoint = tokenLerpAnim.endPoint - delta;
		tokenLerpAnim.transform.localPosition = tokenLerpAnim.startPoint;
		tokenLerpAnim.transform.localScale = Vector3.one;
		tableTokenInstance.SetActive(false);
		tokenLerpAnim.animationDuration = Tuning.Instance.Gacha_Token_LerpTime;
		tokenLerpAnim.BeginAnimation();
		while (tokenLerpAnim.IsAnimating())
		{
			yield return null;
		}
		teleporterAnim.Play("gacha_spinner_slot");
		waitingForAnimEvent = true;
		while (waitingForAnimEvent)
		{
			yield return null;
		}
		animState = GachaAnimationState.Hovering;
		tokenContainer.transform.localPosition = tokenContainerOriginalPos;
		tokenContainer.SetActive(false);
		float pauseForEachReward = Tuning.Instance.Gacha_Spin_PauseForEachReward;
		float pauseBetweenRewards = Tuning.Instance.Gacha_Spin_PauseBetweenEachReward;
		float fadeTime = Tuning.Instance.Gacha_Spin_RewardFadeTime;
		float pauseAndFadeTimeScale = Tuning.Instance.Gacha_Spin_PauseAndFadeTimeScale;
		audioEmitter.Play(startSpinAudioEvent);
		cacTextureBg.gameObject.SetActive(false);
		for (int j = 0; j < displayResults.Length; j++)
		{
			bool isLastItem = j == displayResults.Length - 1;
			DisplayResult currentItem = displayResults[j];
			itemTexture.mainTexture = displayResults[j].texture;
			itemTexture.alpha = 0f;
			cacTextureBg.alpha = 0f;
			itemTexture.gameObject.SetActive(true);
			AdjustTextureDimensionsForCategory(itemTexture, displayResults[j].result.rewardCategory);
			while (itemTexture.alpha < 1f)
			{
				itemTexture.alpha += UnityEngine.Time.deltaTime / fadeTime;
				cacTextureBg.alpha = itemTexture.alpha;
				yield return null;
			}
			if (isLastItem)
			{
				audioEmitter.Play(finalTeleportAudioEvent);
			}
			else
			{
				audioEmitter.Play(teleportAudioEvent);
			}
			float currPauseTime = 0f;
			while (currPauseTime < pauseForEachReward)
			{
				currPauseTime += UnityEngine.Time.deltaTime;
				yield return null;
			}
			if (!isLastItem)
			{
				while (itemTexture.alpha > 0f)
				{
					itemTexture.alpha -= UnityEngine.Time.deltaTime / fadeTime;
					cacTextureBg.alpha = itemTexture.alpha;
					yield return null;
				}
				currPauseTime = 0f;
				while (currPauseTime < pauseBetweenRewards)
				{
					currPauseTime += UnityEngine.Time.deltaTime;
					yield return null;
				}
			}
			else
			{
				itemNameLabel.text = result.rewardNameWithQuantity;
				itemNameLabel.alpha = 0f;
				itemCountLabel.text = result.rewardCount.ToString();
				bool active = result.rewardCount > 1;
				switch (result.rewardCategory)
				{
				case GachaManager.GachaResult.RewardCategory.SoftCurrency:
				case GachaManager.GachaResult.RewardCategory.SkillPoints:
				case GachaManager.GachaResult.RewardCategory.GachaSpin:
					active = false;
					break;
				}
				itemCountContainer.SetActive(active);
				itemTexture.gameObject.SetActive(true);
				itemAwardedHint.alpha = 1f;
				audioEmitter.Play(prizeAwardedAudioEvent);
				while (itemNameLabel.alpha < 1f)
				{
					itemNameLabel.alpha += UnityEngine.Time.deltaTime / fadeTime;
					itemAwardedHint.alpha = itemNameLabel.alpha;
					itemCountLabel.alpha = itemNameLabel.alpha;
					itemCountBg.alpha = itemNameLabel.alpha;
					yield return null;
				}
			}
			if (j > Tuning.Instance.Gacha_Spin_PrespinsAtFullSpeed)
			{
				pauseForEachReward *= pauseAndFadeTimeScale;
				pauseBetweenRewards *= pauseAndFadeTimeScale;
				fadeTime *= pauseAndFadeTimeScale;
			}
		}
		if (teleportOpenVfx != null)
		{
			teleportOpenVfx.Play();
		}
		if (teleportSpinningVfx != null)
		{
			teleportSpinningVfx.Stop();
		}
		float pauseTime = Tuning.Instance.Gacha_Spin_FinalRewardPauseTime;
		while (pauseTime > 0f)
		{
			pauseTime -= UnityEngine.Time.deltaTime;
			yield return null;
		}
		float animSpeed = Tuning.Instance.Gacha_Spin_RewardFlyOffTime;
		if (animSpeed > 0f)
		{
			rewardAnim.Play();
			foreach (AnimationState state in rewardAnim)
			{
				state.speed = 1f / animSpeed;
			}
			while (rewardAnim.isPlaying)
			{
				yield return null;
			}
		}
		itemCountContainer.SetActive(false);
		itemTexture.gameObject.SetActive(false);
		itemAwardedHint.alpha = 0f;
		float finalFadeTime = Tuning.Instance.Gacha_Spin_FinalRewardFadeOutTime;
		while (itemTexture.alpha > 0f)
		{
			itemTexture.alpha -= UnityEngine.Time.deltaTime / finalFadeTime;
			cacTextureBg.alpha = itemTexture.alpha;
			itemNameLabel.alpha = itemTexture.alpha;
			itemAwardedHint.alpha = itemTexture.alpha;
			itemCountLabel.alpha = itemTexture.alpha;
			itemCountBg.alpha = itemTexture.alpha;
			yield return null;
		}
		rewardAnim.transform.localPosition = rewardAnimStartPos;
		tableTokenInstance.SetActive(true);
		tableItem.ScaleToken(true, SafeAction.Wrap(this, () =>
		{
		}));
		Object.Destroy(tokenInstance);
		UnloadTextures();
		animationCompleteCallback();
	}
}
