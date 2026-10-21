using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Conditions/In Range")]
public class SequenceCondition_InRange : Condition
{
	[Property]
	public float Distance;

	[Property]
	public bool OnlyXZ = true;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable A = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable B = Variable.Null;

	[Trigger]
	public Trigger InRange = new Trigger();

	[Trigger]
	public Trigger OutOfRange = new Trigger();

	public override bool Update()
	{
		GameObject gameObject = (GameObject)A.Value;
		GameObject gameObject2 = (GameObject)B.Value;
		if (gameObject == null)
		{
			EB.Debug.LogError("SequenceCondition_Distance: A is null");
			return false;
		}
		if (gameObject2 == null)
		{
			EB.Debug.LogError("SequenceCondition_Distance: B is null");
			return false;
		}
		Vector3 vector = gameObject.transform.position - gameObject2.transform.position;
		if (OnlyXZ)
		{
			vector.y = 0f;
		}
		float magnitude = vector.magnitude;
		if (magnitude <= Distance)
		{
			InRange.Invoke();
		}
		else
		{
			OutOfRange.Invoke();
		}
		return false;
	}
}
