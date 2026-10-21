using Holoville.HOTween;
using UnityEngine;

public class Transition_Bounce_Intro_FromBottom : EBUI_Transition
{
	public override void StartTransition()
	{
		float z = base.gameObject.transform.localPosition.z;
		base.gameObject.transform.localPosition = new Vector3(0f, (float)(-Screen.height) * 2f, z);
		TweenParms tweenParms = new TweenParms();
		tweenParms.Prop("localPosition", new Vector3(0f, 0f, z));
		tweenParms.Ease(EaseType.EaseOutBounce);
		tweenParms.Delay(info.delay);
		tweenParms.OnComplete(StopTransition);
		HOTween.To(base.gameObject.transform, info.duration, tweenParms);
	}
}
