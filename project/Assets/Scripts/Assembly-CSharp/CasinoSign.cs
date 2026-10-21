using UnityEngine;

public class CasinoSign : MonoBehaviour
{
	public Texture2D frame1;

	public Texture2D frame2;

	public float timeToSwitch = 2f;

	private float currentTimer;

	private bool firstFrame = true;

	private void Start()
	{
	}

	private void Update()
	{
		currentTimer += Time.deltaTime;
		if (currentTimer >= timeToSwitch)
		{
			firstFrame = !firstFrame;
			if (firstFrame)
			{
				base.renderer.material.SetTexture("_MainTex", frame1);
			}
			else
			{
				base.renderer.material.SetTexture("_MainTex", frame2);
			}
			currentTimer = 0f;
		}
	}
}
