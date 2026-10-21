using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class GearJoint : FarseerJoint
	{
		private JointType m_typeA;

		private JointType m_typeB;

		private Body m_bodyC;

		private Body m_bodyD;

		private FVector2 m_localAnchorA;

		private FVector2 m_localAnchorB;

		private FVector2 m_localAnchorC;

		private FVector2 m_localAnchorD;

		private FVector2 m_localAxisC;

		private FVector2 m_localAxisD;

		private float m_referenceAngleA;

		private float m_referenceAngleB;

		private float m_constant;

		private float _ratio;

		private float m_impulse;

		private int m_indexA;

		private int m_indexB;

		private int m_indexC;

		private int m_indexD;

		private FVector2 m_lcA;

		private FVector2 m_lcB;

		private FVector2 m_lcC;

		private FVector2 m_lcD;

		private float m_mA;

		private float m_mB;

		private float m_mC;

		private float m_mD;

		private float m_iA;

		private float m_iB;

		private float m_iC;

		private float m_iD;

		private FVector2 m_JvAC;

		private FVector2 m_JvBD;

		private float m_JwA;

		private float m_JwB;

		private float m_JwC;

		private float m_JwD;

		private float m_mass;

		public override FVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.GetWorldPoint(m_localAnchorA);
			}
		}

		public override FVector2 WorldAnchorB
		{
			get
			{
				return base.BodyB.GetWorldPoint(m_localAnchorB);
			}
			set
			{
			}
		}

		public float Ratio
		{
			get
			{
				return _ratio;
			}
			set
			{
				_ratio = value;
			}
		}

		public FarseerJoint JointA { get; set; }

		public FarseerJoint JointB { get; set; }

		public GearJoint(FarseerJoint jointA, FarseerJoint jointB, float ratio)
			: base(jointA.BodyA, jointA.BodyB)
		{
			base.JointType = JointType.Gear;
			JointA = jointA;
			JointB = jointB;
			Ratio = ratio;
			m_typeA = jointA.JointType;
			m_typeB = jointB.JointType;
			float num = 0f;
			float num2 = 0f;
			m_bodyC = JointA.BodyA;
			base.BodyA = JointA.BodyB;
			Transform xf = base.BodyA.Xf;
			float a = base.BodyA.Sweep.A;
			Transform xf2 = m_bodyC.Xf;
			float a2 = m_bodyC.Sweep.A;
			if (m_typeA == JointType.Revolute)
			{
				RevoluteJoint revoluteJoint = (RevoluteJoint)jointA;
				m_localAnchorC = revoluteJoint.LocalAnchorA;
				m_localAnchorA = revoluteJoint.LocalAnchorB;
				m_referenceAngleA = revoluteJoint.ReferenceAngle;
				m_localAxisC = FVector2.Zero;
				num = a - a2 - m_referenceAngleA;
			}
			else
			{
				PrismaticJoint prismaticJoint = (PrismaticJoint)jointA;
				m_localAnchorC = prismaticJoint.LocalAnchorA;
				m_localAnchorA = prismaticJoint.LocalAnchorB;
				m_referenceAngleA = prismaticJoint.ReferenceAngle;
				m_localAxisC = prismaticJoint.LocalXAxisA;
				FVector2 localAnchorC = m_localAnchorC;
				FVector2 fVector = MathUtils.MulT(xf2.q, MathUtils.Mul(xf.q, m_localAnchorA) + (xf.p - xf2.p));
				num = FVector2.Dot(fVector - localAnchorC, m_localAxisC);
			}
			m_bodyD = JointB.BodyA;
			base.BodyB = JointB.BodyB;
			Transform xf3 = base.BodyB.Xf;
			float a3 = base.BodyB.Sweep.A;
			Transform xf4 = m_bodyD.Xf;
			float a4 = m_bodyD.Sweep.A;
			if (m_typeB == JointType.Revolute)
			{
				RevoluteJoint revoluteJoint2 = (RevoluteJoint)jointB;
				m_localAnchorD = revoluteJoint2.LocalAnchorA;
				m_localAnchorB = revoluteJoint2.LocalAnchorB;
				m_referenceAngleB = revoluteJoint2.ReferenceAngle;
				m_localAxisD = FVector2.Zero;
				num2 = a3 - a4 - m_referenceAngleB;
			}
			else
			{
				PrismaticJoint prismaticJoint2 = (PrismaticJoint)jointB;
				m_localAnchorD = prismaticJoint2.LocalAnchorA;
				m_localAnchorB = prismaticJoint2.LocalAnchorB;
				m_referenceAngleB = prismaticJoint2.ReferenceAngle;
				m_localAxisD = prismaticJoint2.LocalXAxisA;
				FVector2 localAnchorD = m_localAnchorD;
				FVector2 fVector2 = MathUtils.MulT(xf4.q, MathUtils.Mul(xf3.q, m_localAnchorB) + (xf3.p - xf4.p));
				num2 = FVector2.Dot(fVector2 - localAnchorD, m_localAxisD);
			}
			_ratio = ratio;
			m_constant = num + _ratio * num2;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			FVector2 fVector = m_impulse * m_JvAC;
			return inv_dt * fVector;
		}

		public override float GetReactionTorque(float inv_dt)
		{
			float num = m_impulse * m_JwA;
			return inv_dt * num;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			m_indexA = base.BodyA.IslandIndex;
			m_indexB = base.BodyB.IslandIndex;
			m_indexC = m_bodyC.IslandIndex;
			m_indexD = m_bodyD.IslandIndex;
			m_lcA = base.BodyA.Sweep.LocalCenter;
			m_lcB = base.BodyB.Sweep.LocalCenter;
			m_lcC = m_bodyC.Sweep.LocalCenter;
			m_lcD = m_bodyD.Sweep.LocalCenter;
			m_mA = base.BodyA.InvMass;
			m_mB = base.BodyB.InvMass;
			m_mC = m_bodyC.InvMass;
			m_mD = m_bodyD.InvMass;
			m_iA = base.BodyA.InvI;
			m_iB = base.BodyB.InvI;
			m_iC = m_bodyC.InvI;
			m_iD = m_bodyD.InvI;
			float a = data.positions[m_indexA].a;
			FVector2 v = data.velocities[m_indexA].v;
			float num = data.velocities[m_indexA].w;
			float a2 = data.positions[m_indexB].a;
			FVector2 v2 = data.velocities[m_indexB].v;
			float num2 = data.velocities[m_indexB].w;
			float a3 = data.positions[m_indexC].a;
			FVector2 v3 = data.velocities[m_indexC].v;
			float num3 = data.velocities[m_indexC].w;
			float a4 = data.positions[m_indexD].a;
			FVector2 v4 = data.velocities[m_indexD].v;
			float num4 = data.velocities[m_indexD].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			Rot q3 = new Rot(a3);
			Rot q4 = new Rot(a4);
			m_mass = 0f;
			if (m_typeA == JointType.Revolute)
			{
				m_JvAC = FVector2.Zero;
				m_JwA = 1f;
				m_JwC = 1f;
				m_mass += m_iA + m_iC;
			}
			else
			{
				FVector2 fVector = MathUtils.Mul(q3, m_localAxisC);
				FVector2 a5 = MathUtils.Mul(q3, m_localAnchorC - m_lcC);
				FVector2 a6 = MathUtils.Mul(q, m_localAnchorA - m_lcA);
				m_JvAC = fVector;
				m_JwC = MathUtils.Cross(a5, fVector);
				m_JwA = MathUtils.Cross(a6, fVector);
				m_mass += m_mC + m_mA + m_iC * m_JwC * m_JwC + m_iA * m_JwA * m_JwA;
			}
			if (m_typeB == JointType.Revolute)
			{
				m_JvBD = FVector2.Zero;
				m_JwB = _ratio;
				m_JwD = _ratio;
				m_mass += _ratio * _ratio * (m_iB + m_iD);
			}
			else
			{
				FVector2 fVector2 = MathUtils.Mul(q4, m_localAxisD);
				FVector2 a7 = MathUtils.Mul(q4, m_localAnchorD - m_lcD);
				FVector2 a8 = MathUtils.Mul(q2, m_localAnchorB - m_lcB);
				m_JvBD = _ratio * fVector2;
				m_JwD = _ratio * MathUtils.Cross(a7, fVector2);
				m_JwB = _ratio * MathUtils.Cross(a8, fVector2);
				m_mass += _ratio * _ratio * (m_mD + m_mB) + m_iD * m_JwD * m_JwD + m_iB * m_JwB * m_JwB;
			}
			m_mass = ((!(m_mass > 0f)) ? 0f : (1f / m_mass));
			if (Settings.EnableWarmstarting)
			{
				v += m_mA * m_impulse * m_JvAC;
				num += m_iA * m_impulse * m_JwA;
				v2 += m_mB * m_impulse * m_JvBD;
				num2 += m_iB * m_impulse * m_JwB;
				v3 -= m_mC * m_impulse * m_JvAC;
				num3 -= m_iC * m_impulse * m_JwC;
				v4 -= m_mD * m_impulse * m_JvBD;
				num4 -= m_iD * m_impulse * m_JwD;
			}
			else
			{
				m_impulse = 0f;
			}
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = num;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = num2;
			data.velocities[m_indexC].v = v3;
			data.velocities[m_indexC].w = num3;
			data.velocities[m_indexD].v = v4;
			data.velocities[m_indexD].w = num4;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			FVector2 v = data.velocities[m_indexA].v;
			float w = data.velocities[m_indexA].w;
			FVector2 v2 = data.velocities[m_indexB].v;
			float w2 = data.velocities[m_indexB].w;
			FVector2 v3 = data.velocities[m_indexC].v;
			float w3 = data.velocities[m_indexC].w;
			FVector2 v4 = data.velocities[m_indexD].v;
			float w4 = data.velocities[m_indexD].w;
			float num = FVector2.Dot(m_JvAC, v - v3) + FVector2.Dot(m_JvBD, v2 - v4);
			num += m_JwA * w - m_JwC * w3 + (m_JwB * w2 - m_JwD * w4);
			float num2 = (0f - m_mass) * num;
			m_impulse += num2;
			v += m_mA * num2 * m_JvAC;
			w += m_iA * num2 * m_JwA;
			v2 += m_mB * num2 * m_JvBD;
			w2 += m_iB * num2 * m_JwB;
			v3 -= m_mC * num2 * m_JvAC;
			w3 -= m_iC * num2 * m_JwC;
			v4 -= m_mD * num2 * m_JvBD;
			w4 -= m_iD * num2 * m_JwD;
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = w;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = w2;
			data.velocities[m_indexC].v = v3;
			data.velocities[m_indexC].w = w3;
			data.velocities[m_indexD].v = v4;
			data.velocities[m_indexD].w = w4;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			FVector2 c = data.positions[m_indexA].c;
			float a = data.positions[m_indexA].a;
			FVector2 c2 = data.positions[m_indexB].c;
			float a2 = data.positions[m_indexB].a;
			FVector2 c3 = data.positions[m_indexC].c;
			float a3 = data.positions[m_indexC].a;
			FVector2 c4 = data.positions[m_indexD].c;
			float a4 = data.positions[m_indexD].a;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			Rot q3 = new Rot(a3);
			Rot q4 = new Rot(a4);
			float num = 0f;
			float num2 = 0f;
			FVector2 fVector;
			float num3;
			float num4;
			float num5;
			if (m_typeA == JointType.Revolute)
			{
				fVector = FVector2.Zero;
				num3 = 1f;
				num4 = 1f;
				num2 += m_iA + m_iC;
				num5 = a - a3 - m_referenceAngleA;
			}
			else
			{
				FVector2 fVector2 = MathUtils.Mul(q3, m_localAxisC);
				FVector2 a5 = MathUtils.Mul(q3, m_localAnchorC - m_lcC);
				FVector2 fVector3 = MathUtils.Mul(q, m_localAnchorA - m_lcA);
				fVector = fVector2;
				num4 = MathUtils.Cross(a5, fVector2);
				num3 = MathUtils.Cross(fVector3, fVector2);
				num2 += m_mC + m_mA + m_iC * num4 * num4 + m_iA * num3 * num3;
				FVector2 fVector4 = m_localAnchorC - m_lcC;
				FVector2 fVector5 = MathUtils.MulT(q3, fVector3 + (c - c3));
				num5 = FVector2.Dot(fVector5 - fVector4, m_localAxisC);
			}
			FVector2 fVector6;
			float num6;
			float num7;
			float num8;
			if (m_typeB == JointType.Revolute)
			{
				fVector6 = FVector2.Zero;
				num6 = _ratio;
				num7 = _ratio;
				num2 += _ratio * _ratio * (m_iB + m_iD);
				num8 = a2 - a4 - m_referenceAngleB;
			}
			else
			{
				FVector2 fVector7 = MathUtils.Mul(q4, m_localAxisD);
				FVector2 a6 = MathUtils.Mul(q4, m_localAnchorD - m_lcD);
				FVector2 fVector8 = MathUtils.Mul(q2, m_localAnchorB - m_lcB);
				fVector6 = _ratio * fVector7;
				num7 = _ratio * MathUtils.Cross(a6, fVector7);
				num6 = _ratio * MathUtils.Cross(fVector8, fVector7);
				num2 += _ratio * _ratio * (m_mD + m_mB) + m_iD * num7 * num7 + m_iB * num6 * num6;
				FVector2 fVector9 = m_localAnchorD - m_lcD;
				FVector2 fVector10 = MathUtils.MulT(q4, fVector8 + (c2 - c4));
				num8 = FVector2.Dot(fVector10 - fVector9, m_localAxisD);
			}
			float num9 = num5 + _ratio * num8 - m_constant;
			float num10 = 0f;
			if (num2 > 0f)
			{
				num10 = (0f - num9) / num2;
			}
			c += m_mA * num10 * fVector;
			a += m_iA * num10 * num3;
			c2 += m_mB * num10 * fVector6;
			a2 += m_iB * num10 * num6;
			c3 -= m_mC * num10 * fVector;
			a3 -= m_iC * num10 * num4;
			c4 -= m_mD * num10 * fVector6;
			a4 -= m_iD * num10 * num7;
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = a;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = a2;
			data.positions[m_indexC].c = c3;
			data.positions[m_indexC].a = a3;
			data.positions[m_indexD].c = c4;
			data.positions[m_indexD].a = a4;
			return num < 0.005f;
		}
	}
}
