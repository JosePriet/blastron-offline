using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using EB.Sparx;
using UnityEngine;

[MenuItem(Path = "Actions/Run Sequence")]
public class SequenceAction_RunSequence : EB.Sequence.Runtime.Action
{
	[Property]
	public string sequenceName;

	[Property]
	public bool parentToCreator;

	[Property]
	public bool persistent;

	[Property]
	public bool mustBeConnected;

	private GameObject sequence;

	private bool isRun;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void RunSequence()
	{
		HubState state = Hub.Instance.State;
		if (mustBeConnected && state == HubState.Idle)
		{
			EB.Debug.LogWarning(string.Format("Aborting {0} due to HubState == {1}", sequenceName, state));
			Finished.Invoke();
			return;
		}
		Object obj = Assets.Load("Prefab_Sequences/" + sequenceName);
		if (obj != null)
		{
			sequence = Object.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
			sequence.name = obj.name;
			if (parentToCreator)
			{
				sequence.transform.parent = base.Parent.transform;
			}
			isRun = true;
			if (persistent)
			{
				Object.DontDestroyOnLoad(sequence);
			}
		}
		else
		{
			Finished.Invoke();
		}
	}

	public override bool Update()
	{
		if (sequence == null && isRun)
		{
			Finished.Invoke();
			return false;
		}
		return true;
	}
}
