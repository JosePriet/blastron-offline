using UnityEngine;

public class AnimationInitializer : MonoBehaviour
{
	public Animation target;

	public string animationName;

	public float normalizedTime;

	public float speed;

	private void Start()
	{
		AnimationState animationState = target[animationName];
		target[animationName].speed = speed;
		animationState.normalizedTime = normalizedTime;
		animationState.enabled = true;
	}
}
