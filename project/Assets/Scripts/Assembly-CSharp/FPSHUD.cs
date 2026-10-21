using UnityEngine;

public class FPSHUD : MonoBehaviour
{
	private const float editorY = 0f;

	private const float deviceRetinaY = 40f;

	private const float deviceY = 20f;

	public float _updateInterval = 0.5f;

	private float _lastInterval;

	private int _frames;

	private float _fps;

	public static bool Show { get; set; }

	public float FPS
	{
		get
		{
			return _fps;
		}
	}

	private void Start()
	{
		_lastInterval = Time.realtimeSinceStartup;
		_frames = 0;
		Show = false;
	}

	private void Update()
	{
		_frames++;
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		if (realtimeSinceStartup > _lastInterval + _updateInterval)
		{
			_fps = (float)_frames / (realtimeSinceStartup - _lastInterval);
			_frames = 0;
			_lastInterval = realtimeSinceStartup;
		}
	}

	private void OnGUI()
	{
		if (Show)
		{
			GUI.color = Color.green;
			GUI.Label(new Rect(200f, Screen.height - 20, 100f, 20f), _fps.ToString("f2") + "/s");
			GUI.color = Color.white;
		}
	}
}
