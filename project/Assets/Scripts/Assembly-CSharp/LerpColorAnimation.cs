using EB;
using UnityEngine;

public class LerpColorAnimation : MonoBehaviour
{
	public enum State
	{
		Stopped = 0,
		WaitingToMove = 1,
		Moving = 2
	}

	public float timeToBeginAnimation;

	public float animationDuration;

	public Color startPoint;

	public Color endPoint;

	private float progress;

	private State state;

	private UIWidget widget;

	public event Action animationComplete;

	public event Action animationStart;

	public void BeginAnimation()
	{
		progress = 0f;
		state = State.WaitingToMove;
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

	private void Awake()
	{
		widget = Util.FindComponent<UIWidget>(base.gameObject);
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
				Color color = endPoint;
				widget.color = color;
				if (animationComplete != null)
				{
					animationComplete();
				}
			}
			else
			{
				Color color = Color.Lerp(startPoint, endPoint, progress / animationDuration);
				widget.color = color;
			}
			break;
		}
	}
}
