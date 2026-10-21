using EB;
using UnityEngine;

public class ButtonFXScript : MonoBehaviour
{
	public float timeBetweenEffect = 2f;

	public float currentTimer;

	public float delayTimer;

	public float StartTimeOffset;

	public int RenderQueueOffset = 10;

	private GameObject outerGlare;

	private GameObject innerGlare;

	private bool delayActive;

	private float startDelayTimer;

	private void Awake()
	{
		innerGlare = Util.GetObjectExactMatch(base.gameObject, "InnerGlare");
		outerGlare = Util.GetObjectExactMatch(base.gameObject, "OuterGlare");
		innerGlare.renderer.sharedMaterial.renderQueue = 3000 + RenderQueueOffset;
		outerGlare.renderer.sharedMaterial.renderQueue = 3000 + RenderQueueOffset;
		EnableShaders(true);
	}

	private void EnableShaders(bool bEnable)
	{
		innerGlare.renderer.enabled = bEnable;
		outerGlare.renderer.enabled = bEnable;
	}

	private void Update()
	{
		if (StartTimeOffset > 0f && startDelayTimer < StartTimeOffset)
		{
			startDelayTimer += UnityEngine.Time.deltaTime;
		}
		else if (!delayActive)
		{
			currentTimer += UnityEngine.Time.deltaTime;
			innerGlare.renderer.material.SetFloat("_Speed0", currentTimer);
			outerGlare.renderer.material.SetFloat("_Speed0", currentTimer);
			if (currentTimer >= 1f)
			{
				currentTimer = 0f;
				EnableShaders(false);
				delayActive = true;
			}
		}
		else
		{
			delayTimer += UnityEngine.Time.deltaTime;
			if (delayTimer >= timeBetweenEffect)
			{
				EnableShaders(true);
				delayTimer = 0f;
				delayActive = false;
			}
		}
	}
}
