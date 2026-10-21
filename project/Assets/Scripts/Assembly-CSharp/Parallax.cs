using UnityEngine;

public class Parallax : MonoBehaviour
{
	public float _parallaxinessX = 20f;

	public float _parallaxinessY = 10f;

	public bool _static;

	public Vector3 _viewportPosition = new Vector3(0.5f, 0.5f, 0f);

	private CameraController _cc;

	private Vector3 _restPosition;

	private Vector3 _restScale;

	private void Start()
	{
		_cc = Camera.mainCamera.GetComponent<CameraController>();
		if (_static)
		{
			float z = base.transform.position.z;
			Vector3 position = Camera.mainCamera.ViewportToWorldPoint(_viewportPosition);
			position.z = z;
			base.transform.position = position;
			base.transform.parent = Camera.mainCamera.transform;
			_restScale = base.transform.localScale;
		}
		_restPosition = base.transform.position;
	}

	private void LateUpdate()
	{
		if (!_static)
		{
			Vector3 vector = _cc.NormalizedCentredLevelPosition();
			base.transform.position = _restPosition + new Vector3(vector.x * _parallaxinessX, vector.y * _parallaxinessY, 0f);
			return;
		}
		float num = Camera.mainCamera.camera.orthographicSize / _cc.OrthoMax;
		base.transform.localScale = num * _restScale;
		Vector3 position = Camera.mainCamera.ViewportToWorldPoint(_viewportPosition);
		position.z = _restPosition.z;
		base.transform.position = position;
	}
}
