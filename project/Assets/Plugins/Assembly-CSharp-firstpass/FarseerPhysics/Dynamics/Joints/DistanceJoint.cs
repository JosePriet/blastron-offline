using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class DistanceJoint : FarseerJoint
	{
		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private float _bias;

		private float _gamma;

		private float _impulse;

		private int m_indexA;

		private int m_indexB;

		private FVector2 _u;

		private FVector2 m_rA;

		private FVector2 m_rB;

		private FVector2 m_localCenterA;

		private FVector2 m_localCenterB;

		private float m_invMassA;

		private float m_invMassB;

		private float m_invIA;

		private float m_invIB;

		private float _mass;

		public float Length { get; set; }

		public float Frequency { get; set; }

		public float DampingRatio { get; set; }

		public sealed override FVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.GetWorldPoint(LocalAnchorA);
			}
		}

		public sealed override FVector2 WorldAnchorB
		{
			get
			{
				return base.BodyB.GetWorldPoint(LocalAnchorB);
			}
			set
			{
			}
		}

		internal DistanceJoint()
		{
			base.JointType = JointType.Distance;
		}

		public DistanceJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Distance;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
			Length = (WorldAnchorB - WorldAnchorA).Length();
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
			m_indexA = base.BodyA.IslandIndex;
			m_indexB = base.BodyB.IslandIndex;
			m_localCenterA = base.BodyA.Sweep.LocalCenter;
			m_localCenterB = base.BodyB.Sweep.LocalCenter;
			m_invMassA = base.BodyA.InvMass;
			m_invMassB = base.BodyB.InvMass;
			m_invIA = base.BodyA.InvI;
			m_invIB = base.BodyB.InvI;
			FVector2 c = data.positions[m_indexA].c;
			float a = data.positions[m_indexA].a;
			FVector2 v = data.velocities[m_indexA].v;
			float num = data.velocities[m_indexA].w;
			FVector2 c2 = data.positions[m_indexB].c;
			float a2 = data.positions[m_indexB].a;
			FVector2 v2 = data.velocities[m_indexB].v;
			float num2 = data.velocities[m_indexB].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			m_rA = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			m_rB = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			_u = c2 + m_rB - c - m_rA;
			float num3 = _u.Length();
			if (num3 > 0.005f)
			{
				_u *= 1f / num3;
			}
			else
			{
				_u = FVector2.Zero;
			}
			float num4 = MathUtils.Cross(m_rA, _u);
			float num5 = MathUtils.Cross(m_rB, _u);
			float num6 = m_invMassA + m_invIA * num4 * num4 + m_invMassB + m_invIB * num5 * num5;
			_mass = ((num6 == 0f) ? 0f : (1f / num6));
			if (Frequency > 0f)
			{
				float num7 = num3 - Length;
				float num8 = (float)Math.PI * 2f * Frequency;
				float num9 = 2f * _mass * DampingRatio * num8;
				float num10 = _mass * num8 * num8;
				float dt = data.step.dt;
				_gamma = dt * (num9 + dt * num10);
				_gamma = ((_gamma == 0f) ? 0f : (1f / _gamma));
				_bias = num7 * dt * num10 * _gamma;
				num6 += _gamma;
				_mass = ((num6 == 0f) ? 0f : (1f / num6));
			}
			else
			{
				_gamma = 0f;
				_bias = 0f;
			}
			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.step.dtRatio;
				FVector2 fVector = _impulse * _u;
				v -= m_invMassA * fVector;
				num -= m_invIA * MathUtils.Cross(m_rA, fVector);
				v2 += m_invMassB * fVector;
				num2 += m_invIB * MathUtils.Cross(m_rB, fVector);
			}
			else
			{
				_impulse = 0f;
			}
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = num;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = num2;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			FVector2 v = data.velocities[m_indexA].v;
			float w = data.velocities[m_indexA].w;
			FVector2 v2 = data.velocities[m_indexB].v;
			float w2 = data.velocities[m_indexB].w;
			FVector2 fVector = v + MathUtils.Cross(w, m_rA);
			FVector2 fVector2 = v2 + MathUtils.Cross(w2, m_rB);
			float num = FVector2.Dot(_u, fVector2 - fVector);
			float num2 = (0f - _mass) * (num + _bias + _gamma * _impulse);
			_impulse += num2;
			FVector2 fVector3 = num2 * _u;
			v -= m_invMassA * fVector3;
			w -= m_invIA * MathUtils.Cross(m_rA, fVector3);
			v2 += m_invMassB * fVector3;
			w2 += m_invIB * MathUtils.Cross(m_rB, fVector3);
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = w;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = w2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			if (Frequency > 0f)
			{
				return true;
			}
			FVector2 c = data.positions[m_indexA].c;
			float a = data.positions[m_indexA].a;
			FVector2 c2 = data.positions[m_indexB].c;
			float a2 = data.positions[m_indexB].a;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			FVector2 fVector3 = c2 + fVector2 - c - fVector;
			float num = fVector3.Length();
			fVector3.Normalize();
			float a3 = num - Length;
			a3 = MathUtils.Clamp(a3, -0.2f, 0.2f);
			float num2 = (0f - _mass) * a3;
			FVector2 fVector4 = num2 * fVector3;
			c -= m_invMassA * fVector4;
			a -= m_invIA * MathUtils.Cross(fVector, fVector4);
			c2 += m_invMassB * fVector4;
			a2 += m_invIB * MathUtils.Cross(fVector2, fVector4);
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = a;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = a2;
			return Math.Abs(a3) < 0.005f;
		}
	}
}
