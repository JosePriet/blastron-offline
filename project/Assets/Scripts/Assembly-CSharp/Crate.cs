using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using UnityEngine;

public class Crate : MonoBehaviour
{
	public enum Type
	{
		kWeapon = 0,
		kBoost = 1,
		kHealth = 2,
		kKey = 3,
		kChest = 4,
		kVault = 5,
		kSkill = 6
	}

	public Type CrateType;

	public int quantity;

	public int pointValue;

	public GameObject inventoryPrefab;

	public string inventoryPrefabName;

	public float widthBuffer = 1f;

	public float depthBuffer;

	private Body _body;

	private bool _done;

	private ReplicationView _view;

	private List<uint> _playersToIgnoreForVault;

	private AudioEmitter _audioEmitter;

	public static string GetPickupMessage(Player player, InventoryDB.MetaData item, int quantity)
	{
		return GetPickupMessage(player.PlayerName, item.friendlyName, quantity);
	}

	public static string GetPickupMessage(string playerName, string itemName, int quantity)
	{
		bool flag = true;
		if (LevelData.Instance != null)
		{
			Player player = LevelData.Instance.GetPlayer();
			flag = player.PlayerName == playerName;
		}
		if (flag)
		{
			return Localizer.Format("ID_GAME_MESSAGE_CRATE_PICKUP", itemName, quantity);
		}
		return Localizer.Format("ID_GAME_MESSAGE_CRATE_PICKUP_OTHER", playerName, itemName, quantity);
	}

	private void Awake()
	{
		if (!GameAttributes.IsTutorialGameMode())
		{
			_view = GetComponent<ReplicationView>();
			if (_view == null)
			{
				_view = base.gameObject.AddComponent<ReplicationView>();
			}
			_view.stateSynchronization = NetworkStateSynchronization.Unreliable;
			_view.observed = this;
			_view.migrationSynchronization = NetworkMigrationSynchronization.Host;
		}
		_done = false;
		_audioEmitter = base.gameObject.AddComponent<AudioEmitter>();
		_audioEmitter.m_PlayMode = AudioEmitter.PlayMode.Orphan;
		_playersToIgnoreForVault = new List<uint>();
	}

	private void Start()
	{
		_body = Misc.DiscoverBody(base.gameObject);
		base.gameObject.SendMessage("SetZ", -20f);
		if (_body != null)
		{
			if (CrateType == Type.kKey)
			{
			}
			if (CrateType == Type.kChest)
			{
			}
			if (CrateType == Type.kSkill)
			{
			}
			_body.CollisionCategories = Category.Cat9;
			_body.CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat9;
			_body.Position = FSHelper.Vector3ToFVector2(base.transform.position);
			_body.Enabled = true;
			_body.SleepingAllowed = false;
			_body.UserData = base.gameObject;
			_body.OnCollision += OnCollisionEvent;
		}
	}

	private void OnObjectKillerDeath()
	{
		if (CrateType == Type.kKey)
		{
			CrateManager.Instance.KeyDestroyed();
		}
		if (CrateType == Type.kChest)
		{
			CrateManager.Instance.ChestDestroyed();
		}
		if (CrateType == Type.kVault)
		{
			CrateManager.Instance.VaultDestroyed();
		}
	}

	private bool OnCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (!_done)
		{
			if (fixtureB.CollisionCategories == Category.Cat2)
			{
				fixtureA.Body.ResetDynamics();
			}
			if (fixtureB.CollisionCategories == Category.Cat1)
			{
				if (fixtureB.Body.UserTag == "Player")
				{
					CrateManager.Instance._cratesInScene.Remove(base.gameObject);
					GameObject gameObject = (GameObject)fixtureB.Body.UserData;
					if (gameObject != null)
					{
						ReplicationView component = gameObject.GetComponent<ReplicationView>();
						if (component != null)
						{
							switch (CrateType)
							{
							case Type.kHealth:
								if (EB.Replication.Manager.IsHost)
								{
									component.RPC("ApplyHealth", RPCMode.AllBuffered, (float)quantity);
									Telemetry.Event("crate/health", quantity);
								}
								break;
							case Type.kWeapon:
							case Type.kBoost:
							{
								if (string.IsNullOrEmpty(inventoryPrefabName))
								{
									break;
								}
								string text = inventoryPrefabName;
								Player component7 = gameObject.GetComponent<Player>();
								ReplicationView component8 = component7.GetComponent<ReplicationView>();
								if (component8 != null && component8.isMine)
								{
									InventoryManager inventoryManager2 = Hub.Instance.InventoryManager;
									if (inventoryManager2.IsOverriden)
									{
										if (!component7.IsAI)
										{
											inventoryManager2.AddOverride(text, quantity);
										}
									}
									else if (!component7.IsAI)
									{
										Telemetry.Event("crate/item", quantity, new Hashtable { { "name", text } });
										inventoryManager2.AddItem(text, quantity);
									}
									UnlocksManager.Instance.Unlock(text + UnlocksManager.showUnlockSuffix);
								}
								GameHUD component9 = Setup.Instance.GetComponent<HUD>().HudObject.GetComponent<GameHUD>();
								InventoryDB.MetaData metaData2 = InventoryDB.GetMetaData(Hash.StringHash(text));
								if (metaData2 != null)
								{
									string pickupMessage2 = GetPickupMessage(component7, metaData2, quantity);
									component9.ShowMessage2(pickupMessage2, Tuning.Instance.Timer_CratePickup);
								}
								else
								{
									EB.Debug.LogWarning("Couldnt find inventory info for item: " + text);
								}
								break;
							}
							case Type.kKey:
								if (EB.Replication.Manager.IsHost)
								{
									Player component6 = gameObject.GetComponent<Player>();
									if (component6.IsDying)
									{
										return true;
									}
									component.RPC("PickupKey", RPCMode.AllBuffered);
								}
								break;
							case Type.kChest:
							{
								Player component3 = gameObject.GetComponent<Player>();
								if ((bool)component3 && component3.HasKey)
								{
									if (EB.Replication.Manager.IsHost)
									{
										component.RPC("ConsumeKey", RPCMode.AllBuffered);
									}
									if (string.IsNullOrEmpty(inventoryPrefabName))
									{
										break;
									}
									ReplicationView component4 = component3.GetComponent<ReplicationView>();
									if (component4 != null && component4.isMine)
									{
										InventoryManager inventoryManager = Hub.Instance.InventoryManager;
										if (inventoryManager.IsOverriden)
										{
											if (!component3.IsAI)
											{
												inventoryManager.AddOverride(inventoryPrefabName, quantity);
											}
										}
										else if (!component3.IsAI)
										{
											Telemetry.Event("crate/chest", quantity, new Hashtable { { "name", inventoryPrefabName } });
											inventoryManager.AddItem(inventoryPrefabName, quantity);
										}
										UnlocksManager.Instance.Unlock(inventoryPrefabName + UnlocksManager.showUnlockSuffix);
									}
									GameHUD component5 = Setup.Instance.GetComponent<HUD>().HudObject.GetComponent<GameHUD>();
									InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(inventoryPrefabName));
									if (metaData != null)
									{
										string pickupMessage = GetPickupMessage(component3, metaData, quantity);
										component5.ShowMessage2(pickupMessage, Tuning.Instance.Timer_CratePickup);
									}
									break;
								}
								fixtureA.Body.ResetDynamics();
								return true;
							}
							case Type.kVault:
								if (!string.IsNullOrEmpty(inventoryPrefabName))
								{
									Player component10 = gameObject.GetComponent<Player>();
									if (_playersToIgnoreForVault.Contains(component10.PlayerId))
									{
										return true;
									}
									ReplicationView component11 = component10.GetComponent<ReplicationView>();
									if (component11 != null && component11.isMine)
									{
										int id = 0;
										try
										{
											id = int.Parse(inventoryPrefabName);
										}
										catch (Exception)
										{
											EB.Debug.LogError("inventoryPrefabName '{0}' could not be parsed into an int.", inventoryPrefabName);
										}
										if (!component10.IsAI)
										{
											Profile.Instance.AddSessionGachaToken(id);
										}
										CrateManager.Instance.VaultPickup(true, "ID_UI_GACHA_SPIN_TOKEN", component10.PlayerName);
									}
								}
								else
								{
									EB.Debug.LogWarning("missing inventory prefab name for vault-type crate, skipping");
								}
								break;
							case Type.kSkill:
							{
								GameHUD component2 = Setup.Instance.GetComponent<HUD>().HudObject.GetComponent<GameHUD>();
								string empty = string.Empty;
								if (component2 != null)
								{
									string playerName = gameObject.GetComponent<Player>().PlayerName;
									if (LevelData.Instance.GetPlayer().PlayerName == playerName)
									{
										AttributesManager.instance.AddSkillPoints(quantity);
										empty = Localizer.Format("ID_GAME_MESSAGE_CRATE_PICKUP", "ID_UI_SKILLPOINTS", quantity);
									}
									else
									{
										empty = Localizer.Format("ID_GAME_MESSAGE_CRATE_PICKUP_OTHER", playerName, "ID_UI_SKILLPOINTS", quantity);
									}
									component2.ShowMessage2(empty, Tuning.Instance.Timer_CratePickup);
								}
								break;
							}
							default:
								EB.Debug.Log("Not implemented crate type " + CrateType);
								break;
							}
							PlayAudio();
							if (component.isMine)
							{
								component.RPC("OnCrate", RPCMode.AllBuffered, pointValue);
							}
						}
						else
						{
							EB.Debug.LogError("missing view on " + gameObject.name);
						}
					}
					else
					{
						EB.Debug.LogError("missing userData on fixture " + fixtureB.UserTag);
					}
					UITutorialArrow component12 = base.gameObject.GetComponent<UITutorialArrow>();
					if (component12 != null)
					{
						EB.Sequence.Component.Activate(base.gameObject, component12.tutorialId, typeof(SequenceEvent_CrateDestroyed));
					}
					else
					{
						EB.Sequence.Component.Activate(base.gameObject, string.Empty, typeof(SequenceEvent_CrateDestroyed));
					}
					_done = true;
					return false;
				}
				fixtureA.Body.ResetDynamics();
			}
			return true;
		}
		return false;
	}

	private void OnPlayerTurnStart(uint playerId)
	{
		if (_playersToIgnoreForVault.Contains(playerId))
		{
			_playersToIgnoreForVault.Remove(playerId);
		}
	}

	private void OnSerializeView(BitStream stream)
	{
		if (_body != null && (!stream.isWriting || !_done))
		{
			FVector2 position = _body.Position;
			FVector2 linearVelocity = _body.LinearVelocity;
			stream.SerializeFixed16(ref position.X, 8);
			stream.SerializeFixed16(ref position.Y, 8);
			stream.SerializeFixed16(ref linearVelocity.X, 8);
			stream.SerializeFixed16(ref linearVelocity.Y, 8);
			if (stream.isReading)
			{
				_body.Position = position;
				_body.LinearVelocity = linearVelocity;
			}
		}
	}

	private void PlayAudio()
	{
		AudioEvent newAudioEvent = null;
		switch (CrateType)
		{
		case Type.kWeapon:
			newAudioEvent = CrateManager.Instance._weaponCrateAudioEvent;
			break;
		case Type.kHealth:
			newAudioEvent = CrateManager.Instance._healthPackAudioEvent;
			break;
		case Type.kKey:
			newAudioEvent = CrateManager.Instance._keyAudioEvent;
			break;
		case Type.kBoost:
			newAudioEvent = CrateManager.Instance._boostAudioEvent;
			break;
		case Type.kChest:
			newAudioEvent = CrateManager.Instance._chestAudioEvent;
			break;
		case Type.kVault:
			newAudioEvent = CrateManager.Instance._chestAudioEvent;
			break;
		}
		_audioEmitter.Play(newAudioEvent);
	}

	private void LateUpdate()
	{
		if (_done)
		{
			CrateManager.Instance.DestroyCrate(base.gameObject);
		}
	}
}
