using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Get Scor")]
public class SequenceAction_GetScore : Action
{
	[Variable(ExpectedType = typeof(uint))]
	public Variable PlayerId = Variable.Null;

	[Variable(ExpectedType = typeof(float), Direction = Direction.Out)]
	public Variable Score = Variable.Null;

	[Trigger]
	public Trigger Complete = new Trigger();

	[Entry]
	public void Get()
	{
		uint value = PlayerId.GetValue<uint>();
		int scoreByPlayer = Scoring.Instance.GetScoreByPlayer(value);
		Score.Value = (float)scoreByPlayer;
		Complete.Invoke();
	}
}
