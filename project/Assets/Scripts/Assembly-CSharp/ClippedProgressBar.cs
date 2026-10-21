using System;
using EB;
using UnityEngine;

public class ClippedProgressBar : MonoBehaviour
{
	private enum AnimationState
	{
		Inactive = 0,
		Lerping = 1,
		Waving = 2,
		SwappingColor = 3
	}

	public Color fadeToColor;

	private Vector4 _fullClipRange = Vector4.zero;

	private Color normalColor;

	private UIPanel clipper;

	private float targetValue;

	private float currentValue;

	private float previousValue;

	private EB.Action animationCallback;

	private UISlicedSprite sprite;

	private AnimationState animationState;

	private float lerpStartTime;

	private float lerpEndTime;

	private float lerpDuration;

	private Vector4 fullClipRange
	{
		get
		{
			return _fullClipRange;
		}
	}

	public void UpdateClipRange()
	{
		_fullClipRange = clipper.clipRange;
	}

	public void SetValueImmediate(float v)
	{
		if (!float.IsNaN(v))
		{
			currentValue = v;
			if (v <= 0f)
			{
				clipper.enabled = false;
				return;
			}
			clipper.enabled = true;
			float num = fullClipRange.z * v;
			float x = num * 0.5f;
			Vector4 clipRange = fullClipRange;
			clipRange.x = x;
			clipRange.z = num;
			clipper.clipRange = clipRange;
		}
	}

	public float GetValuePerPixel()
	{
		return 1f / fullClipRange.z;
	}

	public void SetValueAnimated(float v, EB.Action animCompleteCb)
	{
		sprite.color = normalColor;
		previousValue = currentValue;
		animationState = AnimationState.Lerping;
		targetValue = v;
		lerpStartTime = UnityEngine.Time.time;
		lerpDuration = Tuning.Instance.UpgradeUI_TimeToDoLerp;
		lerpEndTime = lerpStartTime + lerpDuration;
		animationCallback = animCompleteCb;
	}

	private void Awake()
	{
		clipper = Util.FindComponent<UIPanel>(base.gameObject);
		sprite = Util.FindComponent<UISlicedSprite>(base.gameObject);
		normalColor = sprite.color;
	}

	private void BeginWaveAnimation()
	{
		animationState = AnimationState.Waving;
		lerpDuration = Tuning.Instance.UpgradeUI_TimeToDoSineWave;
		lerpStartTime = UnityEngine.Time.time;
		lerpEndTime = lerpStartTime + lerpDuration;
	}

	private void BeginSwappingColorAnimation()
	{
		animationState = AnimationState.SwappingColor;
		lerpDuration = Tuning.Instance.UpgradeUI_ColorSwapDuration;
		lerpStartTime = UnityEngine.Time.time;
		lerpEndTime = lerpStartTime + lerpDuration;
	}

	private void Update()
	{
		UpdateAnimations();
	}

	private void UpdateAnimations()
	{
		if (animationState == AnimationState.Inactive)
		{
			return;
		}
		float num = (lerpDuration - (lerpEndTime - UnityEngine.Time.time)) / lerpDuration;
		float num2 = (lerpEndTime - UnityEngine.Time.time) / lerpDuration;
		bool flag = false;
		if (num >= 1f)
		{
			num = 1f;
			flag = true;
		}
		if (animationState == AnimationState.Lerping)
		{
			if (flag)
			{
				BeginWaveAnimation();
			}
			float valueImmediate = Mathf.Lerp(previousValue, targetValue, num);
			SetValueImmediate(valueImmediate);
		}
		else if (animationState == AnimationState.Waving)
		{
			if (flag)
			{
				BeginSwappingColorAnimation();
			}
			float num3 = targetValue - previousValue;
			float upgradeUI_NumSineWaveOscillations = Tuning.Instance.UpgradeUI_NumSineWaveOscillations;
			float num4 = Mathf.Sin(num * 2f * (float)Math.PI * upgradeUI_NumSineWaveOscillations);
			SetValueImmediate(targetValue + num2 * num3 * num4);
		}
		else if (animationState == AnimationState.SwappingColor)
		{
			sprite.color = Color.Lerp(normalColor, fadeToColor, num);
			if (flag)
			{
				animationState = AnimationState.Inactive;
				OnComplete();
			}
		}
	}

	private void OnComplete()
	{
		if (animationCallback != null)
		{
			animationCallback();
		}
	}
}
