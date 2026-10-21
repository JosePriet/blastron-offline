using System;
using UnityEngine;

public class AudioCategory : MonoBehaviour
{
	[HideInInspector]
	public float m_VolumeDB;

	[NonSerialized]
	private bool m_IsSetUp;

	[NonSerialized]
	private AudioLerper m_VolumeScalarLerper = new AudioLerper();

	public void SetUp()
	{
		if (!m_IsSetUp)
		{
			m_VolumeScalarLerper.Lerp(Utilities.VolumeDecibelsToScalar(m_VolumeDB), 0f);
			m_IsSetUp = true;
		}
	}

	public void ShutDown()
	{
		m_IsSetUp = false;
	}

	public void UpdateLerpers(float DeltaTime)
	{
		SetUp();
		m_VolumeScalarLerper.Update(DeltaTime);
	}

	public float GetVolumeScalar()
	{
		SetUp();
		return m_VolumeScalarLerper.GetValue();
	}

	public void SetVolumeScalar(float VolumeScalar, float TransitionTime)
	{
		SetUp();
		m_VolumeScalarLerper.Lerp(Mathf.Clamp(VolumeScalar, 0.0001f, 1f), TransitionTime);
	}

	public void SetVolumeScalar(float VolumeScalar)
	{
		SetVolumeScalar(VolumeScalar, 0f);
	}
}
