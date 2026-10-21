using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class SliderJoint : FarseerJoint
	{
		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private float _bias;

		private float _gamma;

		private float _impulse;

		private float _mass;

		private FVector2 _u;

		public float MaxLength { get; set; }

		public float MinLength { get; set; }

		public float Frequency { get; set; }

		public float DampingRatio { get; set; }

		public override FVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.GetWorldPoint(LocalAnchorA);
			}
		}

		public override FVector2 WorldAnchorB
		{
			get
			{
				return base.BodyB.GetWorldPoint(LocalAnchorB);
			}
			set
			{
			}
		}

		internal SliderJoint()
		{
			base.JointType = JointType.Slider;
		}

		public SliderJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB, float minLength, float maxlength)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Slider;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
			MaxLength = maxlength;
			MinLength = minLength;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			return inv_dt * _impulse * _u;
		}

		public override float GetReactionTorque(float inv_dt)
		{
			return 0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			Body bodyA = base.BodyA;
			Body bodyB = base.BodyB;
			Transform transform;
			bodyA.GetTransform(out transform);
			Transform transform2;
			bodyB.GetTransform(out transform2);
			FVector2 fVector = MathUtils.Mul(ref transform.q, LocalAnchorA - bodyA.LocalCenter);
			FVector2 fVector2 = MathUtils.Mul(ref transform2.q, LocalAnchorB - bodyB.LocalCenter);
			_u = bodyB.Sweep.C + fVector2 - bodyA.Sweep.C - fVector;
			float num = _u.Length();
			if (!(num < MaxLength) || !(num > MinLength))
			{
				if (num > 0.005f)
				{
					_u *= 1f / num;
				}
				else
				{
					_u = FVector2.Zero;
				}
				float num2 = MathUtils.Cross(fVector, _u);
				float num3 = MathUtils.Cross(fVector2, _u);
				float num4 = bodyA.InvMass + bodyA.InvI * num2 * num2 + bodyB.InvMass + bodyB.InvI * num3 * num3;
				_mass = ((num4 == 0f) ? 0f : (1f / num4));
				if (Frequency > 0f)
				{
					float num5 = num - MaxLength;
					float num6 = (float)Math.PI * 2f * Frequency;
					float num7 = 2f * _mass * DampingRatio * num6;
					float num8 = _mass * num6 * num6;
					_gamma = data.step.dt * (num7 + data.step.dt * num8);
					_gamma = ((_gamma == 0f) ? 0f : (1f / _gamma));
					_bias = num5 * data.step.dt * num8 * _gamma;
					_mass = num4 + _gamma;
					_mass = ((_mass == 0f) ? 0f : (1f / _mass));
				}
				if (Settings.EnableWarmstarting)
				{
					_impulse *= data.step.dtRatio;
					FVector2 fVector3 = _impulse * _u;
					bodyA.LinearVelocityInternal -= bodyA.InvMass * fVector3;
					bodyA.AngularVelocityInternal -= bodyA.InvI * MathUtils.Cross(fVector, fVector3);
					bodyB.LinearVelocityInternal += bodyB.InvMass * fVector3;
					bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(fVector2, fVector3);
				}
				else
				{
					_impulse = 0f;
				}
			}
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			Body bodyA = base.BodyA;
			Body bodyB = base.BodyB;
			Transform transform;
			bodyA.GetTransform(out transform);
			Transform transform2;
			bodyB.GetTransform(out transform2);
			FVector2 fVector = MathUtils.Mul(ref transform.q, LocalAnchorA - bodyA.LocalCenter);
			FVector2 fVector2 = MathUtils.Mul(ref transform2.q, LocalAnchorB - bodyB.LocalCenter);
			float num = (bodyB.Sweep.C + fVector2 - bodyA.Sweep.C - fVector).Length();
			if (!(num < MaxLength) || !(num > MinLength))
			{
				FVector2 fVector3 = bodyA.LinearVelocityInternal + MathUtils.Cross(bodyA.AngularVelocityInternal, fVector);
				FVector2 fVector4 = bodyB.LinearVelocityInternal + MathUtils.Cross(bodyB.AngularVelocityInternal, fVector2);
				float num2 = FVector2.Dot(_u, fVector4 - fVector3);
				float num3 = (0f - _mass) * (num2 + _bias + _gamma * _impulse);
				_impulse += num3;
				FVector2 fVector5 = num3 * _u;
				bodyA.LinearVelocityInternal -= bodyA.InvMass * fVector5;
				bodyA.AngularVelocityInternal -= bodyA.InvI * MathUtils.Cross(fVector, fVector5);
				bodyB.LinearVelocityInternal += bodyB.InvMass * fVector5;
				bodyB.AngularVelocityInternal += bodyB.InvI * MathUtils.Cross(fVector2, fVector5);
			}
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			if (Frequency > 0f)
			{
				return true;
			}
			Body bodyA = base.BodyA;
			Body bodyB = base.BodyB;
			Transform transform;
			bodyA.GetTransform(out transform);
			Transform transform2;
			bodyB.GetTransform(out transform2);
			FVector2 fVector = MathUtils.Mul(ref transform.q, LocalAnchorA - bodyA.LocalCenter);
			FVector2 fVector2 = MathUtils.Mul(ref transform2.q, LocalAnchorB - bodyB.LocalCenter);
			FVector2 u = bodyB.Sweep.C + fVector2 - bodyA.Sweep.C - fVector;
			float num = u.Length();
			if (num < MaxLength && num > MinLength)
			{
				return true;
			}
			if (num == 0f)
			{
				return true;
			}
			u /= num;
			float a = num - MaxLength;
			a = MathUtils.Clamp(a, -0.2f, 0.2f);
			float num2 = (0f - _mass) * a;
			_u = u;
			FVector2 fVector3 = num2 * _u;
			bodyA.Sweep.C -= bodyA.InvMass * fVector3;
			bodyA.Sweep.A -= bodyA.InvI * MathUtils.Cross(fVector, fVector3);
			bodyB.Sweep.C += bodyB.InvMass * fVector3;
			bodyB.Sweep.A += bodyB.InvI * MathUtils.Cross(fVector2, fVector3);
			bodyA.SynchronizeTransform();
			bodyB.SynchronizeTransform();
			return Math.Abs(a) < 0.005f;
		}
	}
}
