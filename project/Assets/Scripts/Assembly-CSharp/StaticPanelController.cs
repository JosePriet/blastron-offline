using EB;
using UnityEngine;

public class StaticPanelController : MonoBehaviour
{
	private UIPanel panel;

	private int frameCounter;

	public void ToggleStaticWidgets()
	{
		base.enabled = true;
		panel.widgetsAreStatic = false;
		frameCounter = 15;
	}

	private void Awake()
	{
		panel = Util.FindComponent<UIPanel>(base.gameObject);
		base.enabled = false;
	}

	private void Update()
	{
		frameCounter--;
		if (frameCounter <= 0)
		{
			panel.widgetsAreStatic = true;
			base.enabled = false;
		}
	}
}
