using EB;
using UnityEngine;

public class BlockingUI : MonoBehaviour
{
	public void SetText(string title, string description)
	{
		UIUtils.SetLabelContents(base.gameObject, "LabelTitle", title);
		UIUtils.SetLabelContents(base.gameObject, "LabelDescription", description);
	}
}
