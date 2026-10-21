using EB.Sequence;

[MenuItem(Path = "Actions/Challenge_OpenScreen")]
public class SequenceAction_Challenge_OpenScreen : Sequence_Challenge
{
	[Property]
	public string ScreenName = string.Empty;

	[Property]
	public string ChallengeName = string.Empty;

	[Entry]
	public void Start()
	{
		base.TheChallengeName = ChallengeName;
		switch (StartSequence("Sequence_ChallengeOpenScreen"))
		{
		case StartStatus.kActivate:
			VariableManager.Instance.SetVariable("ScreenName", ScreenName);
			Activate();
			break;
		case StartStatus.kDone:
			Done.Invoke();
			break;
		case StartStatus.kFail:
			Fail.Invoke();
			break;
		}
	}
}
