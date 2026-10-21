using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/CameraEnable")]
public class SequenceAction_CameraEnable : Action
{
	[Property]
	public bool Enable;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		CameraController component = Camera.mainCamera.GetComponent<CameraController>();
		if (component != null)
		{
			component.SendMessage("DisableInput", !Enable);
		}
		Finished.Invoke();
	}
}
