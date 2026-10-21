using System.Collections.Generic;
using EB.Replication;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using UnityEngine;

public class FSSpecial : MonoBehaviour
{
	public bool _initiallyAsleep = true;

	public bool _impactDamagesPlayer = true;

	public bool _isTargetable;

	private Body _body;

	public string UserTag;

	private List<Misc.DeferredHit> _hitQueue = new List<Misc.DeferredHit>();

	private float _impactAudioVelRange;

	private void Start()
	{
		FSBodyComponent component = GetComponent<FSBodyComponent>();
		if (component != null)
		{
			_body = component.PhysicsBody;
		}
		else
		{
			RangeWeapon component2 = GetComponent<RangeWeapon>();
			if (component2 != null)
			{
				_body = component2.MyBody;
			}
		}
		if (_body != null)
		{
			_body.UserData = base.gameObject;
			if (_initiallyAsleep)
			{
				_body.Awake = false;
			}
			_body.OnCollision += OnCollisionEvent;
			if (string.IsNullOrEmpty(UserTag))
			{
				if (base.tag == "FSStaticDestructible")
				{
					_body.UserTag = base.tag;
				}
				else
				{
					_body.UserTag = base.name;
				}
			}
			else
			{
				_body.UserTag = UserTag;
			}
			_body.CollisionCategories = Category.Cat2;
			_body.CollidesWith = Category.Cat1 | Category.Cat2;
			SendMessage("OnBodyReady", SendMessageOptions.DontRequireReceiver);
		}
		LevelData.Instance.RegisterHitReceiver(base.gameObject);
		if (_isTargetable)
		{
			AISimple._destructablePhysicsObjects.Add(base.gameObject);
		}
	}

	private void Update()
	{
		foreach (Misc.DeferredHit item in _hitQueue)
		{
			item._receiver.RPC("ApplyHit", RPCMode.AllBuffered, item._hitInfo);
		}
		_hitQueue.Clear();
	}

	private bool OnCollisionEvent(Fixture fixtureA, Fixture fixtureB, Contact contact)
	{
		View component = GetComponent<View>();
		if (component != null && component.isMine && _impactDamagesPlayer && fixtureB.UserTag == "Player")
		{
			GameObject gameObject = fixtureB.Body.UserData as GameObject;
			if (gameObject == null)
			{
				return true;
			}
			View component2 = gameObject.GetComponent<View>();
			FVector2 value = fixtureB.Body.Position - fixtureA.Body.Position;
			FVector2 linearVelocity = _body.LinearVelocity;
			if (FVector2.Dot(linearVelocity, value) > 0f)
			{
				float num = linearVelocity.Length();
				num -= Tuning.Instance.Body_MinDamageSpeed;
				if (num > 0f)
				{
					Misc.DeferredHit deferredHit = new Misc.DeferredHit();
					deferredHit._receiver = component2;
					deferredHit._hitInfo._damage = Mathf.Min(num, Tuning.Instance.Body_MaxDamageSpeed) * Tuning.Instance.Body_DamageMultiplier;
					deferredHit._hitInfo._attackerID = uint.MaxValue;
					deferredHit._hitInfo._attackerID = LevelData.CurrentPlayer.PlayerId;
					short num2 = 100;
					if (LevelData.CurrentPlayer == gameObject.GetComponent<Player>())
					{
						num2 = (short)(-num2);
					}
					deferredHit._hitInfo._points = num2;
					_hitQueue.Add(deferredHit);
				}
			}
		}
		return true;
	}

	private void DoApplyHit(object objectIn)
	{
		if (objectIn is Misc.HitInfo)
		{
			Misc.HitInfo hitInfo = (Misc.HitInfo)objectIn;
			_body.ApplyLinearImpulse(hitInfo.Twat);
			Health component = GetComponent<Health>();
			if (component != null)
			{
				component.ApplyHit(hitInfo);
			}
		}
	}

	private void OnDestroy()
	{
		if (_isTargetable)
		{
			AISimple._destructablePhysicsObjects.Remove(base.gameObject);
		}
		LevelData.Instance.UnregisterHitReceiver(base.gameObject);
	}
}
