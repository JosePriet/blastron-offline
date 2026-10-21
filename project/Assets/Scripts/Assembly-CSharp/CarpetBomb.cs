using System.Collections;
using System.Collections.Generic;
using EB;
using EB.Replication;
using UnityEngine;

public class CarpetBomb : MonoBehaviour
{
	public enum Dir
	{
		kleft = 0,
		kRight = 1,
		kRandom = 2
	}

	private struct BombEntry
	{
		public GameObject _bomb;

		public float _launchPower;
	}

	public GameObject _projectile;

	public int _numberOfDropPoints = 5;

	public float _deliverySpeedMax = 1f;

	public float _deliverySpeedMin;

	public Dir _dir;

	public float _intervalMin = 0.1f;

	public float _intervalMax = 1f;

	public bool _aimedEntryPoint;

	public float _spread = 10f;

	private int _inventoryHash = -1;

	private int _weaponLevel = -1;

	private List<BombEntry> _bombs = new List<BombEntry>();

	private void Awake()
	{
		Manager.RegisterPrefab(_projectile);
		WeaponMonitor.Instance.Add(this);
	}

	public void SetInventoryHash(int hash)
	{
		_inventoryHash = hash;
	}

	public void SetWeaponLevel(int level)
	{
		_weaponLevel = level;
	}

	public void Launch(Player playerOwner)
	{
		if (_projectile.GetComponent<BeamWeapon>() != null)
		{
			CreateBeam(playerOwner);
		}
		else
		{
			CreateBombs();
		}
		StartCoroutine(DoLaunch(playerOwner));
	}

	private void CreateBeam(Player playerOwner)
	{
		Vector3 position = base.transform.position;
		position.y += 75f;
		position.z = -30f;
		GameObject gameObject = (GameObject)Object.Instantiate(_projectile, position, Misc.DirectionToQuat(Vector3.down));
		BeamWeapon component = gameObject.GetComponent<BeamWeapon>();
		if (component != null)
		{
			component.SetInventoryHash(_inventoryHash);
			component.SetWeaponLevel(_weaponLevel);
			component.Fire(Vector3.down, playerOwner, null, null, null, false);
		}
	}

	private void CreateBombs()
	{
		CameraController component = Camera.mainCamera.GetComponent<CameraController>();
		float y = component.ViewLimitMax.y;
		float x = LevelData.Instance.GameplayMin.x;
		float x2 = LevelData.Instance.GameplayMax.x;
		float num;
		float num2;
		if (_aimedEntryPoint)
		{
			num = base.transform.position.x - _spread;
			num2 = base.transform.position.x + _spread;
		}
		else if (_dir == Dir.kleft)
		{
			num = x2;
			num2 = x;
		}
		else
		{
			num = x;
			num2 = x2;
		}
		float num3 = (num2 - num) / (float)(_numberOfDropPoints - 1);
		float num4 = num;
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < _numberOfDropPoints; i++)
		{
			switch (_dir)
			{
			case Dir.kleft:
				zero.x = -1f;
				break;
			case Dir.kRight:
				zero.x = 1f;
				break;
			case Dir.kRandom:
				zero.x = Random.Range(0, 100) % 3 - 1;
				break;
			}
			float num5 = Random.Range(_deliverySpeedMin, _deliverySpeedMax);
			zero.x *= num5;
			float x3 = num4 - num5 * 100f * zero.x;
			Quaternion rotation = ((!(zero == Vector3.zero)) ? Quaternion.LookRotation(zero) : Quaternion.identity);
			GameObject gameObject = Object.Instantiate(_projectile, new Vector3(x3, y, -10f), rotation) as GameObject;
			if (gameObject != null)
			{
				BombEntry item = new BombEntry
				{
					_bomb = gameObject,
					_launchPower = num5
				};
				_bombs.Add(item);
			}
			else
			{
				EB.Debug.LogError(string.Format("+++++ CARPET BOMB {0} COULD NOT INSTANTIATE {1}", base.name, _projectile));
			}
			num4 += num3;
		}
	}

	private IEnumerator DoLaunch(Player playerOwner)
	{
		foreach (BombEntry bombEntry in _bombs)
		{
			if (bombEntry._bomb != null)
			{
				RangeWeapon rangeWeapon = bombEntry._bomb.GetComponent<RangeWeapon>();
				if (rangeWeapon != null)
				{
					rangeWeapon.SetInventoryHash(_inventoryHash);
					rangeWeapon.SetWeaponLevel(_weaponLevel);
					rangeWeapon.CreateBody();
					rangeWeapon.Launch(bombEntry._launchPower, playerOwner);
				}
				float wait = Random.Range(_intervalMin, _intervalMax);
				if (wait > 0f)
				{
					yield return new WaitForSeconds(wait);
				}
			}
		}
		Object.Destroy(base.gameObject);
		playerOwner.FinishedFiring();
	}
}
