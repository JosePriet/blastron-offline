using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Enable PlayerInput")]
public class SequenceAction_EnablePlayerInput : EB.Sequence.Runtime.Action
{
	[Property]
	public bool enable = true;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		if ((bool)LevelData.Instance)
		{
			Player player = LevelData.Instance.GetPlayer();
			player.SendMessage("EnableInput", enable);
			HUD component = Setup.Instance.GetComponent<HUD>();
			if (component != null)
			{
				GameObject hudObject = component.HudObject;
				if (hudObject != null)
				{
					GameObject objectExactMatch = Util.GetObjectExactMatch(hudObject, "ButtonAim");
					if (objectExactMatch != null)
					{
						objectExactMatch.SendMessage("ResetAim");
					}
				}
			}
		}
		Finished.Invoke();
	}
}
