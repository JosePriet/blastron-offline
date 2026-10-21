using UnityEngine;

[RequireComponent(typeof(Camera))]
[AddComponentMenu("NGUI/Tween/Field of View")]
public class TweenFOV : UITweener
{
	public float from;

	public float to;

	private Camera mCam;

	public Camera cachedCamera
	{
		get
		{
			if (mCam == null)
			{
				mCam = base.camera;
			}
			return mCam;
		}
	}

	public float fov
	{
		get
		{
			return cachedCamera.fov;
		}
		set
		{
			cachedCamera.fov = value;
		}
	}

	protected override void OnUpdate(float factor, bool isFinished)
	{
		cachedCamera.fov = from * (1f - factor) + to * factor;
	}

	public static TweenFOV Begin(GameObject go, float duration, float to)
	{
		TweenFOV tweenFOV = UITweener.Begin<TweenFOV>(go, duration);
		tweenFOV.from = tweenFOV.fov;
		tweenFOV.to = to;
		return tweenFOV;
	}
}
