using UnityEngine;

public class RimFade : MonoBehaviour
{
	public bool _reverse;

	private float _alpha;

	private void LateUpdate()
	{
		Vector3 lhs = base.transform.rotation * Vector3.up;
		Vector3 forward = SinglePlayerUI.WorldCam.transform.forward;
		float num = Mathf.Clamp01(Vector3.Dot(lhs, -forward));
		if (_reverse)
		{
			num = 1f - num;
		}
		if (_alpha != num)
		{
			_alpha = num;
			Color color = base.renderer.material.color;
			color.a = num;
			base.renderer.material.color = color;
		}
	}
}
