using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Variables/Game Mode Time", VariableType = typeof(float))]
public class SequenceVariable_GameModeTime : Variable
{
	public override object Value
	{
		get
		{
			GameAttributes.ChampionshipType championshipMode = GameAttributes.GetChampionshipMode(GameAttributes.GetValue(GameAttributes.AttributeType.Championship));
			bool flag = championshipMode == GameAttributes.ChampionshipType.kOn;
			GameAttributes.ModeType mode = GameAttributes.GetMode(GameAttributes.GetValue(GameAttributes.AttributeType.Mode));
			bool flag2 = mode == GameAttributes.ModeType.MP;
			GameAttributes.GameModeType gameMode = GameAttributes.GetGameMode(GameAttributes.GetValue(GameAttributes.AttributeType.GameMode));
			if (flag)
			{
				Division.Stage selectedStage = ChampionshipData.Instance.GetSelectedStage();
				if (selectedStage.gameTime > 0)
				{
					return (float)selectedStage.gameTime;
				}
			}
			float num = 0f;
			switch (gameMode)
			{
			case GameAttributes.GameModeType.KillCount:
				num = Tuning.Instance.Timer_KillCount;
				break;
			case GameAttributes.GameModeType.SuddenDeath:
				num = Tuning.Instance.Timer_SuddenDeath;
				break;
			default:
				num = Tuning.Instance.Timer_DeathMatch;
				break;
			}
			return num;
		}
		set
		{
		}
	}
}
