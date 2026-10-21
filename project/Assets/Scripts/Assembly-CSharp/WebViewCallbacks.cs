using UnityEngine;

public class WebViewCallbacks : MonoBehaviour
{
	public WebViewDialogUI parent;

	private void OnLoad(string ignore)
	{
		Debug.LogError("OnLoad");
		if ((bool)parent)
		{
			parent.OnLoad();
		}
	}

	private void OnUrl(string uri)
	{
		Debug.LogError("OnUrl:" + uri);
		if ((bool)parent)
		{
			parent.OnUrl(uri);
		}
	}
}
