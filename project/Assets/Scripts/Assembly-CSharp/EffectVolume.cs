using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

public class EffectVolume : MonoBehaviour
{
	private const int _cBlobCount = 5;

	public float _modifier_speed = 1f;

	public float _modifier_gravity = 1f;

	public GameObject _blob;

	public GameObject _downBlob;

	public float _radius = 5f;

	public int _lifetimeSeconds = 5;

	public float _damageInterval = 3f;

	public float _damageMin = 10f;

	public float _damageMax = 30f;

	public PlayerEffects.Type _eVisualEffect;

	public int _points = 10;

	private int _originatingWeaponHash;

	private int _originatingWeaponLevel;

	private List<Player> _occupants = new List<Player>();

	private Body _body;

	private static List<GameObject> _activeEffectVolumes = new List<GameObject>();

	private Fixture _ignoreFixture;

	private FVector2 _dir;

	private ReplicationView _view;

	public Player Owner { get; set; }

	public RangeWeapon Parent
	{
		set
		{
			Body myBody = value.MyBody;
			if (myBody != null)
			{
				_ignoreFixture = myBody.FixtureList[0];
			}
		}
	}

	public Vector3 ImpactDir
	{
		set
		{
			_dir = FSHelper.Vector3ToFVector2(value);
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
	}

	private void OnSerializeView(BitStream stream)
	{
	}

	private void CreateBody()
	{
		_body = new Body(FSWorldComponent.PhysicsWorld);
		CircleShape circleShape = new CircleShape(_radius, 1f);
		circleShape.Position = FSHelper.Vector3ToFVector2(base.transform.position);
		_body.CreateFixture(circleShape);
		_body.BodyType = BodyType.Static;
		_body.CollidesWith = Category.None;
		_body.CollisionCategories = Category.None;
		_body.UserData = base.gameObject;
		_body.FixtureList[0].UserTag = "EV";
		_body.UserTag = "NoRay";
	}

	public static void ResetAll()
	{
		int num;
		for (num = 0; num < _activeEffectVolumes.Count; num++)
		{
			GameObject obj = _activeEffectVolumes[num];
			_activeEffectVolumes.RemoveAt(num);
			Object.Destroy(obj);
			num--;
		}
	}

	public void SetOriginatingWeaponInfo(int hash, int level)
	{
		_originatingWeaponHash = hash;
		_originatingWeaponLevel = level;
	}

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(0.2f);
		Manager.RegisterRPCs(this);
		CapsuleCollider cc = base.gameObject.AddComponent<CapsuleCollider>();
		cc.radius = _radius;
		cc.height = 60f;
		cc.direction = 2;
		cc.isTrigger = true;
		CreateBody();
		float dy = ((!(_dir.Y > 0f)) ? (-1f) : 1f);
		GameObject blobToSpawn = _blob;
		if (dy > 0f && _downBlob != null)
		{
			blobToSpawn = _downBlob;
		}
		Vector3 dir3D = Vector3.zero;
		dir3D.y = dy;
		Vector3 dir0 = Quaternion.AngleAxis(45f, Vector3.forward) * dir3D;
		Vector3 dir1 = Quaternion.AngleAxis(-45f, Vector3.forward) * dir3D;
		for (int i = 0; i < 5; i++)
		{
			Vector3 dir2 = Vector3.Slerp(dir0, dir1, (float)i / 5f);
			FVector2 dir2D = FSHelper.Vector3ToFVector2(dir2) * _radius * 2f;
			float sx = base.transform.position.x;
			float sy = base.transform.position.y - _radius * dy;
			FVector2 start = new FVector2(sx, sy);
			FVector2 hitPoint = FVector2.Zero;
			FVector2 hitNormal = FVector2.Zero;
			World world = FSWorldComponent.PhysicsWorld;
			bool hit = false;
			world.RayCast((Fixture fixture, FVector2 point, FVector2 normal, float fraction) =>
			{
				if (fixture != _ignoreFixture && fixture.Body.UserTag != "NoRay" && (fixture.CollisionCategories & Category.Cat2) != 0)
				{
					hit = true;
					hitPoint = point;
					hitNormal = normal;
					return fraction;
				}
				return -1f;
			}, start, start + dir2D);
			if (hit && !LevelData.Instance.PointInPolygons(hitPoint + hitNormal * 2f) && blobToSpawn != null)
			{
				Vector3 pos = hitPoint.ToVector3();
				pos.z = -10f;
				GameObject blorb = Object.Instantiate(blobToSpawn, pos, Quaternion.AngleAxis(Mathf.Atan2(0f - hitNormal.X, hitNormal.Y) * 57.29578f, Vector3.forward)) as GameObject;
				blorb.transform.parent = base.transform;
			}
		}
		if (!_view.isMine)
		{
			yield break;
		}
		Collider[] overlaps = Physics.OverlapSphere(base.transform.position, _radius);
		Collider[] array = overlaps;
		foreach (Collider overlapped in array)
		{
			if (overlapped.gameObject != base.gameObject && (bool)overlapped.GetComponent<EffectVolume>())
			{
				DestroyEffectVolume(overlapped.gameObject);
			}
		}
		StartCoroutine(ApplyPain());
		StartCoroutine(Destroy());
	}

	private IEnumerator Destroy()
	{
		yield return new WaitForSeconds(_lifetimeSeconds);
		Manager.Destroy(base.gameObject);
	}

	private void DestroyEffectVolume(GameObject effectVolumeObject)
	{
		_activeEffectVolumes.Remove(effectVolumeObject);
		Manager.Destroy(effectVolumeObject);
	}

	private void OnTriggerEnter(Collider other)
	{
		Player component = other.GetComponent<Player>();
		if (component != null)
		{
			BeginEffect(component);
			_occupants.Add(component);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		Player component = other.GetComponent<Player>();
		if (component != null && !_occupants.Contains(component))
		{
			BeginEffect(component);
			_occupants.Add(component);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Player component = other.GetComponent<Player>();
		if (component != null)
		{
			EndEffect(component);
			_occupants.Remove(component);
		}
	}

	private void OnEnable()
	{
		_activeEffectVolumes.Add(base.gameObject);
	}

	private void OnDisable()
	{
		foreach (Player occupant in _occupants)
		{
			if (occupant != null)
			{
				EndEffect(occupant);
			}
		}
		_occupants.Clear();
	}

	private void OnDestroy()
	{
		if (_body != null && FSWorldComponent.IsValid)
		{
			_body.Dispose();
		}
	}

	private void BeginEffect(Player p)
	{
		p.Modifier_Speed = _modifier_speed;
		p.Modifier_Gravity = _modifier_gravity;
		p.AddVisualEffect(_eVisualEffect);
	}

	private void EndEffect(Player p)
	{
		p.Modifier_Speed = 1f;
		p.Modifier_Gravity = 1f;
		p.RemoveVisualEffect(_eVisualEffect);
	}

	private IEnumerator ApplyPain()
	{
		Misc.HitInfo hitInfo = new Misc.HitInfo
		{
			_attackerID = _view.instantiatorId
		};
		while (true)
		{
			float damageApplied = Random.Range(_damageMin, _damageMax);
			hitInfo._damage = damageApplied;
			hitInfo._attackerID = Owner.PlayerId;
			hitInfo._points = (short)_points;
			hitInfo._weaponHash = _originatingWeaponHash;
			hitInfo._weaponLevel = _originatingWeaponLevel;
			if (hitInfo._points > 0)
			{
				float multiplier = ItemLevelManager.Instance.GetPointsMultiplier(_originatingWeaponHash, _originatingWeaponLevel);
				hitInfo._bonusPoints = (short)Mathf.RoundToInt((float)hitInfo._points * multiplier);
			}
			foreach (Player p in _occupants)
			{
				if (p != null)
				{
					ReplicationView view = p.GetComponent<ReplicationView>();
					if (view != null)
					{
						view.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
					}
				}
			}
			yield return new WaitForSeconds(_damageInterval);
		}
	}

	private void OnDrawGizmos()
	{
		foreach (Player occupant in _occupants)
		{
			Gizmos.DrawLine(base.collider.bounds.center, occupant.transform.position);
		}
		if (base.collider is BoxCollider)
		{
			Gizmos.DrawWireCube(base.transform.position, base.collider.bounds.size);
		}
		else if (base.collider is SphereCollider)
		{
			Gizmos.DrawWireSphere(base.transform.position, base.collider.bounds.extents.x);
		}
	}

	[RPC]
	private void ApplyHit(object hitInfo)
	{
		DestroyEffectVolume(base.gameObject);
	}
}
