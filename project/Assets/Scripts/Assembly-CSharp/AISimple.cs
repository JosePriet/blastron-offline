using System;
using System.Collections;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Terrain;
using UnityEngine;

public class AISimple : AIBase
{
	public class WeaponList
	{
		public float _chanceMod;

		public List<int> _weaponIndices = new List<int>();

		public List<int> _ammo = new List<int>();

		public List<bool> _isAdvanced = new List<bool>();

		public int RandomIdx()
		{
			return _weaponIndices[UnityEngine.Random.Range(0, _weaponIndices.Count)];
		}
	}

	private struct Waypoint
	{
		public FVector2 _p;

		public float _r2;

		public Waypoint(FVector2 p, float r2)
		{
			_p = p;
			_r2 = r2;
		}
	}

	private const int longPath = 20;

	private const float leftRightProbeLength = 2f;

	private const float upProbeLength = 4f;

	private const float downProbeLength = 1f;

	private const float _thinkInterval = 1f;

	private const float _probeCellSpacing = 5f;

	private const int _probesInY = 5;

	private const int _probesInX = 5;

	internal const int _maxWeaponSwitchesPerTurn = 2;

	private const float _crateDistanceMax = 20f;

	private const int _firingSolutionSegs = 16;

	internal const float _repathInterval = 6f;

	internal const float _hidePeriod = 6f;

	internal const int _solutionAttempts = 10;

	internal const int _goAfterCrateMaxAttempts = 5;

	internal const float _solutionAttemptInterval = 0.5f;

	private const float _cTooClose = 5f;

	private const float _cTooCloseToFire = 4f;

	private static SinglePlayerPrefab.Loadout[] _botWeapons = new SinglePlayerPrefab.Loadout[12]
	{
		new SinglePlayerPrefab.Loadout
		{
			name = "40 Bomb",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Blow-It-Up-Fish",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Grenade",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Pineapple Trifecta",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Quatro Bouncers",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Rocket",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Teleport Grenade",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Land Mine",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Grenade",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Ricochet Grenade",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Spear",
			count = 10
		},
		new SinglePlayerPrefab.Loadout
		{
			name = "Mo' Betta Rocket",
			count = 10
		}
	};

	private static SinglePlayerPrefab.Loadout[] _sLoadout;

	private bool _useAdvancedInventory;

	public static bool onlyAttackPlayer;

	private AISimpleStates _simpleStates;

	private AIBossStates _bossStates;

	public static int _AdvancedInventoryIndex;

	public static int _AdvancedInventoryThreshold = 1000;

	public static List<GameObject> _destructablePhysicsObjects = new List<GameObject>();

	public GameObject _PhysicsObjectTarget;

	internal Player _targetPlayer;

	internal bool _triedToshakePlayerOff;

	public WeaponList[] _myArsenal;

	internal int _curWeaponIndexPos;

	internal int _curWeaponType;

	internal FVector2 _CrateTargetPos;

	private TerrainBody _tb;

	internal FVector2 _targetPositionF2;

	internal FVector2 _myPositionF2;

	internal Player _p;

	private float _rangeToTarget;

	internal bool _hitR;

	internal bool _hitL;

	internal bool _hitU;

	internal Navigation.Node _path;

	private List<Waypoint> _waypoints;

	private bool _leftPlatform;

	private bool _rightPlatform;

	internal Vector3 _firingDir;

	internal float _firingMag;

	internal bool _aiming;

	internal bool _goRight;

	internal bool _goLeft;

	internal bool _jump;

	private bool _fire;

	internal int _weaponSwitchesThisTurn;

	internal int _GoAfterCrateAttempts;

	internal bool _firedThisTurn;

	internal Misc.Timer _stuckTimer;

	internal bool IsBot { get; set; }

	public bool IsBoss { get; set; }

	internal Navigation.Node NavPath
	{
		get
		{
			return _path;
		}
		set
		{
			if (!object.ReferenceEquals(value, _path))
			{
				_stuckTimer.Active = false;
				_path = value;
				if (!object.ReferenceEquals(_path, null))
				{
					_stuckTimer.Start(2f);
				}
			}
		}
	}

	internal bool Teleported { get; set; }

	private bool ArsenalInitialized
	{
		get
		{
			return _myArsenal != null;
		}
	}

	internal bool Fire
	{
		get
		{
			return _fire;
		}
		set
		{
			_fire = value;
		}
	}

	internal float FiringMag
	{
		get
		{
			return _firingMag;
		}
		set
		{
			_firingMag = value;
		}
	}

	public static void SetLoadout(SinglePlayerPrefab.Loadout[] loadout)
	{
		_sLoadout = loadout;
	}

	private IEnumerator Start()
	{
		_p = GetComponent<Player>();
		if (!IsBoss)
		{
			_simpleStates = base.gameObject.AddComponent<AISimpleStates>();
			_simpleStates._aiScript = this;
			_simpleStates.CreateStates();
		}
		else
		{
			_bossStates = base.gameObject.AddComponent<AIBossStates>();
			_bossStates._aiScript = this;
			_bossStates.CreateStates();
		}
		_waypoints = new List<Waypoint>();
		if (!ArsenalInitialized && _sLoadout != null)
		{
			OrderArsenal(_sLoadout);
		}
		SetState("StateThink");
		base.Paused = true;
		_stuckTimer = new Misc.Timer(OnStuck);
		yield break;
	}

	private void OnStuck()
	{
		if (_p.Grounded)
		{
			_jump = true;
		}
		SetState("StateThink");
	}

	private void EnableInput(bool bEnable)
	{
		if (bEnable)
		{
			Pause(false);
		}
		else
		{
			Pause(true);
		}
	}

	private void EndGame()
	{
		base.enabled = false;
	}

	public override void Pause(bool pause)
	{
		base.Pause(pause);
		if (pause)
		{
			NavPath = null;
			StopAllCoroutines();
			return;
		}
		Teleported = false;
		_GoAfterCrateAttempts = 0;
		_weaponSwitchesThisTurn = 0;
		_rangeToTarget = float.MaxValue;
		if (IsBot)
		{
			SetState("StatePretendThink");
		}
		else
		{
			SetState("StateThink");
		}
		if (!IsBoss)
		{
			_simpleStates.StartCoroutine("FollowPath");
		}
		else
		{
			_bossStates.StartCoroutine("FollowPath");
		}
		_firedThisTurn = false;
		_triedToshakePlayerOff = false;
		if (onlyAttackPlayer)
		{
			_targetPlayer = LevelData.Instance.GetPlayer();
		}
		else
		{
			_targetPlayer = RandomTarget();
		}
	}

	private void CheckScoreThreshold()
	{
		int scoreByPlayer = Scoring.Instance.GetScoreByPlayer(LevelData.Instance.GetPlayer().PlayerId);
		int scoreByPlayer2 = Scoring.Instance.GetScoreByPlayer(_p.PlayerId);
		int num = scoreByPlayer - scoreByPlayer2;
		if (num > _AdvancedInventoryThreshold)
		{
			_useAdvancedInventory = true;
		}
		else
		{
			_useAdvancedInventory = false;
		}
	}

	protected override void Update()
	{
		if (!base.Paused)
		{
			base.Update();
			_myPositionF2 = _p.MyBody.Position + FVector2.UnitY * _p.Height;
			if (_targetPlayer != null)
			{
				_targetPositionF2 = _targetPlayer.Position();
			}
			_rangeToTarget = FVector2.Distance(_targetPositionF2, _myPositionF2);
			ProbeImmediate();
			Player.PlayerInstructions move = new Player.PlayerInstructions
			{
				aiming = _aiming,
				fireDir = _firingDir,
				fireMag = FiringMag,
				goLeft = _goLeft,
				goRight = _goRight
			};
			if (!_hitU)
			{
				move.jump = _jump;
			}
			move.fire = Fire;
			_p.Move2(move);
			Fire = false;
			_jump = false;
		}
	}

	internal bool CanFire()
	{
		return !_firedThisTurn || !_p.CurrentWeapon._endsTurn;
	}

	internal bool BotOnTopOfMe()
	{
		Ray ray = new Ray(FSHelper.FVector2ToVector3(_p.MyBody.Position), Vector3.up * 10f);
		RaycastHit hitInfo;
		if (Physics.Raycast(ray, out hitInfo, 10f))
		{
			Player component = hitInfo.collider.GetComponent<Player>();
			if (component != null && component.PlayerId != _p.PlayerId)
			{
				_targetPlayer = component;
				return true;
			}
		}
		return false;
	}

	internal bool TooClose()
	{
		return _rangeToTarget < 5f;
	}

	private bool TooCloseToFire()
	{
		return _rangeToTarget < 4f;
	}

	internal bool DetermineHidingPlace(out FVector2 pos)
	{
		FVector2 min = _myPositionF2 - new FVector2(25f, 15f);
		FVector2 max = _myPositionF2 + new FVector2(25f, 15f);
		return DetermineHidingPlace(_myPositionF2, min, max, out pos);
	}

	private bool DetermineHidingPlace(FVector2 me, FVector2 min, FVector2 max, out FVector2 pos)
	{
		return LevelData.Instance.FreeAccessiblePosition(me, min, max, out pos);
	}

	internal bool ProjectileFiringSolution(Weapon w, float parametricPower, out float theta, FVector2 fromPos, FVector2 toPos, bool checkforPlayer = true)
	{
		return AIHelp.ProjectileFiringSolution(w.ProjectileSpeed(parametricPower), FSWorldComponent.PhysicsWorld.Gravity.Y * w.GravityScale, out theta, fromPos, toPos, _p.MyBody, checkforPlayer);
	}

	internal bool CanShootDestructableObject(out float theta, out float power)
	{
		theta = 0f;
		int num = 10;
		float num2 = 1f / (float)num;
		power = 0f;
		foreach (GameObject destructablePhysicsObject in _destructablePhysicsObjects)
		{
			for (int i = 0; i < 10; i++)
			{
				if (ProjectileFiringSolution(_p.CurrentWeapon, power, out theta, _myPositionF2, FSHelper.Vector3ToFVector2(destructablePhysicsObject.transform.position), false))
				{
					_PhysicsObjectTarget = destructablePhysicsObject;
					return true;
				}
				power += num2;
			}
		}
		return false;
	}

	internal string _currentWeaponFiringState()
	{
		switch (_p.CurrentWeapon._eType)
		{
		case Weapon.Type.kBeam:
			return "StateLaserSolution";
		case Weapon.Type.kCarpetBomb:
			return "StateCarpetBombFiringSoulution";
		case Weapon.Type.kMelee:
			return "StateMelee";
		case Weapon.Type.kProjectile:
			return "StateProjectileFiringSolution";
		default:
			return "error";
		}
	}

	private void AddHit(FVector2 p, FVector2 n)
	{
		float r = FVector2.DistanceSquared(p, _myPositionF2);
		_waypoints.Add(new Waypoint(p, r));
	}

	private void ProbeImmediate()
	{
		Misc.IgnoreDynamicBodiesInRayCast = true;
		FVector2 start = _p.Position() + FVector2.UnitY * 0.5f;
		FVector2 hitPoint;
		FVector2 hitNormal;
		Fixture hitFixture;
		_hitR = Misc.RayCast(start, FVector2.UnitX * 2f, out hitPoint, out hitNormal, out hitFixture, _p.MyBody);
		FVector2 hitPoint2;
		_hitL = Misc.RayCast(start, -FVector2.UnitX * 2f, out hitPoint2, out hitNormal, out hitFixture, _p.MyBody);
		_hitU = Misc.RayCast(start, FVector2.UnitY * 4f, _p.MyBody);
		Misc.IgnoreDynamicBodiesInRayCast = false;
	}

	private bool ProbeGap(float dir)
	{
		return Misc.RayCast(_myPositionF2 + FVector2.UnitX * 1f * dir, -FVector2.UnitY * 2f, _p.MyBody);
	}

	public void OrderBotArsenal()
	{
		OrderArsenal(_botWeapons);
	}

	public void OrderArsenal(SinglePlayerPrefab.Loadout[] weapons)
	{
		Array values = Enum.GetValues(typeof(Weapon.Type));
		_myArsenal = new WeaponList[values.Length];
		for (int i = 0; i < values.Length; i++)
		{
			_myArsenal[i] = new WeaponList();
		}
		List<int> list = new List<int>();
		int num = 0;
		foreach (SinglePlayerPrefab.Loadout loadout in weapons)
		{
			int weaponIndex = Arsenal.GetWeaponIndex(loadout.name);
			if (weaponIndex != -1)
			{
				Weapon weapon = Arsenal.WeaponAt(weaponIndex);
				if (weapon != null && weapon.FriendlyName != "Teleport Grenade")
				{
					_myArsenal[(int)weapon._eType]._weaponIndices.Add(weaponIndex);
					_myArsenal[(int)weapon._eType]._ammo.Add(loadout.count);
					if (num >= _AdvancedInventoryIndex - 1)
					{
						list.Add(weaponIndex);
					}
				}
			}
			num++;
		}
		WeaponList[] myArsenal = _myArsenal;
		foreach (WeaponList weaponList in myArsenal)
		{
			float totalPercent = 0f;
			weaponList._weaponIndices.ForEach((int index) =>
			{
				Weapon weapon2 = Arsenal.WeaponAt(index);
				if (weapon2._percentChanceOfAIChoosing < 0)
				{
					weapon2._percentChanceOfAIChoosing = UnityEngine.Random.Range(1, 101);
				}
				totalPercent += weapon2._percentChanceOfAIChoosing;
			});
			weaponList._chanceMod = totalPercent;
			weaponList._weaponIndices.Sort((int a, int b) =>
			{
				Weapon weapon2 = Arsenal.WeaponAt(a);
				Weapon weapon3 = Arsenal.WeaponAt(b);
				return weapon3._percentChanceOfAIChoosing.CompareTo(weapon2._percentChanceOfAIChoosing);
			});
		}
		WeaponList[] myArsenal2 = _myArsenal;
		foreach (WeaponList weaponList2 in myArsenal2)
		{
			int[] array = new int[weaponList2._weaponIndices.Count];
			bool[] array2 = new bool[weaponList2._weaponIndices.Count];
			for (int num3 = 0; num3 < weaponList2._weaponIndices.Count; num3++)
			{
				SinglePlayerPrefab.Loadout[] sLoadout = _sLoadout;
				foreach (SinglePlayerPrefab.Loadout loadout2 in sLoadout)
				{
					if (loadout2.name == Arsenal.WeaponAt(weaponList2._weaponIndices[num3]).FriendlyName)
					{
						array[num3] = loadout2.count;
					}
				}
				foreach (int item in list)
				{
					if (item == weaponList2._weaponIndices[num3])
					{
						array2[num3] = true;
						break;
					}
				}
			}
			weaponList2._ammo = new List<int>(array);
			weaponList2._isAdvanced = new List<bool>(array2);
		}
	}

	private bool HasWeaponType(Weapon.Type eWeaponType)
	{
		return _myArsenal[(int)eWeaponType]._weaponIndices.Count > 0;
	}

	internal void PickAWeapon(Weapon.Type eWeaponType)
	{
		if (IsBot)
		{
			WeaponList weaponList = _myArsenal[(int)eWeaponType];
			int index = weaponList.RandomIdx();
			Weapon weapon = Arsenal.WeaponAt(index);
			if (!weapon.name.ToLower().Contains("teleport"))
			{
				_p.SelectWeapon(index);
			}
			return;
		}
		float num = UnityEngine.Random.Range(0f, 100f);
		WeaponList weaponList2 = _myArsenal[(int)eWeaponType];
		int num2 = 0;
		Weapon currentWeapon = _p.CurrentWeapon;
		Weapon weapon2 = null;
		foreach (int weaponIndex in weaponList2._weaponIndices)
		{
			Weapon weapon3 = Arsenal.WeaponAt(weaponIndex);
			float num3 = (float)weapon3._percentChanceOfAIChoosing * 100f / weaponList2._chanceMod;
			if (num < num3 && num2 < _myArsenal[(int)eWeaponType]._isAdvanced.Count)
			{
				if (!_myArsenal[(int)eWeaponType]._isAdvanced[num2] && !_useAdvancedInventory)
				{
					_curWeaponIndexPos = num2;
					_curWeaponType = (int)eWeaponType;
					weapon2 = Arsenal.WeaponAt(weaponIndex);
					_p.SelectWeapon(weaponIndex);
					if (!IsBoss)
					{
						_simpleStates._currentWeaponIndex = weaponIndex;
					}
					else
					{
						_bossStates._currentWeaponIndex = weaponIndex;
					}
					break;
				}
				if (_useAdvancedInventory && _myArsenal[(int)eWeaponType]._isAdvanced[num2])
				{
					_curWeaponIndexPos = num2;
					_curWeaponType = (int)eWeaponType;
					weapon2 = Arsenal.WeaponAt(weaponIndex);
					_p.SelectWeapon(weaponIndex);
					if (!IsBoss)
					{
						_simpleStates._currentWeaponIndex = weaponIndex;
					}
					else
					{
						_bossStates._currentWeaponIndex = weaponIndex;
					}
					break;
				}
			}
			num2++;
			num -= num3;
		}
		if (weapon2 == null && !_myArsenal[(int)currentWeapon._eType]._weaponIndices.Contains(_simpleStates._currentWeaponIndex))
		{
			_useAdvancedInventory = !_useAdvancedInventory;
			PickAWeapon(eWeaponType);
		}
	}

	internal bool PickTeleport()
	{
		WeaponList weaponList = _myArsenal[0];
		weaponList._weaponIndices.Add(Arsenal.GetWeaponIndex("Teleport Grenade"));
		weaponList._ammo.Add(1);
		weaponList._isAdvanced.Add(_useAdvancedInventory);
		_p.SelectWeapon(Arsenal.GetWeaponIndex("Teleport Grenade"));
		_myArsenal[0] = weaponList;
		return true;
	}

	public FVector2 TargetingPosition(Player p)
	{
		return p.MyBody.Position + FVector2.UnitY * p.Height;
	}

	public Player BestMoveTarget(FVector2 fromPosition)
	{
		if (!onlyAttackPlayer)
		{
			Player result = null;
			float num = float.MaxValue;
			{
				foreach (Player player in LevelData.Instance.GetPlayers())
				{
					if (!player.IsDead() && player != _p)
					{
						float num2 = FVector2.DistanceSquared(fromPosition, TargetingPosition(player));
						if (num2 > 25f && num2 < num)
						{
							result = player;
						}
					}
				}
				return result;
			}
		}
		return LevelData.Instance.GetPlayer();
	}

	public Player ClosestTarget()
	{
		if (!onlyAttackPlayer)
		{
			float num = float.MaxValue;
			Player result = null;
			{
				foreach (Player player in LevelData.Instance.GetPlayers())
				{
					float num2 = FVector2.Distance(_myPositionF2, TargetingPosition(player));
					if (num2 < num && player != _p)
					{
						num = num2;
						result = player;
					}
				}
				return result;
			}
		}
		return LevelData.Instance.GetPlayer();
	}

	public Player BestFiringTarget(FVector2 fromPosition, out float theta, out float power)
	{
		if (!onlyAttackPlayer)
		{
			foreach (Player player3 in LevelData.Instance.GetPlayers())
			{
				if (CanHitPlayer(player3, fromPosition, out theta, out power))
				{
					return player3;
				}
			}
		}
		else
		{
			Player player2 = LevelData.Instance.GetPlayer();
			if (CanHitPlayer(player2, fromPosition, out theta, out power))
			{
				return player2;
			}
		}
		theta = 0f;
		power = 0f;
		return null;
	}

	public Player RandomTarget()
	{
		List<Player> list = new List<Player>();
		foreach (Player player in LevelData.Instance.GetPlayers())
		{
			if (player != _p)
			{
				list.Add(player);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count)];
	}

	public bool CanHitPlayer(Player p, FVector2 fromPosition, out float theta, out float power)
	{
		power = 0f;
		if (p != null && p != _p && !p.IsDead())
		{
			for (int i = 0; i < 10; i++)
			{
				if (ProjectileFiringSolution(_p.CurrentWeapon, power, out theta, fromPosition, TargetingPosition(p)))
				{
					return true;
				}
				power += 0.1f;
			}
		}
		theta = 0f;
		return false;
	}

	public void StatelessFireAtPlayer()
	{
		float waitPriorToFire = 2f;
		float waitAfterFire = 3f;
		StartCoroutine(DoStatelessFireAtPlayer(waitPriorToFire, waitAfterFire));
	}

	private IEnumerator DoStatelessFireAtPlayer(float waitPriorToFire, float waitAfterFire)
	{
		yield return new WaitForSeconds(waitPriorToFire);
		_p.EnableInput(true);
		base.Paused = false;
		SetState("StateNothing");
		_aiming = true;
		float power = 0f;
		_myPositionF2 = _p.MyBody.Position + FVector2.UnitY * _p.Height;
		_targetPlayer = LevelData.Instance.GetPlayer();
		if (!(_targetPlayer != null))
		{
			yield break;
		}
		for (int i = 0; i < 10; i++)
		{
			float theta;
			if (ProjectileFiringSolution(_p.CurrentWeapon, power, out theta, _myPositionF2, TargetingPosition(_targetPlayer)))
			{
				_firingDir.x = Mathf.Cos(theta);
				_firingDir.y = Mathf.Sin(theta);
				_firingMag = power;
				yield return new WaitForSeconds(1f);
				Fire = true;
				yield return new WaitForSeconds(waitAfterFire);
				base.Paused = true;
				break;
			}
			power += 0.1f;
		}
	}

	internal bool MeleeCanAtack()
	{
		MeleeWeapon component = _p.CurrentWeapon.weaponPrefab.GetComponent<MeleeWeapon>();
		float num = 10f;
		if (_targetPlayer == null)
		{
			return false;
		}
		if (component != null)
		{
			num = Mathf.Clamp(component._length, 4f, 10f);
		}
		return FVector2.Distance(_myPositionF2, TargetingPosition(_targetPlayer)) < num;
	}

	internal bool GetNearestCrate(out FVector2 target)
	{
		target = FVector2.Zero;
		float num = float.MaxValue;
		int num2 = 0;
		for (int i = 0; i < CrateManager.Instance._cratesInScene.Count; i++)
		{
			if (!(CrateManager.Instance._cratesInScene[i] != null))
			{
				continue;
			}
			float num3 = 0f;
			FVector2 fVector = FSHelper.Vector3ToFVector2(CrateManager.Instance._cratesInScene[i].transform.position);
			num3 = FVector2.Distance(_p.Position(), fVector);
			if (!CrateManager.Instance._cratesInScene[i].name.StartsWith("Chest"))
			{
				if (num3 < num)
				{
					num = num3;
					num2 = i;
					target = fVector;
				}
			}
			else if (_p.HasKey && num3 < num)
			{
				num = num3;
				num2 = i;
				target = fVector;
			}
		}
		if (num < 20f)
		{
			return true;
		}
		return false;
	}

	internal void RemoveEmptyWeapons()
	{
		for (int i = 0; i < Enum.GetNames(typeof(Weapon.Type)).Length; i++)
		{
			WeaponList weaponList = _myArsenal[i];
			int num = 0;
			foreach (int weaponIndex in weaponList._weaponIndices)
			{
				if (_myArsenal[i]._ammo.Count > 0)
				{
					if (weaponList._ammo[num] == 0)
					{
						_myArsenal[i]._weaponIndices.RemoveAt(num);
						_myArsenal[i]._ammo.RemoveAt(num);
						_myArsenal[i]._isAdvanced.RemoveAt(num);
						break;
					}
					num++;
					continue;
				}
				break;
			}
		}
	}

	internal IEnumerator GetNewWeaponFromArsenal()
	{
		CheckScoreThreshold();
		yield return new WaitForEndOfFrame();
		List<string> weaponTypeList = new List<string>();
		for (int i = 0; i < Enum.GetNames(typeof(Weapon.Type)).Length; i++)
		{
			WeaponList weaponList = _myArsenal[i];
			foreach (int e in weaponList._weaponIndices)
			{
				if (weaponTypeList.Count == 0)
				{
					weaponTypeList.Add(Arsenal.WeaponAt(e)._eType.ToString());
					continue;
				}
				foreach (string s in weaponTypeList)
				{
					if (s != Arsenal.WeaponAt(e)._eType.ToString())
					{
						weaponTypeList.Add(Arsenal.WeaponAt(e)._eType.ToString());
						break;
					}
				}
			}
		}
		if (weaponTypeList.Count != 0)
		{
			Weapon.Type weaponType = (Weapon.Type)(int)Enum.Parse(typeof(Weapon.Type), weaponTypeList[UnityEngine.Random.Range(0, weaponTypeList.Count)]);
			PickAWeapon(weaponType);
		}
		else
		{
			Debug.LogWarning(_p.PlayerName + " has no more ammo! giving him a new arsenal");
			OrderArsenal(_sLoadout);
		}
	}

	public float AddInnacuracyForShot(float theta)
	{
		float num = Innacuracy() * ((float)Math.PI / 180f);
		int scoreByPlayer = Scoring.Instance.GetScoreByPlayer(LevelData.Instance.GetPlayer().PlayerId);
		int scoreByPlayer2 = Scoring.Instance.GetScoreByPlayer(_p.PlayerId);
		float num2 = scoreByPlayer - scoreByPlayer2;
		float value = 1f - num2 / (float)_AdvancedInventoryThreshold;
		value = Mathf.Clamp(value, 0f, 1f);
		num *= value;
		num = UnityEngine.Random.Range(0f - num, num);
		theta += num;
		return theta;
	}

	private float Innacuracy()
	{
		if (StoryManager.Instance._division == 0)
		{
			return Tuning.Instance.MaxInaccuracyforBotsBronze;
		}
		if (StoryManager.Instance._division == 1)
		{
			return Tuning.Instance.MaxInaccuracyforBotsSilver;
		}
		if (StoryManager.Instance._division == 2)
		{
			return Tuning.Instance.MaxInaccuracyforBotsGold;
		}
		return 0f;
	}

	private void OnDrawGizmos()
	{
	}
}
