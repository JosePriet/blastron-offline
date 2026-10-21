using System.Collections;
using EB;
using EB.Sequence;
using UnityEngine;

public class Health : MonoBehaviour
{
	public float hitPoints;

	public bool _autoDestroy = true;

	public GameObject _deathParticles;

	private float playerHitPoints;

	private float _baseHitPoints;

	private GameObject _healthBar;

	private ReplicationView _view;

	public float _selfDamageReductionPercentage = 50f;

	private float max_hp;

	private bool _amDead;

	private float _deferredToPostInitHealthPercent = -1f;

	private CharacterDB.Recipe _constantRecipe;

	public float _hitPoints
	{
		get
		{
			if (IsPlayer)
			{
				return playerHitPoints;
			}
			return hitPoints;
		}
		set
		{
			if (IsPlayer)
			{
				playerHitPoints = value;
			}
			else
			{
				hitPoints = value;
			}
		}
	}

	public float FractionalHealth
	{
		get
		{
			return _hitPoints / MaxHP;
		}
	}

	public float MaxHP
	{
		get
		{
			return max_hp;
		}
	}

	public bool IsDead
	{
		get
		{
			return _amDead;
		}
	}

	private bool IsPlayer { get; set; }

	private void Awake()
	{
		IsPlayer = GetComponent<Player>() != null;
		if (IsPlayer)
		{
			playerHitPoints = AttributesManager.instance._currentHealth;
		}
	}

	public void Initialize(CharacterDB.Recipe recipe, Hashtable attribs)
	{
		_constantRecipe = recipe;
		max_hp = _hitPoints;
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			int partHash = recipe.parts[i];
			float upgradeProgress = recipe.UpgradeProgress[i];
			max_hp += PartManager.Instance.GetPartAttributeValue(partHash, RobotAttribute.RobotAttributeType.Health, upgradeProgress);
		}
		int num = Dot.Integer("health", attribs, 0);
		if (num > 0)
		{
			max_hp = num;
		}
		_hitPoints = max_hp;
		_baseHitPoints = max_hp;
		if (_deferredToPostInitHealthPercent >= 0f)
		{
			SetBy(_deferredToPostInitHealthPercent);
		}
	}

	public void RegisterHealthbar(GameObject go)
	{
		_healthBar = go;
	}

	private void Start()
	{
		max_hp = (_baseHitPoints = _hitPoints);
		_view = GetComponent<ReplicationView>();
	}

	public void ApplyHit(Misc.HitInfo hitInfo)
	{
		if (_amDead)
		{
			return;
		}
		float num = ((_constantRecipe != null) ? PartManager.Instance.GetTotalAttributeValue(_constantRecipe, RobotAttribute.RobotAttributeType.Armor) : 0f);
		hitInfo._damage *= 1f - num * 0.01f;
		bool flag = _view.instantiatorId == hitInfo._attackerID;
		Misc.DamageInfo damageInfo = default(Misc.DamageInfo);
		damageInfo.attackerID = hitInfo._attackerID;
		if (flag)
		{
			float num2 = _selfDamageReductionPercentage / 100f;
			hitInfo._damage *= num2;
		}
		if (IsPlayer && !GetComponent<Player>().IsAI)
		{
			hitInfo._damage -= hitInfo._damage * AttributesManager.instance._currentArmor;
		}
		_hitPoints -= hitInfo._damage;
		damageInfo.dead = _hitPoints <= 0f;
		damageInfo.damage = hitInfo._damage;
		damageInfo.bullet = hitInfo._bullet;
		damageInfo.weaponHash = hitInfo._weaponHash;
		damageInfo.weaponLevel = hitInfo._weaponLevel;
		if (!_amDead)
		{
			base.gameObject.SendMessage("OnDamage", damageInfo, SendMessageOptions.RequireReceiver);
		}
		if (IsPlayer && hitInfo._damage > 0f)
		{
			if (_healthBar != null && _healthBar.active)
			{
				_healthBar.SendMessage("OnDamage", damageInfo);
			}
			if (_view == null)
			{
				_view = GetComponent<ReplicationView>();
			}
			if (hitInfo._attackerID != uint.MaxValue && !flag)
			{
				LootManager.Instance.SpawnLoot(FSHelper.Vector3ToFVector2(base.transform.position), ref hitInfo);
			}
			if (_view.isMine && damageInfo.dead)
			{
				Scoring.Instance.PlayerDied(_view.instantiatorId, damageInfo.attackerID);
			}
			_amDead = damageInfo.dead;
			EB.Sequence.Component.Activate(base.gameObject, _view.instantiatorId, typeof(SequenceEvent_DamageTaken));
		}
	}

	private void Update()
	{
		_hitPoints = Mathf.Min(_hitPoints, MaxHP);
	}

	public void ApplyHeal(float health)
	{
		_hitPoints = Mathf.Min(_hitPoints + health, MaxHP);
		if (_healthBar != null)
		{
			_healthBar.SendMessage("OnHeal", health);
		}
	}

	private void OnDamage(Misc.DamageInfo dmgInfo)
	{
		if (dmgInfo.dead)
		{
			if (_autoDestroy)
			{
				Object.Destroy(_healthBar);
				Object.Destroy(base.gameObject);
			}
			if (_deathParticles != null)
			{
				Object.Instantiate(_deathParticles, base.transform.position + Vector3.back * 10f, Quaternion.identity);
			}
			_amDead = true;
		}
	}

	public void OnDrown()
	{
		OnInstantDeath(false);
	}

	public void OnInstantDeath(bool playDeathParticles)
	{
		if (_autoDestroy)
		{
			Object.Destroy(base.gameObject);
		}
		_hitPoints = 0f;
		_amDead = true;
		if (playDeathParticles && _deathParticles != null)
		{
			Object.Instantiate(_deathParticles, base.transform.position + Vector3.back * 10f, Quaternion.identity);
		}
	}

	public void OnRespawn()
	{
		_hitPoints = _baseHitPoints;
		_amDead = false;
	}

	public void Serialize(BitStream stream)
	{
		if (IsPlayer)
		{
			stream.Serialize(ref playerHitPoints);
		}
		else
		{
			stream.Serialize(ref hitPoints);
		}
	}

	public void SetByPostInit(float percent)
	{
		_deferredToPostInitHealthPercent = percent;
	}

	public void SetBy(float percent)
	{
		_hitPoints = _baseHitPoints * percent * 0.01f;
	}

	private void EndGame()
	{
		if (_healthBar != null)
		{
			_healthBar.SendMessage("EndGame", SendMessageOptions.RequireReceiver);
		}
	}
}
