using UnityEngine;

public class RotateByScale : MonoBehaviour
{
	private float _sign = 1f;

	private void Update()
	{
		float num = Mathf.Sign(base.transform.lossyScale.x);
		if (num != _sign)
		{
			_sign = num;
			base.transform.Rotate(0f, 0f, 180f, Space.Self);
		}
	}
}
