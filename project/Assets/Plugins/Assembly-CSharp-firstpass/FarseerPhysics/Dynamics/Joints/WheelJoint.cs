using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class WheelJoint : FarseerJoint
	{
		private FVector2 m_localXAxisA;

		private FVector2 m_localYAxisA;

		private float m_impulse;

		private float m_motorImpulse;

		private float m_springImpulse;

		private float m_maxMotorTorque;

		private float m_motorSpeed;

		private bool m_enableMotor;

		private int m_indexA;

		private int m_indexB;

		private FVector2 m_localCenterA;

		private FVector2 m_localCenterB;

		private float m_invMassA;

		private float m_invMassB;

		private float m_invIA;

		private float m_invIB;

		private FVector2 m_ax;

		private FVector2 m_ay;

		private float m_sAx;

		private float m_sBx;

		private float m_sAy;

		private float m_sBy;

		private float m_mass;

		private float m_motorMass;

		private float m_springMass;

		private float m_bias;

		private float m_gamma;

		public FVector2 LocalAnchorA { get; set; }

		public FVector2 LocalAnchorB { get; set; }

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

		public float MotorSpeed
		{
			get
			{
				return m_motorSpeed;
			}
			set
			{
				WakeBodies();
				m_motorSpeed = value;
			}
		}

		public float MaxMotorTorque
		{
			get
			{
				return m_maxMotorTorque;
			}
			set
			{
				WakeBodies();
				m_maxMotorTorque = value;
			}
		}

		public float SpringFrequencyHz { get; set; }

		public float SpringDampingRatio { get; set; }

		public float JointTranslation
		{
			get
			{
				Body bodyA = base.BodyA;
				Body bodyB = base.BodyB;
				FVector2 worldPoint = bodyA.GetWorldPoint(LocalAnchorA);
				FVector2 worldPoint2 = bodyB.GetWorldPoint(LocalAnchorB);
				FVector2 value = worldPoint2 - worldPoint;
				FVector2 worldVector = bodyA.GetWorldVector(m_localXAxisA);
				return FVector2.Dot(value, worldVector);
			}
		}

		public float JointSpeed
		{
			get
			{
				float angularVelocity = base.BodyA.AngularVelocity;
				float angularVelocity2 = base.BodyB.AngularVelocity;
				return angularVelocity2 - angularVelocity;
			}
		}

		public bool MotorEnabled
		{
			get
			{
				return m_enableMotor;
			}
			set
			{
				WakeBodies();
				m_enableMotor = value;
			}
		}

		internal WheelJoint()
		{
			base.JointType = JointType.Wheel;
		}

		public WheelJoint(Body bA, Body bB, FVector2 anchor, FVector2 axis)
			: base(bA, bB)
		{
			base.JointType = JointType.Wheel;
			LocalAnchorA = bA.GetLocalPoint(anchor);
			LocalAnchorB = bB.GetLocalPoint(anchor);
			m_localXAxisA = bA.GetLocalVector(axis);
			m_localYAxisA = MathUtils.Cross(1f, m_localXAxisA);
		}

		public override FVector2 GetReactionForce(float invDt)
		{
			return invDt * (m_impulse * m_ay + m_springImpulse * m_ax);
		}

		public override float GetReactionTorque(float invDt)
		{
			return invDt * m_motorImpulse;
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
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
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
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			FVector2 fVector3 = c2 + fVector2 - c - fVector;
			m_ay = MathUtils.Mul(q, m_localYAxisA);
			m_sAy = MathUtils.Cross(fVector3 + fVector, m_ay);
			m_sBy = MathUtils.Cross(fVector2, m_ay);
			m_mass = invMassA + invMassB + invIA * m_sAy * m_sAy + invIB * m_sBy * m_sBy;
			if (m_mass > 0f)
			{
				m_mass = 1f / m_mass;
			}
			m_springMass = 0f;
			m_bias = 0f;
			m_gamma = 0f;
			if (SpringFrequencyHz > 0f)
			{
				m_ax = MathUtils.Mul(q, m_localXAxisA);
				m_sAx = MathUtils.Cross(fVector3 + fVector, m_ax);
				m_sBx = MathUtils.Cross(fVector2, m_ax);
				float num3 = invMassA + invMassB + invIA * m_sAx * m_sAx + invIB * m_sBx * m_sBx;
				if (num3 > 0f)
				{
					m_springMass = 1f / num3;
					float num4 = FVector2.Dot(fVector3, m_ax);
					float num5 = (float)Math.PI * 2f * SpringFrequencyHz;
					float num6 = 2f * m_springMass * SpringDampingRatio * num5;
					float num7 = m_springMass * num5 * num5;
					float dt = data.step.dt;
					m_gamma = dt * (num6 + dt * num7);
					if (m_gamma > 0f)
					{
						m_gamma = 1f / m_gamma;
					}
					m_bias = num4 * dt * num7 * m_gamma;
					m_springMass = num3 + m_gamma;
					if (m_springMass > 0f)
					{
						m_springMass = 1f / m_springMass;
					}
				}
			}
			else
			{
				m_springImpulse = 0f;
			}
			if (m_enableMotor)
			{
				m_motorMass = invIA + invIB;
				if (m_motorMass > 0f)
				{
					m_motorMass = 1f / m_motorMass;
				}
			}
			else
			{
				m_motorMass = 0f;
				m_motorImpulse = 0f;
			}
			if (Settings.EnableWarmstarting)
			{
				m_impulse *= data.step.dtRatio;
				m_springImpulse *= data.step.dtRatio;
				m_motorImpulse *= data.step.dtRatio;
				FVector2 fVector4 = m_impulse * m_ay + m_springImpulse * m_ax;
				float num8 = m_impulse * m_sAy + m_springImpulse * m_sAx + m_motorImpulse;
				float num9 = m_impulse * m_sBy + m_springImpulse * m_sBx + m_motorImpulse;
				v -= m_invMassA * fVector4;
				num -= m_invIA * num8;
				v2 += m_invMassB * fVector4;
				num2 += m_invIB * num9;
			}
			else
			{
				m_impulse = 0f;
				m_springImpulse = 0f;
				m_motorImpulse = 0f;
			}
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = num;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = num2;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			FVector2 v = data.velocities[m_indexA].v;
			float w = data.velocities[m_indexA].w;
			FVector2 v2 = data.velocities[m_indexB].v;
			float w2 = data.velocities[m_indexB].w;
			float num = FVector2.Dot(m_ax, v2 - v) + m_sBx * w2 - m_sAx * w;
			float num2 = (0f - m_springMass) * (num + m_bias + m_gamma * m_springImpulse);
			m_springImpulse += num2;
			FVector2 fVector = num2 * m_ax;
			float num3 = num2 * m_sAx;
			float num4 = num2 * m_sBx;
			v -= invMassA * fVector;
			w -= invIA * num3;
			v2 += invMassB * fVector;
			w2 += invIB * num4;
			float num5 = w2 - w - m_motorSpeed;
			float num6 = (0f - m_motorMass) * num5;
			float motorImpulse = m_motorImpulse;
			float num7 = data.step.dt * m_maxMotorTorque;
			m_motorImpulse = MathUtils.Clamp(m_motorImpulse + num6, 0f - num7, num7);
			num6 = m_motorImpulse - motorImpulse;
			w -= invIA * num6;
			w2 += invIB * num6;
			float num8 = FVector2.Dot(m_ay, v2 - v) + m_sBy * w2 - m_sAy * w;
			float num9 = (0f - m_mass) * num8;
			m_impulse += num9;
			FVector2 fVector2 = num9 * m_ay;
			float num10 = num9 * m_sAy;
			float num11 = num9 * m_sBy;
			v -= invMassA * fVector2;
			w -= invIA * num10;
			v2 += invMassB * fVector2;
			w2 += invIB * num11;
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
			FVector2 fVector3 = c2 - c + fVector2 - fVector;
			FVector2 fVector4 = MathUtils.Mul(q, m_localYAxisA);
			float num = MathUtils.Cross(fVector3 + fVector, fVector4);
			float num2 = MathUtils.Cross(fVector2, fVector4);
			float num3 = FVector2.Dot(fVector3, fVector4);
			float num4 = m_invMassA + m_invMassB + m_invIA * m_sAy * m_sAy + m_invIB * m_sBy * m_sBy;
			float num5 = ((num4 == 0f) ? 0f : ((0f - num3) / num4));
			FVector2 fVector5 = num5 * fVector4;
			float num6 = num5 * num;
			float num7 = num5 * num2;
			c -= m_invMassA * fVector5;
			a -= m_invIA * num6;
			c2 += m_invMassB * fVector5;
			a2 += m_invIB * num7;
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = a;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = a2;
			return Math.Abs(num3) <= 0.005f;
		}

		public float GetMotorTorque(float inv_dt)
		{
			return inv_dt * m_motorImpulse;
		}
	}
}
