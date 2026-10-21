using System.Collections;
using UnityEngine;

public class Fader : MonoBehaviour
{
	private float alpha;

	private float maxAlpha = 0.82f;

	private bool state;

	private float alphaSpeed = 1.5f;

	private IEnumerator ShowDiff(bool enable)
	{
		state = enable;
		if (enable)
		{
			while (alpha <= maxAlpha)
			{
				alpha += alphaSpeed * Time.deltaTime;
				yield return null;
			}
			alpha = maxAlpha;
			StopAllCoroutines();
			yield break;
		}
		while (alpha > 0f)
		{
			alpha -= alphaSpeed * Time.deltaTime;
			yield return null;
		}
		alpha = 0f;
		StopAllCoroutines();
		NGUITools.SetActive(base.gameObject, false);
	}

	public void Enable(bool enable)
	{
		if (base.gameObject.activeSelf)
		{
			StopAllCoroutines();
			StartCoroutine(ShowDiff(enable));
		}
	}

	private void Update()
	{
		base.gameObject.renderer.material.SetColor("_Color", new Color(0f, 0f, 0f, alpha));
	}
}
