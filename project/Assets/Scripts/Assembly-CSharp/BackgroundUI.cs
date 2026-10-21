using EB;
using UnityEngine;

public class BackgroundUI : MonoBehaviour
{
	public void ShowCliff(bool isEnabled)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "home_cliff");
		objectExactMatch.SetActive(isEnabled);
	}

	public void ShowSmoke(bool isEnabled)
	{
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "fx_smoke_bg_ui");
		if (objectExactMatch != null)
		{
			objectExactMatch.SetActive(isEnabled);
		}
	}
}
