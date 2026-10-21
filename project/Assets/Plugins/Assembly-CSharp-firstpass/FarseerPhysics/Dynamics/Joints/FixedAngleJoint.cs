using System;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class FixedAngleJoint : FarseerJoint
	{
		public float BiasFactor;

		public float MaxImpulse;

		public float Softness;

		private float _bias;

		private float _jointError;

		private float _massFactor;

		private float _targetAngle;

		public float TargetAngle
		{
			get
			{
				return _targetAngle;
			}
			set
			{
				if (value != _targetAngle)
				{
					_targetAngle = value;
					WakeBodies();
				}
			}
		}

		public override FVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.Position;
			}
		}

		public override FVector2 WorldAnchorB
		{
			get
			{
				return base.BodyA.Position;
			}
			set
			{
			}
		}

		public FixedAngleJoint(Body bodyA)
			: base(bodyA)
		{
			base.JointType = JointType.FixedAngle;
			TargetAngle = 0f;
			BiasFactor = 0.2f;
			Softness = 0f;
			MaxImpulse = float.MaxValue;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			return FVector2.Zero;
		}

		public override float GetReactionTorque(float inv_dt)
		{
			return 0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			_jointError = base.BodyA.Sweep.A - TargetAngle;
			_bias = (0f - BiasFactor) * data.step.inv_dt * _jointError;
			_massFactor = (1f - Softness) / base.BodyA.InvI;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			float value = (_bias - base.BodyA.AngularVelocity) * _massFactor;
			base.BodyA.AngularVelocity += base.BodyA.InvI * (float)Math.Sign(value) * Math.Min(Math.Abs(value), MaxImpulse);
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}
