using UnityEngine;

public class BreatheFire : MonoBehaviour
{
	private enum FireState
	{
		Waiting = 0,
		Warning = 1,
		Firing = 2
	}

	public float StartDelay;

	public float WaitTime = 20f;

	public float WarnTime = 3f;

	public float FireTime = 5f;

	public GameObject Fire;

	public GameObject Smoke;

	public DamageVolume TheDamageVolume;

	private FireState _state;

	private float _currentTime;

	private void Awake()
	{
		SwitchState(FireState.Waiting);
	}

	private void Update()
	{
		if (StartDelay > 0f)
		{
			StartDelay -= Time.deltaTime;
			return;
		}
		_currentTime -= Time.deltaTime;
		if (_currentTime < 0f)
		{
			switch (_state)
			{
			case FireState.Waiting:
				SwitchState(FireState.Warning);
				break;
			case FireState.Warning:
				SwitchState(FireState.Firing);
				break;
			case FireState.Firing:
				SwitchState(FireState.Waiting);
				break;
			}
		}
	}

	private void SwitchState(FireState newState)
	{
		switch (newState)
		{
		case FireState.Waiting:
			ToggleObject(Fire, false);
			ToggleObject(Smoke, false);
			TheDamageVolume.collider.enabled = false;
			_currentTime = WaitTime;
			break;
		case FireState.Warning:
			ToggleObject(Fire, false);
			ToggleObject(Smoke, true);
			TheDamageVolume.collider.enabled = false;
			_currentTime = WarnTime;
			break;
		case FireState.Firing:
			ToggleObject(Fire, true);
			ToggleObject(Smoke, false);
			TheDamageVolume.collider.enabled = true;
			_currentTime = FireTime;
			break;
		}
		_state = newState;
	}

	private void ToggleObject(GameObject obj, bool enable)
	{
		if (!(obj != null))
		{
			return;
		}
		if ((bool)obj.GetComponent<ParticleSystem>())
		{
			if (enable)
			{
				obj.particleSystem.Play();
			}
			else
			{
				obj.particleSystem.Stop();
			}
			return;
		}
		if ((bool)obj.GetComponent<Renderer>())
		{
			obj.renderer.enabled = enable;
			return;
		}
		foreach (Transform item in obj.transform)
		{
			ToggleObject(item.gameObject, enable);
		}
	}
}
