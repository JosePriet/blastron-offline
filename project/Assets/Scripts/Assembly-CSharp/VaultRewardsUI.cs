using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EB;
using EB.Sequence;
using EB.Sparx;
using UnityEngine;

public class VaultRewardsUI : MonoBehaviour
{
	public enum ReturnMode
	{
		kVaultInventory = 0,
		kHomeUI = 1
	}

	public GameObject weaponRewardPrefab;

	public GameObject cacRewardPrefab;

	public bool autoClose;

	public int autoCloseSeconds = 5;

	public Vector3 ItemScaleStage1 = new Vector3(6f, 6f, 1f);

	public Vector3 ItemScaleStage2 = new Vector3(1.5f, 1.5f, 1f);

	public Vector3 ItemScaleStage3 = new Vector3(1f, 1f, 1f);

	private GameObject slotsContainer;

	private List<GameObject> itemContainers;

	private List<LootinatorSpawnTable.LootEntry> lootRewards = new List<LootinatorSpawnTable.LootEntry>();

	private List<VaultReward> vaultRewards = new List<VaultReward>();

	private List<int> vaultItemAnimStates = new List<int>();

	private GameObject doneButton;

	private GameObject equipButton;

	private GameObject zoomHolder;

	private GameObject starBG;

	private CharacterDB.MetaData cacToEquip;

	private bool waitingForRewardsBeforeTransitioning;

	private int autoCloseStartTime;

	private bool _enableInput = true;

	private bool _isCAC;

	private ReturnMode _returnMode;

	public void SetWeapon(Weapon weapon, int quantity)
	{
		InventoryDB.MetaData metaData = GetMetaData(weapon.name, InventoryItem.Category.Weapon);
		GameObject obj = Object.Instantiate(weaponRewardPrefab) as GameObject;
		VaultRewardWeapon vaultRewardWeapon = Util.FindComponent<VaultRewardWeapon>(obj);
		vaultRewardWeapon.SetData(0, metaData, quantity);
		vaultRewards.Add(vaultRewardWeapon);
		vaultItemAnimStates.Add(0);
		LootinatorSpawnTable.LootEntry lootEntry = new LootinatorSpawnTable.LootEntry();
		lootEntry.type = LootinatorSpawnTable.LootEntry.LootEntryType.Weapon;
		lootEntry.quantity = quantity;
		lootEntry.name = weapon.name;
		lootRewards.Add(lootEntry);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Subtext");
		objectExactMatch.GetComponent<UILabel>().text = Localizer.Format("ID_UI_VAULT_ON_YOUR_NEW", Localizer.GetString("ID_UI_STORE_WEAPONS"));
		InitializeRewardArt();
		ApplyRewardEffects();
	}

	public void SetCAC(GameObject cacPrefab, int quantity)
	{
		GameObject obj = Object.Instantiate(cacRewardPrefab) as GameObject;
		VaultRewardCAC vaultRewardCAC = Util.FindComponent<VaultRewardCAC>(obj);
		int hash = Hash.StringHash(cacPrefab.name);
		cacToEquip = CharacterDB.GetMetaData(hash);
		if (cacToEquip == null)
		{
			EB.Debug.LogError("FAILED TO GET METADATA FOR CAC PART " + cacPrefab.name);
		}
		vaultRewardCAC.SetData(0, cacToEquip, quantity);
		vaultRewards.Add(vaultRewardCAC);
		LootinatorSpawnTable.LootEntry lootEntry = new LootinatorSpawnTable.LootEntry();
		lootEntry.type = LootinatorSpawnTable.LootEntry.LootEntryType.CAC;
		lootEntry.quantity = quantity;
		lootEntry.name = cacPrefab.name;
		lootRewards.Add(lootEntry);
		vaultItemAnimStates.Add(0);
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Subtext");
		objectExactMatch.GetComponent<UILabel>().text = Localizer.Format("ID_UI_VAULT_ON_YOUR_NEW", Localizer.GetString("ID_UI_STORE_PARTS"));
		_isCAC = true;
		HideContainers();
		InitializeRewardArt();
		ApplyRewardEffects();
	}

	public void SetReturnMode(ReturnMode mode)
	{
		_returnMode = mode;
	}

	public void OnOpenPopup()
	{
		waitingForRewardsBeforeTransitioning = true;
	}

	public void OnClose(GameObject caller)
	{
		if (_enableInput)
		{
			GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
			if (persistentScreen != null)
			{
				NGUITools.SetActive(persistentScreen, true);
			}
			doneButton.SetActive(false);
			equipButton.SetActive(false);
			starBG.SetActive(false);
			autoCloseStartTime = 0;
			EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnTransitionOutComplete);
		}
	}

	public void OnEquip(GameObject caller)
	{
		if (_enableInput)
		{
			equipButton.SetActive(false);
			CharacterManager.Instance.LocalPlayer.recipe.SetPart(cacToEquip, Profile.Instance.GetPartUpgradeProgress(cacToEquip.hash));
			CharacterManager.Instance.UpdateLocalCharacter();
			autoCloseStartTime = 0;
			PlayTeleporterFX();
			if (autoClose)
			{
				_enableInput = false;
				Coroutines.Run(DelayClose());
			}
		}
	}

	private IEnumerator DelayClose()
	{
		yield return new WaitForSeconds(5f);
		_enableInput = true;
		OnClose(base.gameObject);
	}

	private void OnTransitionInComplete()
	{
		Coroutines.Run(AnimateRewards());
	}

	private void OnTransitionOutComplete()
	{
		ScreenManager.Instance.FadeIn(() =>
		{
			ScreenManager.Instance.ClosePopup("VaultRewardsUI");
			GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
			BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
			backgroundUI.ShowCliff(true);
			if (Profile.Instance.IsTutorialDone("sequence_tutorial_weapons") && _returnMode == ReturnMode.kVaultInventory)
			{
				ScreenManager.Instance.OpenPopup("VaultInventoryUI");
			}
		});
	}

	private void Awake()
	{
		slotsContainer = Util.GetObjectExactMatch(base.gameObject, "SlotsContainer");
		zoomHolder = Util.GetObjectExactMatch(slotsContainer, "ZoomHolder");
		itemContainers = Util.GetObjects(slotsContainer, "Container").ToList();
		itemContainers.Remove(slotsContainer);
		itemContainers.Sort(SortGameObjectsByName);
		doneButton = Util.GetObjectExactMatch(base.gameObject, "ButtonDone");
		doneButton.SetActive(false);
		equipButton = Util.GetObjectExactMatch(base.gameObject, "ButtonEquip");
		equipButton.SetActive(false);
		starBG = Util.GetObjectExactMatch(base.gameObject, "StarBG");
		TweenRotation tweenRotation = starBG.AddComponent<TweenRotation>();
		tweenRotation.from = new Vector3(0f, 0f, 0f);
		tweenRotation.to = new Vector3(0f, 0f, 180f);
		tweenRotation.style = UITweener.Style.Loop;
		tweenRotation.duration = 2f;
		tweenRotation.enabled = true;
		starBG.SetActive(false);
		GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
		backgroundUI.ShowCliff(false);
		backgroundUI.ShowSmoke(false);
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null)
		{
			NGUITools.SetActive(persistentScreen, false);
		}
	}

	private void OnDisable()
	{
		GameObject obj = ScreenManager.Instance.FindPopup("BackgroundUI");
		BackgroundUI backgroundUI = Util.FindComponent<BackgroundUI>(obj);
		backgroundUI.ShowSmoke(true);
	}

	private void Update()
	{
		if (waitingForRewardsBeforeTransitioning)
		{
			bool flag = true;
			foreach (VaultReward vaultReward in vaultRewards)
			{
				if (!vaultReward.IsReady())
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				waitingForRewardsBeforeTransitioning = false;
				ScreenManager.Instance.FadeOut(() =>
				{
					EBUI_TransitionManager.Instance.RunIntroTransitions(base.gameObject, SafeAction.Wrap(this, OnTransitionInComplete));
				});
			}
		}
		if (autoClose && autoCloseStartTime != 0 && _enableInput)
		{
			int num = EB.Time.Now - autoCloseStartTime;
			if (num >= 5)
			{
				autoCloseStartTime = 0;
				OnClose(base.gameObject);
			}
		}
	}

	private void SetLootTable(LootinatorSpawnTable spawnTable)
	{
		if (!(spawnTable == null))
		{
			GameObject gameObject = Object.Instantiate(spawnTable.gameObject) as GameObject;
			gameObject.transform.parent = base.gameObject.transform;
			LootinatorSpawnTable lootinatorSpawnTable = Util.FindComponent<LootinatorSpawnTable>(gameObject);
			lootRewards = lootinatorSpawnTable.GetRandomPicks();
			PickVaultRewards();
			InitializeRewardArt();
			ApplyRewardEffects();
		}
	}

	private void PickVaultRewards()
	{
		for (int i = 0; i < lootRewards.Count; i++)
		{
			LootinatorSpawnTable.LootEntry lootEntry = lootRewards[i];
			GameObject gameObject = null;
			if (lootEntry.type == LootinatorSpawnTable.LootEntry.LootEntryType.Weapon)
			{
				gameObject = Object.Instantiate(weaponRewardPrefab) as GameObject;
				VaultRewardWeapon vaultRewardWeapon = Util.FindComponent<VaultRewardWeapon>(gameObject);
				InventoryDB.MetaData metaData = GetMetaData(lootEntry.name, InventoryItem.Category.Weapon);
				vaultRewardWeapon.SetData(i, metaData, lootEntry.quantity);
				vaultRewards.Add(vaultRewardWeapon);
				vaultItemAnimStates.Add(0);
			}
			else
			{
				EB.Debug.LogError("Missing type implementation.");
			}
		}
	}

	private void InitializeRewardArt()
	{
		for (int i = 0; i < lootRewards.Count; i++)
		{
			vaultRewards[i].transform.parent = slotsContainer.transform;
			vaultRewards[i].transform.localPosition = zoomHolder.transform.localPosition;
			vaultRewards[i].transform.localScale = new Vector3(2f, 2f, 1f);
			vaultRewards[i].gameObject.SetActive(false);
		}
	}

	private void ApplyRewardEffects()
	{
		bool flag = false;
		foreach (LootinatorSpawnTable.LootEntry lootReward in lootRewards)
		{
			switch (lootReward.type)
			{
			case LootinatorSpawnTable.LootEntry.LootEntryType.Weapon:
				Hub.Instance.InventoryManager.AddItem(lootReward.name, lootReward.quantity);
				Telemetry.Event("item/daily", lootReward.quantity, new Hashtable { { "name", lootReward.name } });
				flag = true;
				break;
			case LootinatorSpawnTable.LootEntry.LootEntryType.CAC:
				Hub.Instance.InventoryManager.AddItem(lootReward.name, lootReward.quantity);
				Telemetry.Event("cac/daily", lootReward.quantity, new Hashtable { { "name", lootReward.name } });
				flag = true;
				break;
			default:
				EB.Debug.LogError("Missing type implementation.");
				break;
			}
		}
		if (flag)
		{
			Telemetry.Flush();
		}
	}

	private InventoryDB.MetaData GetMetaData(string itemName, InventoryItem.Category category)
	{
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(category);
		return categoryList.Find((InventoryDB.MetaData item) => item.name == itemName);
	}

	private void PlayTeleporterFX()
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "fx_teleporter");
		if (objectExactMatch != null)
		{
			ParticleSystem component = objectExactMatch.GetComponent<ParticleSystem>();
			if (component != null)
			{
				component.Play();
				objectExactMatch.AddComponent<PS_Killer>();
			}
		}
	}

	private void PlayFireworksFX()
	{
		GameObject[] objects = Util.GetObjects(base.gameObject, "fx_fireworks_ui");
		GameObject[] array = objects;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(true);
			ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
			if (null != component)
			{
				component.Play();
				gameObject.AddComponent<PS_Killer>();
			}
		}
	}

	private IEnumerator AnimateRewards()
	{
		PlayFireworksFX();
		for (int i = 0; i < lootRewards.Count; i++)
		{
			GameObject lootObj = vaultRewards[i].gameObject;
			lootObj.SetActive(true);
			GameObject itemName = Util.GetObjectExactMatch(lootObj, "ItemName");
			itemName.gameObject.SetActive(false);
			TweenRotation tr = lootObj.AddComponent<TweenRotation>();
			tr.from = new Vector3(0f, 0f, 180f);
			tr.to = new Vector3(0f, 0f, 0f);
			tr.duration = 0.25f;
			tr.enabled = true;
			TweenScale ts = lootObj.AddComponent<TweenScale>();
			ts.from = ItemScaleStage1;
			ts.to = ItemScaleStage2;
			ts.duration = 0.25f;
			ts.enabled = true;
			ts.onFinished = (UITweener tween) =>
			{
				int index = tween.gameObject.GetComponent<VaultReward>().GetIndex();
				itemName.gameObject.SetActive(true);
				vaultItemAnimStates[index] = 1;
			};
			while (vaultItemAnimStates[i] == 0)
			{
				yield return null;
			}
			starBG.SetActive(true);
			yield return new WaitForSeconds(0.75f);
			if (!_isCAC)
			{
				itemName.gameObject.SetActive(false);
				ts.Reset();
				ts.from = ItemScaleStage2;
				ts.to = ItemScaleStage3;
				ts.duration = 0.125f;
				ts.enabled = true;
				ts.onFinished = null;
				TweenPosition tp = lootObj.AddComponent<TweenPosition>();
				tp.from = zoomHolder.transform.localPosition;
				tp.to = itemContainers[i].transform.localPosition;
				tp.duration = 0.125f;
				tp.onFinished = (UITweener tween) =>
				{
					int index = tween.gameObject.GetComponent<VaultReward>().GetIndex();
					vaultItemAnimStates[index] = 2;
				};
				tp.enabled = true;
				starBG.SetActive(false);
				while (vaultItemAnimStates[i] != 2)
				{
					yield return null;
				}
			}
		}
		doneButton.SetActive(true);
		ButtonScript bs = Util.FindComponent<ButtonScript>(doneButton);
		bs.onButtonClick += OnClose;
		if (autoClose)
		{
			autoCloseStartTime = EB.Time.Now;
		}
		if (_isCAC)
		{
			equipButton.SetActive(true);
			ButtonScript bs2 = Util.FindComponent<ButtonScript>(equipButton);
			bs2.onButtonClick += OnEquip;
		}
		EB.Sequence.Component.Activate(base.gameObject, "VaultRewardsUI", typeof(SequenceEvent_OnLootinatorComplete));
	}

	private static int SortGameObjectsByName(GameObject a, GameObject b)
	{
		return string.Compare(a.name, b.name);
	}

	private void EnableInput(bool enable)
	{
		_enableInput = enable;
	}

	private void OnTutorialClose()
	{
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null)
		{
			NGUITools.SetActive(persistentScreen, true);
		}
		EBUI_TransitionManager.Instance.RunOutroTransitions(base.gameObject, OnTransitionOutComplete);
	}

	private void HideContainers()
	{
		foreach (GameObject itemContainer in itemContainers)
		{
			itemContainer.SetActive(false);
		}
	}

	private void OnBack(GameObject caller)
	{
		if (doneButton.activeSelf)
		{
			OnClose(caller);
		}
	}
}
