using UnityEngine;

public class AnimateRotation : EZAnimation
{
	protected Vector3 start;

	protected Vector3 delta;

	protected Vector3 end;

	protected GameObject subject;

	protected Transform subTrans;

	protected Vector3 temp;

	public AnimateRotation()
	{
		type = ANIM_TYPE.Rotate;
	}

	public override object GetSubject()
	{
		return subject;
	}

	public override void _end()
	{
		if (subTrans != null)
		{
			subTrans.localEulerAngles = end;
		}
		base._end();
	}

	protected override void LoopReset()
	{
		if (base.Mode == ANIM_MODE.By && !restartOnRepeat)
		{
			start = end;
			end = start + delta;
		}
	}

	protected override void DoAnim()
	{
		if (subTrans == null)
		{
			_stop();
			return;
		}
		temp.x = interpolator(timeElapsed, start.x, delta.x, interval);
		temp.y = interpolator(timeElapsed, start.y, delta.y, interval);
		temp.z = interpolator(timeElapsed, start.z, delta.z, interval);
		subTrans.localEulerAngles = temp;
	}

	public static AnimateRotation Do(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateRotation animateRotation = (AnimateRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.Rotate);
		animateRotation.Start(sub, mode, begin, dest, interp, dur, delay, startDel, del);
		return animateRotation;
	}

	public static AnimateRotation Do(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		AnimateRotation animateRotation = (AnimateRotation)EZAnimator.instance.GetAnimation(ANIM_TYPE.Rotate);
		animateRotation.Start(sub, mode, dest, interp, dur, delay, startDel, del);
		return animateRotation;
	}

	public override bool Start(GameObject sub, AnimParams parms)
	{
		pingPong = parms.pingPong;
		restartOnRepeat = parms.restartOnRepeat;
		repeatDelay = parms.repeatDelay;
		if (parms.mode == ANIM_MODE.FromTo)
		{
			Start(sub, parms.mode, parms.vec, parms.vec2, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		else
		{
			Start(sub, parms.mode, sub.transform.localEulerAngles, parms.vec, EZAnimation.GetInterpolator(parms.easing), parms.duration, parms.delay, null, parms.transition.OnAnimEnd);
		}
		return true;
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		Start(sub, mode, sub.transform.localEulerAngles, dest, interp, dur, delay, startDel, del);
	}

	public void Start(GameObject sub, ANIM_MODE mode, Vector3 begin, Vector3 dest, Interpolator interp, float dur, float delay, CompletionDelegate startDel, CompletionDelegate del)
	{
		if (!(sub == null))
		{
			subject = sub;
			subTrans = subject.transform;
			start = begin;
			m_mode = mode;
			if (mode == ANIM_MODE.By)
			{
				delta = dest;
			}
			else
			{
				delta = new Vector3(dest.x - start.x, dest.y - start.y, dest.z - start.z);
			}
			end = start + delta;
			interpolator = interp;
			duration = dur;
			m_wait = delay;
			completedDelegate = del;
			startDelegate = startDel;
			StartCommon();
			EZAnimator.instance.AddAnimation(this);
			if (mode == ANIM_MODE.FromTo && delay == 0f)
			{
				subTrans.localEulerAngles = start;
			}
		}
	}

	public void Start()
	{
		if (!(subject == null))
		{
			direction = 1f;
			timeElapsed = 0f;
			wait = m_wait;
			if (m_mode == ANIM_MODE.By)
			{
				start = subject.transform.localEulerAngles;
				end = start + delta;
			}
			EZAnimator.instance.AddAnimation(this);
		}
	}
}
