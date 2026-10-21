using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Check Score")]
public class SequenceCondition_CheckScore : Condition
{
	[Variable(ExpectedType = typeof(float))]
	public Variable Threshold = Variable.Null;

	[Trigger]
	public Trigger ThresholdMet = new Trigger();

	public override bool Update()
	{
		int teamCount = GameAttributes.GetTeamCount();
		float value = Threshold.GetValue<float>();
		for (int i = 0; i < teamCount; i++)
		{
			if ((float)Scoring.Instance.GetScoreByTeam(i) >= value)
			{
				ThresholdMet.Invoke();
				return false;
			}
		}
		return false;
	}
}
