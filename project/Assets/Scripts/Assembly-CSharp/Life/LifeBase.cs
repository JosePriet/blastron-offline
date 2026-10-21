using UnityEngine;

namespace Life
{
	public abstract class LifeBase : ILifeFn
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

		protected virtual void OnFinished()
		{
		}

		protected void Remember(Transform t)
		{
			if (t != null)
			{
				RestPos = t.position;
				RestRot = t.rotation;
				RestScale = t.localScale;
				Transform = t;
			}
		}

		public virtual void Restore()
		{
			if (Transform != null)
			{
				Transform.position = RestPos;
				Transform.rotation = RestRot;
				Transform.localScale = RestScale;
			}
		}

		protected bool Finished()
		{
			if (Duration > 0f && Elapsed >= Duration)
			{
				Restore();
				OnFinished();
				return true;
			}
			return false;
		}
	}
}
