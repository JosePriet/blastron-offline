using System.Collections;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using Microsoft.Xna.Framework;
using UnityEngine;

public class Loot : MonoBehaviour
{
	public enum Type
	{
		kRivets = 0,
		kXp = 1
	}

	private enum Phase
	{
		kEmission = 0,
		kSucking = 1
	}

	private delegate void Fn();

	public Type LootType;

	private GameObject _suckTo;

	private Phase _ePhase;

	private Body _body;

	public AudioEvent _eventBounce;

	private Vector3 _start;

	private float _t;

	private float _tstep;

	private AudioEmitter _audioEmitter;

	public int Value { get; set; }

	private void Awake()
	{
		_body = Misc.DiscoverBody(base.gameObject);
		base.gameObject.SendMessage("SetZ", -20f);
		base.gameObject.SetActive(false);
		if (_body != null)
		{
			_body.CollisionCategories = Category.Cat10;
			_body.CollidesWith = Category.Cat2;
			_body.UserTag = "NoRay";
			_body.Enabled = false;
		}
		_audioEmitter = GetComponent<AudioEmitter>();
	}

	private bool OnCollisionEvent(Fixture f1, Fixture f2, Contact c)
	{
		_audioEmitter.Play(_eventBounce);
		return true;
	}

	private void Spawn()
	{
		StopAllCoroutines();
		_body.OnCollision += OnCollisionEvent;
		FVector2 linearVelocity = FSHelper.Vector2ToFVector2(Random.insideUnitCircle) * 40f;
		linearVelocity.Y = Mathf.Abs(linearVelocity.Y);
		_body.LinearVelocity = linearVelocity;
		_body.Position = FSHelper.Vector3ToFVector2(base.transform.position);
		_body.Enabled = true;
		GetComponent<SimplePhysicsObject>().enabled = true;
		StartCoroutine(CallAfter(Tuning.Instance.Loot_PreSuckPeriod, StartSuck));
	}

	public void SuckTo(uint suckToPlayerID)
	{
		_suckTo = LevelData.Instance.GetPlayer(suckToPlayerID).gameObject;
		Spawn();
	}

	public void Stop()
	{
		StopAllCoroutines();
		Reset();
	}

	private void OnDrawGizmos()
	{
		if (_suckTo != null)
		{
			Gizmos.DrawCube(_suckTo.transform.position, Vector3.one);
		}
	}

	private IEnumerator CallAfter(float interval, Fn fn)
	{
		yield return new WaitForSeconds(interval);
		fn();
	}

	private void StartSuck()
	{
		_ePhase = Phase.kSucking;
		_start = base.transform.position;
		_t = 0f;
		_tstep = 1f;
		Reset();
	}

	private void Update()
	{
		Phase ePhase = _ePhase;
		if (ePhase != Phase.kSucking)
		{
			return;
		}
		_t += _tstep * Time.smoothDeltaTime;
		_tstep *= 1f + Time.smoothDeltaTime;
		if (_suckTo == null)
		{
			Reset();
			return;
		}
		base.transform.position = Vector3.Lerp(_start, _suckTo.transform.position, _t);
		if (_t > 1f)
		{
			switch (LootType)
			{
			case Type.kRivets:
				_suckTo.SendMessage("AddLootRivets", Value);
				break;
			case Type.kXp:
				_suckTo.SendMessage("AddLootXp", Value);
				break;
			}
			Reset();
			_ePhase = Phase.kEmission;
		}
	}

	public void Reset()
	{
		_body.ResetDynamics();
		_body.Enabled = false;
		GetComponent<SimplePhysicsObject>().enabled = false;
		_body.OnCollision -= OnCollisionEvent;
		StopAllCoroutines();
		base.gameObject.SetActive(false);
	}
}
