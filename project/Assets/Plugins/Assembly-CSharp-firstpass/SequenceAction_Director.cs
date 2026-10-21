using EB;
using EB.Director;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Director")]
public class SequenceAction_Director : EB.Sequence.Runtime.Action
{
	[Property]
	public GameObject DirectorData;

	[Variable(Show = false)]
	public Variable[] Groups = new Variable[0];

	[Trigger(Show = false)]
	public Trigger[] Events = new Trigger[0];

	private EB.Director.Component _instance;

	[Trigger]
	public Trigger Started = new Trigger();

	[Trigger]
	public Trigger Stopped = new Trigger();

	public override void Dispose()
	{
		if (_instance != null)
		{
			_instance.Stop();
			Object.Destroy(_instance.gameObject);
		}
	}

	public override void Init()
	{
		GameObject gameObject = (GameObject)Object.Instantiate(DirectorData);
		_instance = ((!(gameObject != null)) ? null : gameObject.GetComponent<EB.Director.Component>());
		if (_instance == null)
		{
			EB.Debug.LogError("ERROR: missing director data on prefab!");
			Object.Destroy(gameObject);
		}
		else
		{
			Object.DontDestroyOnLoad(_instance);
			_instance.Bind(this);
		}
	}

	public override bool Update()
	{
		if (_instance != null)
		{
			return _instance.IsPlaying;
		}
		return false;
	}

	[Entry]
	public void Play()
	{
		if (_instance != null)
		{
			_instance.Play();
			Activate();
		}
	}

	[Entry]
	public void Pause()
	{
		if (_instance != null)
		{
			_instance.Pause();
		}
	}

	[Entry]
	public void Resume()
	{
		if (_instance != null)
		{
			_instance.Resume();
		}
	}

	[Entry]
	public void Stop()
	{
		if (_instance != null)
		{
			_instance.Stop();
		}
	}
}
