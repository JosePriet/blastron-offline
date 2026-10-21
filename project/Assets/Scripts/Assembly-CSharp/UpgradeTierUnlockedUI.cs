using System;
using System.Collections.Generic;
using System.Linq;
using EB;
using UnityEngine;

public class UpgradeTierUnlockedUI : PopupScript
{
	public CharacterDB.MetaData equippedPartNextTier;

	private bool _closedViaCloseButton;

	public void SetParameters(int category, int tier, CharacterDB.MetaData upgradedItem)
	{
		List<CharacterDB.MetaData> allPartsByTier = CharacterDB.GetAllPartsByTier(category, tier);
		List<CharacterDB.MetaData> list = allPartsByTier.FindAll((CharacterDB.MetaData item) => !item.IsLocked && !item.isChampionshipBoss && !item.isEpic);
		List<GameObject> list2 = Util.GetObjects(base.gameObject, "item").ToList();
		int num = Math.Min(list.Count, list2.Count);
		int num2 = (num + 1) / 2;
		int num3 = (list2.Count + 1) / 2;
		float num4 = (list2[2].transform.localPosition.x - list2[0].transform.localPosition.x) / 2f;
		float num5 = (float)(num3 - num2) * num4;
		UIUtils.SetLabelContents(base.gameObject, "LabelUnlockedTier", Localizer.Format("ID_UI_UPGRADE_UNLOCKED_NEW_TIER_BODY", tier, upgradedItem.friendlyName));
		for (int num6 = 0; num6 < list.Count; num6++)
		{
			if (list2.Count > num6)
			{
				Vector3 localPosition = list2[num6].transform.localPosition;
				localPosition.x += num5;
				list2[num6].transform.localPosition = localPosition;
				UITexture tex = Util.FindComponent<UITexture>(list2[num6]);
				TextureFade fader = Util.FindComponent<TextureFade>(list2[num6]);
				string textureName = "cac_ui_parts/" + list[num6].cacTexturePath;
				TexturePoolManager.Instance.LoadTexture(textureName, SafeAction.Wrap(this, (Texture2D t) =>
				{
					tex.mainTexture = t;
					fader.SetTexture(tex);
				}));
			}
		}
	}

	public void OnStoreButtonPress(GameObject caller)
	{
		ScreenManager.Instance.DisplayPopupStack();
		GameObject popup = ScreenManager.Instance.GetPopup("MyRobotUI");
		if (popup != null)
		{
			MyRobotUI myRobotUI = Util.FindComponent<MyRobotUI>(popup);
			myRobotUI.ForceOwned(false);
		}
		ScreenManager.Instance.GetPopup("MyRobotUI").GetComponent<MyRobotUI>().CenterOnNewUnlockedPart(equippedPartNextTier);
		popup = ScreenManager.Instance.GetPopup("MyRobotUpgradeUI");
		if (popup != null)
		{
			GameObject popup2 = ScreenManager.Instance.GetPopup("MyRobotUI");
			popup2.SendMessage("OnSubscreenClose");
			ScreenManager.Instance.ClosePopup("MyRobotUpgradeUI");
		}
		else
		{
			ScreenManager.Instance.ClosePopup("UpgradeUI");
		}
	}

	protected override void Close(GameObject caller)
	{
		ScreenManager.Instance.GetPopup("MyRobotUI").GetComponent<MyRobotUI>().RefreshData();
		_closedViaCloseButton = true;
		base.OnClose();
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		if (_closedViaCloseButton)
		{
			GameObject popup = ScreenManager.Instance.GetPopup("UpgradeUI");
			if (popup != null)
			{
				ScreenManager.Instance.ClosePopup("UpgradeUI");
			}
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "StoreButton");
		ButtonScript buttonScript = Util.FindComponent<ButtonScript>(objectExactMatch);
		buttonScript.onButtonClick += OnStoreButtonPress;
	}
}
