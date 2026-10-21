using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class FrictionJoint : FarseerJoint
	{
		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private FVector2 _linearImpulse;

		private float _angularImpulse;

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

		private float _angularMass;

		private Mat22 _linearMass;

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

		public float MaxForce { get; set; }

		public float MaxTorque { get; set; }

		internal FrictionJoint()
		{
			base.JointType = JointType.Friction;
		}

		public FrictionJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Friction;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			return inv_dt * _linearImpulse;
		}

		public override float GetReactionTorque(float inv_dt)
		{
			return inv_dt * _angularImpulse;
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
			Mat22 mat = default(Mat22);
			mat.ex.X = invMassA + invMassB + invIA * m_rA.Y * m_rA.Y + invIB * m_rB.Y * m_rB.Y;
			mat.ex.Y = (0f - invIA) * m_rA.X * m_rA.Y - invIB * m_rB.X * m_rB.Y;
			mat.ey.X = mat.ex.Y;
			mat.ey.Y = invMassA + invMassB + invIA * m_rA.X * m_rA.X + invIB * m_rB.X * m_rB.X;
			_linearMass = mat.Inverse;
			_angularMass = invIA + invIB;
			if (_angularMass > 0f)
			{
				_angularMass = 1f / _angularMass;
			}
			if (Settings.EnableWarmstarting)
			{
				_linearImpulse *= data.step.dtRatio;
				_angularImpulse *= data.step.dtRatio;
				FVector2 fVector = new FVector2(_linearImpulse.X, _linearImpulse.Y);
				v -= invMassA * fVector;
				num -= invIA * (MathUtils.Cross(m_rA, fVector) + _angularImpulse);
				v2 += invMassB * fVector;
				num2 += invIB * (MathUtils.Cross(m_rB, fVector) + _angularImpulse);
			}
			else
			{
				_linearImpulse = FVector2.Zero;
				_angularImpulse = 0f;
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
			float dt = data.step.dt;
			float num = w2 - w;
			float num2 = (0f - _angularMass) * num;
			float angularImpulse = _angularImpulse;
			float num3 = dt * MaxTorque;
			_angularImpulse = MathUtils.Clamp(_angularImpulse + num2, 0f - num3, num3);
			num2 = _angularImpulse - angularImpulse;
			w -= invIA * num2;
			w2 += invIB * num2;
			FVector2 v3 = v2 + MathUtils.Cross(w2, m_rB) - v - MathUtils.Cross(w, m_rA);
			FVector2 fVector = -MathUtils.Mul(ref _linearMass, v3);
			FVector2 linearImpulse = _linearImpulse;
			_linearImpulse += fVector;
			float num4 = dt * MaxForce;
			if (_linearImpulse.LengthSquared() > num4 * num4)
			{
				_linearImpulse.Normalize();
				_linearImpulse *= num4;
			}
			fVector = _linearImpulse - linearImpulse;
			v -= invMassA * fVector;
			w -= invIA * MathUtils.Cross(m_rA, fVector);
			v2 += invMassB * fVector;
			w2 += invIB * MathUtils.Cross(m_rB, fVector);
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = w;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = w2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}
