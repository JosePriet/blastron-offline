using System.Collections.Generic;
using EB;
using EB.Collections;
using UnityEngine;

public class AIBase : MonoBehaviour
{
	protected class State
	{
		public readonly string _name;

		public readonly string _coroutine;

		public readonly MonoBehaviour _containerScript;

		public State(string stateName, string coroutine, MonoBehaviour script = null, params object[] args)
		{
			_name = stateName;
			_coroutine = coroutine;
			_containerScript = script;
		}
	}

	private EB.Collections.Queue<State> _stateQ = new EB.Collections.Queue<State>();

	private State _next;

	private Dictionary<string, State> _states = new Dictionary<string, State>();

	protected State Current { get; private set; }

	private State Next
	{
		get
		{
			if (_next != null)
			{
				return _next;
			}
			if (Current == null && _stateQ.Count > 0)
			{
				_next = _stateQ.Dequeue();
			}
			return _next;
		}
		set
		{
			_next = value;
		}
	}

	internal bool Paused { get; set; }

	protected string CurrentStateName
	{
		get
		{
			if (Current != null)
			{
				return Current._name;
			}
			return "The state you have tried to reach is unavailable";
		}
	}

	protected virtual void Update()
	{
		if (!Paused)
		{
			SwitchState();
		}
	}

	private void SwitchState()
	{
		if (Next != null)
		{
			ExitCurrent();
			StartNext();
		}
	}

	internal void SetState(string name)
	{
		State value;
		if (_states.TryGetValue(name, out value))
		{
			_stateQ.Clear();
			Next = value;
		}
		else
		{
			EB.Debug.LogError("AIBase: Tried to SetState on unregistered state: " + name);
		}
	}

	protected void QueueState(State stateToQueue)
	{
		_stateQ.Enqueue(stateToQueue);
	}

	protected void YieldCurrentState()
	{
		Current = null;
	}

	protected void ExitCurrent()
	{
		if (Current != null)
		{
			if (Current._containerScript == null)
			{
				StopCoroutine(Current._coroutine);
			}
			else
			{
				Current._containerScript.StopCoroutine(Current._coroutine);
			}
		}
	}

	public virtual void Pause(bool bPause)
	{
		Paused = bPause;
		if (Paused)
		{
			ExitCurrent();
		}
		else if (Current != null)
		{
			if (Current._containerScript == null)
			{
				StartCoroutine(Current._coroutine);
			}
			else
			{
				Current._containerScript.StartCoroutine(Current._coroutine);
			}
		}
	}

	private void StartNext()
	{
		Current = Next;
		Next = null;
		if (Current != null)
		{
			if (Current._containerScript == null)
			{
				StartCoroutine(Current._coroutine);
			}
			else
			{
				Current._containerScript.StartCoroutine(Current._coroutine);
			}
		}
	}

	internal bool IsCurrent(string stateName)
	{
		if (Current != null)
		{
			return Current._name == stateName;
		}
		return false;
	}

	protected void AddState(string name, string coroutine)
	{
		_states.Add(name, new State(name, coroutine, null));
	}

	internal void AddState(string name, string coroutine, MonoBehaviour script)
	{
		_states.Add(name, new State(name, coroutine, script));
	}
}
