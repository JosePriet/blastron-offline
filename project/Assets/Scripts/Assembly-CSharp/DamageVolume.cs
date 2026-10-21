using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class DamageVolume : MonoBehaviour
{
	private struct PlayerEntry
	{
		public Player _p;

		public uint _attacker;

		public float _lastSeenTime;
	}

	private const float MAX_LAST_SEEN_TIME = 1f;

	public PlayerEffects.Type _eVisualEffect;

	public float _damageInterval = 3f;

	public float _damageMin = 10f;

	public float _damageMax = 30f;

	public bool _water;

	public bool _instantDeath = true;

	public GameObject _entryEffect;

	private List<PlayerEntry> _occupants = new List<PlayerEntry>();

	private ReplicationView _view;

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

	private void Start()
	{
		if (_view.isMine && !_water && !_instantDeath)
		{
			StartCoroutine(ApplyPain());
		}
	}

	private void OnSerializeView(BitStream stream)
	{
	}

	private void OnTriggerEnter(Collider other)
	{
		Player component = other.GetComponent<Player>();
		if (component != null)
		{
			BeginEffect(component);
			_occupants.Add(new PlayerEntry
			{
				_p = component,
				_attacker = component.PlayerId,
				_lastSeenTime = UnityEngine.Time.realtimeSinceStartup
			});
		}
		if (_entryEffect != null)
		{
			Object.Instantiate(_entryEffect, other.transform.position, Quaternion.identity);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		Player p = other.GetComponent<Player>();
		if (p != null && !_occupants.Exists((PlayerEntry pe) => pe._p == p))
		{
			BeginEffect(p);
			_occupants.Add(new PlayerEntry
			{
				_p = p,
				_attacker = p.PlayerId,
				_lastSeenTime = UnityEngine.Time.realtimeSinceStartup
			});
		}
	}

	private void OnTriggerExit(Collider other)
	{
		Player p = other.GetComponent<Player>();
		if (p != null)
		{
			EndEffect(p);
			_occupants.RemoveAll((PlayerEntry pe) => pe._p == p);
		}
	}

	private void BeginEffect(Player p)
	{
		if (_water)
		{
			p.BeginDrown();
		}
		else if (_instantDeath)
		{
			p.InstantDeath();
		}
		p.AddVisualEffect(_eVisualEffect);
	}

	private void EndEffect(Player p)
	{
		p.RemoveVisualEffect(_eVisualEffect);
	}

	private IEnumerator ApplyPain()
	{
		Misc.HitInfo hitInfo = default(Misc.HitInfo);
		while (true)
		{
			float damageApplied = Random.Range(_damageMin, _damageMax);
			hitInfo._damage = damageApplied;
			float timeNow = UnityEngine.Time.realtimeSinceStartup;
			_occupants.RemoveAll((PlayerEntry playerEntry) =>
			{
				float num = timeNow - playerEntry._lastSeenTime;
				bool flag = num >= 1f || playerEntry._p.IsDying || playerEntry._p.IsDead();
				if (flag)
				{
					EB.Debug.LogWarning("player has not been colliding against volume since " + num + " seconds, removing from occupancy list");
					EndEffect(playerEntry._p);
				}
				return flag;
			});
			for (int i = 0; i < _occupants.Count; i++)
			{
				PlayerEntry pe = _occupants[i];
				ReplicationView view = pe._p.GetComponent<ReplicationView>();
				if (view != null)
				{
					hitInfo._attackerID = pe._attacker;
					pe._lastSeenTime = timeNow;
					view.RPC("ApplyHit", RPCMode.AllBuffered, hitInfo);
				}
			}
			yield return new WaitForSeconds(_damageInterval);
		}
	}
}
