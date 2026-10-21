using EB;
using EB.Sequence;
using EB.Sequence.Runtime;

[MenuItem(Path = "Actions/Load Bundle")]
public class SequenceAction_LoadBundle : EB.Sequence.Runtime.Action
{
	[Property]
	public string BundleName = string.Empty;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Load()
	{
		Assets.LoadBundle(BundleName);
		Activate();
	}

	public override bool Update()
	{
		if (!Assets.IsBundleLoaded(BundleName))
		{
			return true;
		}
		Finished.Invoke();
		return false;
	}
}
