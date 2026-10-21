using EB;
using UnityEngine;

public class BeginnerSaleUI : PopupScript
{
	private GameObject timerActiveObject;

	private GameObject timerSaleEndedObject;

	private bool goToCurrency;

	private UITexture chestTexture;

	private string chestTexturePath = "currency_ui/beginner_chest_2.png";

	protected override void OnAwake()
	{
		base.OnAwake();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Timer");
		timerActiveObject = Util.GetObjectExactMatch(objectExactMatch, "Active");
		timerSaleEndedObject = Util.GetObjectExactMatch(objectExactMatch, "SaleEnded");
		timerSaleEndedObject.SetActive(false);
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "Chest");
		chestTexture = Util.FindComponent<UITexture>(objectExactMatch2);
		TexturePoolManager.Instance.LoadTexture(chestTexturePath, SafeAction.Wrap(this, (Texture2D t) =>
		{
			chestTexture.mainTexture = t;
			chestTexture.MakePixelPerfect();
		}));
	}

	protected override void OnStart()
	{
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "ButtonBuyNow");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnBuyNow;
		ScreenManager.Instance.FadeOut(null);
	}

	private void OnBuyNow(GameObject caller)
	{
		goToCurrency = true;
		ClosePopup();
	}

	protected override void OnTransitionOutComplete()
	{
		if (goToCurrency)
		{
			HomeUI component = parentScreen.GetComponent<HomeUI>();
			component.OnBeginnerSaleOfferBuyPressed();
		}
		TexturePoolManager.Instance.ReleaseTexture(chestTexturePath);
		base.OnTransitionOutComplete();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		int num = Profile.Instance.UpdateBeginnerSaleTimeRemaining();
		if (num > 0)
		{
			int num2 = num;
			int num3 = Mathf.FloorToInt(num2 / 3600);
			num2 -= num3 * 3600;
			int num4 = Mathf.FloorToInt(num2 / 60);
			num2 -= num4 * 60;
			int num5 = num3 / 10;
			int num6 = num3 % 10;
			int num7 = num4 / 10;
			int num8 = num4 % 10;
			int num9 = num2 / 10;
			int num10 = num2 % 10;
			UIUtils.SetLabelContents(timerActiveObject, "LabelHour0", num5.ToString());
			UIUtils.SetLabelContents(timerActiveObject, "LabelHour1", num6.ToString());
			UIUtils.SetLabelContents(timerActiveObject, "LabelMinute0", num7.ToString());
			UIUtils.SetLabelContents(timerActiveObject, "LabelMinute1", num8.ToString());
			UIUtils.SetLabelContents(timerActiveObject, "LabelSecond0", num9.ToString());
			UIUtils.SetLabelContents(timerActiveObject, "LabelSecond1", num10.ToString());
		}
		else
		{
			timerActiveObject.SetActive(false);
			timerSaleEndedObject.SetActive(false);
		}
	}
}
