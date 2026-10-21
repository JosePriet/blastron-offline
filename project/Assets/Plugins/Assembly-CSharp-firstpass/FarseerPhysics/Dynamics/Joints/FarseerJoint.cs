using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public abstract class FarseerJoint
	{
		public float Breakpoint = float.MaxValue;

		internal JointEdge EdgeA = new JointEdge();

		internal JointEdge EdgeB = new JointEdge();

		public bool Enabled = true;

		internal bool IslandFlag;

		protected int m_index;

		public JointType JointType { get; protected set; }

		public Body BodyA { get; set; }

		public Body BodyB { get; set; }

		public abstract FVector2 WorldAnchorA { get; }

		public abstract FVector2 WorldAnchorB { get; set; }

		public object UserData { get; set; }

		public bool Active
		{
			get
			{
				return BodyA.Enabled && BodyB.Enabled;
			}
		}

		public bool CollideConnected { get; set; }

		public event Action<FarseerJoint, float> Broke;

		protected FarseerJoint()
		{
		}

		protected FarseerJoint(Body body, Body bodyB)
		{
			BodyA = body;
			BodyB = bodyB;
			CollideConnected = false;
		}

		protected FarseerJoint(Body body)
		{
			BodyA = body;
			CollideConnected = false;
		}

		public abstract FVector2 GetReactionForce(float inv_dt);

		public abstract float GetReactionTorque(float inv_dt);

		protected void WakeBodies()
		{
			BodyA.Awake = true;
			if (BodyB != null)
			{
				BodyB.Awake = true;
			}
		}

		public bool IsFixedType()
		{
			return JointType == JointType.FixedRevolute || JointType == JointType.FixedDistance || JointType == JointType.FixedPrismatic || JointType == JointType.FixedLine || JointType == JointType.FixedMouse || JointType == JointType.FixedAngle || JointType == JointType.FixedFriction;
		}

		internal abstract void InitVelocityConstraints(ref SolverData data);

		internal void Validate(float invDT)
		{
			if (!Enabled)
			{
				return;
			}
			float num = GetReactionForce(invDT).Length();
			if (!(Math.Abs(num) <= Breakpoint))
			{
				Enabled = false;
				if (Broke != null)
				{
					Broke(this, num);
				}
			}
		}

		internal abstract void SolveVelocityConstraints(ref SolverData data);

		internal abstract bool SolvePositionConstraints(ref SolverData data);
	}
}
