using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director.Runtime
{
	public abstract class BlendableTrackInstance : TrackInstance
	{
		public static float Blend(BlendMode mode, float t)
		{
			switch (mode)
			{
			case BlendMode.Cut:
				t = 0f;
				break;
			case BlendMode.EaseIn:
				t = EZAnimation.sinusOut(t, 0f, 1f, 1f);
				break;
			case BlendMode.EaseOut:
				t = EZAnimation.sinusIn(t, 0f, 1f, 1f);
				break;
			case BlendMode.CubicIn:
				t = EZAnimation.cubicIn(t, 0f, 1f, 1f);
				break;
			case BlendMode.CubicOut:
				t = EZAnimation.cubicOut(t, 0f, 1f, 1f);
				break;
			case BlendMode.EaseOutIn:
				t = EZAnimation.sinusInOut(t, 0f, 1f, 1f);
				break;
			case BlendMode.EaseInOut:
				t = EZAnimation.sinusOutIn(t, 0f, 1f, 1f);
				break;
			case BlendMode.CubicInOut:
				t = EZAnimation.cubicInOut(t, 0f, 1f, 1f);
				break;
			case BlendMode.CubicOutIn:
				t = EZAnimation.cubicOutIn(t, 0f, 1f, 1f);
				break;
			}
			return t;
		}

		public override void Update(float frameTime)
		{
			int frameNumber = (int)frameTime;
			KeyFrame keyFrame = LowerEqual(frameNumber);
			KeyFrame keyFrame2 = Greater(frameNumber);
			if (keyFrame != null && keyFrame2 != null)
			{
				float t = Mathf.Clamp01((frameTime - (float)keyFrame.frame) / (float)(keyFrame2.frame - keyFrame.frame));
				t = Blend(keyFrame.mode, t);
				Apply(t, keyFrame, keyFrame2);
			}
			else if (keyFrame != null)
			{
				Apply(0f, keyFrame, keyFrame);
			}
		}

		public KeyFrame LowerEqual(int frameNumber)
		{
			KeyFrame result = null;
			foreach (KeyFrame frame in _track.frames)
			{
				if (frame.frame <= frameNumber)
				{
					result = frame;
					continue;
				}
				break;
			}
			return result;
		}

		public KeyFrame Greater(int frameNumber)
		{
			foreach (KeyFrame frame in _track.frames)
			{
				if (frame.frame > frameNumber)
				{
					return frame;
				}
			}
			return null;
		}

		public abstract void Apply(float blend, KeyFrame from, KeyFrame to);
	}
}
