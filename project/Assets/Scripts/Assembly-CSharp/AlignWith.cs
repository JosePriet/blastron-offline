using UnityEngine;

public class AlignWith : MonoBehaviour
{
	private Camera _target;

	public Camera Target
	{
		get
		{
			return _target;
		}
		set
		{
			_target = value;
		}
	}

	private void Update()
	{
		if (_target != null)
		{
			base.transform.LookAt(base.transform.position + Target.transform.forward, base.transform.up);
		}
	}
}
