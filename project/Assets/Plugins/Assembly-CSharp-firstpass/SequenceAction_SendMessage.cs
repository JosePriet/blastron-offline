using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Send Message")]
public class SequenceAction_SendMessage : Action
{
	[Property]
	public string functionName = string.Empty;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable target = Variable.Null;

	[Entry]
	public void Fire()
	{
		GameObject gameObject = (GameObject)target.Value;
		if (gameObject != null)
		{
			gameObject.SendMessage(functionName, SendMessageOptions.RequireReceiver);
		}
	}
}
