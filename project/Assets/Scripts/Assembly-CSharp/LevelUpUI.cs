using System.Collections.Generic;
using EB;
using EB.Sequence;
using EB.Sparx;
using SmoothMoves;
using UnityEngine;

public class LevelUpUI : PopupScript
{
	private const int maxRewards = 4;

	private bool initializedUiElements;

	private UITexture image;

	private List<UITexture> weaponRewardsTextures = new List<UITexture>();

	private int currentTextureIndex;

	private int unlockIndex;

	public GameObject itemPrefab;

	private TableHelperUI tableHelper;

	private UIGrid grid;

	private bool dragging;

	private BoneAnimation boneAnim;

	private ParticleSystem unlockFx;

	private bool isPlayingAnim;

	private BoneAnimation sm_anim;

	private bool _transitioningOut;

	public float BackToLobbyTimer { get; set; }

	private bool isSP
	{
		get
		{
			return GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) == GameAttributes.ModeType.SP;
		}
	}

	private bool InputEnabled { get; set; }

	protected override void OnAwake()
	{
		UIUtils.SetLabelContents(base.gameObject, "LabelName", Hub.Instance.LoginManager.LocalUser.Name);
		string labelText = Localizer.Format("ID_UI_LEVELUP_LEVEL", Profile.Instance.Level);
		UIUtils.SetLabelContents(base.gameObject, "LabelLevel", labelText);
		unlockIndex = 0;
		grid = Util.GetObjectExactMatch(base.gameObject, "Grid").GetComponent<UIGrid>();
		tableHelper = base.gameObject.AddComponent<TableHelperUI>();
		tableHelper.SetMaxVisibleItems(1);
		tableHelper.SetDelta(1);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "padlock_item");
		boneAnim = objectExactMatch.GetComponentInChildren<BoneAnimation>();
		GameObject objectExactMatch2 = Util.GetObjectExactMatch(base.gameObject, "fx_padlock");
		unlockFx = objectExactMatch2.GetComponent<ParticleSystem>();
		isPlayingAnim = false;
		InputEnabled = true;
	}

	protected override void OnStart()
	{
		ScreenManager.Instance.AddPersistentScreen("BackgroundUI");
		base.OnStart();
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "UnlocksPanel");
		SetupRewards(objectExactMatch);
	}

	protected override void OnTransitionInComplete()
	{
		if (boneAnim != null)
		{
			boneAnim.Play("padlock_open_clamp");
			isPlayingAnim = true;
		}
		if (unlockFx != null)
		{
			unlockFx.Play();
		}
		EB.Sequence.Component.Activate(base.gameObject, "LevelUpUI", typeof(SequenceEvent_OnScreenOpened));
	}

	private void OnTimerExpired()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game != null && game.IsHost)
		{
			game.End();
		}
		ScreenManager.Instance.ClosePopup("LevelUpUI");
	}

	protected override void OnUpdate()
	{
		if (!_transitioningOut)
		{
			if (dragging && !Input.GetMouseButton(0))
			{
				tableHelper.CenterOnCurrentItem();
				dragging = false;
			}
			if (!initializedUiElements)
			{
				initializedUiElements = true;
				UpdateUnlockedItems();
				tableHelper.Reset();
			}
			if (isPlayingAnim && !boneAnim.isPlaying)
			{
				isPlayingAnim = false;
			}
		}
	}

	public void OnScrollDrag(GameObject caller, bool pressed)
	{
		if (pressed)
		{
			dragging = true;
		}
	}

	protected override void OnClose()
	{
		if (InputEnabled)
		{
			Close();
		}
	}

	private void Close()
	{
		GameObject popup = ScreenManager.Instance.GetPopup("ResultsUI");
		if (popup != null)
		{
			ResultsUI component = popup.GetComponent<ResultsUI>();
			if (component != null)
			{
				component.BackToLobbyTimer = BackToLobbyTimer;
				component.LevelUpComplete = true;
			}
		}
		base.OnClose();
	}

	private void UpdateUnlockedItems()
	{
		tableHelper.Clear();
		List<InventoryDB.MetaData> unlockedItems = InventoryDB.GetUnlockedItems(Profile.Instance.Level);
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		while (num3 < unlockedItems.Count)
		{
			if (!UnlocksManager.Instance.IsUnlocked(unlockedItems[num3].name) && !unlockedItems[num3].IsHidden)
			{
				GameObject gameObject = NGUITools.AddChild(grid.gameObject, itemPrefab);
				gameObject.AddComponent<UIDragPanelContents>();
				LevelUpUnlockUiItem levelUpUnlockUiItem = Util.FindComponent<LevelUpUnlockUiItem>(gameObject);
				tableHelper.AddItem(gameObject);
				InventoryDB.MetaData metaData = unlockedItems[num3];
				levelUpUnlockUiItem.SetItemName(metaData.friendlyName);
				levelUpUnlockUiItem.SetTexturePath(metaData.uiTexturePath);
				num2++;
			}
			num3++;
			num++;
		}
		EB.Debug.Log("LevelUpUI > UpdateUnlockedItems > newUnlocks: " + num2);
		if (num2 < 1)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "PopupTransitionBottom");
			objectExactMatch.SetActive(false);
		}
		tableHelper.Reset();
	}

	private void SetupRewards(GameObject panel)
	{
		int level = Profile.Instance.Level;
		int num = Mathf.Max(0, level - 2);
		Tuning.LevelInfo levelInfo = Tuning.Instance.Levels[num];
		List<Tuning.LevelReward> list = new List<Tuning.LevelReward>(levelInfo.Rewards);
		list.RemoveAll((Tuning.LevelReward obj) => obj.type == Tuning.LevelRewardType.None);
		Tuning.LevelReward[] array = list.ToArray();
		int num2;
		for (num2 = 0; num2 < Mathf.Min(4, array.Length); num2++)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(panel, "Reward" + num2);
			if (!objectExactMatch)
			{
				continue;
			}
			Tuning.LevelReward levelReward = array[num2];
			UIUtils.SetLabelContents(objectExactMatch, "Label", levelReward.quantity);
			switch (levelReward.type)
			{
			case Tuning.LevelRewardType.HardCurrency:
			{
				UISprite component3 = Util.GetObjectExactMatch(objectExactMatch, "Sprite").GetComponent<UISprite>();
				component3.spriteName = "currency_hard";
				break;
			}
			case Tuning.LevelRewardType.SoftCurrency:
			{
				UISprite component2 = Util.GetObjectExactMatch(objectExactMatch, "Sprite").GetComponent<UISprite>();
				component2.spriteName = "currency_soft";
				break;
			}
			case Tuning.LevelRewardType.SkillPoints:
			{
				UISprite component4 = Util.GetObjectExactMatch(objectExactMatch, "Sprite").GetComponent<UISprite>();
				component4.spriteName = "skill_points_token";
				break;
			}
			case Tuning.LevelRewardType.Inventory:
			{
				UISprite component = Util.GetObjectExactMatch(objectExactMatch, "Sprite").GetComponent<UISprite>();
				component.gameObject.SetActive(false);
				GameObject objectExactMatch2 = Util.GetObjectExactMatch(objectExactMatch, "Label");
				Vector3 localPosition = objectExactMatch2.transform.localPosition;
				localPosition.x = 70f;
				objectExactMatch2.transform.localPosition = localPosition;
				objectExactMatch2.GetComponent<UILabel>().pivot = UIWidget.Pivot.Center;
				InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(levelReward.name.Trim()));
				image = objectExactMatch.GetComponentInChildren<UITexture>();
				if (metaData != null)
				{
					weaponRewardsTextures.Add(image);
					TexturePoolManager.Instance.LoadTexture(metaData.uiTexturePath, OnTextureLoaded);
				}
				else
				{
					EB.Debug.LogError(string.Format("BAD: Hashing with {0} did not result in a match in InventoryDB - IGNORING!", levelReward.name));
				}
				break;
			}
			}
		}
		if (panel.name == "RewardPanel")
		{
			if (num2 < 4)
			{
				GameObject objectExactMatch3 = Util.GetObjectExactMatch(panel, "divider3");
				objectExactMatch3.SetActive(false);
			}
			if (num2 < 3)
			{
				GameObject objectExactMatch4 = Util.GetObjectExactMatch(panel, "divider2");
				objectExactMatch4.SetActive(false);
			}
			if (num2 < 2)
			{
				GameObject objectExactMatch5 = Util.GetObjectExactMatch(panel, "divider1");
				objectExactMatch5.SetActive(false);
			}
		}
		for (; num2 < 4; num2++)
		{
			GameObject objectExactMatch6 = Util.GetObjectExactMatch(panel, "Reward" + num2);
			if ((bool)objectExactMatch6)
			{
				objectExactMatch6.SetActive(false);
			}
		}
		if (array.Length == 0)
		{
			UIUtils.SetLabelContents(panel, "Label", string.Empty);
			GameObject objectExactMatch7 = Util.GetObjectExactMatch(panel, "UnlockPanelHolder");
			if (objectExactMatch7 != null)
			{
				objectExactMatch7.transform.localPosition = Vector3.zero;
			}
			GameObject objectExactMatch8 = Util.GetObjectExactMatch(panel, "ButtonLeft");
			if (objectExactMatch8 != null)
			{
				Vector3 localPosition2 = objectExactMatch8.transform.localPosition;
				localPosition2.y = 20f;
				objectExactMatch8.transform.localPosition = localPosition2;
			}
			GameObject objectExactMatch9 = Util.GetObjectExactMatch(panel, "ButtonRight");
			if (objectExactMatch9 != null)
			{
				Vector3 localPosition3 = objectExactMatch9.transform.localPosition;
				localPosition3.y = 20f;
				objectExactMatch9.transform.localPosition = localPosition3;
			}
			GameObject objectExactMatch10 = Util.GetObjectExactMatch(panel, "dividerLine");
			if (objectExactMatch10 != null)
			{
				objectExactMatch10.SetActive(false);
			}
		}
	}

	public void OnTextureLoaded(Texture2D t)
	{
		if (image != null && t != null)
		{
			weaponRewardsTextures[currentTextureIndex].mainTexture = t;
			weaponRewardsTextures[currentTextureIndex].MakePixelPerfect();
			weaponRewardsTextures[currentTextureIndex].transform.localScale = weaponRewardsTextures[currentTextureIndex].transform.localScale * 0.5f;
			currentTextureIndex++;
		}
	}

	private void EnableInput(bool enable)
	{
		InputEnabled = enable;
	}

	protected override void OnTransitionOutComplete()
	{
		base.OnTransitionOutComplete();
		EB.Sequence.Component.Activate(base.gameObject, "LevelUpUI", typeof(SequenceEvent_OnScreenClosed));
	}
}
