using UnityEngine;

public class PS_Killer : MonoBehaviour
{
	private void LateUpdate()
	{
		if (!(base.particleSystem == null) && !base.particleSystem.IsAlive(true))
		{
			Object.Destroy(base.gameObject);
		}
	}
}
