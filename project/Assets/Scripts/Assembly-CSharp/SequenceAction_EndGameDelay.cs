using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/DelayForActiveProjectiles")]
public class SequenceAction_EndGameDelay : Action
{
	private const float _safetyQuitTime = 10f;

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
		Activate();
		_time = Time.realtimeSinceStartup;
		if (GameHUD.Instance != null)
		{
			GameHUD.Instance.DoSuddenDeathTimerCountdown();
		}
	}

	public override bool Update()
	{
		if (GameHUD.Instance != null)
		{
			if (Time.realtimeSinceStartup - _time < 10f && (ProjectileIndicatorManager.Instance.DelayEndOfGame() || GameHUD.Instance.IsSuddenDeathTimerCountdownRunning))
			{
				return true;
			}
			Done.Invoke();
		}
		return false;
	}
}
