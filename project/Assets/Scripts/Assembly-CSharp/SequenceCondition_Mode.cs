using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Mode")]
public class SequenceCondition_Mode : Condition
{
	[Trigger]
	public Trigger SP = new Trigger();

	[Trigger]
	public Trigger MP = new Trigger();

	public override bool Update()
	{
		GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
		if (mode == GameAttributes.ModeType.SP)
		{
			SP.Invoke();
		}
		else
		{
			MP.Invoke();
		}
		return false;
	}
}
