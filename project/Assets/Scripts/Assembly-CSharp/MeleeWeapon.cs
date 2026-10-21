using System.Collections;
using System.Collections.Generic;
using EB.Replication;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
	public enum MeleeType
	{
		OneShot = 0,
		DoT = 1
	}

	private const float _cMinWidth = 1f;

	private const float _cMaxWidth = 5f;

	public float _width = 5f;

	public float _lifetime = 5f;

	public float _damageInterval = 1f;

	public float _damagePerInterval = 10f;

	public float _length = 10f;

	public float _kbPower;

	public bool _constantShake;

	public float _shakeMagnitude = 1f;

	public int _shakeCycles = 3;

	public GameObject _spurty;

	public GameObject _thumpy;

	public float _effectorOffset = 1f;

	public bool _hitUp;

	public int _pointsPerInterval = 100;

	public MeleeType _meleeType;

	private int _weaponHash = -1;

	private int _weaponLevel = -1;

	private ReplicationView _view;

	private Player _owner;

	private string _animationName;

	private UnityEngine.Transform _parent;

	private Vector3 _effectorPos;

	private float _effectorAngle;

	private bool _hitsEnabled;

	private List<GameObject> _thingsToApplyDamageTo = new List<GameObject>();

	private Body _body;

	public static bool DebugOn { get; set; }

	public bool HitsEnabled
	{
		get
		{
			return _hitsEnabled;
		}
		set
		{
			if (!value && _hitsEnabled)
			{
				ApplyDamage();
			}
			_hitsEnabled = value;
		}
	}

	private void Awake()
	{
		_view = GetComponent<ReplicationView>();
		if (_view == null)
		{
			_view = base.gameObject.AddComponent<ReplicationView>();
		}
		_view.observed = this;
		_view.stateSynchronization = NetworkStateSynchronization.Off;
	}

	private void Start()
	{
		_width = Mathf.Clamp(_width, 1f, 5f);
		if (_spurty != null)
		{
			Manager.RegisterPrefab(_spurty);
		}
		if (_thumpy != null)
		{
			Manager.RegisterPrefab(_thumpy);
		}
		if (_view.isMine)
		{
			_body = new Body(FSWorldComponent.PhysicsWorld);
			_body.BodyType = BodyType.Kinematic;
			Vertices vertices = PolygonTools.CreateRectangle(_length / 2f, _width / 2f);
			float num = _length * 0.5f;
			for (int i = 0; i < vertices.Count; i++)
			{
				FVector2 value = vertices[i];
				value.X += num;
				vertices[i] = value;
			}
			_body.CreateFixture(new PolygonShape(vertices, 1f));
			_body.OnCollision += OnCollisionEvent;
			_body.CollisionCategories = Category.None;
			_body.CollidesWith = Category.Cat1;
			_body.UserTag = "NoRay";
			_body.Mass = 100f;
		}
	}

	private void OnDestroy()
	{
		if (_body != null)
		{
			_body.OnCollision -= OnCollisionEvent;
			_body.Dispose();
		}
		if (!string.IsNullOrEmpty(_animationName))
		{
			_owner.SendMessageUpwards("StopAnim", _animationName, SendMessageOptions.DontRequireReceiver);
		}
		Camera.mainCamera.SendMessage("EndRattle", SendMessageOptions.RequireReceiver);
		_owner.FinishedFiring();
	}

	public void SetInventoryHash(int hash)
	{
		_weaponHash = hash;
	}

	public void SetWeaponLevel(int level)
	{
		_weaponLevel = level;
	}

	public void Attack(Player owner, string animationName, UnityEngine.Transform parent)
	{
		_parent = parent;
		_owner = owner;
		Object.Destroy(base.gameObject, _lifetime);
		if (_meleeType == MeleeType.DoT)
		{
			StartCoroutine(ApplyDamageInterval());
		}
		_animationName = animationName;
		if (_animationName != null)
		{
			owner.PlayAnim(_animationName);
		}
		if (_constantShake)
		{
			Camera.mainCamera.SendMessage("BeginRattle", new CameraShake.ShakeInfo(_shakeMagnitude, _shakeCycles), SendMessageOptions.RequireReceiver);
		}
	}

	private IEnumerator ApplyDamageInterval()
	{
		while (true)
		{
			ApplyDamage();
			_thingsToApplyDamageTo.Clear();
			yield return new WaitForSeconds(_damageInterval);
		}
	}

	public void SpawnHitEffect()
	{
		if (_thumpy != null)
		{
			Manager.Instantiate(_thumpy, _effectorPos, Quaternion.identity);
		}
	}

	private void LateUpdate()
	{
		EffectorPosAngle();
		base.transform.position = _effectorPos;
		base.transform.rotation = Quaternion.AngleAxis(_effectorAngle * 57.29578f, Vector3.forward);
		if (_body != null)
		{
			_body.Position = FSHelper.Vector3ToFVector2(_effectorPos);
			_body.Rotation = _effectorAngle;
		}
	}

	private void EffectorPosAngle()
	{
		if (_parent != null)
		{
			Vector3 vector = _parent.transform.localToWorldMatrix.MultiplyVector(Vector3.right);
			vector.Normalize();
			_effectorPos = _parent.transform.position + vector * _effectorOffset;
			_effectorAngle = Mathf.Atan2(vector.y, vector.x);
		}
	}

	private bool OnCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		if ((HitsEnabled || _meleeType == MeleeType.DoT) && fixtureB.Body.UserTag == "Player")
		{
			GameObject gameObject = fixtureB.Body.UserData as GameObject;
			if (gameObject != null && gameObject != _owner.gameObject && !_thingsToApplyDamageTo.Contains(gameObject))
			{
				_thingsToApplyDamageTo.Add(gameObject);
			}
		}
		return false;
	}

	private void ApplyDamage()
	{
		foreach (GameObject item in _thingsToApplyDamageTo)
		{
			Vector3 input = item.transform.position - _owner.transform.position;
			input.Normalize();
			View component = item.GetComponent<View>();
			Misc.HitInfo hitInfo = new Misc.HitInfo
			{
				_damage = _damagePerInterval,
				_dir = ((!_hitUp) ? FSHelper.Vector3ToFVector2(input) : FVector2.UnitY),
				_mag = _kbPower,
				_attackerID = _view.instantiatorId,
				_points = (short)_pointsPerInterval,
				_weaponLevel = _weaponLevel,
				_weaponHash = _weaponHash
			};
			if (hitInfo._points > 0)
			{
				float pointsMultiplier = ItemLevelManager.Instance.GetPointsMultiplier(_weaponHash, _weaponLevel);
				hitInfo._bonusPoints = (short)Mathf.RoundToInt((float)hitInfo._points * pointsMultiplier);
			}
			_owner.ProcessOutgoingHit(ref hitInfo);
			component.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
		}
		if (_thingsToApplyDamageTo.Count > 0 && _spurty != null)
		{
			Manager.Instantiate(_spurty, _thingsToApplyDamageTo[0].transform.position, Quaternion.identity);
		}
		if (_meleeType == MeleeType.OneShot)
		{
			_thingsToApplyDamageTo.Clear();
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		Gizmos.DrawLine(base.transform.position, base.transform.position + localToWorldMatrix.MultiplyVector(Vector3.right) * 10f);
	}
}
