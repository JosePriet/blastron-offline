using EB.Replication;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Match Outro")]
public class SequenceAction_Outro : Action
{
	private const float _safetyQuitTime = 6f;

	private float _time;

	[Trigger]
	public Trigger Done = new Trigger();

	public override void Init()
	{
		base.Init();
	}

	[Entry]
	public void Start()
	{
		if (GameHUD.Instance != null)
		{
			GameHUD.Instance.Visible = false;
		}
		Manager.RPC("Celebrate", RPCMode.All);
		Activate();
		_time = Time.realtimeSinceStartup;
	}

	public override bool Update()
	{
		bool flag = Time.realtimeSinceStartup - _time > 6f;
		CameraController component = Camera.main.GetComponent<CameraController>();
		if (component != null && !component.OutroRunning)
		{
			flag = true;
		}
		if (flag)
		{
			Done.Invoke();
			return false;
		}
		return true;
	}
}
