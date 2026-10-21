using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Play Outro Story")]
public class sequenceAction_PlayOutroStory : Action
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
		StoryManager.Instance.OutroStory();
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
