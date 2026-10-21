using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Is Human")]
public class SequenceCondition_IsHuman : Condition
{
	[Variable(ExpectedType = typeof(uint))]
	public Variable PlayerId = Variable.Null;

	[Trigger(EditorName = "Human")]
	public Trigger Human = new Trigger();

	[Trigger(EditorName = "AI")]
	public Trigger AI = new Trigger();

	public override bool Update()
	{
		if (PlayerId != Variable.Null)
		{
			uint playerId = (uint)PlayerId.Value;
			if (LevelData.Instance.PlayerIsAI(playerId))
			{
				AI.Invoke();
			}
			else
			{
				Human.Invoke();
			}
		}
		else
		{
			Debug.Log("IsHuman test failed (input Id is null)! - Parent:" + base.Parent.name);
		}
		return false;
	}
}
