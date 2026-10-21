using EB;
using UnityEngine;

public class OptionItem : MonoBehaviour
{
	public string Name = string.Empty;

	public int DefaultValue = 1;

	private UISprite _cross;

	private void Awake()
	{
		_cross = Util.GetObjectExactMatch(base.gameObject, "Cross").GetComponent<UISprite>();
		UpdateSprite();
	}

	private void OnClick()
	{
		int option = Options.GetOption(Name, DefaultValue);
		option = ((option <= 0) ? 1 : 0);
		Options.SetOption(Name, option);
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		int option = Options.GetOption(Name, DefaultValue);
		_cross.enabled = option == 0;
	}
}
