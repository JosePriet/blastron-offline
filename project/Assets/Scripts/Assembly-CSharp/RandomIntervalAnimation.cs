using SmoothMoves;
using UnityEngine;

public class RandomIntervalAnimation : MonoBehaviour
{
	public string AnimationName;

	public float MinInterval;

	public float MaxInterval;

	private float timeLeft;

	private BoneAnimation _smCharacter;

	private Animation _animation;

	private void Awake()
	{
		_animation = GetComponentInChildren<Animation>();
	}

	private void Start()
	{
		_smCharacter = base.gameObject.GetComponent<BoneAnimation>();
		timeLeft = GetNextIntervalTime();
	}

	private void Update()
	{
		if (!string.IsNullOrEmpty(AnimationName) && _smCharacter != null)
		{
			timeLeft -= Time.deltaTime;
			if (timeLeft <= 0f && Misc.CurrentlyPlayingAnimCount(_animation) <= 1)
			{
				_smCharacter.Play(AnimationName, PlayMode.StopSameLayer);
				timeLeft = GetNextIntervalTime();
			}
		}
	}

	private float GetNextIntervalTime()
	{
		return Random.Range(MinInterval, MaxInterval);
	}
}
