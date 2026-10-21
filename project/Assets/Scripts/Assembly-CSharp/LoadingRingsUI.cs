using EB;
using UnityEngine;

public class LoadingRingsUI : MonoBehaviour
{
	private GameObject rings;

	private BoxCollider bc;

	private bool ready;

	public void Awake()
	{
		rings = Util.GetObjectExactMatch(base.gameObject, "ui_loading_rings");
		bc = GetComponent<BoxCollider>();
		ready = true;
		EnableRings(false);
	}

	public void EnableRings(bool state)
	{
		if (ready)
		{
			bc.enabled = state;
			NGUITools.SetActive(rings, state);
		}
	}

	private void Start()
	{
		GameObject persistentScreen = ScreenManager.Instance.GetPersistentScreen("StatusBar");
		if (persistentScreen != null)
		{
			NGUITools.SetActive(persistentScreen, false);
		}
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
