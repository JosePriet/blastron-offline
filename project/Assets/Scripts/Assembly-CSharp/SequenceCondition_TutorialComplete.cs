using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Conditions/TutorialComplete")]
public class SequenceCondition_TutorialComplete : Condition
{
	[Property]
	public string tutorialName;

	[Trigger(EditorName = "Yep")]
	public Trigger Yes = new Trigger();

	[Trigger(EditorName = "Nope")]
	public Trigger No = new Trigger();

	public override bool Update()
	{
		if (!string.IsNullOrEmpty(tutorialName))
		{
			if (Profile.Instance.IsTutorialDone(tutorialName))
			{
				Yes.Invoke();
			}
			else
			{
				No.Invoke();
			}
		}
		else
		{
			Debug.LogError("No name was supplied for the tutorial complete sequence comparison");
		}
		return false;
	}
}
