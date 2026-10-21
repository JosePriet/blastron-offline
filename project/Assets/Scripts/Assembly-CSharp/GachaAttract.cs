using System.Collections.Generic;
using EB;
using UnityEngine;

public class GachaAttract : MonoBehaviour
{
	private enum State
	{
		Idle = 0,
		FadeUp = 1,
		Wait = 2,
		FadeDown = 3,
		HiddenWait = 4
	}

	private class AttractItem
	{
		public Texture2D texture;

		public GachaManager.GachaRewardInfo rewardInfo;
	}

	private LerpAlphaAnimation[] lerpAlphas;

	private UILabel itemName;

	private UITexture itemTexture;

	private bool isCycling;

	private bool dataReady;

	private List<AttractItem> attractItems;

	private int currentItemIndex;

	private State currentItemState;

	private float waitTime;

	public void EnableCycling(bool enabled)
	{
		EB.Debug.Log("GachaAttract > EnableCycling > " + enabled);
		isCycling = enabled;
	}

	public void SetData(List<string> items)
	{
		Reset();
		attractItems = new List<AttractItem>();
		int pendingLoads = items.Count;
		foreach (string item in items)
		{
			GachaManager.GachaRewardInfo rewardInfo = GachaManager.Instance.GetRewardInfo(item);
			AttractItem attractItem = new AttractItem();
			attractItem.rewardInfo = rewardInfo;
			attractItems.Add(attractItem);
			TexturePoolManager.Instance.LoadTexture(rewardInfo.texturePath, this, (Texture2D tex) =>
			{
				attractItem.texture = tex;
				pendingLoads--;
				if (pendingLoads == 0)
				{
					dataReady = true;
				}
			});
		}
	}

	private void Awake()
	{
		lerpAlphas = Util.FindAllComponents<LerpAlphaAnimation>(base.gameObject);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ItemName");
		itemName = Util.FindComponent<UILabel>(objectExactMatch);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "ItemTexture");
		itemTexture = Util.FindComponent<UITexture>(objectExactMatch2);
		itemName.text = string.Empty;
		LerpAlphaAnimation[] array = lerpAlphas;
		foreach (LerpAlphaAnimation lerpAlphaAnimation in array)
		{
			UIWidget component = lerpAlphaAnimation.gameObject.GetComponent<UIWidget>();
			if (component != null)
			{
				component.alpha = 0f;
			}
		}
	}

	private void Update()
	{
		if (!isCycling || !dataReady)
		{
			return;
		}
		AttractItem attractItem = attractItems[currentItemIndex];
		if (currentItemState == State.Idle)
		{
			itemName.text = attractItem.rewardInfo.friendlyName;
			itemTexture.mainTexture = attractItem.texture;
			AdjustTextureDimensionsForCategory(itemTexture, attractItem.rewardInfo.category);
			LerpAlphaAnimation[] array = lerpAlphas;
			foreach (LerpAlphaAnimation lerpAlphaAnimation in array)
			{
				lerpAlphaAnimation.startPoint = 0f;
				lerpAlphaAnimation.endPoint = 1f;
				lerpAlphaAnimation.animationDuration = Tuning.Instance.Gacha_Attract_FadeDuration;
				lerpAlphaAnimation.BeginAnimation();
			}
			currentItemState = State.FadeUp;
		}
		else if (currentItemState == State.FadeUp)
		{
			if (!lerpAlphas[0].IsAnimating())
			{
				currentItemState = State.Wait;
				waitTime = Tuning.Instance.Gacha_Attract_TimeToShowItem;
			}
		}
		else if (currentItemState == State.Wait)
		{
			waitTime -= UnityEngine.Time.deltaTime;
			if (waitTime <= 0f)
			{
				LerpAlphaAnimation[] array2 = lerpAlphas;
				foreach (LerpAlphaAnimation lerpAlphaAnimation2 in array2)
				{
					lerpAlphaAnimation2.startPoint = 1f;
					lerpAlphaAnimation2.endPoint = 0f;
					lerpAlphaAnimation2.animationDuration = Tuning.Instance.Gacha_Attract_FadeDuration;
					lerpAlphaAnimation2.BeginAnimation();
				}
				currentItemState = State.FadeDown;
			}
		}
		else if (currentItemState == State.FadeDown)
		{
			if (!lerpAlphas[0].IsAnimating())
			{
				currentItemState = State.HiddenWait;
				waitTime = Tuning.Instance.Gacha_Attract_TimeBetweenItems;
			}
		}
		else if (currentItemState == State.HiddenWait)
		{
			waitTime -= UnityEngine.Time.deltaTime;
			if (waitTime <= 0f)
			{
				currentItemIndex = (currentItemIndex + 1) % attractItems.Count;
				currentItemState = State.Idle;
			}
		}
	}

	private void OnDestroy()
	{
		ReleaseTextures();
	}

	private void AdjustTextureDimensionsForCategory(UITexture itemTexture, GachaManager.GachaResult.RewardCategory rewardCategory)
	{
		Vector3 localScale = itemTexture.transform.localScale;
		Vector3 localScale2 = localScale;
		switch (rewardCategory)
		{
		case GachaManager.GachaResult.RewardCategory.Weapon:
		case GachaManager.GachaResult.RewardCategory.Boost:
			localScale2 = new Vector3(localScale.x, localScale.x / 2f, 1f);
			break;
		case GachaManager.GachaResult.RewardCategory.Cac:
		case GachaManager.GachaResult.RewardCategory.SoftCurrency:
		case GachaManager.GachaResult.RewardCategory.SkillPoints:
			localScale2 = new Vector3(localScale.x, localScale.x, 1f);
			break;
		}
		if (Mathf.Abs(localScale2.y - localScale.y) > 1f)
		{
			itemTexture.transform.localScale = localScale2;
		}
	}

	private void ReleaseTextures()
	{
		if (attractItems == null)
		{
			return;
		}
		foreach (AttractItem attractItem in attractItems)
		{
			TexturePoolManager.Instance.ReleaseTexture(attractItem.rewardInfo.texturePath);
		}
		attractItems = null;
	}

	private void Reset()
	{
		ReleaseTextures();
		LerpAlphaAnimation[] array = lerpAlphas;
		foreach (LerpAlphaAnimation lerpAlphaAnimation in array)
		{
			UIWidget component = lerpAlphaAnimation.gameObject.GetComponent<UIWidget>();
			if (component != null)
			{
				component.alpha = 0f;
			}
		}
		dataReady = false;
	}
}
