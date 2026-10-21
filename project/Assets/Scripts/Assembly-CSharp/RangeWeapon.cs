using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using EB.Sparx;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using SmoothMoves;
using UnityEngine;

public class RangeWeapon : MonoBehaviour
{
	private enum Stage
	{
		kAirborne = 0,
		kLanded = 1,
		kBlammo = 2
	}

	public enum Prim
	{
		kEllipse = 0,
		kRectangle = 1
	}

	public const int _maxShots = 8;

	public const float _maxFiringArc = 90f;

	private const int _cMinStatusEffectDamageInterval = 1;

	public float _mass = 1f;

	public float _blastRadius = 32f;

	public float _detonateAfterInterval = 5f;

	public float _detonateWithinRange = 10f;

	public bool _detonateOnContact;

	public bool _detonateOnPlayerContact = true;

	public bool _stick;

	public bool _crawl;

	public float _crawlSpeed = 2f;

	public float _concussionRadius = 64f;

	public float _concussionImpulse = 50f;

	public float _concussionKnockUp = 30f;

	public float _concussionUpThreshold = 0.95f;

	public bool _concussionHorizontalOnly = true;

	public bool _alignToFlight;

	public float _damageMax = 10f;

	public float _damageMin = 1f;

	public float _shotSpeedMin = 1f;

	public float _shotSpeedMax = 50f;

	public float _gravityScale = 1f;

	public float _restitution = 0.707f;

	public float _linearDamping;

	public float _postCollisionDampening;

	public float _width = 1f;

	public float _height = 1f;

	public Prim _ePrim;

	public float _shakeMagnitude = 1f;

	public int _shakeCycles = 3;

	public float _reactionRadius = 64f;

	public float _reactionCheckStartTime = 0.5f;

	public int _shotCount = 1;

	public float _firingArc;

	public float _shotInterval;

	public bool _displayConcussionDebug;

	public GameObject _explosionObject;

	public GameObject _contactObject;

	public GameObject _muzzleFlashObject;

	public Vector2 _muzzleFlashOffset;

	public GameObject _secondaryProjectile;

	public float _secondaryShotArc = 30f;

	public float _secondaryShotLaunchPower = 0.5f;

	public int _secondaryShotCount = 1;

	public bool _secondaryShotUseDir;

	public bool _teleport;

	public float _postTriggerDetonationInterval;

	public AnimationCurve _powerCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public float _activateOwnerCollisionAfterInterval = 0.5f;

	public float _damagePerInterval = 5f;

	public float _intervalDamageRadius = 40f;

	public int _pointsMin = 100;

	public int _pointsMax = 500;

	public float _activationDelay;

	public bool _stickToPlayer;

	public GameObject _residualObject;

	public Player.StatusEffects.Item _statusEffect;

	public bool _noOwnerDamage;

	public AudioEvent _triggerAudio;

	public AudioEvent _impactAudio;

	private bool _blast;

	private bool _triggered;

	private float _detonatingAt;

	private Stage _eStage;

	private float _killFloor = -50f;

	private float _killCeiling = 200f;

	private Body _body;

	private bool _done;

	private Player _owner;

	private bool _partOfScene;

	private Vector3 _dirOnCollision;

	private float _attachedScale = 70f;

	private ReplicationView _view;

	private int _weaponHash = -1;

	private int _weaponLevel = -1;

	private FVector2 _crawlDir;

	private FVector2 _crawlNormal;

	private FVector2 _contactPoint;

	private float _crawlSense;

	private EB.Action _fnUpdate;

	private bool _contact;

	private AABB _contactAABB;

	private bool _attached;

	private GameObject _concreteContactObject;

	private FVector2 _collisionContact;

	private float BlastRadius
	{
		get
		{
			return _blastRadius * 0.1f;
		}
	}

	private float ConcussionRadius
	{
		get
		{
			return _concussionRadius * 0.1f;
		}
	}

	private float ReactionRadius
	{
		get
		{
			return _reactionRadius * 0.1f;
		}
	}

	public Body MyBody
	{
		get
		{
			return _body;
		}
	}

	private void Awake()
	{
		_view = GetComponent<ReplicationView>();
		if (_view == null)
		{
			_view = base.gameObject.AddComponent<ReplicationView>();
		}
		_view.stateSynchronization = NetworkStateSynchronization.Unreliable;
		_view.observed = this;
		_view.migrationSynchronization = NetworkMigrationSynchronization.Destroy;
		WeaponMonitor.Instance.Add(this);
		if (_contactObject != null)
		{
			EB.Replication.Manager.RegisterPrefab(_contactObject);
		}
	}

	private void Start()
	{
		if (_partOfScene)
		{
			CreateBody();
		}
	}

	public void SetInventoryHash(int hash)
	{
		_weaponHash = hash;
	}

	public void SetWeaponLevel(int level)
	{
		_weaponLevel = level;
	}

	public void CreateBody()
	{
		bool flag = false;
		if (_teleport)
		{
			Fixture fixture = FSWorldComponent.PhysicsWorld.TestPoint(FSHelper.Vector3ToFVector2(base.transform.position));
			if (fixture != null && (fixture.CollisionCategories & Category.Cat2) != 0)
			{
				_teleport = false;
				flag = true;
			}
		}
		switch (_ePrim)
		{
		case Prim.kRectangle:
			_body = BodyFactory.CreateRectangle(FSWorldComponent.PhysicsWorld, _width, _height, 1f);
			break;
		case Prim.kEllipse:
			_body = BodyFactory.CreateEllipse(FSWorldComponent.PhysicsWorld, _width, _height, 8, 1f);
			break;
		}
		_body.BodyType = BodyType.Dynamic;
		_body.Position = FSHelper.Vector3ToFVector2(base.transform.position);
		_body.Rotation = base.transform.eulerAngles.z * ((float)Math.PI / 180f);
		_body.CollisionGroup = 0;
		_body.Restitution = _restitution;
		_body.Friction = 1f;
		if (_stick || _crawl)
		{
			_body.CollidesWith = Category.Cat1 | Category.Cat2;
			if (_contactObject != null)
			{
				_body.IsBullet = true;
			}
		}
		else
		{
			_body.CollidesWith = Category.Cat1 | Category.Cat2 | Category.Cat7;
			if (_detonateOnContact || _detonateOnPlayerContact)
			{
				_body.IsBullet = true;
			}
		}
		_body.UserTag = base.name;
		_body.FixedRotation = _alignToFlight;
		_body.Mass = _mass;
		_body.GravityScale = _gravityScale;
		_body.LinearDamping = _linearDamping;
		_body.UserData = base.gameObject;
		ProjectileIndicatorManager.Instance.RegisterProjectile(this);
		Enable(true);
		SendMessage("OnBodyReady", SendMessageOptions.DontRequireReceiver);
		if (flag)
		{
			Detonate();
		}
	}

	private void Enable(bool enable)
	{
		_body.Enabled = enable;
	}

	public void CreateMuzzleFlash(UnityEngine.Transform trans)
	{
		if (_muzzleFlashObject != null && DebugSettings.MuzzleFlashOn)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_muzzleFlashObject, trans.position, trans.rotation) as GameObject;
			Vector3 localScale = gameObject.transform.localScale;
			gameObject.transform.parent = trans;
			gameObject.transform.localScale = localScale;
			Vector3 localPosition = gameObject.transform.localPosition;
			localPosition.x += _muzzleFlashOffset.x;
			localPosition.y += _muzzleFlashOffset.y;
			gameObject.transform.localPosition = localPosition;
			if ((bool)gameObject.GetComponent<ParticleSystem>())
			{
				gameObject.AddComponent<RotateByScale>();
			}
		}
	}

	public float PowerToSpeed(float power)
	{
		return Mathf.Lerp(_shotSpeedMin, _shotSpeedMax, _powerCurve.Evaluate(power));
	}

	public void Launch(float power, Player playerOwner)
	{
		float num = PowerToSpeed(power);
		FVector2 linearVelocity = FSHelper.Vector3ToFVector2(base.transform.forward * num);
		_body.LinearVelocity = linearVelocity;
		if (playerOwner != null && !playerOwner.RightFacing)
		{
			Vector3 localScale = base.transform.localScale;
			localScale.y *= -1f;
			base.transform.localScale = localScale;
		}
		Release(playerOwner, Mathf.Atan2(linearVelocity.Y, linearVelocity.X));
	}

	public void Release(Player playerOwner, float angle)
	{
		_body.Rotation = angle;
		Enable(true);
		_owner = playerOwner;
		if (_owner != null)
		{
			_body.IgnoreCollisionWith(_owner.MyBody);
		}
		_body.AngularVelocity = 0f;
		_eStage = Stage.kAirborne;
		if (_detonateAfterInterval > 0f)
		{
			Detonate(_detonateAfterInterval);
		}
		StartCoroutine(CallAfter(_activationDelay, ActivateCollisionEvents));
		StartCoroutine(CallAfter(Mathf.Max(0.5f, _activateOwnerCollisionAfterInterval), ActivateOwnerCollision));
		RegisterWithNonOwners();
		StartCoroutine(CallAfter(_reactionCheckStartTime, RegisterWithOwner));
	}

	private void OnLevelWasLoaded()
	{
		_partOfScene = true;
	}

	private void ActivateOwnerCollision()
	{
		if (_owner != null)
		{
			_body.RestoreCollisionWith(_owner.MyBody);
		}
	}

	private void ActivateCollisionEvents()
	{
		_body.OnCollision += OnCollisionEvent;
	}

	private void RegisterWithOwner()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		List<EB.Sparx.Player> players = game.Players;
		foreach (EB.Sparx.Player item in players)
		{
			uint playerId = item.PlayerId;
			if (playerId == _view.instantiatorId)
			{
				Player player = LevelData.Instance.GetPlayer(playerId);
				if (player != null)
				{
					player.RegisterProjectile(base.transform, ReactionRadius);
				}
				break;
			}
		}
	}

	private void RegisterWithNonOwners()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		List<EB.Sparx.Player> players = game.Players;
		foreach (EB.Sparx.Player item in players)
		{
			uint playerId = item.PlayerId;
			if (playerId != _view.instantiatorId)
			{
				Player player = LevelData.Instance.GetPlayer(playerId);
				if (player != null)
				{
					player.RegisterProjectile(base.transform, ReactionRadius);
				}
			}
		}
	}

	private void UnregisterWithPlayers()
	{
		Game game = Hub.Instance.GameManager.Game;
		if (game == null)
		{
			return;
		}
		List<EB.Sparx.Player> players = game.Players;
		foreach (EB.Sparx.Player item in players)
		{
			uint playerId = item.PlayerId;
			Player player = LevelData.Instance.GetPlayer(playerId);
			if (player != null)
			{
				player.UnregisterProjectile(base.transform);
			}
		}
	}

	private IEnumerator DetonateAfter(float seconds)
	{
		if (seconds > 0f)
		{
			yield return new WaitForSeconds(seconds);
		}
		_collisionContact = _body.Position;
		Detonate();
	}

	private IEnumerator CallAfter(float period, EB.Action fn)
	{
		if (period > 0f)
		{
			yield return new WaitForSeconds(period);
		}
		fn();
	}

	private void LateUpdate()
	{
		if (_blast)
		{
			_blast = false;
			if (_view.isMine && _owner.PlayerId == _view.instantiatorId)
			{
				FVector2 fVector = _collisionContact;
				if (_stick || _crawl || _stickToPlayer)
				{
					fVector = _body.Position;
				}
				_view.RPC("Blast", RPCMode.AllBuffered, fVector, _concussionImpulse, ConcussionRadius);
				ApplyConcussion(fVector, _concussionImpulse, ConcussionRadius);
				WeaponMonitor.Instance.Blast(FSHelper.FVector2ToVector3(fVector), ConcussionRadius, BlastRadius);
				EB.Replication.Manager.Destroy(base.gameObject);
			}
			UnregisterWithPlayers();
			Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in componentsInChildren)
			{
				renderer.enabled = false;
			}
		}
		if (_body == null)
		{
			return;
		}
		if (_body.Enabled)
		{
			FVector2 linearVelocity = _body.LinearVelocity;
			if (_alignToFlight)
			{
				_body.Rotation = Mathf.Atan2(linearVelocity.Y, linearVelocity.X);
			}
			Vector3 position = base.transform.position;
			position.x = _body.Position.X;
			position.y = _body.Position.Y;
			Vector3 zero = Vector3.zero;
			zero.z = _body.Rotation * 57.29578f;
			position.z = -10f;
			base.transform.position = position;
			base.transform.rotation = Quaternion.Euler(zero);
		}
		else
		{
			_body.Position = FSHelper.Vector3ToFVector2(base.transform.position);
		}
		if ((base.transform.position.y < _killFloor || base.transform.position.y > _killCeiling) && _view.isMine)
		{
			EB.Replication.Manager.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (_body != null && FSWorldComponent.IsValid)
		{
			_body.Dispose();
		}
		ProjectileIndicatorManager.Instance.UnregisterProjectile(this);
		if ((bool)_owner)
		{
			_owner.SpawnProjectile();
		}
		StopAllCoroutines();
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

	private void PlaySound(AudioEvent audioEvent)
	{
		if (audioEvent != null)
		{
			AudioEmitter component = GetComponent<AudioEmitter>();
			if (component != null)
			{
				component.Play(audioEvent);
			}
		}
	}

	private bool OnCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		PlaySound(_impactAudio);
		FVector2 normal;
		FixedArray2<FVector2> points;
		contact.GetWorldManifold(out normal, out points);
		if (FVector2.Dot(_body.LinearVelocity, normal) > 0f)
		{
			normal = -normal;
		}
		_collisionContact = points[0];
		_dirOnCollision = base.transform.right;
		if (!_done)
		{
			if (fixtureB.Body.UserTag == "Player")
			{
				if (!_detonateOnPlayerContact)
				{
					if (_stickToPlayer)
					{
						GameObject gameObject = fixtureB.Body.UserData as GameObject;
						if (gameObject != null)
						{
							Player component = gameObject.GetComponent<Player>();
							component.AttachToHead(base.transform, Vector3.forward * -10f);
							Vector3 normalized = base.transform.localScale.normalized;
							normalized *= _attachedScale;
							base.transform.localScale = normalized;
							_attached = true;
							BoneAnimation componentInChildren = GetComponentInChildren<BoneAnimation>();
							if (componentInChildren != null && componentInChildren.animation["trigger_anim"] != null)
							{
								componentInChildren.Play("trigger_anim");
								PlaySound(_triggerAudio);
							}
							if (_postTriggerDetonationInterval > 0f)
							{
								StartDetonation(_postTriggerDetonationInterval);
							}
						}
						else
						{
							EB.Debug.LogError(string.Format("Body identifying itself as a \"Player\" does not have a GameObject as UserData {0}!", fixtureB.Body.UserData));
						}
						_body.OnCollision -= OnCollisionEvent;
					}
					return true;
				}
				StartDetonation(0f);
				return true;
			}
			if (_detonateOnContact)
			{
				StartDetonation(0f);
				return true;
			}
			_body.LinearDamping = _postCollisionDampening;
			if (_crawl && _eStage == Stage.kAirborne)
			{
				if (fixtureB.Body.UserTag == "Terrain" || fixtureB.Body.UserTag == "FSStatic")
				{
					_eStage = Stage.kLanded;
					RayCastInput input = default(RayCastInput);
					input.Point1 = FSHelper.Vector3ToFVector2(base.transform.position) - _body.LinearVelocity;
					input.Point2 = input.Point1 + _body.LinearVelocity;
					input.MaxFraction = 10f;
					RayCastOutput output;
					if (fixtureB.RayCast(out output, ref input, 0))
					{
						FVector2 linearVelocity = _body.LinearVelocity;
						linearVelocity.Normalize();
						_crawlSense = CrawlSenseFromImpactDirection(linearVelocity, output.Normal);
						_crawlDir = RightDirFromNormal(output.Normal) * _crawlSense;
						_crawlNormal = output.Normal;
						_contactPoint = input.Point1 + output.Fraction * (input.Point2 - input.Point1);
					}
					fixtureB.GetAABB(out _contactAABB, 0);
					_contact = true;
				}
			}
			else if (_stick)
			{
				_contact = true;
				if (_contactObject != null)
				{
					CreateContactObject(normal);
				}
				if (_shotInterval > 0f)
				{
					StartCoroutine(ApplyDamage());
				}
			}
		}
		return true;
	}

	private void CreateHole(FVector2 pos)
	{
		if (BlastRadius > 0.1f)
		{
			LevelData.Instance.BlastHole(pos, BlastRadius);
		}
	}

	public bool WillDelayEndOfGame()
	{
		if (_crawl && _detonateAfterInterval > 0f)
		{
			return true;
		}
		if (_stick && _eStage == Stage.kLanded)
		{
			return false;
		}
		return true;
	}

	[RPC]
	private void Blast(FVector2 pos, float power, float range)
	{
		EB.Debug.LogWarning(string.Format("BLAST! p {0} + r {1}", pos, range));
		CreateHole(pos);
		if (_explosionObject != null && DebugSettings.ExplosionsOn)
		{
			Instantianator.Instance.Enqueue(_explosionObject, base.transform.position + Vector3.back * 10f, Quaternion.identity);
		}
		if (_secondaryProjectile != null)
		{
			FireSecondary(pos);
		}
		if (_residualObject != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_residualObject, pos.ToVector3() + Vector3.back * 10f, Quaternion.identity) as GameObject;
			EffectVolume component = gameObject.GetComponent<EffectVolume>();
			if (component != null)
			{
				component.Parent = this;
				component.ImpactDir = _dirOnCollision;
				component.Owner = _owner;
				component.SetOriginatingWeaponInfo(_weaponHash, _weaponLevel);
			}
		}
		Camera.mainCamera.SendMessage("Shake", new CameraShake.ShakeInfo(_shakeMagnitude, _shakeCycles));
	}

	private void FireSecondary(FVector2 pos)
	{
		int secondaryShotCount = _secondaryShotCount;
		float secondaryShotArc = _secondaryShotArc;
		Vector3 vector = _body.LinearVelocity.ToVector3();
		vector.Normalize();
		Vector3 vector2 = ((!_secondaryShotUseDir) ? Vector3.up : vector);
		Vector3 vector3 = Quaternion.AngleAxis(secondaryShotArc * 0.5f, Vector3.forward) * vector2;
		Vector3 to = Quaternion.AngleAxis((0f - secondaryShotArc) * 0.5f, Vector3.forward) * vector2;
		float num = 0f;
		if (secondaryShotCount > 1)
		{
			num = 1f / (float)(secondaryShotCount - 1);
		}
		float num2 = 0f;
		for (int i = 0; i < secondaryShotCount; i++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_secondaryProjectile, pos.ToVector3(), Quaternion.identity) as GameObject;
			RangeWeapon component = gameObject.GetComponent<RangeWeapon>();
			component.SetInventoryHash(_weaponHash);
			component.SetWeaponLevel(_weaponLevel);
			Firing.Instance.FireSingle(gameObject.GetComponent<RangeWeapon>(), Vector3.Slerp(vector3, to, num2), _owner, _secondaryShotLaunchPower, base.transform, Category.Cat6, false, "bot_shoot");
			num2 += num;
		}
	}

	private void ApplyConcussion(FVector2 pos, float power, float range)
	{
		Fixture hitter = _body.FixtureList[0];
		float num = range * range;
		FVector2 linearVelocity = _body.LinearVelocity;
		if (linearVelocity.LengthSquared() > 1.1920929E-07f)
		{
			linearVelocity.Normalize();
		}
		List<Misc.DeferredHit> list = new List<Misc.DeferredHit>();
		List<Fixture> withinRangeDamageables = new List<Fixture>();
		range = Mathf.Max(4f, range);
		LevelData.Instance.PlayerRangeQuery((Fixture f) =>
		{
			withinRangeDamageables.Add(f);
		}, pos, range);
		LevelData.Instance.DestructibleRangeQuery((Fixture f) =>
		{
			withinRangeDamageables.Add(f);
		}, pos, range);
		WeaponMonitor.Instance.WeaponRangeQuery((Fixture f) =>
		{
			withinRangeDamageables.Add(f);
		}, pos, range);
		foreach (Fixture item in withinRangeDamageables)
		{
			if (!Misc.CanDamage(item, hitter) || (item.CollisionCategories & Category.Cat6) != 0)
			{
				continue;
			}
			Body body = item.Body;
			if (item.UserTag == "Player" && _noOwnerDamage)
			{
				GameObject gameObject = body.UserData as GameObject;
				if (gameObject != null && gameObject.GetComponent<Player>() == _owner)
				{
					continue;
				}
			}
			AABB fatAABB = Misc.GetFatAABB(item);
			FarseerPhysics.Common.Transform transform;
			body.GetTransform(out transform);
			FVector2 fVector = Misc.ClosestPointOnAABB(pos, fatAABB);
			FVector2 fVector2 = fVector - pos;
			float num2 = fVector2.LengthSquared();
			if (!(num2 < num))
			{
				continue;
			}
			float num3 = 1f - Mathf.Min(Mathf.Sqrt(num2) / Mathf.Sqrt(num));
			FVector2 fVector3 = fVector2;
			if (fVector2.LengthSquared() > 1.1920929E-07f)
			{
				fVector3.Normalize();
			}
			else
			{
				fVector3 = linearVelocity;
			}
			if (num3 > _concussionUpThreshold)
			{
				fVector3.Y = Mathf.Abs(fVector3.Y);
			}
			if (_concussionHorizontalOnly)
			{
				fVector3.Y = 0f;
			}
			FVector2 linearImpulse = (fVector3 * power + new FVector2(0f, _concussionKnockUp)) * num3;
			Misc.DeferredHit deferredHit = new Misc.DeferredHit();
			deferredHit._linearImpulse = linearImpulse;
			deferredHit._fixture = item;
			list.Add(deferredHit);
			if (item.Body.UserData == null)
			{
				continue;
			}
			GameObject gameObject2 = item.Body.UserData as GameObject;
			if (gameObject2 == null)
			{
				continue;
			}
			ReplicationView component = gameObject2.GetComponent<ReplicationView>();
			if (component == null)
			{
				continue;
			}
			deferredHit._receiver = component;
			float damage = num3 * (_damageMax - _damageMin) + _damageMin;
			deferredHit._hitInfo._dir = fVector2;
			deferredHit._hitInfo._damage = damage;
			deferredHit._hitInfo._mag = power * num3;
			deferredHit._hitInfo._pos = item.Body.Position;
			deferredHit._hitInfo._attackerID = _view.instantiatorId;
			deferredHit._hitInfo._points = (short)Mathf.RoundToInt(Mathf.Lerp(_pointsMin, _pointsMax, num3));
			deferredHit._hitInfo._weaponHash = _weaponHash;
			deferredHit._hitInfo._weaponLevel = _weaponLevel;
			if (deferredHit._hitInfo._points > 0)
			{
				float pointsMultiplier = ItemLevelManager.Instance.GetPointsMultiplier(_weaponHash, _weaponLevel);
				deferredHit._hitInfo._bonusPoints = (short)Mathf.RoundToInt((float)deferredHit._hitInfo._points * pointsMultiplier);
			}
			if (_statusEffect._eType != Player.StatusEffects.Type.kNone && _statusEffect._lifetime > 0f)
			{
				if (_statusEffect._eType == Player.StatusEffects.Type.kDamage)
				{
					_statusEffect._applicationInterval = Mathf.Max(1, _statusEffect._applicationInterval);
				}
				deferredHit._hitInfo._effectType = (ushort)_statusEffect._eType;
				deferredHit._hitInfo._effectAmount = (ushort)_statusEffect._amount;
				deferredHit._hitInfo._effectVisualType = (ushort)_statusEffect._eVisualType;
				deferredHit._hitInfo._effectApplicationInterval = (ushort)_statusEffect._applicationInterval;
				deferredHit._hitInfo._effectDurationSecs = (ushort)_statusEffect._lifetime;
			}
			if ((bool)_owner)
			{
				_owner.ProcessOutgoingHit(ref deferredHit._hitInfo);
			}
		}
		foreach (Misc.DeferredHit item2 in list)
		{
			if (item2._receiver != null)
			{
				item2._receiver.RPC("ApplyHit", RPCMode.AllBuffered, item2._hitInfo);
			}
			else
			{
				item2._fixture.Body.ApplyLinearImpulse(item2._linearImpulse);
			}
		}
	}

	private void Update()
	{
		if (_contact)
		{
			_body.BodyType = BodyType.Kinematic;
			_body.IgnoreGravity = true;
			_body.LinearVelocity = FVector2.Zero;
			_body.Position = _collisionContact;
			if (_stick)
			{
				_body.AngularVelocity = 0f;
			}
			_eStage = Stage.kLanded;
			_contact = false;
			_body.OnCollision -= OnCollisionEvent;
			if (_crawl)
			{
				_fnUpdate = Crawl;
			}
		}
		if (_fnUpdate != null)
		{
			_fnUpdate();
		}
		if (_attached)
		{
			_body.Enabled = false;
		}
		if (!_triggered && _detonateWithinRange > 0f && _eStage == Stage.kLanded)
		{
			AABB aabb = new AABB(_body.Position, _detonateWithinRange * 2f, _detonateWithinRange * 2f);
			LevelData.Instance.AABBPlayerQuery((Player player) =>
			{
				StartDetonation(_postTriggerDetonationInterval);
			}, aabb);
		}
	}

	private void StartDetonation(float delay)
	{
		Detonate(delay);
	}

	private void Detonate(float seconds)
	{
		if (seconds <= 0f)
		{
			StopAllCoroutines();
			Detonate();
		}
		else if (!_triggered || seconds < _detonatingAt - UnityEngine.Time.realtimeSinceStartup)
		{
			StopAllCoroutines();
			_triggered = true;
			_detonatingAt = UnityEngine.Time.realtimeSinceStartup + seconds;
			StartCoroutine(DetonateAfter(seconds));
		}
	}

	private void Detonate()
	{
		_body.OnCollision -= OnCollisionEvent;
		if (!_done)
		{
			_blast = true;
			_done = true;
			if (_teleport)
			{
				Body myBody = _owner.MyBody;
				myBody.SetTransform(_body.Position, 0f);
			}
			if (_concreteContactObject != null)
			{
				UnityEngine.Debug.Log("Destroying the contact object! " + _concreteContactObject);
				EB.Replication.Manager.Destroy(_concreteContactObject);
			}
		}
	}

	private FVector2 RightDirFromNormal(FVector2 n)
	{
		return new FVector2(n.Y, 0f - n.X);
	}

	private float CrawlSenseFromImpactDirection(FVector2 hitDir, FVector2 n)
	{
		if (FVector2.Dot(hitDir, RightDirFromNormal(n)) > 0f)
		{
			return 1f;
		}
		return -1f;
	}

	private void Crawl()
	{
		_body.LinearVelocity = _crawlDir * _crawlSpeed;
		FVector2 fVector = _body.Position + _crawlNormal * _height * 0.5f;
		FVector2 fVector2 = -_crawlNormal * _height * 2f;
		FVector2 start = fVector;
		FVector2 dir = -fVector2;
		if (Misc.TestPoint(fVector, _body.FixtureList[0]))
		{
			_fnUpdate = null;
			StartDetonation(0f);
			_body.LinearVelocity = FVector2.Zero;
			return;
		}
		FVector2 hitPoint;
		FVector2 hitNormal;
		Fixture hitFixture;
		Fixture fixture;
		FVector2 hitPoint2;
		FVector2 hitNormal2;
		Fixture hitFixture2;
		if (Misc.RayCastForClosest(start, dir, out hitPoint, out hitNormal, out hitFixture, _body))
		{
			_crawlNormal = hitNormal;
			_crawlDir = RightDirFromNormal(hitNormal) * _crawlSense;
			_body.Position = hitPoint;
			fixture = hitFixture;
		}
		else if (Misc.RayCastForClosest(fVector, fVector2, out hitPoint2, out hitNormal2, out hitFixture2, _body))
		{
			_crawlNormal = hitNormal2;
			_crawlDir = RightDirFromNormal(hitNormal2) * _crawlSense;
			fixture = hitFixture2;
		}
		else
		{
			(_contactAABB.Center - _body.Position).Normalize();
			fVector += fVector2 * 0.5f;
			fVector2 = RightDirFromNormal(fVector2) * _crawlSense * 2f;
			if (Misc.RayCastForClosest(fVector, fVector2, out hitPoint2, out hitNormal2, out hitFixture2, _body))
			{
				_crawlNormal = hitNormal2;
				_crawlDir = -RightDirFromNormal(hitNormal2) * _crawlSense;
				_body.Position = hitPoint2;
				fixture = hitFixture2;
			}
			else
			{
				_body.Enabled = true;
				_body.BodyType = BodyType.Dynamic;
				_body.ApplyForce(_crawlNormal * 5f);
				_eStage = Stage.kAirborne;
				_body.IgnoreGravity = false;
				fixture = null;
				StartCoroutine(DoCollideAfter(0.2f));
				_fnUpdate = null;
			}
		}
		if (fixture != null)
		{
			fixture.GetAABB(out _contactAABB, 0);
			if (fixture.Body.UserTag == "FSStatic")
			{
				_contactAABB.LowerBound += fixture.Body.Position;
				_contactAABB.UpperBound += fixture.Body.Position;
			}
		}
	}

	private IEnumerator DoCollideAfter(float time)
	{
		yield return new WaitForSeconds(time);
		_body.OnCollision += OnCollisionEvent;
	}

	private void OnDamage(Misc.DamageInfo dmgInfo)
	{
		StartDetonation(_postTriggerDetonationInterval);
	}

	private void Stop()
	{
	}

	private void CreateContactObject(FVector2 normal)
	{
		if (_concreteContactObject == null && _view.isMine)
		{
			Vector3 position = _body.Position.ToVector3();
			position.z = -30f;
			_concreteContactObject = (GameObject)EB.Replication.Manager.Instantiate(_contactObject, position, Misc.DirectionToQuat(normal.ToVector3()));
			BeamWeapon component = _concreteContactObject.GetComponent<BeamWeapon>();
			if (component != null)
			{
				component.Fire(normal.ToVector3(), _owner, null, null, base.gameObject, true);
			}
		}
	}

	private IEnumerator ApplyDamage()
	{
		while (true)
		{
			Camera.mainCamera.SendMessage("Shake", new CameraShake.ShakeInfo(_shakeMagnitude, _shakeCycles));
			DoApplyDamage();
			yield return new WaitForSeconds(_shotInterval);
		}
	}

	private void DoApplyDamage()
	{
		List<GameObject> list = new List<GameObject>();
		FVector2 pos = FSHelper.Vector3ToFVector2(base.transform.position);
		Misc.HitInfo hitInfo = new Misc.HitInfo
		{
			_damage = _damagePerInterval,
			_attackerID = _view.instantiatorId,
			_pos = pos,
			_weaponLevel = _weaponLevel
		};
		List<Fixture> withinRangeDamageables = new List<Fixture>();
		float range = Mathf.Max(4f, _intervalDamageRadius);
		LevelData.Instance.PlayerRangeQuery((Fixture f) =>
		{
			withinRangeDamageables.Add(f);
		}, pos, range);
		if (!_view.isMine)
		{
			return;
		}
		if (_owner != null)
		{
			_owner.ProcessOutgoingHit(ref hitInfo);
		}
		foreach (Fixture item in withinRangeDamageables)
		{
			if (!(item.UserTag == "Player"))
			{
				continue;
			}
			GameObject gameObject = item.Body.UserData as GameObject;
			if (gameObject != null && (!_noOwnerDamage || !(_owner.gameObject == gameObject)))
			{
				ReplicationView component = gameObject.GetComponent<ReplicationView>();
				if (component != null)
				{
					component.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
		Misc.DrawRaycastGizmos();
		if (_body != null)
		{
			if (_detonateWithinRange > 0f)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(_body.Position.ToVector3(), _detonateWithinRange);
			}
			if (_blastRadius > 0f)
			{
				Gizmos.color = new Color(1f, 0.5f, 0f);
				Gizmos.DrawWireCube(_body.Position.ToVector3(), new Vector3(BlastRadius * 2f, BlastRadius * 2f, 1f));
			}
			if (_concussionRadius > 0f)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_body.Position.ToVector3(), ConcussionRadius);
			}
			if (_reactionRadius > 0f)
			{
				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(_body.Position.ToVector3(), ReactionRadius);
			}
			if (_body.Enabled)
			{
				Gizmos.color = ((!_body.Enabled) ? Color.cyan : Color.red);
			}
			Gizmos.DrawSphere(_body.Position.ToVector3(), 0.5f);
		}
	}

	[RPC]
	private void ApplyHit(object hitInfo)
	{
		if (_stick)
		{
			Detonate();
		}
	}
}
