using System.Collections;
using EB;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
	public enum Type
	{
		kFire = 0,
		kPoison = 1,
		kSlow = 2,
		kElectric = 3,
		kOther1 = 4,
		kCount = 5
	}

	private const float _HACKYINVERSEPLAYERSCALE = 100f;

	private GameObject[] _effects = new GameObject[5];

	private int[] _counts = new int[5];

	private IEnumerator Start()
	{
		string path = "PlayerEffects";
		Object[] objects = Assets.LoadAll(path, typeof(GameObject));
		if (objects == null)
		{
			yield break;
		}
		Object[] array = objects;
		foreach (Object o in array)
		{
			string name = o.name.Split('_')[0];
			if (!string.IsNullOrEmpty(name))
			{
				switch (name.ToLower())
				{
				case "fire":
					_effects[0] = Object.Instantiate(o) as GameObject;
					break;
				case "poison":
					_effects[1] = Object.Instantiate(o) as GameObject;
					break;
				case "slow":
					_effects[2] = Object.Instantiate(o) as GameObject;
					break;
				case "electric":
					_effects[3] = Object.Instantiate(o) as GameObject;
					break;
				}
			}
		}
	}

	public void AttachEffects(Player p)
	{
		Vector3 localPos = Vector3.forward * -10f + Vector3.up * p.Height * 100f * 0.5f;
		for (int i = 0; i < _effects.Length; i++)
		{
			if (_effects[i] != null)
			{
				p.AttachToHead(_effects[i].transform, localPos);
			}
		}
	}

	public void AddEffect(Type ve)
	{
		if (_counts[(int)ve] != 0)
		{
			return;
		}
		if (_effects[(int)ve] != null)
		{
			ParticleSystem[] componentsInChildren = _effects[(int)ve].GetComponentsInChildren<ParticleSystem>();
			ParticleSystem[] array = componentsInChildren;
			foreach (ParticleSystem particleSystem in array)
			{
				if (particleSystem != null)
				{
					particleSystem.time = 0f;
					particleSystem.enableEmission = true;
					particleSystem.Play(true);
				}
			}
		}
		_counts[(int)ve]++;
	}

	public void RemoveEffect(Type ve)
	{
		_counts[(int)ve]--;
		if (_counts[(int)ve] != 0 || !(_effects[(int)ve] != null))
		{
			return;
		}
		ParticleSystem component = _effects[(int)ve].GetComponent<ParticleSystem>();
		if (!(component != null))
		{
			return;
		}
		if (component.gameObject.transform.childCount > 0)
		{
			ParticleSystem[] componentsInChildren = component.gameObject.GetComponentsInChildren<ParticleSystem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enableEmission = false;
			}
		}
		component.enableEmission = false;
	}

	public void RemoveAllEffects()
	{
		for (int i = 0; i < _effects.Length; i++)
		{
			if (_effects[i] != null)
			{
				ParticleSystem component = _effects[i].GetComponent<ParticleSystem>();
				if (component != null)
				{
					component.enableEmission = false;
				}
			}
		}
	}
}
