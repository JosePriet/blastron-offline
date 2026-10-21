using System;
using System.Collections;
using UnityEngine;

public class Brio : MonoBehaviour
{
	public enum Mode
	{
		kJumpingBean = 0,
		kShiver = 1,
		kWobblyScale = 2
	}

	private interface ILifeFn
	{
		void Start(Transform t, float duration, float amplitude, float oscillations);

		bool Update(float elapsed);
	}

	private abstract class LifeBase : ILifeFn
	{
		protected Vector3 RestPos { get; set; }

		protected Quaternion RestRot { get; set; }

		protected Vector3 RestScale { get; set; }

		protected float Duration { get; set; }

		protected float Amplitude { get; set; }

		protected float Oscillations { get; set; }

		protected float Elapsed { get; set; }

		protected Transform Transform { get; set; }

		public virtual void Start(Transform t, float duration, float amplitude, float oscillations)
		{
			Remember(t);
			Duration = duration;
			Amplitude = amplitude;
			Oscillations = oscillations;
			Elapsed = 0f;
		}

		public abstract bool Update(float elapsed);

		protected void Remember(Transform t)
		{
			RestPos = t.position;
			RestRot = t.rotation;
			RestScale = t.localScale;
			Transform = t;
		}

		protected void Restore()
		{
			Transform.position = RestPos;
			Transform.rotation = RestRot;
			Transform.localScale = RestScale;
		}

		protected bool Finished()
		{
			if (Elapsed >= Duration)
			{
				Restore();
				return true;
			}
			return false;
		}
	}

	private class JumpingBean : LifeBase
	{
		private Vector3 _vel;

		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float y = base.Amplitude * Mathf.Abs(Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI / base.Duration)) * H0(base.Elapsed / base.Duration);
			base.Transform.position = base.RestPos + new Vector3(0f, y, 0f);
			return !Finished();
		}
	}

	private class Shiver : LifeBase
	{
		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float y = base.Amplitude * Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI * 2f) * H0(base.Elapsed / base.Duration);
			base.Transform.rotation = base.RestRot * Quaternion.Euler(0f, y, 0f);
			return !Finished();
		}
	}

	private class WobblyScale : LifeBase
	{
		public override bool Update(float elapsed)
		{
			base.Elapsed += elapsed;
			float num = 1f + base.Amplitude * (Mathf.Sin(base.Oscillations * base.Elapsed * (float)Math.PI * 2f) * H0(base.Elapsed / base.Duration));
			base.Transform.localScale = base.RestScale * num;
			return !Finished();
		}
	}

	public Mode _eMode;

	public float MinInterval = 4f;

	public float MaxInterval = 16f;

	public float Duration = 1f;

	public float Oscillations = 3f;

	public float Amplitude = 0.3f;

	private Rigidbody _rb;

	private bool Resting { get; set; }

	private bool brioEnabled { get; set; }

	private bool Kinematic
	{
		get
		{
			if (_rb != null)
			{
				return _rb.isKinematic;
			}
			return true;
		}
		set
		{
			if (_rb != null)
			{
				_rb.isKinematic = value;
			}
		}
	}

	private void Start()
	{
		EnableBrio(true);
	}

	public void EnableBrio(bool bEnable)
	{
		brioEnabled = bEnable;
		if (brioEnabled)
		{
			_rb = GetComponent<Rigidbody>();
			if (_rb != null)
			{
				_rb.isKinematic = true;
			}
			StartCoroutine(Waiting());
		}
		else
		{
			StopAllCoroutines();
		}
	}

	private IEnumerator Waiting()
	{
		while (true)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(MinInterval, MaxInterval));
			switch (_eMode)
			{
			case Mode.kJumpingBean:
				yield return StartCoroutine(Animate(new JumpingBean()));
				break;
			case Mode.kShiver:
				yield return StartCoroutine(Animate(new Shiver()));
				break;
			case Mode.kWobblyScale:
				yield return StartCoroutine(Animate(new WobblyScale()));
				break;
			}
		}
	}

	private IEnumerator Animate<T>(T t) where T : ILifeFn
	{
		t.Start(base.transform, Duration, Amplitude, Oscillations);
		do
		{
			yield return new WaitForFixedUpdate();
		}
		while (t.Update(Time.fixedDeltaTime) && Kinematic);
	}

	private void OnCollisionEnter(Collision collision)
	{
		Kinematic = false;
	}

	private void OnCollisionExit(Collision collision)
	{
		Kinematic = true;
	}

	private static float H0(float t)
	{
		return 2f * t * t * t - 3f * t * t + 1f;
	}
}
