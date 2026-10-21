using UnityEngine;

public class TextureFade : MonoBehaviour
{
	public UITexture tex;

	private bool fade;

	private float alpha;

	private float speedMult = 2f;

	public void HideTexture()
	{
		if (tex != null)
		{
			tex.alpha = 0f;
			fade = false;
		}
	}

	public void SetTexture(UITexture t)
	{
		tex = t;
		if (tex != null)
		{
			tex.alpha = 0f;
			alpha = 0f;
			fade = true;
		}
	}

	private void Update()
	{
		if (fade)
		{
			alpha += Time.deltaTime * speedMult;
			if (alpha >= 1f)
			{
				alpha = 1f;
			}
			tex.alpha = alpha;
			if (tex.alpha == 1f)
			{
				fade = false;
			}
		}
	}
}
