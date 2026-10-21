using EB;
using UnityEngine;

public class LoadingScreenTipsUI : MonoBehaviour
{
	private void Awake()
	{
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null)
		{
			NGUITools.SetActive(persistentScreen, false);
		}
		string key = "loading_tips";
		int num = SecurePrefs.GetInt(key, 1);
		string id = "ID_TIPS_" + num;
		string value = string.Empty;
		if (!Localizer.GetString(id, out value))
		{
			num = 1;
			value = Localizer.GetString("ID_TIPS_" + num);
		}
		EB.Debug.Log("Tip: " + value);
		UIUtils.SetLabelContents(base.gameObject, "LabelTip", value.ToUpper());
		num++;
		SecurePrefs.SetInt(key, num);
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
