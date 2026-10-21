using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Enable HUD Element")]
public class SequenceAction_EnableHUDElement : Action
{
	[Property]
	public bool enable = true;

	[Property]
	public string elementName = string.Empty;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		GameHUD component = Setup.Instance.GetComponent<HUD>().HudObject.GetComponent<GameHUD>();
		if (component != null)
		{
			Debug.Log("Enabling hud element " + elementName);
			component.EnableGameHUDElement(elementName, enable);
		}
		Finished.Invoke();
	}
}
