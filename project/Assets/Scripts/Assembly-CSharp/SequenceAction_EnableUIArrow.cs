using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Enable UI Arrow")]
public class SequenceAction_EnableUIArrow : Action
{
	[Property]
	public string tutorialId;

	[Property]
	public bool show = true;

	[Property]
	public bool autoHide = true;

	[Trigger]
	public Trigger Finished = new Trigger();

	public override void Dispose()
	{
		if (show && autoHide)
		{
			TutorialManager.Instance.EnableArrow(tutorialId, false);
		}
		base.Dispose();
	}

	[Entry]
	public void Process()
	{
		if (show)
		{
			TutorialManager.Instance.EnableArrow(tutorialId, true);
		}
		else
		{
			TutorialManager.Instance.EnableArrow(tutorialId, false);
		}
		Finished.Invoke();
	}
}
