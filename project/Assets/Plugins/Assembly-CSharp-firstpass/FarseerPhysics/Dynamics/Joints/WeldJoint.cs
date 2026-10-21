using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class WeldJoint : FarseerJoint
	{
		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private FVector3 _impulse;

		private float m_gamma;

		protected float m_frequencyHz;

		protected float m_dampingRatio;

		private float m_bias;

		private int m_indexA;

		private int m_indexB;

		private FVector2 m_rA;

		private FVector2 m_rB;

		private FVector2 m_localCenterA;

		private FVector2 m_localCenterB;

		private float m_invMassA;

		private float m_invMassB;

		private float m_invIA;

		private float m_invIB;

		private Mat33 _mass;

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

		public float ReferenceAngle { get; private set; }

		internal WeldJoint()
		{
			base.JointType = JointType.Weld;
		}

		public WeldJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Weld;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
			ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			return inv_dt * new FVector2(_impulse.X, _impulse.Y);
		}

		public override float GetReactionTorque(float inv_dt)
		{
			return inv_dt * _impulse.Z;
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
			float a = data.positions[m_indexA].a;
			FVector2 v = data.velocities[m_indexA].v;
			float num = data.velocities[m_indexA].w;
			float a2 = data.positions[m_indexB].a;
			FVector2 v2 = data.velocities[m_indexB].v;
			float num2 = data.velocities[m_indexB].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			m_rA = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			m_rB = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			Mat33 mat = default(Mat33);
			mat.ex.X = invMassA + invMassB + m_rA.Y * m_rA.Y * invIA + m_rB.Y * m_rB.Y * invIB;
			mat.ey.X = (0f - m_rA.Y) * m_rA.X * invIA - m_rB.Y * m_rB.X * invIB;
			mat.ez.X = (0f - m_rA.Y) * invIA - m_rB.Y * invIB;
			mat.ex.Y = mat.ey.X;
			mat.ey.Y = invMassA + invMassB + m_rA.X * m_rA.X * invIA + m_rB.X * m_rB.X * invIB;
			mat.ez.Y = m_rA.X * invIA + m_rB.X * invIB;
			mat.ex.Z = mat.ez.X;
			mat.ey.Z = mat.ez.Y;
			mat.ez.Z = invIA + invIB;
			if (m_frequencyHz > 0f)
			{
				mat.GetInverse22(ref _mass);
				float num3 = invIA + invIB;
				float num4 = ((!(num3 > 0f)) ? 0f : (1f / num3));
				float num5 = a2 - a - ReferenceAngle;
				float num6 = (float)Math.PI * 2f * m_frequencyHz;
				float num7 = 2f * num4 * m_dampingRatio * num6;
				float num8 = num4 * num6 * num6;
				float dt = data.step.dt;
				m_gamma = dt * (num7 + dt * num8);
				m_gamma = ((m_gamma == 0f) ? 0f : (1f / m_gamma));
				m_bias = num5 * dt * num8 * m_gamma;
				num3 += m_gamma;
				_mass.ez.Z = ((num3 == 0f) ? 0f : (1f / num3));
			}
			else
			{
				mat.GetSymInverse33(ref _mass);
				m_gamma = 0f;
				m_bias = 0f;
			}
			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.step.dtRatio;
				FVector2 fVector = new FVector2(_impulse.X, _impulse.Y);
				v -= invMassA * fVector;
				num -= invIA * (MathUtils.Cross(m_rA, fVector) + _impulse.Z);
				v2 += invMassB * fVector;
				num2 += invIB * (MathUtils.Cross(m_rB, fVector) + _impulse.Z);
			}
			else
			{
				_impulse = FVector3.Zero;
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
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			if (m_frequencyHz > 0f)
			{
				float num = w2 - w;
				float num2 = (0f - _mass.ez.Z) * (num + m_bias + m_gamma * _impulse.Z);
				_impulse.Z += num2;
				w -= invIA * num2;
				w2 += invIB * num2;
				FVector2 v3 = v2 + MathUtils.Cross(w2, m_rB) - v - MathUtils.Cross(w, m_rA);
				FVector2 fVector = -MathUtils.Mul22(_mass, v3);
				_impulse.X += fVector.X;
				_impulse.Y += fVector.Y;
				FVector2 fVector2 = fVector;
				v -= invMassA * fVector2;
				w -= invIA * MathUtils.Cross(m_rA, fVector2);
				v2 += invMassB * fVector2;
				w2 += invIB * MathUtils.Cross(m_rB, fVector2);
			}
			else
			{
				FVector2 fVector3 = v2 + MathUtils.Cross(w2, m_rB) - v - MathUtils.Cross(w, m_rA);
				float z = w2 - w;
				FVector3 fVector4 = -MathUtils.Mul(v: new FVector3(fVector3.X, fVector3.Y, z), A: _mass);
				_impulse += fVector4;
				FVector2 fVector5 = new FVector2(fVector4.X, fVector4.Y);
				v -= invMassA * fVector5;
				w -= invIA * (MathUtils.Cross(m_rA, fVector5) + fVector4.Z);
				v2 += invMassB * fVector5;
				w2 += invIB * (MathUtils.Cross(m_rB, fVector5) + fVector4.Z);
			}
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
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			Mat33 mat = default(Mat33);
			mat.ex.X = invMassA + invMassB + fVector.Y * fVector.Y * invIA + fVector2.Y * fVector2.Y * invIB;
			mat.ey.X = (0f - fVector.Y) * fVector.X * invIA - fVector2.Y * fVector2.X * invIB;
			mat.ez.X = (0f - fVector.Y) * invIA - fVector2.Y * invIB;
			mat.ex.Y = mat.ey.X;
			mat.ey.Y = invMassA + invMassB + fVector.X * fVector.X * invIA + fVector2.X * fVector2.X * invIB;
			mat.ez.Y = fVector.X * invIA + fVector2.X * invIB;
			mat.ex.Z = mat.ez.X;
			mat.ey.Z = mat.ez.Y;
			mat.ez.Z = invIA + invIB;
			float num;
			float num2;
			if (m_frequencyHz > 0f)
			{
				FVector2 b = c2 + fVector2 - c - fVector;
				num = b.Length();
				num2 = 0f;
				FVector2 fVector3 = -mat.Solve22(b);
				c -= invMassA * fVector3;
				a -= invIA * MathUtils.Cross(fVector, fVector3);
				c2 += invMassB * fVector3;
				a2 += invIB * MathUtils.Cross(fVector2, fVector3);
			}
			else
			{
				FVector2 fVector4 = c2 + fVector2 - c - fVector;
				float num3 = a2 - a - ReferenceAngle;
				num = fVector4.Length();
				num2 = Math.Abs(num3);
				FVector3 b2 = new FVector3(fVector4.X, fVector4.Y, num3);
				FVector3 fVector5 = -mat.Solve33(b2);
				FVector2 fVector6 = new FVector2(fVector5.X, fVector5.Y);
				c -= invMassA * fVector6;
				a -= invIA * (MathUtils.Cross(fVector, fVector6) + fVector5.Z);
				c2 += invMassB * fVector6;
				a2 += invIB * (MathUtils.Cross(fVector2, fVector6) + fVector5.Z);
			}
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = a;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = a2;
			return num <= 0.005f && num2 <= 0.03490659f;
		}
	}
}
