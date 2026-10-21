using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

public class Sequence_Challenge : EB.Sequence.Runtime.Action
{
	protected enum StartStatus
	{
		kActivate = 0,
		kDone = 1,
		kFail = 2
	}

	private GameObject challengeSequence;

	[Trigger]
	public Trigger Done = new Trigger();

	[Trigger]
	public Trigger Fail = new Trigger();

	[Trigger]
	public Trigger Rescan = new Trigger();

	protected string TheChallengeName { get; set; }

	protected StartStatus StartSequence(string prefabName)
	{
		if (string.IsNullOrEmpty(TheChallengeName))
		{
			return StartStatus.kFail;
		}
		if (ChallengeManager.Instance.IsDone(TheChallengeName))
		{
			return StartStatus.kDone;
		}
		UnityEngine.Debug.LogWarning("Activating " + TheChallengeName);
		Object original = Assets.Load("Prefab_Sequences/" + prefabName);
		challengeSequence = Object.Instantiate(original) as GameObject;
		challengeSequence.name = TheChallengeName;
		challengeSequence.transform.parent = ChallengeManager.Instance.SequenceRoot.transform;
		return StartStatus.kActivate;
	}

	public override bool Update()
	{
		if (challengeSequence == null)
		{
			if (ChallengeManager.Instance.MarkDone(TheChallengeName, true))
			{
				Rescan.Invoke();
			}
			else
			{
				Done.Invoke();
			}
			return false;
		}
		return true;
	}
}
