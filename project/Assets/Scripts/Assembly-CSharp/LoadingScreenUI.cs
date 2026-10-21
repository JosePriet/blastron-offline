using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
	public void Awake()
	{
	}

	private void Start()
	{
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null)
		{
			NGUITools.SetActive(persistentScreen, false);
		}
		ScreenManager.Instance.FadeOut(null);
	}

	private void OnDisable()
	{
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null)
		{
			NGUITools.SetActive(persistentScreen, true);
		}
	}
}
