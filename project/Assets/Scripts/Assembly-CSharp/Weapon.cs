using System.Collections.Generic;
using EB;
using FarseerPhysics.Dynamics;
using SmoothMoves;
using UnityEngine;

public class Weapon : InventoryItem
{
	public enum Type
	{
		kProjectile = 0,
		kBeam = 1,
		kCarpetBomb = 2,
		kMelee = 3
	}

	public enum FiringAnimation
	{
		kShoot = 0,
		kThrow = 1,
		kDrop = 2,
		kRapidShoot = 3,
		kHack = 4,
		kHackFast = 5,
		kStab = 6,
		kUppercut = 7,
		kNone = 8
	}

	public GameObject weaponPrefab;

	public Type _eType;

	public bool _postActivationAim;

	public string spriteName;

	public TextureAtlas spriteAtlas;

	public Vector2 proxyOffset;

	public Vector2 proxyScale;

	public Vector2 deviceOffset = new Vector2(0.5f, 0.5f);

	public Material proxyMaterial;

	public bool _endsTurn = true;

	public bool _isPlaced;

	public GameObject _aimEffect;

	public int _percentChanceOfAIChoosing = -1;

	public float _fireImpulse;

	public FiringAnimation _eFiringAnimation;

	private GameObject _aimEffectInstance;

	private static string[] _firingAnimationNames = new string[9] { "bot_shoot", "bot_throw", "bot_drop", "bot_chainsaw", "bot_hack", "bot_hack_fast", "bot_stab", "bot_uppercut", null };

	private Dictionary<Transform, GameObject> _projectiles;

	private GameObject currentProxy;

	private GameObject _weaponObject;

	public string animationName
	{
		get
		{
			return _firingAnimationNames[(int)_eFiringAnimation];
		}
	}

	public override Category ItemCategory
	{
		get
		{
			return Category.Weapon;
		}
	}

	public GameObject _currentProxy
	{
		get
		{
			return currentProxy;
		}
		private set
		{
			currentProxy = value;
		}
	}

	public float GravityScale
	{
		get
		{
			if (weaponPrefab != null)
			{
				RangeWeapon component = weaponPrefab.GetComponent<RangeWeapon>();
				if (component != null)
				{
					return component._gravityScale;
				}
			}
			return 1f;
		}
	}

	public override bool IsValid()
	{
		return true;
	}

	private void Awake()
	{
		category = Category.Weapon;
		Init();
	}

	public void Init()
	{
		if (_projectiles == null)
		{
			_projectiles = new Dictionary<Transform, GameObject>();
		}
		if (weaponPrefab == null)
		{
			UnityEngine.Debug.LogError("There is no weapon type attached to the inventory item : " + base.name);
		}
	}

	public void ShowProjectile(Transform parent, bool show)
	{
		GameObject value;
		if (_projectiles.TryGetValue(parent, out value))
		{
			value.SetActive(show);
		}
	}

	public void CreateProjectileProxy(Transform parent)
	{
		if (proxyMaterial != null && !_projectiles.ContainsKey(parent))
		{
			Mesh mesh = Misc.MakeQuadMesh(-0.5f, -0.5f, 0.5f, 0.5f);
			GameObject gameObject = new GameObject(base.name + "_proxy");
			MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
			meshFilter.mesh = mesh;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.material = proxyMaterial;
			gameObject.transform.parent = parent;
			Vector3 localPosition = new Vector3(proxyOffset.x, proxyOffset.y, -1f);
			gameObject.transform.localPosition = localPosition;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = new Vector3(proxyScale.x, proxyScale.y, 1f);
			currentProxy = gameObject;
			_projectiles[parent] = gameObject;
		}
	}

	public void DestroyProjectileProxy(Transform parent)
	{
		GameObject value;
		if (_projectiles.TryGetValue(parent, out value))
		{
			currentProxy = null;
			_projectiles.Remove(parent);
			Object.Destroy(value);
		}
	}

	public bool Fire(Vector3 direction, float power, Player owner, Transform parent, int weaponLevel)
	{
		bool result = true;
		if (weaponPrefab != null)
		{
			switch (_eType)
			{
			case Type.kProjectile:
			{
				RangeWeapon component2 = weaponPrefab.GetComponent<RangeWeapon>();
				if (component2 == null)
				{
					EB.Debug.LogError(string.Format("The weapon prefab {0} although marked as 'projectile', does not have RangeWeapon script. Is this meant to be a beam weapon?", base.name));
					return false;
				}
				if (component2._shotCount > 1)
				{
					RangeWeapon[] array = new RangeWeapon[component2._shotCount];
					for (int i = 0; i < component2._shotCount; i++)
					{
						Vector3 position = parent.transform.position;
						position.z = -10f;
						GameObject gameObject = Object.Instantiate(weaponPrefab, position, Quaternion.identity) as GameObject;
						array[i] = gameObject.GetComponent<RangeWeapon>();
						array[i].SetInventoryHash(Hash.StringHash(base.name));
						array[i].SetWeaponLevel(weaponLevel);
						if (array[i]._shotInterval > 0f && i > 0)
						{
							gameObject.SetActive(false);
						}
					}
					Firing.Instance.FireMulti(array, direction, owner, power, parent, FarseerPhysics.Dynamics.Category.Cat5);
				}
				else
				{
					Vector3 position2 = parent.transform.position;
					position2.z = -10f;
					GameObject gameObject2 = Object.Instantiate(weaponPrefab, position2, Quaternion.identity) as GameObject;
					RangeWeapon component3 = gameObject2.GetComponent<RangeWeapon>();
					component3.SetInventoryHash(Hash.StringHash(base.name));
					component3.SetWeaponLevel(weaponLevel);
					Firing.Instance.FireSingle(component3, direction, owner, power, parent, FarseerPhysics.Dynamics.Category.Cat5, _isPlaced, animationName);
					owner.FinishedFiring();
				}
				break;
			}
			case Type.kBeam:
			{
				Vector3 position3 = parent.transform.position;
				position3.z = -30f;
				_weaponObject = (GameObject)Object.Instantiate(weaponPrefab, position3, Misc.DirectionToQuat(direction));
				BeamWeapon component4 = _weaponObject.GetComponent<BeamWeapon>();
				if (component4 != null)
				{
					component4.SetInventoryHash(Hash.StringHash(base.name));
					if (parent != null)
					{
						component4.CreateMuzzleFlash(parent, !_postActivationAim, false);
					}
					component4.SetWeaponLevel(weaponLevel);
					component4.Fire(direction, owner, animationName, parent, null, false);
				}
				break;
			}
			case Type.kCarpetBomb:
			{
				Vector3 min;
				Vector3 max;
				Misc.CameraViewLimits(Camera.mainCamera, out min, out max);
				float value = parent.transform.position.x + direction.x * Mathf.Abs((max.y - parent.transform.position.y) / direction.y);
				value = Mathf.Clamp(value, min.x, max.x);
				GameObject gameObject3 = Object.Instantiate(weaponPrefab, Misc.TopOfScreenIntersectionWorld(owner.transform.position, direction), Quaternion.identity) as GameObject;
				CarpetBomb component5 = gameObject3.GetComponent<CarpetBomb>();
				component5.Launch(owner);
				component5.SetInventoryHash(Hash.StringHash(base.name));
				component5.SetWeaponLevel(weaponLevel);
				break;
			}
			case Type.kMelee:
			{
				_weaponObject = (GameObject)Object.Instantiate(weaponPrefab, parent.transform.position, Misc.DirectionToQuat(direction));
				_weaponObject.name = "melee_weapon";
				MeleeWeapon component = _weaponObject.GetComponent<MeleeWeapon>();
				if (component != null)
				{
					component.SetInventoryHash(Hash.StringHash(base.name));
					component.SetWeaponLevel(weaponLevel);
					component.Attack(owner, animationName, parent);
				}
				result = false;
				break;
			}
			}
		}
		WeaponMonitor.Instance.Fire();
		return result;
	}

	public void FireStop()
	{
		if (_eType == Type.kBeam && _weaponObject != null)
		{
			_weaponObject.SendMessage("Stop");
		}
		_weaponObject = null;
	}

	public float ProjectileSpeed(float parametricPower)
	{
		if (weaponPrefab != null)
		{
			RangeWeapon component = weaponPrefab.GetComponent<RangeWeapon>();
			if (component != null)
			{
				return component.PowerToSpeed(parametricPower);
			}
		}
		return 0f;
	}

	public void OnHitWindowOpen()
	{
		if (_weaponObject != null && _eType == Type.kMelee)
		{
			MeleeWeapon component = _weaponObject.GetComponent<MeleeWeapon>();
			component.HitsEnabled = true;
		}
	}

	public void OnHitWindowClose()
	{
		if (_weaponObject != null && _eType == Type.kMelee)
		{
			MeleeWeapon component = _weaponObject.GetComponent<MeleeWeapon>();
			component.HitsEnabled = false;
		}
	}

	public void OnHitEvent(Transform root, float angleDeg)
	{
		if (_weaponObject != null && _eType == Type.kMelee)
		{
			MeleeWeapon component = _weaponObject.GetComponent<MeleeWeapon>();
			component.SpawnHitEffect();
			Camera.mainCamera.SendMessage("Shake", new CameraShake.ShakeInfo(component._shakeMagnitude, component._shakeCycles), SendMessageOptions.RequireReceiver);
		}
	}

	public void OnStartSwing()
	{
		if (_weaponObject != null && _eType == Type.kMelee)
		{
			AudioEmitter component = _weaponObject.GetComponent<AudioEmitter>();
			if (component != null)
			{
				component.Play();
			}
		}
	}

	public void OnBeginAim(Transform parent)
	{
		if (_aimEffect != null && _aimEffectInstance == null)
		{
			_aimEffectInstance = Object.Instantiate(_aimEffect) as GameObject;
			_aimEffectInstance.transform.parent = parent;
			_aimEffectInstance.transform.localPosition = Vector3.zero;
		}
	}

	public void OnEndAim()
	{
		if (_aimEffectInstance != null)
		{
			Object.Destroy(_aimEffectInstance);
		}
	}
}
