using EB;
using UnityEngine;

public class LerpAnimation : MonoBehaviour
{
	public enum State
	{
		Stopped = 0,
		WaitingToMove = 1,
		Moving = 2
	}

	public float timeToBeginAnimation;

	public float animationDuration;

	public Vector3 startPoint;

	public Vector3 endPoint;

	private float progress;

	private State state;

	public event Action animationComplete;

	public event Action animationStart;

	public void BeginAnimation()
	{
		progress = 0f;
		state = State.WaitingToMove;
	}

	public State GetAnimationState()
	{
		return state;
	}

	public void StopAnimation()
	{
		state = State.Stopped;
	}

	public bool IsAnimating()
	{
		return state != State.Stopped;
	}

	public float GetAnimationProgress()
	{
		if (state == State.WaitingToMove)
		{
			return 0f;
		}
		return progress / animationDuration;
	}

	public void Update()
	{
		switch (state)
		{
		case State.Stopped:
			break;
		case State.WaitingToMove:
			progress += UnityEngine.Time.deltaTime;
			if (progress >= timeToBeginAnimation)
			{
				if (animationStart != null)
				{
					animationStart();
				}
				state = State.Moving;
				progress = 0f;
			}
			break;
		case State.Moving:
			progress += UnityEngine.Time.deltaTime;
			if (progress >= animationDuration)
			{
				state = State.Stopped;
				progress = 0f;
				Vector3 localPosition = endPoint;
				base.transform.localPosition = localPosition;
				if (animationComplete != null)
				{
					animationComplete();
				}
			}
			else
			{
				Vector3 localPosition = Vector3.Lerp(startPoint, endPoint, progress / animationDuration);
				base.transform.localPosition = localPosition;
			}
			break;
		}
	}
}
