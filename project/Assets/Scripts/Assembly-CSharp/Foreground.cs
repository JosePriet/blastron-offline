using System.Collections;
using UnityEngine;

public class Foreground : MonoBehaviour
{
	public delegate void fadeInCallback();

	public delegate void fadeOutCallback();

	private float alpha;

	private float maxAlpha = 1f;

	private bool state;

	private float alphaDelta = 0.15f;

	private fadeInCallback fic;

	private fadeOutCallback foc;

	private IEnumerator ShowDiff(bool enable)
	{
		state = enable;
		if (enable)
		{
			while (alpha <= maxAlpha)
			{
				alpha += alphaDelta;
				yield return null;
			}
			alpha = maxAlpha;
			base.gameObject.renderer.material.SetColor("_Color", new Color(0f, 0f, 0f, alpha));
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			StopAllCoroutines();
			if (fic != null)
			{
				fic();
			}
		}
		else
		{
			while (alpha >= 0f)
			{
				alpha -= alphaDelta;
				yield return null;
			}
			alpha = 0f;
			base.gameObject.renderer.material.SetColor("_Color", new Color(0f, 0f, 0f, alpha));
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			StopAllCoroutines();
			NGUITools.SetActive(base.gameObject, false);
			if (foc != null)
			{
				foc();
			}
		}
		ScreenManager.Instance.EnableTransitionBlocker(false);
	}

	public void FadeIn(fadeInCallback fi)
	{
		fic = fi;
		if (!base.gameObject.activeSelf)
		{
			NGUITools.SetActive(base.gameObject, true);
		}
		StopAllCoroutines();
		StartCoroutine(ShowDiff(true));
	}

	public void FadeOut(fadeOutCallback fi)
	{
		foc = fi;
		if (!base.gameObject.activeSelf)
		{
			NGUITools.SetActive(base.gameObject, true);
		}
		StopAllCoroutines();
		StartCoroutine(ShowDiff(false));
	}

	private void Update()
	{
		base.gameObject.renderer.material.SetColor("_Color", new Color(0f, 0f, 0f, alpha));
	}
}
