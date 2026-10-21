using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class PulleyJoint : FarseerJoint
	{
		public FVector2 GroundAnchorA;

		public FVector2 GroundAnchorB;

		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private float _impulse;

		protected float _limitImpulse1;

		protected float _limitImpulse2;

		private float m_constant;

		private int m_indexA;

		private int m_indexB;

		private FVector2 m_uA;

		private FVector2 m_uB;

		private FVector2 m_rA;

		private FVector2 m_rB;

		private FVector2 m_localCenterA;

		private FVector2 m_localCenterB;

		private float m_invMassA;

		private float m_invMassB;

		private float m_invIA;

		private float m_invIB;

		private float m_mass;

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

		public float LengthA { get; set; }

		public float LengthB { get; set; }

		public float CurrentLengthA
		{
			get
			{
				FVector2 worldPoint = base.BodyA.GetWorldPoint(LocalAnchorA);
				FVector2 groundAnchorA = GroundAnchorA;
				return (worldPoint - groundAnchorA).Length();
			}
		}

		public float CurrentLengthB
		{
			get
			{
				FVector2 worldPoint = base.BodyB.GetWorldPoint(LocalAnchorB);
				FVector2 groundAnchorB = GroundAnchorB;
				return (worldPoint - groundAnchorB).Length();
			}
		}

		public float Ratio { get; set; }

		internal PulleyJoint()
		{
			base.JointType = JointType.Pulley;
		}

		public PulleyJoint(Body bA, Body bB, FVector2 groundA, FVector2 groundB, FVector2 anchorA, FVector2 anchorB, float ratio)
			: base(bA, bB)
		{
			base.JointType = JointType.Pulley;
			GroundAnchorA = groundA;
			GroundAnchorB = groundB;
			LocalAnchorA = anchorA;
			LocalAnchorB = anchorB;
			Ratio = ratio;
			LengthA = (base.BodyA.GetWorldPoint(anchorA) - groundA).Length();
			LengthB = (base.BodyB.GetWorldPoint(anchorB) - groundB).Length();
			m_constant = LengthA + ratio * LengthB;
			_impulse = 0f;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			FVector2 fVector = _impulse * m_uB;
			return inv_dt * fVector;
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
			m_uA = c + m_rA - GroundAnchorA;
			m_uB = c2 + m_rB - GroundAnchorB;
			float num3 = m_uA.Length();
			float num4 = m_uB.Length();
			if (num3 > 0.049999997f)
			{
				m_uA *= 1f / num3;
			}
			else
			{
				m_uA = FVector2.Zero;
			}
			if (num4 > 0.049999997f)
			{
				m_uB *= 1f / num4;
			}
			else
			{
				m_uB = FVector2.Zero;
			}
			float num5 = MathUtils.Cross(m_rA, m_uA);
			float num6 = MathUtils.Cross(m_rB, m_uB);
			float num7 = m_invMassA + m_invIA * num5 * num5;
			float num8 = m_invMassB + m_invIB * num6 * num6;
			m_mass = num7 + Ratio * Ratio * num8;
			if (m_mass > 0f)
			{
				m_mass = 1f / m_mass;
			}
			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.step.dtRatio;
				FVector2 fVector = (0f - (_impulse + _limitImpulse1)) * m_uA;
				FVector2 fVector2 = ((0f - Ratio) * _impulse - _limitImpulse2) * m_uB;
				v += m_invMassA * fVector;
				num += m_invIA * MathUtils.Cross(m_rA, fVector);
				v2 += m_invMassB * fVector2;
				num2 += m_invIB * MathUtils.Cross(m_rB, fVector2);
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
			FVector2 value = v + MathUtils.Cross(w, m_rA);
			FVector2 value2 = v2 + MathUtils.Cross(w2, m_rB);
			float num = 0f - FVector2.Dot(m_uA, value) - Ratio * FVector2.Dot(m_uB, value2);
			float num2 = (0f - m_mass) * num;
			_impulse += num2;
			FVector2 fVector = (0f - num2) * m_uA;
			FVector2 fVector2 = (0f - Ratio) * num2 * m_uB;
			v += m_invMassA * fVector;
			w += m_invIA * MathUtils.Cross(m_rA, fVector);
			v2 += m_invMassB * fVector2;
			w2 += m_invIB * MathUtils.Cross(m_rB, fVector2);
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = w;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = w2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			FVector2 c = data.positions[m_indexA].c;
			float a = data.positions[m_indexA].a;
			FVector2 c2 = data.positions[m_indexB].c;
			float a2 = data.positions[m_indexB].a;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			FVector2 fVector3 = c + fVector - GroundAnchorA;
			FVector2 fVector4 = c2 + fVector2 - GroundAnchorB;
			float num = fVector3.Length();
			float num2 = fVector4.Length();
			if (num > 0.049999997f)
			{
				fVector3 *= 1f / num;
			}
			else
			{
				fVector3 = FVector2.Zero;
			}
			if (num2 > 0.049999997f)
			{
				fVector4 *= 1f / num2;
			}
			else
			{
				fVector4 = FVector2.Zero;
			}
			float num3 = MathUtils.Cross(fVector, fVector3);
			float num4 = MathUtils.Cross(fVector2, fVector4);
			float num5 = m_invMassA + m_invIA * num3 * num3;
			float num6 = m_invMassB + m_invIB * num4 * num4;
			float num7 = num5 + Ratio * Ratio * num6;
			if (num7 > 0f)
			{
				num7 = 1f / num7;
			}
			float num8 = m_constant - num - Ratio * num2;
			float num9 = Math.Abs(num8);
			float num10 = (0f - num7) * num8;
			FVector2 fVector5 = (0f - num10) * fVector3;
			FVector2 fVector6 = (0f - Ratio) * num10 * fVector4;
			c += m_invMassA * fVector5;
			a += m_invIA * MathUtils.Cross(fVector, fVector5);
			c2 += m_invMassB * fVector6;
			a2 += m_invIB * MathUtils.Cross(fVector2, fVector6);
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = a;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = a2;
			return num9 < 0.005f;
		}
	}
}
