using System;
using UnityEngine;

public abstract class EBUI_Transition : MonoBehaviour
{
	public enum TransitionTypes
	{
		None = 0,
		SlideIn_FromLeft = 1,
		SlideIn_FromTop = 2,
		SlideIn_FromRight = 3,
		SlideIn_FromBottom = 4,
		SlideOut_ToLeft = 5,
		SlideOut_ToTop = 6,
		SlideOut_ToRight = 7,
		SlideOut_ToBottom = 8,
		BounceIn_FromLeft = 9,
		BounceIn_FromTop = 10,
		BounceIn_FromRight = 11,
		BounceIn_FromBottom = 12
	}

	[Serializable]
	public class TransitionPropertyInfo
	{
		public TransitionTypes transitionType;

		public float duration = 0.5f;

		public float delay;
	}

	public static string[] transitionScripts = new string[13]
	{
		"Transition_Default", "Transition_Slide_Intro_FromLeft", "Transition_Slide_Intro_FromTop", "Transition_Slide_Intro_FromRight", "Transition_Slide_Intro_FromBottom", "Transition_Slide_Outro_ToLeft", "Transition_Slide_Outro_ToTop", "Transition_Slide_Outro_ToRight", "Transition_Slide_Outro_ToBottom", "Transition_Bounce_Intro_FromLeft",
		"Transition_Bounce_Intro_FromTop", "Transition_Bounce_Intro_FromRight", "Transition_Bounce_Intro_FromBottom"
	};

	public TransitionPropertyInfo info;

	public EBUI_TransitionManager.transitionIDCallback callback;

	private string _id;

	public void Run(TransitionPropertyInfo i, EBUI_TransitionManager.transitionIDCallback cb, string id)
	{
		info = i;
		callback = cb;
		_id = id;
		StartTransition();
	}

	public void Awake()
	{
	}

	public virtual void StartTransition()
	{
	}

	public virtual void StopTransition()
	{
		if (callback != null)
		{
			callback(_id);
		}
		UnityEngine.Object.Destroy(this);
	}
}
