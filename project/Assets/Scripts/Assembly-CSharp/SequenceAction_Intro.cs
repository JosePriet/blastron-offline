using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Match Intro")]
public class SequenceAction_Intro : Action
{
	[Trigger]
	public Trigger Done = new Trigger();

	public override void Init()
	{
		base.Init();
	}

	[Entry]
	public void Start()
	{
		Manager.RPC("IntroSequence", RPCMode.All);
		Activate();
	}

	public override bool Update()
	{
		CameraController component = Camera.main.GetComponent<CameraController>();
		if (component != null && !component.IntroRunning)
		{
			Done.Invoke();
			return false;
		}
		return true;
	}
}
