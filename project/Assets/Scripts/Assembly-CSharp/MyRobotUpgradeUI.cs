using EB;
using EB.Sequence;
using UnityEngine;

public class MyRobotUpgradeUI : MonoBehaviour
{
	private MyRobotUI parentScreen;

	private bool _enableInput = true;

	public void SetParentScreen(MyRobotUI parentScreen)
	{
		this.parentScreen = parentScreen;
	}

	public void RefreshPartsData()
	{
		CharacterDB.Recipe recipe = CharacterManager.Instance.LocalPlayer.recipe;
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "CacItem" + i);
			CacUiItem component = objectExactMatch.GetComponent<CacUiItem>();
			component.metaData = CharacterDB.GetMetaData(recipe.GetPart((CharacterDB.Category)i));
			component.UpdateArt();
		}
	}

	public void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void Start()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BackButton");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnBack;
		CharacterDB.Recipe recipe = CharacterManager.Instance.LocalPlayer.recipe;
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "CacItem" + i);
			if (objectExactMatch2 != null)
			{
				CacUiItem component = objectExactMatch2.GetComponent<CacUiItem>();
				if (component != null)
				{
					component.metaData = CharacterDB.GetMetaData(recipe.GetPart((CharacterDB.Category)i));
					component.UpdateArt();
					component.upgradeButton.onButtonClick += OnUpgrade;
				}
			}
		}
		EB.Sequence.Component.Activate(base.gameObject, "MyRobotUpgradeUI", typeof(SequenceEvent_OnScreenOpened));
		ScreenManager.Instance.EnableTransitionBlocker(false);
	}

	private void OnBack(GameObject caller)
	{
		if (_enableInput)
		{
			parentScreen.OnSubscreenClose();
			parentScreen = null;
			ScreenManager.Instance.ClosePopup();
			EB.Sequence.Component.Activate(base.gameObject, "MyRobotUpgradeUI", typeof(SequenceEvent_OnScreenClosed));
		}
	}

	private void OnUpgrade(GameObject caller)
	{
		if (_enableInput)
		{
			CacUiItem cacUiItem = Util.FindComponentUpwards<CacUiItem>(caller);
			if (cacUiItem != null)
			{
				UITexture uITexture = Util.FindComponent<UITexture>(cacUiItem.gameObject);
				GameObject obj = ScreenManager.Instance.OpenPopup("UpgradeUI", true, true);
				UpgradeUI upgradeUI = Util.FindComponent<UpgradeUI>(obj);
				upgradeUI.SetParameters(cacUiItem.metaData);
				upgradeUI.parentScreen = base.gameObject;
			}
		}
	}
}
