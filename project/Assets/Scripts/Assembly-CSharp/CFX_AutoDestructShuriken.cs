using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class CFX_AutoDestructShuriken : MonoBehaviour
{
	private void Update()
	{
		if (!base.particleSystem.IsAlive(true))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
