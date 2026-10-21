using System.Collections;
using System.Collections.Generic;
using EB.Replication;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

public class BeamWeapon : MonoBehaviour
{
	private const float _cMinWidth = 1f;

	private const float _cMaxWidth = 30f;

	public float _width = 5f;

	public float _lifetime = 5f;

	public float _damagePerInterval = 10f;

	public bool _bullet;

	private float _length = 200f;

	public int _pointsPerInterval = 100;

	public float _damageInterval = 0.2f;

	public GameObject _muzzleFlashObject;

	public Vector2 _muzzleFlashOffset;

	private GameObject _muzzleFlashInstance;

	public GameObject _impactEffect;

	public float _shakeMagnitude = 0.2f;

	public int _shakeCycles = 3;

	private Transform _muzzleFlashParent;

	private bool _stationary = true;

	private int _weaponHash = -1;

	private int _weaponLevel = -1;

	private ReplicationView _view;

	private string _animationName;

	private Transform _parent;

	private bool _detachEffect;

	private Player _owner;

	private GameObject _emitter;

	private Vector3 _firePos;

	private Vector3 _fireDir;

	private void Awake()
	{
		_view = GetComponent<ReplicationView>();
		if (_view == null)
		{
			_view = base.gameObject.AddComponent<ReplicationView>();
		}
		_view.observed = this;
		_view.stateSynchronization = NetworkStateSynchronization.Off;
		if (_impactEffect != null)
		{
			Manager.RegisterPrefab(_impactEffect);
		}
	}

	private void Start()
	{
		_width = Mathf.Clamp(_width, 1f, 30f);
		Transform transform = base.transform.FindChild("beam");
		if (_lifetime > 0f)
		{
			Object.Destroy(base.gameObject, _lifetime);
		}
		if (transform != null)
		{
			transform.localScale = new Vector3(_length, _width, 1f);
			if (!_bullet)
			{
				Vector2 one = Vector2.one;
				one.x = _length / ((float)transform.renderer.material.mainTexture.width * 0.1f);
				transform.renderer.material.mainTextureScale = one;
			}
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

	public void Fire(Vector3 dir, Player owner, string animationName, Transform parent, GameObject emitter, bool createReplicatedSourceFlash)
	{
		_animationName = animationName;
		_owner = owner;
		_parent = parent;
		_emitter = emitter;
		if (_stationary)
		{
			if (_parent != null)
			{
				_firePos = _parent.transform.position;
				_fireDir = dir;
			}
			else
			{
				_firePos = base.transform.position;
				_fireDir = base.transform.rotation * Vector3.right;
			}
		}
		StartCoroutine(ApplyDamage());
	}

	private void EmissionPosRot(out Vector3 pos, out Vector3 dir)
	{
		if (_stationary)
		{
			pos = _firePos;
			dir = _fireDir;
		}
		else if (_parent != null)
		{
			Quaternion rot;
			TransformToPosRot(_parent, out pos, out rot, Vector2.zero);
			dir = rot * Vector3.right;
		}
		else
		{
			pos = Vector3.zero;
			dir = Vector3.right;
		}
	}

	private void OnDestroy()
	{
		if (_muzzleFlashInstance != null)
		{
			Object.Destroy(_muzzleFlashInstance);
		}
		if (_owner != null)
		{
			if (!string.IsNullOrEmpty(_animationName))
			{
				_owner.SendMessageUpwards("StopAnim", _animationName, SendMessageOptions.DontRequireReceiver);
			}
			_owner.FinishedFiring();
		}
	}

	private IEnumerator ApplyDamage()
	{
		while (true)
		{
			if (_animationName != null && _owner != null)
			{
				_owner.PlayAnim(_animationName);
			}
			Camera.mainCamera.SendMessage("Shake", new CameraShake.ShakeInfo(_shakeMagnitude, _shakeCycles));
			if (_bullet)
			{
				DoApplyBulletDamage();
			}
			else
			{
				DoApplyDamage();
			}
			yield return new WaitForSeconds(_damageInterval);
		}
	}

	private void DoApplyDamage()
	{
		List<GameObject> hitItems = new List<GameObject>();
		Vector3 pos;
		Vector3 dir;
		EmissionPosRot(out pos, out dir);
		FVector2 origin = FSHelper.Vector3ToFVector2(pos);
		FVector2 ray = FSHelper.Vector3ToFVector2(dir);
		origin -= ray * 2f;
		Misc.HitInfo hitInfo = new Misc.HitInfo
		{
			_damage = _damagePerInterval,
			_attackerID = _view.instantiatorId,
			_pos = origin,
			_points = (short)_pointsPerInterval,
			_dir = ray,
			_bullet = true,
			_weaponHash = _weaponHash,
			_weaponLevel = _weaponLevel
		};
		if (hitInfo._points > 0)
		{
			float pointsMultiplier = ItemLevelManager.Instance.GetPointsMultiplier(_weaponHash, _weaponLevel);
			hitInfo._bonusPoints = (short)Mathf.RoundToInt((float)hitInfo._points * pointsMultiplier);
		}
		LevelData.Instance.FatRayGameObjectQuery((GameObject hitObject) =>
		{
			if (hitObject != _emitter)
			{
				hitItems.Add(hitObject);
			}
		}, _width * 0.5f, ref origin, ref ray, 3);
		if (!_view.isMine)
		{
			return;
		}
		if (_owner != null)
		{
			_owner.ProcessOutgoingHit(ref hitInfo);
		}
		foreach (GameObject item in hitItems)
		{
			ReplicationView component = item.GetComponent<ReplicationView>();
			if (component != null)
			{
				component.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
			}
		}
	}

	private void DoApplyBulletDamage()
	{
		if (_owner == null)
		{
			return;
		}
		Vector3 pos;
		Quaternion rot;
		TransformToPosRot(_parent, out pos, out rot, Vector2.zero);
		FVector2 fVector = FSHelper.Vector3ToFVector2(pos);
		FVector2 fVector2 = FSHelper.Vector3ToFVector2(rot * Vector3.right);
		fVector -= fVector2 * 2f;
		Misc.HitInfo hitInfo = new Misc.HitInfo
		{
			_damage = _damagePerInterval,
			_attackerID = _view.instantiatorId,
			_pos = fVector,
			_points = (short)_pointsPerInterval,
			_dir = fVector2,
			_bullet = true,
			_weaponHash = _weaponHash,
			_weaponLevel = _weaponLevel
		};
		if (hitInfo._points > 0)
		{
			float pointsMultiplier = ItemLevelManager.Instance.GetPointsMultiplier(_weaponHash, _weaponLevel);
			hitInfo._bonusPoints = (short)Mathf.RoundToInt((float)hitInfo._points * pointsMultiplier);
		}
		FVector2 hitPoint;
		FVector2 hitNormal;
		Fixture hitFixture;
		if (!Misc.RayCastForClosest(fVector, fVector2 * _length, out hitPoint, out hitNormal, out hitFixture, _owner.MyBody))
		{
			return;
		}
		if (hitFixture.Body.UserTag == "Player" && _view.isMine)
		{
			GameObject gameObject = hitFixture.Body.UserData as GameObject;
			ReplicationView component = gameObject.GetComponent<ReplicationView>();
			if (component != null)
			{
				if (_owner != null)
				{
					_owner.ProcessOutgoingHit(ref hitInfo);
				}
				component.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
			}
		}
		if ((bool)_impactEffect)
		{
			Manager.Instantiate(_impactEffect, hitPoint.ToVector3(), Quaternion.identity);
		}
	}

	private float AccumulateHits(FVector2 pos, FVector2 dir, float length, List<GameObject> hits)
	{
		GameObject hit = null;
		FSWorldComponent.PhysicsWorld.RayCast((Fixture fixture, FVector2 point, FVector2 normal, float fraction) =>
		{
			object userData = fixture.Body.UserData;
			if (userData != null && userData is GameObject)
			{
				hit = userData as GameObject;
				if (!hits.Contains(hit))
				{
					hits.Add(hit);
				}
			}
			return -1f;
		}, pos, pos + dir * length);
		return length;
	}

	public void CreateMuzzleFlash(Transform trans, bool detach, bool createReplicated)
	{
		_detachEffect = detach;
		if (_muzzleFlashObject != null && DebugSettings.MuzzleFlashOn)
		{
			_muzzleFlashParent = trans;
			Vector3 pos;
			Quaternion rot;
			TransformToPosRot(_muzzleFlashParent, out pos, out rot, _muzzleFlashOffset);
			if (createReplicated)
			{
				_muzzleFlashInstance = Manager.Instantiate(_muzzleFlashObject, pos, rot) as GameObject;
			}
			else
			{
				_muzzleFlashInstance = Object.Instantiate(_muzzleFlashObject, pos, rot) as GameObject;
			}
		}
	}

	private void LateUpdate()
	{
		if (_muzzleFlashInstance != null && !_detachEffect)
		{
			Vector3 pos;
			Quaternion rot;
			TransformToPosRot(_muzzleFlashParent, out pos, out rot, _muzzleFlashOffset);
			_muzzleFlashInstance.transform.position = pos;
			_muzzleFlashInstance.transform.rotation = rot;
		}
	}

	private void TransformToPosRot(Transform trans, out Vector3 pos, out Quaternion rot, Vector2 offset)
	{
		Matrix4x4 localToWorldMatrix = trans.localToWorldMatrix;
		Vector3 vector = localToWorldMatrix.MultiplyVector(Vector3.right);
		Vector3 vector2 = localToWorldMatrix.MultiplyVector(Vector3.up);
		vector2.y = Mathf.Abs(vector2.y);
		vector.Normalize();
		vector2.Normalize();
		pos = trans.position + vector * offset.x + vector2 * offset.y;
		rot = Misc.DirectionToQuat(vector);
	}
}
