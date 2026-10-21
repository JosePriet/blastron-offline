using System;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
	public struct ShakeInfo
	{
		public float _magnitude;

		public int _cycles;

		public ShakeInfo(float magnitude, int cycles)
		{
			_magnitude = magnitude;
			_cycles = cycles;
		}
	}

	private const float decreasePercentage = 0.5f;

	private const float speed = 50f;

	private Vector3 _shake;

	private Vector3 _rattle;

	private bool _rattling;

	private IEnumerator ShakeRoutine(float magnitude, int cycles)
	{
		float startTime = Time.time;
		float shakeDistance = magnitude;
		while (cycles > 0)
		{
			float timer = (Time.time - startTime) * 50f;
			_shake.x = Mathf.Sin(timer) * shakeDistance;
			_shake.y = Mathf.Cos(timer) * shakeDistance;
			if (timer > (float)Math.PI * 2f)
			{
				startTime = Time.time;
				shakeDistance *= 0.5f;
				cycles--;
			}
			yield return new WaitForFixedUpdate();
		}
		_shake = Vector3.zero;
	}

	private IEnumerator RattleRoutine(float magnitude)
	{
		_rattling = true;
		float startTime = Time.time;
		while (_rattling)
		{
			float timer = (Time.time - startTime) * 50f;
			_shake.x = Mathf.Sin(timer) * magnitude;
			_shake.y = Mathf.Cos(timer) * magnitude;
			yield return new WaitForFixedUpdate();
		}
	}

	private void LateUpdate()
	{
		base.transform.position += _shake + _rattle;
	}

	private void Shake(ShakeInfo s)
	{
		StartCoroutine(ShakeRoutine(s._magnitude, s._cycles));
	}

	private void BeginRattle(ShakeInfo s)
	{
		StartCoroutine(RattleRoutine(s._magnitude));
	}

	private void EndRattle()
	{
		_rattling = false;
		_rattle = Vector3.zero;
	}
}
