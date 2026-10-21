using UnityEngine;

[RequireComponent(typeof(Light))]
public class CFX_LightIntensityFade : MonoBehaviour
{
	public float duration = 1f;

	public float delay;

	public float finalIntensity;

	private float baseIntensity;

	public bool autodestruct;

	private float lifetime;

	private void Start()
	{
		baseIntensity = base.light.intensity;
		if (delay > 0f)
		{
			base.light.enabled = false;
		}
	}

	private void Update()
	{
		if (delay > 0f)
		{
			delay -= Time.deltaTime;
			if (delay <= 0f)
			{
				base.light.enabled = true;
			}
		}
		else if (lifetime / duration < 1f)
		{
			base.light.intensity = Mathf.Lerp(baseIntensity, finalIntensity, lifetime / duration);
			lifetime += Time.deltaTime;
		}
		else if (autodestruct)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
