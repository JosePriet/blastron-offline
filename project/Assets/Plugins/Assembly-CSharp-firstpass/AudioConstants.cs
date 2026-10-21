using UnityEngine;

public static class AudioConstants
{
	public const float s_VolumeScalarMin = 0.0001f;

	public const float s_VolumeScalarMax = 1f;

	public const float s_VolumeScalarDefault = 1f;

	public const float s_VolumeDecibelsMin = -80f;

	public const float s_VolumeDecibelsMax = 0f;

	public const float s_VolumeDecibelsDefault = 0f;

	public const float s_PitchScalarMin = 0.0625f;

	public const float s_PitchScalarMax = 16f;

	public const float s_PitchScalarDefault = 1f;

	public const float s_PitchSemitonesMin = -48f;

	public const float s_PitchSemitonesMax = 48f;

	public const float s_PitchSemitonesDefault = 0f;

	public const int s_PriorityMin = 0;

	public const int s_PriorityMax = 255;

	public const int s_PriorityDefault = 128;

	public const float s_DopplerLevelMin = 0f;

	public const float s_DopplerLevelMax = 2f;

	public const float s_DopplerLevelDefault = 0f;

	public const float s_MinDistanceMin = 0f;

	public const float s_MinDistanceMax = 1000f;

	public const float s_MinDistanceDefault = 1f;

	public const float s_MaxDistanceMin = 0f;

	public const float s_MaxDistanceMax = 1000f;

	public const float s_MaxDistanceDefault = 10f;

	public const AudioRolloffMode s_RolloffModeDefault = AudioRolloffMode.Logarithmic;

	public const float s_FadeTimeMin = 0f;

	public const float s_FadeTimeMax = 30f;
}
