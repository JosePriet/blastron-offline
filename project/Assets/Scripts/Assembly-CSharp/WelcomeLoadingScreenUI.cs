using System.Collections;
using EB;
using EB.Sparx;
using UnityEngine;

public class WelcomeLoadingScreenUI : MonoBehaviour
{
	private void Awake()
	{
		ScreenManager.Instance.AddPersistentScreen("SplashUI");
	}

	private IEnumerator Start()
	{
		while (ChartboostPlugin.Instance == null)
		{
			yield return 1;
		}
		ChartboostPlugin.Instance.ShowInterstitial(string.Empty);
		ScreenManager.Instance.FadeOut(null);
	}

	public Coroutine CheckForContent(string baseUrl)
	{
		return StartCoroutine(DoCheckForContent(baseUrl));
	}

	private void OnProgress(string text)
	{
		UIUtils.SetLabelContents(base.gameObject, "LabelLoading", text.ToUpper());
	}

	private IEnumerator DoCheckForContent(string baseUrl)
	{
		EB.Debug.Log("check for content " + baseUrl);
		ObbManager manager = new ObbManager();
		while (!manager.IsMounted)
		{
			yield return manager.Check();
		}
		OnProgress("ID_UI_LOADING");
	}
}
