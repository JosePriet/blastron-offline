using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Rotate")]
public class SequenceAction_Rotate : EB.Sequence.Runtime.Action
{
	[Property]
	public float X;

	[Property]
	public float Y;

	[Property]
	public float Z;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable Target = Variable.Null;

	[Entry]
	public void Rotate()
	{
		GameObject gameObject = (GameObject)Target.Value;
		if (gameObject == null)
		{
			EB.Debug.LogError("No Target to rotate");
		}
		else
		{
			gameObject.transform.rotation = Quaternion.Euler(X, Y, Z);
		}
	}
}
