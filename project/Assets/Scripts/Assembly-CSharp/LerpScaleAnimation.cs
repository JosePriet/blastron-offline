using EB;
using UnityEngine;

public class LerpScaleAnimation : MonoBehaviour
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

	public State state { get; private set; }

	public event Action animationComplete;

	public event Action animationStart;

	private void Awake()
	{
		state = State.Stopped;
	}

	public void BeginAnimation()
	{
		progress = 0f;
		state = State.WaitingToMove;
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
				Vector3 localScale = endPoint;
				base.transform.localScale = localScale;
				if (animationComplete != null)
				{
					animationComplete();
				}
			}
			else
			{
				Vector3 localScale = Vector3.Lerp(startPoint, endPoint, progress / animationDuration);
				base.transform.localScale = localScale;
			}
			break;
		}
	}
}
