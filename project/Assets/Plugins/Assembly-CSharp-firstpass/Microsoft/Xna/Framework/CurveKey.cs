using System;

namespace Microsoft.Xna.Framework
{
	public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
	{
		private CurveContinuity continuity;

		private float position;

		private float tangentIn;

		private float tangentOut;

		private float value;

		public CurveContinuity Continuity
		{
			get
			{
				return continuity;
			}
			set
			{
				continuity = value;
			}
		}

		public float Position
		{
			get
			{
				return position;
			}
		}

		public float TangentIn
		{
			get
			{
				return tangentIn;
			}
			set
			{
				tangentIn = value;
			}
		}

		public float TangentOut
		{
			get
			{
				return tangentOut;
			}
			set
			{
				tangentOut = value;
			}
		}

		public float Value
		{
			get
			{
				return value;
			}
			set
			{
				this.value = value;
			}
		}

		public CurveKey(float position, float value)
			: this(position, value, 0f, 0f, CurveContinuity.Smooth)
		{
		}

		public CurveKey(float position, float value, float tangentIn, float tangentOut)
			: this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
		{
		}

		public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
		{
			this.position = position;
			this.value = value;
			this.tangentIn = tangentIn;
			this.tangentOut = tangentOut;
			this.continuity = continuity;
		}

		public int CompareTo(CurveKey other)
		{
			return position.CompareTo(other.position);
		}

		public bool Equals(CurveKey other)
		{
			return this == other;
		}

		public CurveKey Clone()
		{
			return new CurveKey(position, value, tangentIn, tangentOut, continuity);
		}

		public override bool Equals(object obj)
		{
			return obj is CurveKey && (CurveKey)obj == this;
		}

		public override int GetHashCode()
		{
			return position.GetHashCode() ^ value.GetHashCode() ^ tangentIn.GetHashCode() ^ tangentOut.GetHashCode() ^ continuity.GetHashCode();
		}

		public static bool operator !=(CurveKey a, CurveKey b)
		{
			return !(a == b);
		}

		public static bool operator ==(CurveKey a, CurveKey b)
		{
			if (object.Equals(a, null))
			{
				return object.Equals(b, null);
			}
			if (object.Equals(b, null))
			{
				return object.Equals(a, null);
			}
			return a.position == b.position && a.value == b.value && a.tangentIn == b.tangentIn && a.tangentOut == b.tangentOut && a.continuity == b.continuity;
		}
	}
}
