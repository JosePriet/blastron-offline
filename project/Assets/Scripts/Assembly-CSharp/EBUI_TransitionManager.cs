using System.Collections.Generic;
using EB;
using Holoville.HOTween;
using UnityEngine;

public class EBUI_TransitionManager : MonoBehaviour
{
	private class TransitionRecord
	{
		public Action _callback;

		public int _counter;
	}

	public delegate void transitionCallback();

	public delegate void transitionIDCallback(string id);

	private static EBUI_TransitionManager _this;

	private bool isAnimating;

	private Dictionary<string, TransitionRecord> _transitions = new Dictionary<string, TransitionRecord>();

	public static EBUI_TransitionManager Instance
	{
		get
		{
			return _this;
		}
	}

	public bool IsAnimating
	{
		get
		{
			return isAnimating;
		}
	}

	public void Awake()
	{
		_this = this;
	}

	public void Start()
	{
		HOTween.Init(false, false, true);
		HOTween.EnableOverwriteManager();
	}

	private void StartSingleTransition(GameObject go, EBUI_Transition.TransitionPropertyInfo info, transitionIDCallback cb, string id)
	{
		string text = EBUI_Transition.transitionScripts[(int)info.transitionType];
		if (!string.IsNullOrEmpty(text))
		{
			EBUI_Transition eBUI_Transition = (EBUI_Transition)go.AddComponent(text);
			eBUI_Transition.Run(info, cb, id);
		}
	}

	public void RunIntroTransitions(GameObject scr, Action callback)
	{
		ScreenManager.Instance.EnableTransitionBlocker(true);
		EBUI_TransitionInfo[] componentsInChildren = scr.GetComponentsInChildren<EBUI_TransitionInfo>();
		_transitions[scr.name] = new TransitionRecord
		{
			_callback = callback,
			_counter = componentsInChildren.Length
		};
		isAnimating = true;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Instance.StartSingleTransition(componentsInChildren[i].gameObject, componentsInChildren[i].transition_intro, OnTransitionsComplete, scr.name);
		}
	}

	public void RunOutroTransitions(GameObject scr, Action callback)
	{
		ScreenManager.Instance.EnableTransitionBlocker(true);
		EBUI_TransitionInfo[] componentsInChildren = scr.GetComponentsInChildren<EBUI_TransitionInfo>();
		_transitions[scr.name] = new TransitionRecord
		{
			_callback = callback,
			_counter = componentsInChildren.Length
		};
		isAnimating = true;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Instance.StartSingleTransition(componentsInChildren[i].gameObject, componentsInChildren[i].transition_outro, OnTransitionsComplete, scr.name);
		}
	}

	public void OnTransitionsComplete(string id)
	{
		TransitionRecord value;
		if (_transitions.TryGetValue(id, out value))
		{
			if (value._counter > 0)
			{
				value._counter--;
			}
			if (value._counter <= 0)
			{
				if (value._callback != null)
				{
					value._callback();
				}
				_transitions.Remove(id);
			}
		}
		if (_transitions.Count == 0)
		{
			ScreenManager.Instance.EnableTransitionBlocker(false);
			isAnimating = false;
		}
	}

	public void Clear()
	{
		_transitions.Clear();
		isAnimating = false;
	}

	public void HackClear()
	{
		isAnimating = false;
	}
}
