using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/Championship Mode")]
public class SequenceCondition_ChampionshipMode : Condition
{
	[Trigger]
	public Trigger isChamp = new Trigger();

	[Trigger]
	public Trigger isNot = new Trigger();

	[Entry]
	public new void Check()
	{
		GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
		if (championshipMode == GameAttributes.ChampionshipType.kOn)
		{
			isChamp.Invoke();
		}
		else
		{
			isNot.Invoke();
		}
	}
}
