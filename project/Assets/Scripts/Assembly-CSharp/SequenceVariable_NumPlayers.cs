using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;

[MenuItem(Path = "Variables/Num Players", VariableType = typeof(float))]
public class SequenceVariable_NumPlayers : Variable
{
	public override object Value
	{
		get
		{
			Game game = Hub.Instance.GameManager.Game;
			if (game != null)
			{
				return game.Players.Count;
			}
			return 0;
		}
		set
		{
		}
	}
}
