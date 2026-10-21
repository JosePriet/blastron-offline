using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Play Intro Story")]
public class SequenceAction_PlayIntroStory : Action
{
	[Trigger]
	public Trigger Finished = new Trigger();

	public override void Init()
	{
		base.Init();
	}

	[Entry]
	public void Start()
	{
		StoryManager.Instance.IntroStory();
		Activate();
	}

	public override bool Update()
	{
		if (!StoryManager.Instance.isStoryPlaying)
		{
			Finished.Invoke();
			return false;
		}
		return true;
	}
}
