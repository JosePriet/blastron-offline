using System.Collections;
using FarseerPhysics.Dynamics;
using UnityEngine;

public class Firing : MonoBehaviour
{
	public static Firing Instance { get; private set; }

	public void Awake()
	{
		Instance = this;
	}

	public void FireSingle(RangeWeapon projectile, Vector3 direction, Player owner, float power, Transform parent, Category eCCat, bool place, string fireAnim)
	{
		owner.PlayAnim(fireAnim);
		projectile.transform.rotation = Quaternion.LookRotation(direction);
		ParticleSystem componentInChildren = projectile.GetComponentInChildren<ParticleSystem>();
		if (componentInChildren != null)
		{
			componentInChildren.Play();
		}
		RangeWeapon component = projectile.GetComponent<RangeWeapon>();
		if (!(component != null))
		{
			return;
		}
		if (parent != null && !Misc.IsSlow())
		{
			component.CreateMuzzleFlash(parent);
		}
		component.CreateBody();
		Body myBody = component.MyBody;
		myBody.CollisionCategories = eCCat;
		if (place)
		{
			if (direction.x < 0f)
			{
				direction *= -1f;
			}
			component.Release(owner, Mathf.Atan2(direction.y, direction.x));
		}
		else
		{
			component.Launch(power, owner);
		}
	}

	public void FireMulti(RangeWeapon[] projectiles, Vector3 direction, Player owner, float power, Transform parent, Category eCCat)
	{
		StartCoroutine(DoFireMulti(projectiles, direction, owner, power, parent, eCCat));
	}

	public IEnumerator DoFireMulti(RangeWeapon[] projectiles, Vector3 direction, Player owner, float power, Transform parent, Category eCCat)
	{
		RangeWeapon rangeWeaponTemplate = projectiles[0].GetComponent<RangeWeapon>();
		int shots = Mathf.Clamp(rangeWeaponTemplate._shotCount, 1, 8);
		bool spreadShot = rangeWeaponTemplate._shotInterval < 0.1f;
		float firingArc = Mathf.Clamp(rangeWeaponTemplate._firingArc, 0f, (!spreadShot) ? 0f : 90f);
		Vector3 dir0 = Quaternion.AngleAxis(firingArc * 0.5f, Vector3.forward) * direction;
		Vector3 dir1 = Quaternion.AngleAxis((0f - firingArc) * 0.5f, Vector3.forward) * direction;
		float dt = 1f / (float)(shots - 1);
		float t = 0f;
		for (int i = 0; i < shots; i++)
		{
			if (!(projectiles[i] == null))
			{
				if (i > 0 && projectiles[i]._shotInterval > 0.1f)
				{
					projectiles[i].gameObject.SetActive(true);
				}
				if (spreadShot)
				{
					FireSingle(projectiles[i], Vector3.Slerp(dir0, dir1, t), owner, power, parent, eCCat, false, "bot_shoot");
				}
				else
				{
					FireSingle(projectiles[i], owner.AimingDir, owner, power, parent, eCCat, false, "bot_shoot");
				}
				t += dt;
				yield return new WaitForSeconds(rangeWeaponTemplate._shotInterval);
			}
		}
		owner.FinishedFiring();
	}
}
