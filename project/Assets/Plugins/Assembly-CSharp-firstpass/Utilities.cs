using UnityEngine;

public static class Utilities
{
	public static float VolumeScalarToDecibels(float VolumeScalar)
	{
		return 20f * Mathf.Log10(Mathf.Clamp(VolumeScalar, 0.0001f, 1f));
	}

	public static float VolumeDecibelsToScalar(float VolumeDecibels)
	{
		return Mathf.Pow(10f, Mathf.Clamp(VolumeDecibels, -80f, 0f) / 20f);
	}

	public static float PitchScalarToSemitones(float PitchScalar)
	{
		return 12f * (Mathf.Log10(Mathf.Clamp(PitchScalar, 0.0625f, 16f)) / 0.30103f);
	}

	public static float PitchSemitonesToScalar(float PitchSemitones)
	{
		return Mathf.Pow(2f, Mathf.Clamp(PitchSemitones, -48f, 48f) / 12f);
	}

	public static bool AreFloatsEqual(float A, float B)
	{
		return !(Mathf.Abs(A - B) > float.Epsilon);
	}
}
