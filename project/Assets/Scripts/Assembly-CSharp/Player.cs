using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EB;
using EB.Replication;
using EB.Sequence;
using EB.Sparx;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using SmoothMoves;
using UnityEngine;

public class Player : MonoBehaviour
{
	public enum JetpackMode
	{
		kNone = 0,
		kJetPack = 1,
		kHoverPack = 2
	}

	private class RevengeSaleInfo
	{
		public string weaponName;

		public string attackerName;
	}

	public class StatusEffects
	{
		public enum Type
		{
			kNone = 0,
			kDamage = 1,
			kSpeed = 2
		}

		[System.Serializable]
		public class Item
		{
			public Type _eType;

			public PlayerEffects.Type _eVisualType;

			public int _amount;

			[NonSerialized]
			public uint _attackerID;

			public int _applicationInterval;

			[NonSerialized]
			public float _currentTime;

			public float _lifetime;

			public Item(Type eType, PlayerEffects.Type eVisualType, int amount, uint attackerID, int applicationInterval, float lifetime)
			{
				_eType = eType;
				_eVisualType = eVisualType;
				_amount = amount;
				_attackerID = attackerID;
				_applicationInterval = applicationInterval;
				_currentTime = _applicationInterval;
				_lifetime = lifetime;
			}
		}

		private float _speedModifier = 1f;

		private List<Item> _activeItems = new List<Item>();

		private Player _player;

		public float SpeedModifier
		{
			get
			{
				return 1f - _speedModifier * 0.01f;
			}
		}

		public StatusEffects(Player p)
		{
			_player = p;
		}

		public void Clear()
		{
			for (int i = 0; i < _activeItems.Count; i++)
			{
				_player._view.RPC("RemoveVisualEffectRPC", RPCMode.AllBuffered, _activeItems[i]._eVisualType.ToString());
			}
			_activeItems.Clear();
			CalculateSpeedModifier();
		}

		public void Add(Type eType, PlayerEffects.Type eVisualType, int amount, uint attackerID, int applicationInterval, float durationSecs)
		{
			_activeItems.Add(new Item(eType, eVisualType, amount, attackerID, applicationInterval, durationSecs));
			CalculateSpeedModifier();
		}

		private void CalculateSpeedModifier()
		{
			_speedModifier = 0f;
			_activeItems.ForEach((Item item) =>
			{
				if (item._eType == Type.kSpeed)
				{
					_speedModifier += item._amount;
				}
			});
			_speedModifier = Mathf.Min(_speedModifier, 100f);
		}

		public void ApplyIntervalStatusEffects(Player p)
		{
			for (int i = 0; i < _activeItems.Count; i++)
			{
				Item item = _activeItems[i];
				if (item._eType == Type.kDamage)
				{
					item._currentTime -= UnityEngine.Time.smoothDeltaTime;
					if (item._currentTime < 0f)
					{
						Misc.HitInfo hitInfo = new Misc.HitInfo
						{
							_dir = FVector2.Zero,
							_damage = item._amount,
							_mag = 0f,
							_pos = FVector2.Zero,
							_attackerID = item._attackerID,
							_points = 0
						};
						p.GetComponent<View>().RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
						item._currentTime = item._applicationInterval;
					}
				}
				item._lifetime -= UnityEngine.Time.smoothDeltaTime;
				if (item._lifetime < 0f)
				{
					_player._view.RPC("RemoveVisualEffectRPC", RPCMode.AllBuffered, _activeItems[i]._eVisualType.ToString());
					_activeItems.RemoveAt(i);
					i--;
					CalculateSpeedModifier();
				}
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(string.Format("There are {0} active effects", _activeItems.Count));
			foreach (Item activeItem in _activeItems)
			{
				stringBuilder.AppendLine(activeItem._eType.ToString());
				stringBuilder.AppendLine(activeItem._eVisualType.ToString());
				stringBuilder.AppendLine(activeItem._eType.ToString());
				stringBuilder.AppendLine(activeItem._amount.ToString());
				stringBuilder.AppendLine(activeItem._lifetime.ToString());
			}
			return stringBuilder.ToString();
		}
	}

	public struct ActiveBoost
	{
		public enum State
		{
			kInactive = 0,
			kActive = 1,
			kCooldown = 2
		}

		private int Duration;

		private float remainingTime;

		public GameObject currentEffect;

		private State BoostState;

		public bool Valid
		{
			get
			{
				return Boost != null;
			}
		}

		public BoostItem Boost { get; set; }

		public float _remainingTime
		{
			get
			{
				if (Valid)
				{
					return remainingTime;
				}
				return 0f;
			}
			set
			{
				remainingTime = value;
			}
		}

		public void Set(BoostItem boostItem)
		{
			Boost = boostItem;
			if (Boost != null)
			{
				BoostState = State.kActive;
				Duration = Boost._durationSecs;
				remainingTime = Duration;
			}
		}

		public bool UpdateState()
		{
			bool result = false;
			if (TimerExpired())
			{
				switch (BoostState)
				{
				case State.kActive:
					Cooldown();
					result = true;
					break;
				case State.kCooldown:
					BoostState = State.kInactive;
					result = true;
					Boost = null;
					ScreenManager.Instance.BroadcastMessage("UpdateBoosts");
					break;
				}
			}
			return result;
		}

		public State GetState()
		{
			return BoostState;
		}

		public float RemainingDurationPercentage()
		{
			if (Valid)
			{
				float num = remainingTime;
				float value = num / (float)Duration;
				return Mathf.Clamp(value, 0f, 1f);
			}
			return 0f;
		}

		public bool TimerExpired()
		{
			return remainingTime <= 0f;
		}

		public void Cooldown()
		{
			if (BoostState == State.kActive)
			{
				BoostState = State.kCooldown;
				Duration = Boost._cooldownSecs;
				remainingTime = Duration;
				ScreenManager.Instance.BroadcastMessage("UpdateBoosts");
			}
		}

		public bool IsActive()
		{
			return Valid && BoostState == State.kActive;
		}

		public bool IsCooldown()
		{
			return Valid && BoostState == State.kCooldown;
		}

		public override string ToString()
		{
			if (Valid)
			{
				return string.Format("{0}, {1}: {2}", Boost.name, (!IsActive()) ? "CooldownTime" : "RemainingTime", remainingTime.ToString());
			}
			return "invalid";
		}
	}

	public struct InactiveBoost
	{
		public float cooldownTime;

		public BoostItem.Type boostType;
	}

	public struct PlayerInstructions
	{
		public bool faceRight;

		public bool faceLeft;

		public bool goLeft;

		public bool goRight;

		public bool jump;

		public bool drop;

		public bool fire;

		public bool airborne;

		public bool aiming;

		public Vector3 fireDir;

		public float fireMag;

		public Vector3 moveDir;

		public override string ToString()
		{
			return string.Format("faceRight {0}\nfaceLeft {1}\ngoLeft {2}\ngoRight {3}\njump {4}\nfire {5}\nairborne {6}\naiming {7}\nfireDir {8}\nfireMag {9}", faceLeft, faceRight, goLeft, goRight, jump, fire, airborne, aiming, fireDir, fireMag);
		}
	}

	public struct Move
	{
		public bool _left;

		public bool _right;

		public bool _up;

		public bool _down;

		public bool _fire;

		public bool _aiming;

		public Vector3 _aimDir;

		public float _power;

		public bool Transmitted { get; set; }

		public bool Idle
		{
			get
			{
				return Moving == _fire != _aiming;
			}
		}

		public bool Moving
		{
			get
			{
				return _left || _right || _up;
			}
		}

		public void Serialize(BitStream stream)
		{
			stream.Serialize(ref _left);
			stream.Serialize(ref _right);
			stream.Serialize(ref _up);
			stream.Serialize(ref _fire);
			stream.Serialize(ref _aiming);
			if (_aiming || _fire)
			{
				if (stream.isReading)
				{
					float value = 0f;
					stream.SerializeFixed16(ref value, 8);
					_aimDir = new Vector3(Mathf.Cos(value), Mathf.Sin(value), 0f);
				}
				else
				{
					float value2 = Mathf.Atan2(_aimDir.y, _aimDir.x);
					stream.SerializeFixed16(ref value2, 8);
				}
				stream.SerializeFixed8(ref _power, 4);
			}
			Transmitted = true;
		}

		public void Merge(Move other)
		{
			_left = other._left;
			_right = other._right;
			_up = other._up;
			_down = other._down;
			_fire |= other._fire;
			_aiming = other._aiming;
			_power = other._power;
			_aimDir = other._aimDir;
		}

		public void Reset()
		{
			_left = (_right = (_up = (_fire = (_aiming = false))));
		}
	}

	private struct AnimEventInfo
	{
		public string animName;

		public string fnName;

		public float time;

		public string parm;
	}

	private delegate void IdleFunction();

	private const float SmoothMoves_PlayerHeight = 300f;

	public const int _boostSlotCount = 3;

	private Body _body;

	private bool _dying;

	private bool _specialIdle;

	private bool _hitAnticipation;

	private bool _knockback;

	private bool _allowIdle;

	private BoneAnimation _smCharacter;

	private bool _started;

	private bool _scheduledSpawnProjectileAndGameHud;

	private ReplicationView _view;

	private bool _centreProbe;

	private float _height;

	private float _radius;

	private float _baseJumpSpeed;

	public string tutorialId;

	private bool _hasKey;

	private PlayerEffects _playerEffects;

	private Health _health;

	private Healthbar _healthBar;

	private Shield _shield;

	private uint _lastAttackerID;

	private bool _inputEnabled;

	private float _damageMultiplier = 1f;

	public float severeDamagePercentage = 0.5f;

	public ParticleSystem severeDamageParticlesPrefab;

	private ParticleSystem severeDamageParticles;

	public float severeDamageMinDelay;

	public float severeDamageMaxDelay = 1f;

	public float criticalDamagePercentage = 0.3f;

	public ParticleSystem criticalDamageParticlesPrefab;

	private ParticleSystem criticalDamageParticles;

	public AudioEvent _eventLand;

	public AudioEvent _eventMove;

	public AudioEvent _eventMyTurn;

	public AudioEvent _eventCelebrate;

	public GameObject respawnFX;

	private GameObject respawnInstance;

	private int StartingWeapon = -1;

	private RevengeSaleInfo _revengeSaleInfo;

	private int extraShots;

	private GameObject weaponBone;

	private GameObject _headBone;

	private CharacterDB.Recipe _myRecipe;

	private int _currentWeaponIndex = Arsenal.EmptyWeaponIndex;

	private ActiveBoost[] _activeBoosts = new ActiveBoost[3];

	public InactiveBoost[] inactiveBoosts = new InactiveBoost[10];

	private bool _jumping;

	private string[] _uninterruptibleAnims = new string[2] { "bot_drowning", "bot_death" };

	private Vector3 _userMoveDir;

	private string _currentIdle = "bot_idle";

	private Move _move2;

	private Move _move2Repl;

	private Vector3 LAUNCHPOS;

	private int _scoreLabelID;

	private bool _aiming;

	private int _contactCount;

	private bool _grounded;

	private bool _flying;

	private bool _shieldActive;

	private string[] _idleAnims = new string[6] { "bot_bored_polish_head_idle", "bot_yoga_poses_idle", "bot_worm", "bot_pushups", "bot_moonwalk", "bot_checks_phone" };

	private string[] _attractAnims = new string[6] { "bot_running_man_celeb", "bot_dance_celeb", "bot_bodybuilding poses_idle", "bot_james_brown", "bot_karate_kid", "bot_backflip" };

	private string[] _canPlayWhileAimingAnims = new string[15]
	{
		"bot_idle", "bot_run", "bot_run_bk", "bot_jump", "bot_shoot", "bot_blink", "bot_chainsaw", "bot_hack", "bot_hack_fast", "bot_stab",
		"bot_throw", "bot_drop_object", "bot_death", "bot_drowning", "bot_uppercut"
	};

	private bool IsShowingProjectile;

	private string[] _shootingAnims = new string[2] { "bot_shoot", "bot_throw" };

	private string[] _stunnedAnims = new string[1] { "bot_stunned" };

	private string[] _moveAnims = new string[4] { "bot_run", "bot_run_bk", "bot_jump", "bot_carry_object" };

	private UnityEngine.Transform activeProjectile;

	private float projectileRadius;

	private bool _bInteraction;

	private Misc.Timer _idleTimer;

	private string[] _emitterBones = new string[8] { "body", "shoulder_R", "arm_R", "head", "upper_leg_R", "lower_leg_R", "upper_leg_L", "lower_leg_L" };

	private bool isUpdatingHealthStateParticles;

	private JetpackMode _jetpackMode;

	private bool _boostAimAssistActive;

	private float _boostSpeedFactor = 1f;

	private StatusEffects _statusEffects;

	private bool _falling;

	private float _zenith;

	private bool _bFiringInProgress;

	private AudioEmitter _audioEmitter;

	private static AnimEventInfo[] _animEvents = new AnimEventInfo[16]
	{
		new AnimEventInfo
		{
			animName = "bot_hack",
			fnName = "OnAnimEvent",
			time = 37f,
			parm = "OnPlayerAnimEvent:bot_hack_hit"
		},
		new AnimEventInfo
		{
			animName = "bot_hack",
			fnName = "OnAnimEvent",
			time = 20f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_open"
		},
		new AnimEventInfo
		{
			animName = "bot_hack",
			fnName = "OnAnimEvent",
			time = 60f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_close"
		},
		new AnimEventInfo
		{
			animName = "bot_hack",
			fnName = "OnAnimEvent",
			time = 28f,
			parm = "OnPlayerAnimEvent:bot_hack_start"
		},
		new AnimEventInfo
		{
			animName = "bot_hack_fast",
			fnName = "OnAnimEvent",
			time = 34f,
			parm = "OnPlayerAnimEvent:bot_hack_hit"
		},
		new AnimEventInfo
		{
			animName = "bot_hack_fast",
			fnName = "OnAnimEvent",
			time = 20f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_open"
		},
		new AnimEventInfo
		{
			animName = "bot_hack_fast",
			fnName = "OnAnimEvent",
			time = 60f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_close"
		},
		new AnimEventInfo
		{
			animName = "bot_stab",
			fnName = "OnAnimEvent",
			time = 10f,
			parm = "OnPlayerAnimEvent:bot_hack_hit"
		},
		new AnimEventInfo
		{
			animName = "bot_stab",
			fnName = "OnAnimEvent",
			time = 5f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_open"
		},
		new AnimEventInfo
		{
			animName = "bot_stab",
			fnName = "OnAnimEvent",
			time = 20f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_close"
		},
		new AnimEventInfo
		{
			animName = "bot_uppercut",
			fnName = "OnAnimEvent",
			time = 10f,
			parm = "OnPlayerAnimEvent:bot_hack_hit"
		},
		new AnimEventInfo
		{
			animName = "bot_uppercut",
			fnName = "OnAnimEvent",
			time = 5f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_open"
		},
		new AnimEventInfo
		{
			animName = "bot_uppercut",
			fnName = "OnAnimEvent",
			time = 20f,
			parm = "OnPlayerAnimEvent:bot_hack_hit_window_close"
		},
		new AnimEventInfo
		{
			animName = "bot_uppercut",
			fnName = "OnAnimEvent",
			time = 10f,
			parm = "OnPlayerAnimEvent:bot_fire_impulse"
		},
		new AnimEventInfo
		{
			animName = "bot_drop_object",
			fnName = "OnAnimEvent",
			time = 13f,
			parm = "OnPlayerAnimEvent:bot_drop_object"
		},
		new AnimEventInfo
		{
			animName = "bot_drowning",
			fnName = "OnAnimEvent",
			time = 20f,
			parm = "OnPlayerAnimEvent:bot_begin_sink"
		}
	};

	private string[] _audioAnims = new string[7] { "bot_hit_reaction", "bot_hit_reaction02", "bot_knockback_reaction", "bot_stab", "bot_bored_polish_head_idle", "bot_jump", "bot_drowning" };

	private Dictionary<string, AudioEvent> _animAudio = new Dictionary<string, AudioEvent>();

	public AudioEvent[] _animationAudioEvents;

	private Animation _ourAnimationComponent;

	private bool _selectingWeapon;

	private float _animSpeed;

	private bool _cameraShake;

	public Fixture QueryFixture { get; private set; }

	private bool AllowIdle
	{
		get
		{
			return !_bFiringInProgress && !_dying && !SelectingWeapon;
		}
	}

	public float HalfHeight
	{
		get
		{
			return _height * 0.5f;
		}
	}

	public bool HasKey
	{
		get
		{
			return _hasKey;
		}
	}

	public bool IsDying
	{
		get
		{
			return _dying;
		}
	}

	public float Height
	{
		get
		{
			return _height;
		}
	}

	public bool IsInputEnabled
	{
		get
		{
			return _inputEnabled;
		}
	}

	public float Modifier_Speed { get; set; }

	public float Modifier_Gravity
	{
		set
		{
			_body.GravityScale = value;
		}
	}

	public Move CurrentMove
	{
		get
		{
			return _move2;
		}
	}

	private bool isExtraShot
	{
		get
		{
			if (extraShots > 0)
			{
				extraShots--;
				return true;
			}
			return false;
		}
	}

	public float HealthPercent
	{
		get
		{
			return _health.FractionalHealth * 100f;
		}
	}

	public string PlayerName { get; private set; }

	public uint PlayerId
	{
		get
		{
			return _view.instantiatorId;
		}
	}

	public Body MyBody
	{
		get
		{
			return _body;
		}
	}

	public Fixture MyFixture
	{
		get
		{
			return _body.FixtureList[0];
		}
	}

	public bool RightFacing { get; set; }

	private bool Jumping
	{
		get
		{
			return _jumping;
		}
		set
		{
			_jumping = value;
			if (_jumping)
			{
				Friction = 0.002f;
			}
		}
	}

	private float Friction
	{
		get
		{
			return _body.Friction;
		}
		set
		{
			if (value != _body.Friction)
			{
				_body.Friction = value;
				ResetContactFriction();
			}
		}
	}

	private string CurrentIdle
	{
		get
		{
			return _currentIdle;
		}
		set
		{
			_currentIdle = value;
			PlayAnim(_currentIdle, PlayMode.StopSameLayer, false);
		}
	}

	public float RealFirePower
	{
		get
		{
			return _move2._power;
		}
	}

	private Body Pushee { get; set; }

	private bool Aiming
	{
		get
		{
			return _aiming;
		}
		set
		{
			if (value && !_aiming)
			{
				if (CurrentWeapon != null && weaponBone != null)
				{
					CurrentWeapon.OnBeginAim(weaponBone.transform);
				}
				if (!_bFiringInProgress && !IsPlayingShootingAnim())
				{
					_smCharacter.Stop();
				}
			}
			else if (!value && _aiming)
			{
				CurrentWeapon.OnEndAim();
			}
			_aiming = value;
		}
	}

	public bool Grounded
	{
		get
		{
			return _grounded;
		}
		set
		{
			if (value && !_grounded)
			{
				_audioEmitter.Play(_eventLand);
				if (_cameraShake)
				{
					Camera.mainCamera.SendMessage("Shake", new CameraShake.ShakeInfo(1f, 3));
				}
			}
			_grounded = value;
		}
	}

	public bool Flying
	{
		get
		{
			return _flying;
		}
		set
		{
			if (!value || !_flying)
			{
			}
			_flying = value;
		}
	}

	public bool ShieldActive
	{
		get
		{
			return _shieldActive;
		}
	}

	public Weapon CurrentWeapon
	{
		get
		{
			return Arsenal.WeaponAt(_currentWeaponIndex);
		}
	}

	public bool CanFire { get; private set; }

	public Vector3 AimingDir
	{
		get
		{
			return _move2._aimDir;
		}
	}

	public static bool DebugOn { get; set; }

	public float JumpHeight
	{
		get
		{
			return Tuning.Instance.Player_JumpHeight * 0.1f;
		}
	}

	private float JumpSpeed
	{
		get
		{
			return _baseJumpSpeed * (1f + PartManager.Instance.GetTotalAttributeValue(_myRecipe, RobotAttribute.RobotAttributeType.Jump) * 0.01f) + _baseJumpSpeed * AttributesManager.instance._currentJumpHeight;
		}
	}

	private float FlySpeed
	{
		get
		{
			return Tuning.Instance.Player_FlySpeed;
		}
	}

	private float RunSpeed
	{
		get
		{
			return Tuning.Instance.Player_RunSpeed * (1f + PartManager.Instance.GetTotalAttributeValue(_myRecipe, RobotAttribute.RobotAttributeType.Speed) * 0.01f) + Tuning.Instance.Player_RunSpeed * AttributesManager.instance._currentSpeed;
		}
	}

	private bool Spawning { get; set; }

	private bool Falling
	{
		get
		{
			return _falling;
		}
		set
		{
			if (value != _falling)
			{
				_falling = value;
				if (_falling)
				{
					_zenith = base.transform.position.y;
				}
			}
		}
	}

	private static bool AnimEventsInitialized { get; set; }

	public bool IsAI { get; set; }

	private int ChangeWeaponTo { get; set; }

	public bool SelectingWeapon
	{
		get
		{
			return _selectingWeapon;
		}
		set
		{
			_selectingWeapon = value;
		}
	}

	private bool UseBossAnims { get; set; }

	private float ModifySpeed(float speed)
	{
		return speed * Modifier_Speed * _statusEffects.SpeedModifier * _boostSpeedFactor;
	}

	public void AddStatusEffect(StatusEffects.Type eType, PlayerEffects.Type eVisualType, int amount, uint attackerID, int applicationInterval, float durationTurns)
	{
		_statusEffects.Add(eType, eVisualType, amount, attackerID, applicationInterval, durationTurns);
		AddVisualEffect(eVisualType);
	}

	public void AddVisualEffect(PlayerEffects.Type eVisualType)
	{
		_playerEffects.AddEffect(eVisualType);
		switch (eVisualType)
		{
		case PlayerEffects.Type.kFire:
			PlayAnim("bot_on_fire", PlayMode.StopSameLayer, false);
			break;
		case PlayerEffects.Type.kPoison:
			PlayAnim("bot_puking", PlayMode.StopSameLayer, false);
			break;
		}
	}

	[RPC]
	public void RemoveVisualEffectRPC(string type)
	{
		_playerEffects.RemoveEffect((PlayerEffects.Type)(int)Enum.Parse(typeof(PlayerEffects.Type), type));
	}

	public void RemoveVisualEffect(PlayerEffects.Type eVisualType)
	{
		_playerEffects.RemoveEffect(eVisualType);
		switch (eVisualType)
		{
		case PlayerEffects.Type.kFire:
			StopAnim("bot_on_fire");
			break;
		case PlayerEffects.Type.kPoison:
			StopAnim("bot_puking");
			break;
		}
	}

	private void Awake()
	{
		_statusEffects = new StatusEffects(this);
		respawnInstance = null;
	}

	private void Start()
	{
		LevelData.Instance.RegisterPlayer(this);
		_view = GetComponent<ReplicationView>();
		_view.migrationSynchronization = NetworkMigrationSynchronization.Destroy;
		CharacterDB.Recipe recipe = CharacterManager.Instance.LocalPlayer.recipe;
		Game game = Hub.Instance.GameManager.Game;
		PlayerName = string.Empty;
		Hashtable hashtable = null;
		if (game != null)
		{
			uint p = _view.viewId.p;
			EB.Sparx.Player player = game.GetPlayer(p);
			if (player != null)
			{
				recipe = CharacterManager.Instance.DeserializeRecipe(player.Attributes);
				hashtable = player.Attributes;
				PlayerName = player.Attributes["name"] as string;
				if (player.Attributes["aiType"] != null)
				{
					int num = int.Parse(player.Attributes["aiType"].ToString());
					if (num == 1)
					{
						GetComponent<AISimple>().IsBoss = true;
					}
					UseBossAnims = (bool)player.Attributes["bossAnims"];
				}
			}
			else
			{
				EB.Debug.LogError("player " + p + " is null!");
			}
		}
		_idleTimer = new Misc.Timer(DoIdle);
		_idleTimer.Start(UnityEngine.Random.Range(Tuning.Instance.Player_SpecialIdleWaitMin, Tuning.Instance.Player_SpecialIdleWaitMax));
		base.name = "Player_" + _view.instantiatorId;
		float num2 = Dot.Single("sizem", hashtable, 1f);
		_animSpeed = Dot.Single("animsp", hashtable, 1f);
		_cameraShake = Dot.Bool("camsh", hashtable, false);
		_height = Tuning.Instance.Player_Height * 0.1f * num2;
		_radius = _height * Tuning.Instance.Player_CollisionWidthFrac * 0.5f * Tuning.Instance.Player_CollisionHeightFrac;
		Vertices vertices;
		Vertices vertices2;
		if (UseBossAnims)
		{
			float num3 = _height * Tuning.Instance.Player_CollisionHeightFrac * 2.5f;
			float y = num3 * 0.6f;
			float num4 = _height * 2.5f;
			float y2 = num4 * 0.7f;
			vertices = PolygonTools.CreateCapsule(num3, _radius * 1.5f, 10);
			vertices2 = PolygonTools.CreateCapsule(num4, _radius * 2f, 15);
			vertices.Translate(new FVector2(0f, y));
			vertices2.Translate(new FVector2(0f, y2));
		}
		else
		{
			float num5 = _height * Tuning.Instance.Player_CollisionHeightFrac;
			float y3 = num5 * 0.5f;
			float height = _height;
			float y4 = height * 0.5f;
			vertices = PolygonTools.CreateCircle(_radius * 1.5f, 15);
			vertices2 = PolygonTools.CreateCircle(_radius * 2.4f, 15);
			vertices.Translate(new FVector2(0f, y3));
			vertices2.Translate(new FVector2(0f, y4));
		}
		Shape shape = new PolygonShape(vertices, 1f);
		Shape shape2 = new PolygonShape(vertices2, 1f);
		_body = new Body(FSWorldComponent.PhysicsWorld);
		_body.BodyType = BodyType.Dynamic;
		Fixture fixture = _body.CreateFixture(shape);
		QueryFixture = _body.CreateFixture(shape2);
		_body.Position = new FVector2(base.transform.position.x, base.transform.position.y);
		_body.FixedRotation = true;
		_body.SleepingAllowed = false;
		_body.LinearDamping = 0f;
		_body.UserData = base.gameObject;
		_body.Mass *= num2;
		fixture.CollisionCategories = Category.None;
		fixture.CollidesWith = Category.Cat2 | Category.Cat9;
		QueryFixture.CollisionCategories = Category.Cat1;
		QueryFixture.CollidesWith = Category.Cat1;
		_body.UserTag = "Player";
		fixture.UserTag = "NoHit";
		QueryFixture.UserTag = "Player";
		QueryFixture.OnCollision = OnPlayerCollision;
		QueryFixture.OnSeparation = OnPlayerSeparation;
		Friction = 1f;
		fixture.Restitution = 0f;
		_dying = false;
		_baseJumpSpeed = Misc.JumpSpeedRequiredToAttainHeight(Tuning.Instance.Player_JumpHeight * 0.1f, 0f - FSWorldComponent.PhysicsWorld.Gravity.Y);
		Vector3 position = base.transform.position;
		position.z = 0f;
		base.transform.position = position;
		RightFacing = true;
		_lastAttackerID = uint.MaxValue;
		_health = GetComponent<Health>();
		_health.Initialize(recipe, hashtable);
		_playerEffects = GetComponent<PlayerEffects>();
		_damageMultiplier = Dot.Single("damageMultiplier", hashtable, 1f);
		if (game != null)
		{
			GameObject original = Resources.Load("UI/healthbar") as GameObject;
			GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
			GameObject gameObject2 = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
			gameObject.transform.parent = gameObject2.transform;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			_healthBar = gameObject.GetComponent<Healthbar>();
			_healthBar.RegisterPlayerObject(base.gameObject, PlayerName, Scoring.Instance.GetTeamId(_view.instantiatorId));
		}
		Modifier_Speed = 1f;
		Modifier_Gravity = 1f;
		IsShowingProjectile = true;
		_hasKey = false;
		CharacterDB.Instantiate(recipe, true, SafeAction.Wrap(this, (GameObject gameObject3) =>
		{
			if (gameObject3 != null)
			{
				gameObject3.transform.parent = base.transform;
				gameObject3.transform.localPosition = Vector3.zero;
				gameObject3.transform.localRotation = Quaternion.identity;
				gameObject3.transform.localScale = Vector3.one;
				gameObject3.SetActive(true);
				_smCharacter = gameObject3.GetComponent<BoneAnimation>();
				_smCharacter.HideBone("weapon", false);
				float num6 = _height / 300f;
				base.transform.localScale = new Vector3(num6, num6, 1f);
				weaponBone = Util.GetObjectExactMatch(base.gameObject, "gun_flash");
				_headBone = Util.GetObjectExactMatch(base.gameObject, "head");
				PlayerEffects component = GetComponent<PlayerEffects>();
				if (component != null)
				{
					component.AttachEffects(this);
				}
				if (Hub.Instance.InventoryManager.IsOverriden && !IsAI)
				{
					SelectFirstOverrideWeapon();
				}
				else if (PlayerId == EB.Replication.Manager.LocalPlayerId)
				{
					StartingWeapon = FindFallbackWeaponIndex();
					_view.RPC("SelectStartingWeaponRPC", RPCMode.Others, StartingWeapon);
					SelectWeaponRPC(StartingWeapon);
					if (GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) == GameAttributes.ModeType.MP)
					{
						Profile.Instance.OnGamePlayed();
					}
				}
				else if (IsAI)
				{
					SelectWeaponRPC(FindFallbackWeaponIndex());
				}
				Util.SetLayerRecursive(gameObject3, base.gameObject.layer);
			}
			InitAnimEvents();
			BuildAnimAudio();
			ShowProjectile(true);
			CurrentIdle = string.Empty;
			_ourAnimationComponent = GetComponentInChildren<Animation>();
			if (_animSpeed != 1f)
			{
				foreach (AnimationState item in _ourAnimationComponent)
				{
					item.speed *= _animSpeed;
				}
			}
			StartCoroutine(UpdatePlayerCac());
		}));
		CapsuleCollider capsuleCollider = base.gameObject.AddComponent<CapsuleCollider>();
		Vector3 zero = Vector3.zero;
		capsuleCollider.height = _height / base.transform.localScale.y;
		capsuleCollider.radius = _radius / base.transform.localScale.x;
		zero.y += capsuleCollider.height * 0.5f;
		capsuleCollider.center = zero;
		_started = true;
		Spawning = true;
		_audioEmitter = GetComponent<AudioEmitter>();
		if (_audioEmitter == null)
		{
			_audioEmitter = base.gameObject.AddComponent<AudioEmitter>();
		}
		_audioEmitter.m_PlayMode = AudioEmitter.PlayMode.Stack;
		IsAI = GetComponent<AISimple>() != null;
		ChangeWeaponTo = -1;
		_myRecipe = recipe;
		RegisterPointLabel();
	}

	private IEnumerator UpdatePlayerCac()
	{
		if (base.transform.childCount > 0)
		{
			base.transform.GetChild(0).gameObject.SetActive(false);
		}
		yield return new WaitForEndOfFrame();
		if (base.transform.childCount > 0)
		{
			base.transform.GetChild(0).gameObject.SetActive(true);
		}
	}

	public void ReleasePlayer()
	{
		if (_smCharacter != null)
		{
			UnityEngine.Object.Destroy(_smCharacter.gameObject);
			_smCharacter = null;
		}
		CleanUp();
	}

	private void CleanUp()
	{
		if (_healthBar != null)
		{
			UnityEngine.Object.Destroy(_healthBar.gameObject);
			_healthBar = null;
		}
		FSWorldComponent.PhysicsWorld.RemoveBody(MyBody);
		UnityEngine.Object.Destroy(criticalDamageParticles);
		UnityEngine.Object.Destroy(severeDamageParticles);
	}

	private void OnDestroy()
	{
		LevelData.Instance.UnregisterPlayer(this);
		CleanUp();
	}

	public void EnableInput(bool enable)
	{
		_inputEnabled = enable;
		if (enable)
		{
			_lastAttackerID = uint.MaxValue;
			CanFire = true;
			if (!CanUseWeapon(_currentWeaponIndex))
			{
				if (Hub.Instance.InventoryManager.IsOverriden && !IsAI)
				{
					SelectFirstOverrideWeapon();
				}
				else
				{
					SelectWeapon(Arsenal.EmptyWeaponIndex);
				}
			}
		}
		else
		{
			CanFire = false;
			if (!_bFiringInProgress)
			{
				_move2.Reset();
			}
			else
			{
				StartCoroutine(waitToReset());
			}
			ResetLinerVelocity();
		}
	}

	private IEnumerator waitToReset()
	{
		while (_bFiringInProgress)
		{
			yield return new WaitForEndOfFrame();
		}
		_move2.Reset();
	}

	private void ResetLinerVelocity()
	{
		MyBody.LinearVelocity = new FVector2(0f, 0f);
	}

	public void SpawnProjectile()
	{
		if (weaponBone != null)
		{
			CurrentWeapon.CreateProjectileProxy(weaponBone.transform);
		}
		else
		{
			EB.Debug.Log("Trying to spawn a projectile before character loaded - will spawn it later after the character is loaded");
		}
	}

	public void ResetIdle()
	{
		StopSpecialIdleAnims();
		_idleTimer.Reset();
	}

	public int GetFreeBoostSlot()
	{
		for (int i = 0; i < _activeBoosts.Length; i++)
		{
			if (!_activeBoosts[i].Valid)
			{
				return i;
			}
		}
		return -1;
	}

	public bool HasAnActiveBoost()
	{
		for (int i = 0; i < _activeBoosts.Length; i++)
		{
			if (_activeBoosts[i].Valid)
			{
				return true;
			}
		}
		return false;
	}

	public float GetActiveBoostRemainingDurationPercentage(int slot)
	{
		return _activeBoosts[slot].RemainingDurationPercentage();
	}

	public ActiveBoost GetActiveBoost(int slot)
	{
		return _activeBoosts[slot];
	}

	public BoostItem GetBoost(int slot)
	{
		return _activeBoosts[slot].Boost;
	}

	public bool CanUseBoost(string boostName, ref string errorReason)
	{
		int count = Hub.Instance.InventoryManager.GetCount(boostName);
		if (count <= 0)
		{
			return false;
		}
		if (GetFreeBoostSlot() == -1)
		{
			errorReason = Localizer.GetString("ID_UI_BOOSTS_ALL_BOOST_ALL_SLOTS_IN_USE");
			return false;
		}
		BoostItem boostItem = Arsenal.InventoryItemWithName(InventoryItem.Category.Boost, boostName) as BoostItem;
		for (int i = 0; i < _activeBoosts.Length; i++)
		{
			if (_activeBoosts[i].Valid)
			{
				if (_activeBoosts[i].Boost.name == boostItem.name)
				{
					errorReason = Localizer.GetString("ID_UI_BOOSTS_BOOST_IN_USE");
					return false;
				}
				if (_activeBoosts[i].Boost._type == boostItem._type)
				{
					errorReason = Localizer.Format("ID_UI_BOOSTS_CATEGORY_SLOT_IN_USE", Localizer.GetString("ID_UI_BOOSTS_CATEGORY_" + boostItem._type.ToString().ToUpper()), _activeBoosts[i].Boost.FriendlyName, boostItem.FriendlyName);
					return false;
				}
			}
		}
		InactiveBoost[] array = inactiveBoosts;
		for (int j = 0; j < array.Length; j++)
		{
			InactiveBoost inactiveBoost = array[j];
			if (inactiveBoost.cooldownTime > 0f && inactiveBoost.boostType == boostItem._type)
			{
				errorReason = Localizer.GetString("ID_UI_BOOSTS_BOOST_IN_USE");
				return false;
			}
		}
		return true;
	}

	public void SetBoost(int slot, string name)
	{
		BoostItem boostItem = Arsenal.InventoryItemWithName(InventoryItem.Category.Boost, name) as BoostItem;
		_view.RPC("SetBoostRPC", RPCMode.AllBuffered, name, slot);
		UnityEngine.Transform boneTransform = _smCharacter.GetBoneTransform(boostItem._effectBone);
		if (boostItem._effectPrefab != null)
		{
			_view.RPC("SetBoostEffect", RPCMode.All, slot, PlayerId, name);
		}
		switch (boostItem._type)
		{
		case BoostItem.Type.kHeal:
			_view.RPC("ApplyHealth", RPCMode.AllBuffered, boostItem._value);
			break;
		case BoostItem.Type.kJetpack:
			if (boostItem._subType == BoostItem.SubType.kJetpackHover)
			{
				SetJetpackMode(JetpackMode.kHoverPack);
			}
			else
			{
				SetJetpackMode(JetpackMode.kJetPack);
			}
			break;
		case BoostItem.Type.kShield:
			_view.RPC("ApplyShieldRPC", RPCMode.All, boostItem._value);
			break;
		case BoostItem.Type.kSpeed:
			_boostSpeedFactor = 1f + boostItem._value;
			break;
		case BoostItem.Type.kAim:
			_boostAimAssistActive = true;
			break;
		case BoostItem.Type.kMultipleShots:
			extraShots = (int)boostItem._value;
			break;
		}
		for (int i = 0; i < inactiveBoosts.Length; i++)
		{
			if (inactiveBoosts[i].cooldownTime <= 0f)
			{
				inactiveBoosts[i].cooldownTime = _activeBoosts[slot]._remainingTime;
				inactiveBoosts[i].boostType = _activeBoosts[slot].Boost._type;
				GameHUD.Instance.GetComponent<HUDSubscreenManager>().storeBoostInGameUi.UpdateItems();
				break;
			}
		}
		_view.RPC("UpdateHUDBoostPortraitIconsRPC", RPCMode.AllBuffered, slot, (int)boostItem._type);
	}

	[RPC]
	private void SetBoostRPC(string BoostName, int slot)
	{
		BoostItem boostItem = (BoostItem)Assets.Load("inventory/Boosts/" + BoostName, typeof(BoostItem));
		_activeBoosts[slot].Set(boostItem);
	}

	[RPC]
	public void SetBoostEffect(int slot, uint instantitor, string BoostName)
	{
		BoostItem boostItem = (BoostItem)Assets.Load("inventory/Boosts/" + BoostName, typeof(BoostItem));
		Player player = LevelData.Instance.GetPlayer(instantitor);
		UnityEngine.Transform boneTransform = player._smCharacter.GetBoneTransform(boostItem._effectBone);
		player.PlayAnim("bot_bodybuilding poses_idle");
		UnityEngine.Debug.Log("seting boost Effect at bone : " + player.gameObject.name);
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(boostItem._effectPrefab, Vector3.zero, Quaternion.identity);
		gameObject.transform.parent = boneTransform;
		gameObject.transform.localPosition = Vector3.zero + boostItem._effectOffset;
		player._activeBoosts[slot].currentEffect = gameObject;
	}

	[RPC]
	private void StopAndDestroyBoostEffect(int slot, uint instantitor)
	{
		Player player = LevelData.Instance.GetPlayer(instantitor);
		if (player != null && player._activeBoosts[slot].currentEffect != null)
		{
			UnityEngine.Object.Destroy(player._activeBoosts[slot].currentEffect);
		}
	}

	[RPC]
	private void UpdateHUDBoostPortraitIconsRPC(int slot, int type)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["slot"] = slot;
		hashtable["playerId"] = PlayerId;
		hashtable["type"] = (BoostItem.Type)type;
		ScreenManager.Instance.BroadcastMessage("OnActiveBoostsUpdated", hashtable, SendMessageOptions.DontRequireReceiver);
	}

	[RPC]
	private void ApplyShieldRPC(float shieldHP)
	{
		_shield = base.gameObject.AddComponent<Shield>();
		_shield.Initialize(shieldHP);
		_shield.RegisterHealthbar(_healthBar, _healthBar.GetShieldProgressBar());
		_shield.Activate();
	}

	private void startBoostEffect(Vector2 offset, ParticleSystem ps, string effectBone)
	{
	}

	public void ProcessOutgoingHit(ref Misc.HitInfo hitInfo)
	{
		float num = PartManager.Instance.GetTotalAttributeValue(_myRecipe, RobotAttribute.RobotAttributeType.Damage);
		ActiveBoost[] activeBoosts = _activeBoosts;
		for (int i = 0; i < activeBoosts.Length; i++)
		{
			ActiveBoost activeBoost = activeBoosts[i];
			if (activeBoost.IsActive())
			{
				switch (activeBoost.Boost._type)
				{
				case BoostItem.Type.kDamage:
					num += activeBoost.Boost._value;
					break;
				case BoostItem.Type.kBonusXp:
					hitInfo._XpBoost = (ushort)activeBoost.Boost._value;
					break;
				}
			}
		}
		hitInfo._damage += hitInfo._damage * 0.01f * num + hitInfo._damage * AttributesManager.instance._currentAttack;
		hitInfo._damage *= _damageMultiplier;
	}

	public void EndTurn(uint currentPlayerId, uint nextPlayerId)
	{
		if (nextPlayerId == EB.Replication.Manager.LocalPlayerId && !_dying)
		{
			PlayAnim(Misc.RandomString(_attractAnims), PlayMode.StopSameLayer, true);
		}
	}

	private void OnRespawn(Vector2 pos)
	{
		Vector3 position = pos;
		position.z = 0f;
		base.transform.position = position;
		_body.Position = FSHelper.Vector2ToFVector2(pos);
		_body.LinearVelocity = FVector2.Zero;
		_dying = false;
		_health.OnRespawn();
		DoRespawnFX();
		PlayAnim("bot_idle", PlayMode.StopAll, true);
		StartCoroutine(EnableBody());
		if (_revengeSaleInfo != null)
		{
			GameHUD.Instance.ShowRevengeSalePopup(_revengeSaleInfo.weaponName, _revengeSaleInfo.attackerName);
			_revengeSaleInfo = null;
		}
	}

	private void DoRespawnFX()
	{
		if (respawnFX != null)
		{
			respawnInstance = UnityEngine.Object.Instantiate(respawnFX) as GameObject;
			respawnInstance.transform.position = base.transform.position + Vector3.back * 10f;
			StartCoroutine(EnableBody());
		}
	}

	private IEnumerator EnableBody()
	{
		yield return new WaitForSeconds(1f);
		_body.Enabled = true;
		if (respawnInstance != null)
		{
			UnityEngine.Object.Destroy(respawnInstance);
			respawnInstance = null;
		}
	}

	private IEnumerator UpdateDeathMessage()
	{
		yield return new WaitForSeconds(Tuning.Instance.DeathMessage_Duration);
		GameObject hudObj = Setup.Instance.GetComponent<HUD>().HudObject;
		if (hudObj != null)
		{
			GameHUD hud = hudObj.GetComponent<GameHUD>();
			if (hud != null)
			{
				hud.ClearMessage2();
			}
		}
	}

	private void ResetContactFriction()
	{
		for (ContactEdge contactEdge = _body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
		{
			contactEdge.Contact.ResetFriction();
		}
	}

	private bool PlayingInterruptible()
	{
		if (_smCharacter != null)
		{
			string[] uninterruptibleAnims = _uninterruptibleAnims;
			foreach (string animationName in uninterruptibleAnims)
			{
				if (_smCharacter.IsPlaying(animationName))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void PlayAnimWithAudio(string animName, PlayMode ePlayMode, AudioEvent audioEvent, bool force)
	{
		if (_smCharacter == null)
		{
			return;
		}
		if (UseBossAnims)
		{
			animName += "_boss";
		}
		if ((PlayingInterruptible() && !force) || (Aiming && !IsCanPlayWhileAiming(animName)))
		{
			return;
		}
		if (_smCharacter.IsPlaying(animName) && force)
		{
			StopAnim(animName);
		}
		if (!_smCharacter.IsPlaying(animName) || force)
		{
			if (audioEvent != null && (bool)audioEvent)
			{
				_audioEmitter.Play(audioEvent);
			}
			_smCharacter.Play(animName, ePlayMode);
		}
	}

	public void PlayAnim(string animName)
	{
		PlayAnim(animName, PlayMode.StopSameLayer, false);
	}

	public void PlayAnim(string animName, PlayMode ePlayMode, bool force)
	{
		if (_smCharacter == null)
		{
			return;
		}
		if (UseBossAnims)
		{
			animName += "_boss";
		}
		if (PlayingInterruptible() && !force)
		{
			return;
		}
		Animation animation = _smCharacter.animation;
		if (animation == null || (Aiming && !IsCanPlayWhileAiming(animName)))
		{
			return;
		}
		AnimationClip clip = animation.GetClip(animName);
		if (!(clip == null) && (!_smCharacter.IsPlaying(animName) || force))
		{
			AudioEvent value;
			if (_animAudio.TryGetValue(animName, out value) && _audioEmitter != null)
			{
				_audioEmitter.Play(value);
			}
			if (ePlayMode == PlayMode.StopAll)
			{
				_currentIdle = string.Empty;
			}
			_smCharacter.Play(animName, ePlayMode);
		}
	}

	public void PlayEndGameCelebAnim()
	{
		_audioEmitter.Play(_eventCelebrate);
		PlayAnim(Tuning.Instance.Outro_CelebAnimName, PlayMode.StopAll, true);
	}

	private void PreDie(string DeathText)
	{
		if (_view.isMine)
		{
			StatManager.Instance.IncrementStat(StatManager.Stat.Deaths);
		}
		GameObject hudObject = Setup.Instance.GetComponent<HUD>().HudObject;
		if (hudObject != null && !string.IsNullOrEmpty(PlayerName))
		{
			GameHUD component = hudObject.GetComponent<GameHUD>();
			component.ShowMessage2(DeathText);
		}
		StartCoroutine(UpdateDeathMessage());
		_body.Enabled = false;
		string text = tutorialId;
		if (string.IsNullOrEmpty(text))
		{
			text = "player";
		}
		EB.Sequence.Component.Activate(base.gameObject, text, typeof(SequenceEvent_PlayerDiedTutorial));
	}

	private void Die(string DeathText, bool playDeathAnim)
	{
		PreDie(DeathText);
		_dying = true;
		if (playDeathAnim)
		{
			StartCoroutine(DoDying());
		}
		else
		{
			PostDie();
		}
	}

	private void PostDie()
	{
		if (!LevelData.Instance.enableRespawn)
		{
			Vector3 vector = base.transform.position + Vector3.back * 1000f;
			if (vector.y < Tuning.Instance.Player_KillFloor)
			{
				vector.y = Tuning.Instance.Player_KillFloor + 1f;
			}
			base.transform.position = vector;
			_body.Position = FSHelper.Vector3ToFVector2(vector);
		}
		Vector3 vector2 = LevelData.Instance.DetermineRespawn(this);
		if (_view.isMine)
		{
			EB.Replication.Manager.RPC("OnPlayerDied", RPCMode.AllBuffered, _view.ownerId, vector2);
		}
		if (PlayerId == EB.Replication.Manager.LocalPlayerId)
		{
			for (int i = 0; i < _activeBoosts.Length; i++)
			{
				if (_activeBoosts[i].Valid && _activeBoosts[i].GetState() == ActiveBoost.State.kActive)
				{
					CooldownBoost(i);
				}
			}
		}
		Spawning = true;
		Falling = false;
		_dying = false;
	}

	private IEnumerator DoDying()
	{
		PlayAnim("bot_death", PlayMode.StopAll, true);
		yield return new WaitForSeconds(Tuning.Instance.Player_RespawnDelay);
		PostDie();
	}

	private void CooldownBoost(int slot)
	{
		if (slot < 0 || slot >= _activeBoosts.Length || !_activeBoosts[slot].Valid)
		{
			return;
		}
		_activeBoosts[slot].Cooldown();
		_view.RPC("StopAndDestroyBoostEffect", RPCMode.All, slot, PlayerId);
		_view.RPC("UpdateHUDBoostPortraitIconsRPC", RPCMode.AllBuffered, slot, 8);
		switch (_activeBoosts[slot].Boost._type)
		{
		case BoostItem.Type.kJetpack:
			SetJetpackMode(JetpackMode.kNone);
			break;
		case BoostItem.Type.kShield:
			_view.RPC("DeactivateShieldRPC", RPCMode.AllBuffered);
			break;
		case BoostItem.Type.kSpeed:
			_boostSpeedFactor = 1f;
			break;
		case BoostItem.Type.kAim:
			_boostAimAssistActive = false;
			break;
		case BoostItem.Type.kMultipleShots:
			extraShots = 0;
			break;
		}
		for (int i = 0; i < inactiveBoosts.Length; i++)
		{
			if (inactiveBoosts[i].cooldownTime <= 0f || _activeBoosts[slot].Boost._type == inactiveBoosts[i].boostType)
			{
				inactiveBoosts[i].cooldownTime = _activeBoosts[slot]._remainingTime;
				inactiveBoosts[i].boostType = _activeBoosts[slot].Boost._type;
				_activeBoosts[slot].Boost = null;
				break;
			}
		}
		GameHUD.Instance.GetComponent<HUDSubscreenManager>().storeBoostInGameUi.UpdateItems();
		_view.RPC("UpdateHUDBoostPortraitIconsRPC", RPCMode.AllBuffered, slot, 8);
	}

	[RPC]
	private void DeactivateShieldRPC()
	{
		_shield.Deactivate();
	}

	private void UpdateInactiveBoosts()
	{
		if (!(GameHUD.Instance != null))
		{
			return;
		}
		for (int i = 0; i < inactiveBoosts.Length; i++)
		{
			if (inactiveBoosts[i].cooldownTime > 0f)
			{
				inactiveBoosts[i].cooldownTime -= UnityEngine.Time.deltaTime;
				if (inactiveBoosts[i].cooldownTime <= 0f)
				{
					GameHUD.Instance.GetComponent<HUDSubscreenManager>().storeBoostInGameUi.UpdateItems();
				}
			}
		}
	}

	public BoostItem.Type[] inactiveBoostTypes()
	{
		List<BoostItem.Type> list = new List<BoostItem.Type>();
		for (int i = 0; i < inactiveBoosts.Length; i++)
		{
			if (inactiveBoosts[i].cooldownTime > 0f)
			{
				list.Add(inactiveBoosts[i].boostType);
			}
		}
		return list.ToArray();
	}

	private void UpdateAnim()
	{
		if (!_started || _body == null || _smCharacter == null || _dying)
		{
			return;
		}
		if (_scheduledSpawnProjectileAndGameHud)
		{
			LateSpawnProjectileAndGameHudNotification();
		}
		PlayIdle();
		if (!_move2.Idle && _idleTimer != null)
		{
			StopSpecialIdleAnims();
			_idleTimer.Reset();
		}
		if (activeProjectile != null)
		{
			Vector3 vector = activeProjectile.position - base.transform.position;
			vector.z = 0f;
			if (vector.sqrMagnitude < projectileRadius * projectileRadius && !_smCharacter.IsPlaying("bot_hit_reaction"))
			{
				_hitAnticipation = true;
				UnregisterProjectile(activeProjectile);
			}
		}
		if (_hitAnticipation)
		{
			PlayAnim("bot_hit_reaction", PlayMode.StopSameLayer, false);
			_hitAnticipation = false;
		}
		float num = 0f;
		if (Aiming)
		{
			num = Mathf.Atan2(_move2._aimDir.y, _move2._aimDir.x);
			RightFacing = Mathf.Abs(num) < (float)Math.PI / 2f;
			if (!RightFacing)
			{
				num = Mathf.Sign(num) * (float)Math.PI - num;
			}
			if (_move2._up || Jumping || Flying)
			{
				if (IsJetpackActive() || IsHoverpackActive())
				{
					if (!Flying)
					{
						GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "lower_leg_L");
						StartHealthStateParticles(criticalDamageParticlesPrefab, objectExactMatch, ref criticalDamageParticles);
					}
					PlayIdle();
					Flying = true;
				}
				else
				{
					PlayAnim("bot_jump", PlayMode.StopSameLayer, false);
					Jumping = true;
				}
			}
			else if (_move2._left || _move2._right)
			{
				if ((_move2._left && RightFacing) || (_move2._right && !RightFacing))
				{
					PlayAnimWithAudio("bot_run_bk", PlayMode.StopSameLayer, _eventMove, false);
				}
				else
				{
					PlayAnimWithAudio("bot_run", PlayMode.StopSameLayer, _eventMove, false);
				}
			}
			else if (_body.LinearVelocity.Length() < 10.5f)
			{
				StopMoveAnims();
			}
		}
		else
		{
			if (_move2._right)
			{
				RightFacing = true;
			}
			else if (_move2._left)
			{
				RightFacing = false;
			}
			if (_move2._up || Jumping || Flying)
			{
				if (IsJetpackActive() || IsHoverpackActive())
				{
					PlayIdle();
					Flying = true;
				}
				else
				{
					PlayAnim("bot_jump", PlayMode.StopSameLayer, false);
					Jumping = true;
				}
			}
			else if (_move2._left || _move2._right)
			{
				if (CurrentWeapon._isPlaced)
				{
					PlayAnimWithAudio("bot_carry_object", PlayMode.StopSameLayer, _eventMove, false);
				}
				PlayAnimWithAudio("bot_run", PlayMode.StopSameLayer, _eventMove, false);
			}
			else if (_body.LinearVelocity.Length() < 10.5f && Grounded)
			{
				StopMoveAnims();
			}
		}
		if (LevelData.Instance.IsReady && StartingWeapon != -1)
		{
			ChangeWeaponTo = StartingWeapon;
			StartingWeapon = -1;
		}
		if (ChangeWeaponTo != -1 && !_bFiringInProgress && !IsPlayingShootingAnim())
		{
			_view.RPC("SelectWeaponRPC", RPCMode.All, ChangeWeaponTo);
			ChangeWeaponTo = -1;
		}
		UnityEngine.Transform boneTransform = _smCharacter.GetBoneTransform("dummy_bone");
		boneTransform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
	}

	public void PlayIdle()
	{
		bool flag = _ourAnimationComponent.IsPlaying("bot_on_fire");
		bool flag2 = _ourAnimationComponent.IsPlaying("bot_puking");
		bool flag3 = _ourAnimationComponent.IsPlaying("bot_death");
		bool flag4 = _ourAnimationComponent.IsPlaying("bot_drowning");
		if (_dying || flag || flag2 || flag3 || flag4)
		{
			return;
		}
		CurrentIdle = "bot_idle";
		Health component = GetComponent<Health>();
		if (!(component.FractionalHealth <= 0f))
		{
			if (CurrentWeapon._isPlaced && CanFire && CurrentWeapon._currentProxy != null)
			{
				PlayAnim("bot_carry_idle", PlayMode.StopSameLayer, false);
			}
			else if (component.FractionalHealth < Tuning.Instance.Player_PlayStunnedAtPercentHealth / 100f && !_move2._aiming)
			{
				CurrentIdle = "bot_stunned";
			}
			else
			{
				CurrentIdle = "bot_idle";
			}
		}
	}

	public void Move2Ancillary(PlayerInstructions move)
	{
		move.aiming = _move2._aiming;
		move.fireDir = _move2._aimDir;
		Move2(move);
	}

	public void Move2(PlayerInstructions move)
	{
		if (!_inputEnabled)
		{
			return;
		}
		_userMoveDir = move.moveDir;
		if (!_started || _body == null || _dying)
		{
			return;
		}
		_move2._left = move.goLeft;
		_move2._right = move.goRight;
		_move2._up = move.jump;
		_move2._down = move.drop;
		_move2._fire = move.fire;
		if (move.aiming || _bFiringInProgress || IsPlayingShootingAnim())
		{
			_move2._aiming = true;
			if (move.aiming)
			{
				_move2._aimDir = move.fireDir;
				_move2._power = move.fireMag;
			}
		}
		else
		{
			_move2._aiming = false;
		}
		if (_view.isMine && _move2.Moving)
		{
			Camera.mainCamera.SendMessage("PlayerMoved", base.gameObject);
		}
		_move2Repl.Merge(_move2);
		MoveBody(_body, _move2);
		Weapon currentWeapon = CurrentWeapon;
		if (move.fire && CanFire)
		{
			_bFiringInProgress = true;
			Fire(move.fireDir, RealFirePower);
			if (currentWeapon._endsTurn && !isExtraShot)
			{
				CanFire = false;
			}
		}
	}

	private void MoveBody(Body body, Move move)
	{
		FVector2 fVector = new FVector2(0f, 0f);
		bool flag = IsHoverpackActive() || IsJetpackActive();
		float num = ((!flag) ? RunSpeed : FlySpeed);
		if (move._left)
		{
			fVector.X = 0f - num;
		}
		else if (move._right)
		{
			fVector.X = num;
		}
		else if (Grounded || flag)
		{
			fVector.X = 0f - body.LinearVelocity.X;
		}
		float num2 = ((!flag) ? JumpSpeed : FlySpeed);
		if (move._up)
		{
			fVector.Y = num2;
			Modifier_Gravity = ((!flag) ? 1f : 0f);
		}
		else if (!IsHoverpackActive())
		{
			Modifier_Gravity = 1f;
			Flying = false;
		}
		if (move._down && IsHoverpackActive())
		{
			fVector.Y = 0f - num2;
		}
		FVector2 linearVelocity = body.LinearVelocity;
		if (move._up || move._down)
		{
			linearVelocity.Y = 0f;
		}
		linearVelocity += fVector;
		linearVelocity.X = ModifySpeed(Mathf.Clamp(linearVelocity.X, 0f - num, num));
		if (IsHoverpackActive() && !move._up && !move._down)
		{
			linearVelocity.Y = 0f;
		}
		else if (Flying)
		{
			linearVelocity.Y = ModifySpeed(Mathf.Clamp(linearVelocity.Y, 0f - num2, num2));
		}
		_body.LinearVelocity = linearVelocity;
	}

	public void FireAt(Vector3 dir)
	{
		float power = 15f;
		Fire(dir, power);
	}

	private void Fire(Vector3 direction, float power)
	{
		int currentWeaponIndex = _currentWeaponIndex;
		Weapon currentWeapon = CurrentWeapon;
		int num = -1;
		if (!IsAI)
		{
			if (!GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kInfiniteAmmo))
			{
				if (!currentWeapon.IsFree && !InventoryListener.Instance.UseInGame(currentWeapon.name, 1))
				{
					EB.Debug.LogError("Failed to use selected weapon, using default");
					ChangeWeaponTo = Arsenal.EmptyWeaponIndex;
					_bFiringInProgress = false;
					return;
				}
				InventoryManager inventoryManager = Hub.Instance.InventoryManager;
				if (inventoryManager.IsOverriden && !inventoryManager.EverythingFree && currentWeapon.IsFree && !InventoryListener.Instance.UseInGame(currentWeapon.name, 1))
				{
					SelectFirstOverrideWeapon();
					_bFiringInProgress = false;
					return;
				}
			}
			num = WeaponLevel(currentWeapon.name);
		}
		if (currentWeapon._isPlaced)
		{
			PlaceWeapon(currentWeaponIndex, weaponBone.transform.position, direction, power);
		}
		else
		{
			_view.RPC("FireRPC", RPCMode.All, currentWeaponIndex, weaponBone.transform.position, direction, power, num);
		}
		if (!CanUseWeapon(_currentWeaponIndex))
		{
			SelectWeapon(Arsenal.EmptyWeaponIndex);
		}
		GameHUD.Instance.OnWeaponSelected(this);
	}

	private int WeaponLevel(string weaponName)
	{
		if (!Hub.Instance.InventoryManager.IsOverriden)
		{
			InventoryDB.MetaData metaData = InventoryDB.GetMetaData(Hash.StringHash(weaponName));
			if (metaData != null)
			{
				return ItemLevelManager.Instance.GetLevel(metaData.hash);
			}
		}
		return -1;
	}

	[RPC]
	private void FireRPC(int weaponIndex, Vector3 position, Vector3 direction, float power, int weaponLevel)
	{
		Weapon currentWeapon = CurrentWeapon;
		currentWeapon.DestroyProjectileProxy(weaponBone.transform);
		LAUNCHPOS = weaponBone.transform.position;
		if (currentWeapon.Fire(direction, power, this, weaponBone.transform, weaponLevel))
		{
			Camera.mainCamera.SendMessage("PlayerFired");
			if (GameHUD.Instance != null)
			{
				GameHUD.Instance.SendMessage("OnWeaponFired", this);
			}
		}
		if (currentWeapon._endsTurn && !isExtraShot)
		{
			EB.Sequence.Component.Activate(base.gameObject, _view.viewId.p, typeof(SequenceEvent_PlayerFired));
		}
		else
		{
			EB.Sequence.Component.Activate(base.gameObject, _view.viewId.p, typeof(SequenceEvent_PlayerFiredNoEndTurn));
		}
		if (!IsAI && PlayerId == EB.Replication.Manager.LocalPlayerId && !Hub.Instance.InventoryManager.IsOverriden)
		{
			int num = Hash.StringHash(currentWeapon.name);
			if (ItemLevelManager.Instance.IncrementUse(num))
			{
				if (ItemLevelManager.Instance.IsMaxLevel(num))
				{
					_view.RPC("OnWeaponLevelMaxRPC", RPCMode.All, num, PlayerId);
				}
				else
				{
					_view.RPC("OnWeaponLevelUpRPC", RPCMode.All, num, PlayerId, ItemLevelManager.Instance.GetLevel(num));
				}
			}
		}
		if (!IsAI && Hub.Instance.InventoryManager.IsOverriden && Hub.Instance.InventoryManager.IsEmpty())
		{
			GameObject hudObject = Setup.Instance.GetComponent<HUD>().HudObject;
			if (hudObject != null)
			{
				GameHUD component = hudObject.GetComponent<GameHUD>();
				component.ShowMessage2("ID_GAME_MESSAGE_INVENTORY_EMPTY");
			}
		}
	}

	[RPC]
	private void OnWeaponLevelUpRPC(int weaponHash, uint playerId, int newLevel)
	{
		GameObject hudObject = Setup.Instance.GetComponent<HUD>().HudObject;
		if (!(hudObject != null))
		{
			return;
		}
		GameHUD component = hudObject.GetComponent<GameHUD>();
		if (Hub.Instance.GameManager.Game == null)
		{
			return;
		}
		EB.Sparx.Player player = Hub.Instance.GameManager.Game.GetPlayer(playerId);
		if (player != null)
		{
			string text = Dot.String("name", player.Attributes, string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				string friendlyName = InventoryDB.GetMetaData(weaponHash).friendlyName;
				component.ShowMessage(Localizer.Format("ID_GAME_ITEMLEVEL_LEVELLED_UP", text, friendlyName, newLevel + 1));
			}
		}
	}

	[RPC]
	private void OnWeaponLevelMaxRPC(int weaponHash, uint playerId)
	{
		GameObject hudObject = Setup.Instance.GetComponent<HUD>().HudObject;
		if (!(hudObject != null))
		{
			return;
		}
		GameHUD component = hudObject.GetComponent<GameHUD>();
		if (Hub.Instance.GameManager.Game == null)
		{
			return;
		}
		EB.Sparx.Player player = Hub.Instance.GameManager.Game.GetPlayer(playerId);
		if (player != null)
		{
			string text = Dot.String("name", player.Attributes, string.Empty);
			if (!string.IsNullOrEmpty(text))
			{
				string friendlyName = InventoryDB.GetMetaData(weaponHash).friendlyName;
				component.ShowMessage(Localizer.Format("ID_GAME_ITEMLEVEL_MAXED", text, friendlyName));
			}
		}
	}

	[RPC]
	private void EndFire()
	{
	}

	[RPC]
	private void ApplyHit(Misc.HitInfo hit)
	{
		bool isDead = _health.IsDead;
		EB.Debug.LogWarning("ApplyHitRPC " + hit.ToString() + " to " + PlayerId);
		for (int i = 0; i < _activeBoosts.Length; i++)
		{
			if (_activeBoosts[i].IsActive() && _activeBoosts[i].Boost._type == BoostItem.Type.kShield)
			{
				if (_shield.ApplyHit(ref hit))
				{
					CooldownBoost(i);
				}
				break;
			}
		}
		_lastAttackerID = hit._attackerID;
		if (hit._damage > 0f)
		{
			if (hit._effectType != 0)
			{
				AddStatusEffect((StatusEffects.Type)hit._effectType, (PlayerEffects.Type)hit._effectVisualType, hit._effectAmount, hit._attackerID, hit._effectApplicationInterval, (int)hit._effectDurationSecs);
			}
			_health.ApplyHit(hit);
			if (!isDead && hit._attackerID != uint.MaxValue && (Scoring.Instance.GetTeamId(hit._attackerID) != Scoring.Instance.GetTeamId(_view.instantiatorId) || hit._points < 0))
			{
				Player player = LevelData.Instance.GetPlayer(hit._attackerID);
				if (player != null && hit._points > 0)
				{
					player.ScoreVisualTreatment(hit._points, hit._bonusPoints);
				}
				Scoring.Instance.AddScore(hit._attackerID, hit._points + hit._bonusPoints);
			}
		}
		_body.LinearVelocity = hit._dir * hit._mag;
		if (!(hit._mag > 0f))
		{
			return;
		}
		for (int j = 0; j < _activeBoosts.Length; j++)
		{
			if (_activeBoosts[j].IsActive() && _activeBoosts[j].Boost._type == BoostItem.Type.kJetpack)
			{
				CooldownBoost(j);
				break;
			}
		}
	}

	[RPC]
	private void ApplyHealth(float quantity)
	{
		_health.ApplyHeal(quantity);
	}

	[RPC]
	private void PickupKey()
	{
		_hasKey = true;
	}

	[RPC]
	private void ConsumeKey()
	{
		_hasKey = false;
		CrateManager.Instance.KeyDestroyed();
		CrateManager.Instance.ChestDestroyed();
	}

	[RPC]
	private void OnCrate(int points)
	{
		if (points > 0)
		{
			ScoreVisualTreatment(points);
			Scoring.Instance.AddScore(_view.ownerPlayer.PlayerId, points);
		}
	}

	private void PlaceWeapon(int weaponIndex, Vector3 position, Vector3 direction, float power)
	{
		PlayAnim("bot_drop_object", PlayMode.StopAll, true);
	}

	private void RegisterPointLabel()
	{
		GameObject template = Assets.Load("UI/InGameScoreIncrease") as GameObject;
		GameObject gameObject = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
		_scoreLabelID = HUDLabels.Instance.Register(template, gameObject.transform, 10);
	}

	public void ScoreVisualTreatment(int points, int bonusPoints = 0)
	{
		if (IsAI)
		{
			return;
		}
		if (GameAttributes.IsAuxFlagSet(GameAttributes.AuxFlagsType.kResetable))
		{
			WeaponMonitor.Instance.AddPoints(points + bonusPoints);
			return;
		}
		HUDLabels.Instance.Next(_scoreLabelID, AnimateScore, Localizer.Format("ID_GAME_SCORING_POINTS", points), _healthBar.transform, Vector3.up * 10f);
		if (bonusPoints > 0)
		{
			GameObject gameObject = HUDLabels.Instance.Next(_scoreLabelID, AnimateScore, Localizer.Format("ID_GAME_SCORING_BONUS_POINTS", bonusPoints), _healthBar.transform, Vector3.up * 35f);
		}
	}

	private IEnumerator AnimateScore(GameObject go, UnityEngine.Transform relativeTo, Vector3 offset)
	{
		Vector3 portraitPos = Vector3.zero;
		GameObject hudObj = Setup.Instance.GetComponent<HUD>().HudObject;
		GameHUD gameHUD = null;
		if (hudObj != null)
		{
			gameHUD = hudObj.GetComponent<GameHUD>();
			if (gameHUD != null)
			{
				portraitPos = gameHUD.PortraitPosition(PlayerId);
			}
		}
		float time = 2f;
		Vector3 speed = new Vector3(0f, 50f, 0f);
		Vector3 delta = Vector3.zero;
		while (time > 0f)
		{
			delta += speed * UnityEngine.Time.deltaTime;
			if (go != null)
			{
				go.transform.localPosition = relativeTo.localPosition + offset + delta;
				time -= UnityEngine.Time.deltaTime;
				yield return 1;
				continue;
			}
			yield break;
		}
		time = 0.25f;
		float mod = 1f / time;
		Vector3 startPos = go.transform.position;
		float startTime = UnityEngine.Time.time;
		while (time > 0f)
		{
			if (go != null)
			{
				go.transform.position = Vector3.Lerp(startPos, portraitPos, (UnityEngine.Time.time - startTime) * mod);
				time -= UnityEngine.Time.deltaTime;
				yield return 1;
				continue;
			}
			yield break;
		}
		if (gameHUD != null)
		{
			gameHUD.PointsSucked(PlayerId);
		}
		go.SetActive(false);
	}

	private bool PollGround()
	{
		Vector3 dir = new Vector3(0f, -1f);
		Vector3 position = base.transform.position;
		position.y += 0.5f;
		_centreProbe = false;
		Vector3 hitPoint;
		Vector3 hitNormal;
		Fixture hitFixture;
		if (Misc.RayCast(position, dir, out hitPoint, out hitNormal, out hitFixture, _body))
		{
			_centreProbe = true;
			return true;
		}
		dir.y -= _radius;
		if (Misc.RayCast(position + new Vector3(0f - _radius, _radius, 0f), dir, out hitPoint, out hitNormal, out hitFixture, _body) && hitNormal.y > 0.259f)
		{
			return true;
		}
		if (Misc.RayCast(position + new Vector3(_radius, _radius, 0f), dir, out hitPoint, out hitNormal, out hitFixture, _body) && hitNormal.y > 0.259f)
		{
			return true;
		}
		return false;
	}

	private bool OnPlayerCollision(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if (MyTurn() && _view.isMine)
		{
			FVector2 value = fixtureB.Body.Position - fixtureA.Body.Position;
			FVector2 linearVelocity = _body.LinearVelocity;
			if (FVector2.Dot(linearVelocity, value) > 0f)
			{
				GameObject gameObject = fixtureB.Body.UserData as GameObject;
				if (gameObject != null)
				{
					ReplicationView component = gameObject.GetComponent<ReplicationView>();
					if (component != null && component.GetComponent<Player>() != null)
					{
						Pushee = fixtureB.Body;
						Misc.HitInfo hitInfo = new Misc.HitInfo
						{
							_damage = 0f,
							_attackerID = _view.instantiatorId
						};
						component.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
					}
				}
			}
		}
		return true;
	}

	private void OnPlayerSeparation(Fixture fixtureA, Fixture fixtureB)
	{
		if (fixtureB.Body == Pushee)
		{
			Pushee = null;
		}
	}

	private void ProcessPush()
	{
		if (_move2.Moving)
		{
			if (Pushee != null)
			{
				FVector2 value = Pushee.Position - MyBody.Position;
				FVector2 linearVelocity = _body.LinearVelocity;
				if (FVector2.Dot(linearVelocity, value) > 0f)
				{
					PlayAnim("bot_push_broom");
				}
				return;
			}
			Vector3 dir = new Vector3(2f, 0f, 0f);
			Vector3 position = base.transform.position;
			position.y += HalfHeight;
			if (_move2._left)
			{
				dir.x = 0f - dir.x;
			}
			Vector3 hitPoint;
			Vector3 hitNormal;
			Fixture hitFixture;
			if (Misc.RayCast(position, dir, out hitPoint, out hitNormal, out hitFixture, MyBody) && (hitFixture.CollisionCategories & Category.Cat1) == 0)
			{
				PlayAnim("bot_push_on_wall");
			}
		}
		else
		{
			StopAnim("bot_push_broom");
			StopAnim("bot_push_on_wall");
		}
	}

	private void Update()
	{
		TrackActiveBoosts();
		TrackPushing();
		TrackFlying();
		TrackFalling();
		Aiming = _move2._aiming;
		Vector3 localScale = base.transform.localScale;
		localScale.x = Mathf.Abs(localScale.x) * ((!RightFacing) ? (-1f) : 1f);
		base.transform.localScale = localScale;
		if (!_view.isMine)
		{
			MoveBody(move: new Move
			{
				_left = _move2._left,
				_right = _move2._right
			}, body: _body);
		}
		if (_view.isMine)
		{
			_statusEffects.ApplyIntervalStatusEffects(this);
		}
		if (!isUpdatingHealthStateParticles)
		{
			StartCoroutine(UpdateHealthStateParticles());
		}
		for (int i = 0; i < _activeBoosts.Length; i++)
		{
			_activeBoosts[i]._remainingTime -= UnityEngine.Time.deltaTime;
		}
		UpdateInactiveBoosts();
	}

	private void TrackActiveBoosts()
	{
		if (PlayerId != EB.Replication.Manager.LocalPlayerId)
		{
			return;
		}
		for (int i = 0; i < _activeBoosts.Length; i++)
		{
			if (_activeBoosts[i].Valid && _activeBoosts[i].UpdateState())
			{
				ActiveBoost.State state = _activeBoosts[i].GetState();
				if (_activeBoosts[i].IsCooldown())
				{
					CooldownBoost(i);
				}
			}
		}
	}

	private void TrackPushing()
	{
	}

	private void TrackFlying()
	{
		if (_view.isMine && Grounded)
		{
			Flying = false;
		}
	}

	private void TrackFalling()
	{
		if (!_view.isMine)
		{
			return;
		}
		if (!Grounded && !Falling && !Spawning && !Flying)
		{
			if (_body.LinearVelocity.Y < 0f)
			{
				Falling = true;
			}
		}
		else if (Grounded)
		{
			Spawning = false;
			if (Falling)
			{
				Falling = false;
				float num = _zenith - base.transform.position.y;
				float num2 = num - Tuning.Instance.Player_MinFallHeight;
				EB.Debug.Log("Damage fall: " + num2);
				if (num2 > 0f)
				{
					float damage = Mathf.Min(num2, Tuning.Instance.Player_MaxFallHeight) * Tuning.Instance.Player_FallDamageMultiplier;
					Misc.HitInfo hitInfo = new Misc.HitInfo
					{
						_damage = damage,
						_attackerID = uint.MaxValue
					};
					_view.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
				}
			}
		}
		else if (Flying)
		{
			Falling = false;
		}
	}

	private void LateUpdate()
	{
		if (MyBody != null)
		{
			ContactEdge contactEdge = _body.ContactList;
			_contactCount = 0;
			while (contactEdge != null)
			{
				_contactCount++;
				contactEdge = contactEdge.Next;
			}
		}
		_idleTimer.Update();
		Vector3 position = base.transform.position;
		position.x = _body.Position.X;
		position.y = _body.Position.Y;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles.z = _body.Rotation * 57.29578f;
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(eulerAngles);
		Grounded = PollGround();
		if (MyTurn())
		{
			ProcessPush();
		}
		if (Jumping && _body.LinearVelocity.Y < -1f)
		{
			Jumping = false;
		}
		if (_centreProbe && !Jumping)
		{
			Friction = 1f;
		}
		if (_knockback && _contactCount > 0)
		{
			_knockback = false;
			StopAnim("bot_knockback_reaction");
		}
		if (position.y < Tuning.Instance.Player_KillFloor)
		{
			if (_view.isMine && !IsDying)
			{
				UnityEngine.Debug.LogError(string.Format("Player {0} dying with attacker {1}", PlayerId, _lastAttackerID));
				Scoring.Instance.PlayerDied(PlayerId, _lastAttackerID);
				Die(Localizer.Format("ID_GAME_DEATH_FALLEN", PlayerName), true);
				if (_hasKey)
				{
					_hasKey = false;
					CrateManager.Instance.KeyDestroyed();
				}
			}
			Player player = LevelData.Instance.GetPlayer();
			if (player != null && _lastAttackerID == player.PlayerId)
			{
				if (!_view.isMine)
				{
					StatManager.Instance.IncrementStat(StatManager.Stat.Kills);
				}
				else
				{
					StatManager.Instance.IncrementStat(StatManager.Stat.Suicides);
				}
				_lastAttackerID = uint.MaxValue;
			}
			_body.Enabled = false;
		}
		UpdateAnim();
		if ((!IsShowingProjectile && !IsPlayingSpecialIdleAnim() && !IsPlayingStunnedAnim()) || _move2._aiming || IsAI)
		{
			ShowProjectile(true);
		}
		if (_healthBar != null)
		{
			if (_health._hitPoints <= 0f && _healthBar.IsShowing)
			{
				_healthBar.Show(false);
			}
			else if (_health._hitPoints > 0f && !_healthBar.IsShowing)
			{
				_healthBar.Show(true);
			}
		}
		float angle = Mathf.Atan2(_move2._aimDir.y, _move2._aimDir.x) * 57.29578f;
		if (!IsAI)
		{
			if (CurrentWeapon._isPlaced || CurrentWeapon._eType == Weapon.Type.kBeam || CurrentWeapon._eType == Weapon.Type.kCarpetBomb || CurrentWeapon._eType == Weapon.Type.kMelee)
			{
				if (Aiming && CanFire)
				{
					CarpetBomb component = CurrentWeapon.weaponPrefab.GetComponent<CarpetBomb>();
					if (component != null && component._aimedEntryPoint)
					{
						PlayerHUD.DrawAirstrikeAimDevice(base.transform.position, _move2._power, _move2._aimDir);
					}
					else
					{
						PlayerHUD.DrawAimDevice(base.transform.position, CurrentWeapon.deviceOffset, angle);
					}
				}
			}
			else if (Aiming)
			{
				if (!_bFiringInProgress && CanFire)
				{
					if (weaponBone != null)
					{
						PlayerHUD.DrawPowerArrowFINAL(weaponBone.transform.position, CurrentWeapon.deviceOffset, angle, RealFirePower, Tuning.Instance.Control_PowerMeterDisplayScale);
						if (GameAttributes.IsTutorialGameMode() || _boostAimAssistActive)
						{
							PlayerHUD.DrawProjectileFlight(weaponBone.transform.position, CurrentWeapon.deviceOffset, angle, RealFirePower, CurrentWeapon, MyBody);
						}
					}
				}
				else if (CurrentWeapon._postActivationAim)
				{
					PlayerHUD.DrawAimDevice(weaponBone.transform.position, CurrentWeapon.deviceOffset, angle);
				}
			}
		}
		if (_view.isMine && !IsAI && MyTurn())
		{
			PlayerHUD.DrawMoveRing(base.transform.position + Vector3.up, 5f);
			if (_userMoveDir.magnitude > 0.25f)
			{
				PlayerHUD.DrawMoveIndicator(base.transform.position + Vector3.up, 5f, _userMoveDir);
			}
		}
	}

	public bool MyTurn()
	{
		return LevelData.CurrentPlayer == this;
	}

	private void OnSerializeView(BitStream stream)
	{
		if (_body != null)
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
				_move2.Serialize(stream);
			}
			else
			{
				_move2Repl.Serialize(stream);
				_move2Repl.Reset();
			}
			_health.Serialize(stream);
			stream.Serialize(ref _hasKey);
			stream.Serialize(ref _selectingWeapon);
		}
	}

	public FVector2 LinearVelocity()
	{
		return _body.LinearVelocity;
	}

	private void OnDamage(Misc.DamageInfo dmgInfo)
	{
		_lastAttackerID = dmgInfo.attackerID;
		if (EB.Replication.Manager.LocalPlayer != null && dmgInfo.attackerID == EB.Replication.Manager.LocalPlayer.PlayerId && dmgInfo.dead)
		{
			if (!_view.isMine)
			{
				StatManager.Instance.IncrementStat(StatManager.Stat.Kills);
			}
			else if (PlayerId == dmgInfo.attackerID)
			{
				StatManager.Instance.IncrementStat(StatManager.Stat.Suicides);
			}
			_lastAttackerID = uint.MaxValue;
		}
		if (dmgInfo.attackerID == uint.MaxValue && EB.Replication.Manager.LocalPlayer != null && EB.Replication.Manager.LocalPlayer.PlayerId == PlayerId)
		{
			StatManager.Instance.IncrementStat(StatManager.Stat.Suicides);
		}
		if (dmgInfo.dead)
		{
			string text = string.Empty;
			Game game = Hub.Instance.GameManager.Game;
			if (game != null)
			{
				EB.Sparx.Player player = game.GetPlayer(dmgInfo.attackerID);
				if (player != null)
				{
					text = player.Attributes["name"] as string;
				}
			}
			string deathText = Localizer.Format("ID_GAME_DEATH_DIED", PlayerName);
			if (!string.IsNullOrEmpty(text))
			{
				if (text == PlayerName)
				{
					deathText = Localizer.Format("ID_GAME_DEATH_SUICIDE", PlayerName);
				}
				else if (Hub.Instance.InventoryManager.IsOverriden)
				{
					deathText = Localizer.Format("ID_GAME_DEATH_KILLED", text, PlayerName);
				}
				else
				{
					InventoryDB.MetaData metaData = InventoryDB.GetMetaData(dmgInfo.weaponHash);
					if (metaData == null)
					{
						EB.Debug.LogWarning("Invalid hash for weapon: " + dmgInfo.weaponHash + ", displaying default kill text");
						deathText = Localizer.Format("ID_GAME_DEATH_KILLED", text, PlayerName);
					}
					else if (LevelData.Instance.PlayerIsAI(dmgInfo.attackerID))
					{
						deathText = Localizer.Format("ID_GAME_DEATH_KILLED", text, PlayerName);
					}
					else
					{
						string id = "ID_GAME_DEATH_KILLED_WEAPONLEVEL_RANGE0";
						for (int i = 0; i < Tuning.Instance.ItemLevelling_HudRange.Length && dmgInfo.weaponLevel >= Tuning.Instance.ItemLevelling_HudRange[i]; i++)
						{
							id = "ID_GAME_DEATH_KILLED_WEAPONLEVEL_RANGE" + i;
						}
						string text2 = Localizer.GetString(metaData.friendlyName);
						deathText = Localizer.Format(id, text, PlayerName, dmgInfo.weaponLevel + 1, text2);
					}
				}
			}
			Die(deathText, true);
			if (_hasKey)
			{
				_hasKey = false;
				if (dmgInfo.damage >= Tuning.Instance.KeyDamageRespawnThreshold)
				{
					CrateManager.Instance.KeyDestroyed();
				}
				else if (!IsAI)
				{
					if (!GameAttributes.IsTutorialGameMode())
					{
						CrateManager.Instance.SpawnKey(base.transform);
					}
				}
				else
				{
					CrateManager.Instance.SpawnKey(base.transform);
				}
			}
			_statusEffects.Clear();
			if (ImpulseWeaponPurchaseUI.IsActive || GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode)) != GameAttributes.ModeType.MP || !Profile.Instance.RevengeSaleAvailable || dmgInfo.attackerID == EB.Replication.Manager.LocalPlayer.PlayerId || PlayerId != EB.Replication.Manager.LocalPlayer.PlayerId)
			{
				return;
			}
			float num = UnityEngine.Random.Range(0f, 1f);
			if (!Profile.Instance.RevengeSaleAlwaysShows && !DebugSettings.ForceRevengeSaleRoll && !(num <= Tuning.Instance.RevengeSaleProbability))
			{
				return;
			}
			Player player2 = LevelData.Instance.GetPlayer(dmgInfo.attackerID);
			if (!(player2 != null))
			{
				return;
			}
			Weapon currentWeapon = player2.CurrentWeapon;
			if (!(currentWeapon != null))
			{
				return;
			}
			InventoryDB.MetaData metaData2 = InventoryDB.GetMetaData(Hash.StringHash(currentWeapon.name));
			if (metaData2 == null)
			{
				return;
			}
			EB.Debug.Log("killed by " + metaData2.name + ", RV quantity: " + metaData2.revengeSaleQuantity);
			if (!Profile.Instance.RevengeSaleAlwaysShows && metaData2.revengeSaleQuantity <= 0)
			{
				return;
			}
			bool flag = Hub.Instance.InventoryManager.GetCount(metaData2.name) > 0;
			bool flag2 = !metaData2.IsLocked;
			bool isHidden = metaData2.isHidden;
			if (Profile.Instance.RevengeSaleAlwaysShows || (!isHidden && !flag && !flag2))
			{
				float num2 = GameHUD.Instance.GameTimeSecsRemaining;
				if (Profile.Instance.RevengeSaleAlwaysShows || num2 >= Tuning.Instance.RevengeSaleMinimumGameTimeSecs)
				{
					_revengeSaleInfo = new RevengeSaleInfo();
					_revengeSaleInfo.attackerName = player2.PlayerName;
					_revengeSaleInfo.weaponName = metaData2.name;
				}
			}
		}
		else if (dmgInfo.bullet)
		{
			PlayAnim("bot_riddled_death", PlayMode.StopAll, false);
		}
		else if (!Aiming)
		{
			_knockback = true;
			PlayAnim("bot_knockback_reaction", PlayMode.StopSameLayer, true);
		}
	}

	public bool CanUseWeapon(int index)
	{
		if (!IsAI && !Hub.Instance.InventoryManager.EverythingFree)
		{
			Weapon weapon = Arsenal.WeaponAt(index);
			if (!weapon.IsFree && Hub.Instance.InventoryManager.GetCount(weapon.name) == 0)
			{
				EB.Debug.Log("Can't use weapon " + weapon.name);
				return false;
			}
			if (Hub.Instance.InventoryManager.IsOverriden && Hub.Instance.InventoryManager.GetCount(weapon.name) == 0)
			{
				EB.Debug.Log("Can't use weapon " + weapon.name);
				return false;
			}
		}
		return true;
	}

	public void SelectFirstOverrideWeapon()
	{
		InventoryManager inventoryManager = Hub.Instance.InventoryManager;
		if (!inventoryManager.IsOverriden)
		{
			return;
		}
		for (int i = 0; i < inventoryManager.OverrideCount; i++)
		{
			string overrideItem = inventoryManager.GetOverrideItem(i);
			if (inventoryManager.GetCount(overrideItem) > 0)
			{
				int weaponIndex = Arsenal.GetWeaponIndex(overrideItem);
				if (weaponIndex >= 0)
				{
					SelectWeapon(weaponIndex);
				}
				break;
			}
		}
	}

	public void SelectWeapon(int index)
	{
		ChangeWeaponTo = index;
	}

	public void SelectFirstAvailableWeapon()
	{
		ChangeWeaponTo = FindFallbackWeaponIndex();
	}

	[RPC]
	private void SelectStartingWeaponRPC(int index)
	{
		StartingWeapon = index;
	}

	[RPC]
	private void SelectWeaponRPC(int index)
	{
		if (index < 0 || index >= Arsenal.NumItems(InventoryItem.Category.Weapon))
		{
			EB.Debug.LogError(string.Format("Select Weapon called with out of range weapon index! Num weapons {0} requested index {1}", Arsenal.NumItems(InventoryItem.Category.Weapon), index));
		}
		Weapon weapon = Arsenal.WeaponAt(_currentWeaponIndex);
		if (weapon != null && weaponBone != null)
		{
			weapon.DestroyProjectileProxy(weaponBone.transform);
		}
		_currentWeaponIndex = index;
		weapon = Arsenal.WeaponAt(_currentWeaponIndex);
		if (_smCharacter != null && !string.IsNullOrEmpty(weapon.spriteName))
		{
			string replaceAtlasName = "weapon_1";
			if (weapon.spriteAtlas != null)
			{
				_smCharacter.ClearAddedAtlases();
				_smCharacter.AddAtlas(weapon.spriteAtlas);
				replaceAtlasName = weapon.spriteAtlas.name;
			}
			else
			{
				EB.Debug.LogWarning("Weapon's sprite atlas is null!!! How did this happen???");
			}
			_smCharacter.SwapBoneTexture("weapon", "weapon_1", "shotgun", replaceAtlasName, weapon.spriteName);
			_smCharacter.GenerateMesh();
			DoIdle();
		}
		_scheduledSpawnProjectileAndGameHud = true;
	}

	private void LateSpawnProjectileAndGameHudNotification()
	{
		_scheduledSpawnProjectileAndGameHud = false;
		SpawnProjectile();
		if (GameHUD.Instance != null)
		{
			GameHUD.Instance.OnWeaponSelected(this);
		}
	}

	public int WeaponIndex()
	{
		return _currentWeaponIndex;
	}

	private void DoIdle()
	{
		if (AllowIdle)
		{
			string animName = Misc.RandomString(_idleAnims);
			PlayAnim(animName, PlayMode.StopSameLayer, false);
			if (DebugSettings.FasterIdling)
			{
				_idleTimer.Start(UnityEngine.Random.Range(3, 4));
			}
			else
			{
				_idleTimer.Start(UnityEngine.Random.Range(Tuning.Instance.Player_SpecialIdleWaitMin, Tuning.Instance.Player_SpecialIdleWaitMax));
			}
		}
	}

	private void ShowProjectile(bool show)
	{
		if (!(_smCharacter == null))
		{
			_smCharacter.HideBone("weapon", !show);
			Weapon weapon = Arsenal.WeaponAt(_currentWeaponIndex);
			if (weapon != null && weaponBone != null)
			{
				weapon.ShowProjectile(weaponBone.transform, show);
			}
			else if (weaponBone == null)
			{
				EB.Debug.LogWarning("trying to access weapon bone after it's been nulled");
			}
			else
			{
				EB.Debug.LogWarning("Unable to find weapon at index " + _currentWeaponIndex + ",  there are " + Arsenal.NumItems(InventoryItem.Category.Weapon) + " weapons in the arsenal");
			}
			IsShowingProjectile = show;
		}
	}

	public bool IsPlayingOneOfTheseAnims(string[] anims)
	{
		if (_smCharacter != null)
		{
			foreach (string animationName in anims)
			{
				if (_smCharacter.IsPlaying(animationName))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool IsPlayingShootingAnim()
	{
		return IsPlayingOneOfTheseAnims(_shootingAnims);
	}

	private bool IsPlayingStunnedAnim()
	{
		return IsPlayingOneOfTheseAnims(_stunnedAnims);
	}

	private bool IsPlayingSpecialIdleAnim()
	{
		return IsPlayingOneOfTheseAnims(_idleAnims);
	}

	private void StopMoveAnims()
	{
		if (_smCharacter != null)
		{
			string[] moveAnims = _moveAnims;
			foreach (string animationName in moveAnims)
			{
				_smCharacter.Stop(animationName);
			}
		}
	}

	private void StopStunnedAnims()
	{
		if (_smCharacter != null)
		{
			string[] stunnedAnims = _stunnedAnims;
			foreach (string animationName in stunnedAnims)
			{
				_smCharacter.Stop(animationName);
			}
		}
	}

	private void StopSpecialIdleAnims()
	{
		if (_smCharacter != null)
		{
			string[] idleAnims = _idleAnims;
			foreach (string animationName in idleAnims)
			{
				_smCharacter.Stop(animationName);
			}
		}
	}

	public void StopAnim(string animName)
	{
		if (_smCharacter != null)
		{
			_smCharacter.Stop(animName);
		}
	}

	public void RegisterProjectile(UnityEngine.Transform trans, float radius)
	{
		activeProjectile = trans;
		projectileRadius = radius;
	}

	public void UnregisterProjectile(UnityEngine.Transform trans)
	{
		if (activeProjectile == trans)
		{
			activeProjectile = null;
		}
	}

	public bool CanAim()
	{
		return CanFire || (CurrentWeapon._postActivationAim && _bFiringInProgress) || (CurrentWeapon._isPlaced && CanFire);
	}

	private void AddLootRivets(int val)
	{
		if (_view.isMine)
		{
			Profile.Instance.AddSessionSoftCurrency(val);
		}
	}

	private void AddLootXp(int val)
	{
		if (_view.isMine)
		{
			Profile.Instance.AddSessionXP(val);
		}
	}

	private IEnumerator UpdateHealthStateParticles()
	{
		if (_health.FractionalHealth < criticalDamagePercentage)
		{
			if (criticalDamageParticles == null || (criticalDamageParticles != null && !criticalDamageParticles.isPlaying))
			{
				GameObject rootObj = Util.GetObjectExactMatch(base.gameObject, "head");
				StopHealthStateParticles(severeDamageParticles);
				StartHealthStateParticles(criticalDamageParticlesPrefab, rootObj, ref criticalDamageParticles);
			}
		}
		else if (_health.FractionalHealth < severeDamagePercentage)
		{
			if (severeDamageParticles == null || (severeDamageParticles != null && !severeDamageParticles.isPlaying))
			{
				GameObject rootObj2 = Util.GetObjectExactMatch(name: _emitterBones[UnityEngine.Random.Range(0, _emitterBones.Length)], obj: base.gameObject);
				float delay = UnityEngine.Random.Range(severeDamageMinDelay, severeDamageMaxDelay);
				isUpdatingHealthStateParticles = true;
				yield return new WaitForSeconds(delay);
				StopHealthStateParticles(criticalDamageParticles);
				StartHealthStateParticles(severeDamageParticlesPrefab, rootObj2, ref severeDamageParticles);
				isUpdatingHealthStateParticles = false;
			}
		}
		else
		{
			StopHealthStateParticles(criticalDamageParticles);
			StopHealthStateParticles(severeDamageParticles);
		}
	}

	private void StartHealthStateParticles(ParticleSystem prefab, GameObject rootObj, ref ParticleSystem instance)
	{
		if (!(prefab != null))
		{
			return;
		}
		if (instance == null)
		{
			instance = UnityEngine.Object.Instantiate(prefab) as ParticleSystem;
			if (rootObj != null)
			{
				UnityEngine.Transform parent = rootObj.transform;
				instance.gameObject.transform.parent = parent;
				instance.gameObject.transform.localPosition = Vector3.zero;
			}
		}
		else if (!instance.isPlaying)
		{
			if (rootObj != null)
			{
				UnityEngine.Transform parent2 = rootObj.transform;
				instance.gameObject.transform.parent = parent2;
				instance.gameObject.transform.localPosition = Vector3.zero;
			}
			instance.Play();
		}
	}

	private void StopHealthStateParticles(ParticleSystem instance)
	{
		if (instance != null)
		{
			instance.Stop();
		}
	}

	private void SetJetpackMode(JetpackMode mode)
	{
		_jetpackMode = mode;
		switch (_jetpackMode)
		{
		case JetpackMode.kJetPack:
			_body.IgnoreGravity = true;
			break;
		case JetpackMode.kHoverPack:
			_body.IgnoreGravity = true;
			Modifier_Gravity = 0f;
			break;
		case JetpackMode.kNone:
			Modifier_Gravity = 1f;
			Flying = false;
			break;
		}
	}

	public bool IsJetpackActive()
	{
		return _jetpackMode == JetpackMode.kJetPack;
	}

	public bool IsHoverpackActive()
	{
		return _jetpackMode == JetpackMode.kHoverPack;
	}

	private void OnDrawGizmos()
	{
		if (_body != null)
		{
			Gizmos.color = Color.red;
			Misc.DrawFatAABB(_body);
		}
		if (weaponBone != null)
		{
			Gizmos.matrix = Matrix4x4.identity;
			Gizmos.color = new Color(0.8f, 0.7f, 1f);
			Gizmos.DrawWireCube(weaponBone.transform.position, Vector3.one * 0.5f);
		}
		Gizmos.color = new Color(1f, 0.7f, 0.7f);
		Gizmos.DrawSphere(LAUNCHPOS, 0.5f);
	}

	public void Debug_UpdateTuningVariables()
	{
		_baseJumpSpeed = Misc.JumpSpeedRequiredToAttainHeight(Tuning.Instance.Player_JumpHeight * 0.1f, 0f - FSWorldComponent.PhysicsWorld.Gravity.Y);
	}

	public FVector2 Position()
	{
		FVector2 result = FSHelper.Vector3ToFVector2(base.transform.position);
		result.Y += HalfHeight * 0.5f;
		return result;
	}

	public void FinishedFiring()
	{
		_bFiringInProgress = false;
		Aiming = false;
	}

	private void InitAnimEvents()
	{
		Animation componentInChildren = GetComponentInChildren<Animation>();
		if (componentInChildren != null)
		{
			AnimRebroadcast component = componentInChildren.GetComponent<AnimRebroadcast>();
			if (component == null)
			{
				componentInChildren.gameObject.AddComponent<AnimRebroadcast>();
			}
		}
		if (AnimEventsInitialized || !(componentInChildren != null))
		{
			return;
		}
		AnimEventInfo[] animEvents = _animEvents;
		for (int i = 0; i < animEvents.Length; i++)
		{
			AnimEventInfo animEventInfo = animEvents[i];
			AnimationState animationState = componentInChildren[animEventInfo.animName];
			if (animationState != null)
			{
				AnimationEvent animationEvent = new AnimationEvent();
				animationEvent.functionName = animEventInfo.fnName;
				animationEvent.time = animEventInfo.time;
				animationEvent.stringParameter = animEventInfo.parm;
				animationState.clip.AddEvent(animationEvent);
			}
		}
		AnimEventsInitialized = true;
	}

	private void BuildAnimAudio()
	{
		string[] audioAnims = _audioAnims;
		string s;
		for (int i = 0; i < audioAnims.Length; i++)
		{
			s = audioAnims[i];
			AudioEvent audioEvent = Array.Find(_animationAudioEvents, (AudioEvent ae) => ae.name.ToLower().Contains(s));
			if (audioEvent != null)
			{
				_animAudio.Add(s, audioEvent);
			}
			else
			{
				EB.Debug.LogError("There is no audio event to match the audio anim " + s);
			}
		}
	}

	private void OnPlayerAnimEvent(string stringParm)
	{
		string[] array = stringParm.Split('#');
		switch (array[0])
		{
		case "bot_hack_hit":
		{
			float angleDeg = Mathf.Atan2(_move2._aimDir.y, _move2._aimDir.x) * 57.29578f;
			CurrentWeapon.OnHitEvent(weaponBone.transform, angleDeg);
			break;
		}
		case "bot_hack_hit_window_open":
			CurrentWeapon.OnHitWindowOpen();
			break;
		case "bot_hack_hit_window_close":
			CurrentWeapon.OnHitWindowClose();
			break;
		case "bot_drop_object":
			_view.RPC("FireRPC", RPCMode.All, _currentWeaponIndex, weaponBone.transform.position, _move2._aimDir, 1f, WeaponLevel(CurrentWeapon.name));
			break;
		case "bot_audioevent":
			PlayAudioEvent(array[1]);
			break;
		case "bot_hack_start":
			CurrentWeapon.OnStartSwing();
			break;
		case "bot_begin_sink":
			StartCoroutine(BotSink());
			break;
		case "bot_fire_impulse":
			if (CurrentWeapon != null)
			{
				switch (CurrentWeapon._eType)
				{
				case Weapon.Type.kMelee:
					_body.LinearVelocity += FVector2.UnitY * CurrentWeapon._fireImpulse;
					break;
				case Weapon.Type.kProjectile:
					break;
				case Weapon.Type.kBeam:
				case Weapon.Type.kCarpetBomb:
					break;
				}
			}
			break;
		}
	}

	private IEnumerator BotSink()
	{
		FVector2 down = new FVector2(0f, -0.1f);
		while (_dying)
		{
			_body.Position += down;
			yield return new WaitForFixedUpdate();
		}
	}

	private void PlayAudioEvent(string audioEvent)
	{
		AudioEvent audioEvent2 = Array.Find(_animationAudioEvents, (AudioEvent ae) => ae.name.ToLower().EndsWith(audioEvent));
		if (audioEvent2 != null)
		{
			_audioEmitter.Play(audioEvent2);
		}
	}

	public void InstantDeath()
	{
		_health.OnInstantDeath(true);
		HandleDrowningOrInstaDeathScoring(true);
		Die(Localizer.Format("ID_GAME_DEATH_DIED", PlayerName), true);
	}

	public void BeginDrown()
	{
		StartCoroutine(DoDrown());
	}

	private IEnumerator DoDrown()
	{
		_body.Enabled = false;
		_dying = true;
		PlayAnim("bot_drowning", PlayMode.StopAll, true);
		_health.OnDrown();
		HandleDrowningOrInstaDeathScoring(false);
		yield return new WaitForSeconds(Tuning.Instance.Player_DrownPeriod);
		Die(Localizer.Format("ID_GAME_DEATH_DIED", PlayerName), false);
	}

	private void HandleDrowningOrInstaDeathScoring(bool playDeathAnims)
	{
		if (_view.isMine)
		{
			Scoring.Instance.PlayerDied(PlayerId, _lastAttackerID);
		}
		_statusEffects.Clear();
		Player player = LevelData.Instance.GetPlayer();
		if ((bool)player)
		{
			if (player.PlayerId == _lastAttackerID)
			{
				if (PlayerId != player.PlayerId)
				{
					StatManager.Instance.IncrementStat(StatManager.Stat.Kills);
				}
				else
				{
					StatManager.Instance.IncrementStat(StatManager.Stat.Suicides);
				}
			}
			else if (_lastAttackerID == uint.MaxValue && player.PlayerId == PlayerId)
			{
				StatManager.Instance.IncrementStat(StatManager.Stat.Suicides);
			}
		}
		if (_view.isMine && _hasKey)
		{
			_hasKey = false;
			CrateManager.Instance.KeyDestroyed();
		}
	}

	private bool IsCanPlayWhileAiming(string animName)
	{
		return Array.Exists(_canPlayWhileAimingAnims, (string s) => s == animName);
	}

	private void PrintAI(string s)
	{
		if (IsAI)
		{
			UnityEngine.Debug.LogWarning(s);
		}
	}

	public void AttachToHead(UnityEngine.Transform attachee, Vector3 localPos)
	{
		attachee.parent = _headBone.transform;
		attachee.transform.localPosition = localPos;
	}

	public bool IsDead()
	{
		return HealthPercent <= 0f;
	}

	private int FindFallbackWeaponIndex()
	{
		if (IsAI)
		{
			EB.Debug.Log("AI using fallback weapon Rocket");
			return Arsenal.GetWeaponIndex("Rocket");
		}
		List<InventoryDB.MetaData> categoryList = InventoryDB.GetCategoryList(InventoryItem.Category.Weapon);
		foreach (InventoryDB.MetaData item in categoryList)
		{
			int weaponIndex = Arsenal.GetWeaponIndex(item.name);
			if (CanUseWeapon(weaponIndex) && !item.name.Equals("Teleport Grenade") && weaponIndex != Arsenal.EmptyWeaponIndex)
			{
				EB.Debug.Log("using fallback weapon: " + weaponIndex + "/" + Arsenal.WeaponAt(weaponIndex).name);
				return weaponIndex;
			}
		}
		EB.Debug.LogWarning("no suitable fallback weapon found, using Empty weapon!");
		return Arsenal.GetWeaponIndex("Empty");
	}

	public void OnGameHUDVisible(bool bVisible)
	{
		if (_healthBar != null)
		{
			_healthBar.OnGameHUDVisible(bVisible);
		}
	}
}
