using UnityEngine;

public class UpgradeWheelItem : MonoBehaviour
{
	private UILabel resultLabel;

	private string finalLabelString;

	private float endPos;

	private bool _isEnded;

	private float velocity;

	private bool forceEnd;

	private bool changeColorOnEnd;

	private Color originalColor;

	private bool _isEnabled;

	public float lerpPos;

	private bool _doColorLerp;

	private float colorLerpTimer;

	public void SetChangeColorOnEnd()
	{
		changeColorOnEnd = true;
	}

	public void StartSpin(float initialLerpPos, string initialString, string endString)
	{
		lerpPos = initialLerpPos;
		_isEnded = false;
		forceEnd = false;
		velocity = Tuning.Instance.UpgradeUI_SpinVelocity;
		finalLabelString = endString;
		resultLabel.text = initialString;
		resultLabel.color = originalColor;
		_isEnabled = true;
	}

	public void EndSpin(float endLerpPos)
	{
		_isEnded = true;
		endPos = endLerpPos;
	}

	public void ForceEndSpin()
	{
		forceEnd = true;
	}

	private void Awake()
	{
		resultLabel = base.gameObject.GetComponent<UILabel>();
		_isEnabled = false;
		originalColor = resultLabel.color;
		_doColorLerp = false;
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (_isEnabled)
		{
			lerpPos += velocity * Time.deltaTime;
			if (lerpPos > 1f)
			{
				lerpPos = 0f + (lerpPos - 1f);
				resultLabel.text = finalLabelString;
			}
			if (_isEnded)
			{
				velocity = Mathf.Max(Tuning.Instance.UpgradeUI_SpinMinVelocity, velocity * Tuning.Instance.UpgradeUI_SpinVelocityDecay);
				if (Mathf.Abs(lerpPos - endPos) <= velocity * Time.deltaTime && velocity == Tuning.Instance.UpgradeUI_SpinMinVelocity)
				{
					lerpPos = endPos;
					_isEnabled = false;
					SendMessageUpwards("EnableRings", false);
					if (changeColorOnEnd)
					{
						SendMessageUpwards("StartProgressAnimation");
						_doColorLerp = true;
						colorLerpTimer = Time.time;
					}
				}
			}
			if (forceEnd)
			{
				lerpPos = endPos;
				_isEnabled = false;
			}
			base.transform.localPosition = Vector3.Lerp(Tuning.Instance.UpgradeUI_WheelTop, Tuning.Instance.UpgradeUI_WheelBottom, lerpPos);
		}
		if (_doColorLerp)
		{
			float num = (Time.time - colorLerpTimer) / Tuning.Instance.UpgradeUI_ColorSwapDuration;
			resultLabel.color = Color.Lerp(originalColor, Tuning.Instance.UpgradeUI_UpgradeColor, num);
			if (num >= 1f)
			{
				_doColorLerp = false;
			}
		}
	}
}
