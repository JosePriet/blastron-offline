using System;
using UnityEngine;

public class TintAnimation : MonoBehaviour
{
	[Serializable]
	public class TintAnimData
	{
		public float duration;

		public Color tintTo;
	}

	public TintAnimData[] tintAnimData;

	public UIWidget target;

	private int animIdx = -1;

	private float animStartTime;

	private Color startColor;

	public void Animate()
	{
		animIdx = 0;
		animStartTime = Time.time;
		startColor = target.color;
	}

	private void Update()
	{
		if (animIdx < 0)
		{
			return;
		}
		TintAnimData tintAnimData = this.tintAnimData[animIdx];
		float num = (Time.time - animStartTime) / tintAnimData.duration;
		bool flag = false;
		if (num >= 1f)
		{
			num = 1f;
			flag = true;
		}
		target.color = Color.Lerp(startColor, tintAnimData.tintTo, num);
		if (flag)
		{
			animStartTime = Time.time;
			animIdx++;
			startColor = tintAnimData.tintTo;
			if (animIdx >= this.tintAnimData.Length)
			{
				animIdx = -1;
			}
		}
	}
}
