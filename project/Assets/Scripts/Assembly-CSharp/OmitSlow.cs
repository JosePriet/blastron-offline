using UnityEngine;

public class OmitSlow : MonoBehaviour
{
	private void Awake()
	{
		if (Misc.IsSlow())
		{
			if (base.particleSystem != null)
			{
				Object.Destroy(base.particleSystem);
			}
			base.gameObject.SetActive(false);
		}
	}
}
