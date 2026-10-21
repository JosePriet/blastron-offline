using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class RevoluteJoint : FarseerJoint
	{
		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private FVector3 _impulse;

		private float _motorImpulse;

		private bool _enableMotor;

		private float _maxMotorTorque;

		private float _motorSpeed;

		private bool _enableLimit;

		private float _referenceAngle;

		private float _lowerAngle;

		private float _upperAngle;

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

		private Mat33 m_mass;

		private float m_motorMass;

		private LimitState _limitState;

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

		public float ReferenceAngle
		{
			get
			{
				return _referenceAngle;
			}
			set
			{
				WakeBodies();
				_referenceAngle = value;
			}
		}

		public float JointAngle
		{
			get
			{
				return base.BodyB.Sweep.A - base.BodyA.Sweep.A - ReferenceAngle;
			}
		}

		public float JointSpeed
		{
			get
			{
				return base.BodyB.AngularVelocityInternal - base.BodyA.AngularVelocityInternal;
			}
		}

		public bool LimitEnabled
		{
			get
			{
				return _enableLimit;
			}
			set
			{
				if (_enableLimit != value)
				{
					WakeBodies();
					_enableLimit = value;
					_impulse.Z = 0f;
				}
			}
		}

		public float LowerLimit
		{
			get
			{
				return _lowerAngle;
			}
			set
			{
				if (_lowerAngle != value)
				{
					WakeBodies();
					_lowerAngle = value;
					_impulse.Z = 0f;
				}
			}
		}

		public float UpperLimit
		{
			get
			{
				return _upperAngle;
			}
			set
			{
				if (_upperAngle != value)
				{
					WakeBodies();
					_upperAngle = value;
					_impulse.Z = 0f;
				}
			}
		}

		public bool MotorEnabled
		{
			get
			{
				return _enableMotor;
			}
			set
			{
				WakeBodies();
				_enableMotor = value;
			}
		}

		public float MotorSpeed
		{
			get
			{
				return _motorSpeed;
			}
			set
			{
				WakeBodies();
				_motorSpeed = value;
			}
		}

		public float MaxMotorTorque
		{
			get
			{
				return _maxMotorTorque;
			}
			set
			{
				WakeBodies();
				_maxMotorTorque = value;
			}
		}

		public float MotorImpulse
		{
			get
			{
				return _motorImpulse;
			}
			set
			{
				WakeBodies();
				_motorImpulse = value;
			}
		}

		internal RevoluteJoint()
		{
			base.JointType = JointType.Revolute;
		}

		public RevoluteJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Revolute;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
			ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
			_impulse = FVector3.Zero;
			_limitState = LimitState.Inactive;
		}

		public RevoluteJoint(Body bodyA, Body bodyB, FVector2 worldAnchor)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Revolute;
			LocalAnchorA = bodyA.GetLocalPoint(worldAnchor);
			LocalAnchorB = bodyB.GetLocalPoint(worldAnchor);
			ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
			_impulse = FVector3.Zero;
			_limitState = LimitState.Inactive;
		}

		public void SetLimits(float lower, float upper)
		{
			if (lower != _lowerAngle || upper != _upperAngle)
			{
				WakeBodies();
				_upperAngle = upper;
				_lowerAngle = lower;
				_impulse.Z = 0f;
			}
		}

		public float GetMotorTorque(float inv_dt)
		{
			return inv_dt * _motorImpulse;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			FVector2 fVector = new FVector2(_impulse.X, _impulse.Y);
			return inv_dt * fVector;
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
			bool flag = invIA + invIB == 0f;
			m_mass.ex.X = invMassA + invMassB + m_rA.Y * m_rA.Y * invIA + m_rB.Y * m_rB.Y * invIB;
			m_mass.ey.X = (0f - m_rA.Y) * m_rA.X * invIA - m_rB.Y * m_rB.X * invIB;
			m_mass.ez.X = (0f - m_rA.Y) * invIA - m_rB.Y * invIB;
			m_mass.ex.Y = m_mass.ey.X;
			m_mass.ey.Y = invMassA + invMassB + m_rA.X * m_rA.X * invIA + m_rB.X * m_rB.X * invIB;
			m_mass.ez.Y = m_rA.X * invIA + m_rB.X * invIB;
			m_mass.ex.Z = m_mass.ez.X;
			m_mass.ey.Z = m_mass.ez.Y;
			m_mass.ez.Z = invIA + invIB;
			m_motorMass = invIA + invIB;
			if (m_motorMass > 0f)
			{
				m_motorMass = 1f / m_motorMass;
			}
			if (!_enableMotor || flag)
			{
				_motorImpulse = 0f;
			}
			if (_enableLimit && !flag)
			{
				float num3 = a2 - a - ReferenceAngle;
				if (Math.Abs(_upperAngle - _lowerAngle) < 0.06981318f)
				{
					_limitState = LimitState.Equal;
				}
				else if (num3 <= _lowerAngle)
				{
					if (_limitState != LimitState.AtLower)
					{
						_impulse.Z = 0f;
					}
					_limitState = LimitState.AtLower;
				}
				else if (num3 >= _upperAngle)
				{
					if (_limitState != LimitState.AtUpper)
					{
						_impulse.Z = 0f;
					}
					_limitState = LimitState.AtUpper;
				}
				else
				{
					_limitState = LimitState.Inactive;
					_impulse.Z = 0f;
				}
			}
			else
			{
				_limitState = LimitState.Inactive;
			}
			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.step.dtRatio;
				_motorImpulse *= data.step.dtRatio;
				FVector2 fVector = new FVector2(_impulse.X, _impulse.Y);
				v -= invMassA * fVector;
				num -= invIA * (MathUtils.Cross(m_rA, fVector) + MotorImpulse + _impulse.Z);
				v2 += invMassB * fVector;
				num2 += invIB * (MathUtils.Cross(m_rB, fVector) + MotorImpulse + _impulse.Z);
			}
			else
			{
				_impulse = FVector3.Zero;
				_motorImpulse = 0f;
			}
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = num;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = num2;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			FVector2 v = data.velocities[m_indexA].v;
			float num = data.velocities[m_indexA].w;
			FVector2 v2 = data.velocities[m_indexB].v;
			float num2 = data.velocities[m_indexB].w;
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			bool flag = invIA + invIB == 0f;
			if (_enableMotor && _limitState != LimitState.Equal && !flag)
			{
				float num3 = num2 - num - _motorSpeed;
				float num4 = m_motorMass * (0f - num3);
				float motorImpulse = _motorImpulse;
				float num5 = data.step.dt * _maxMotorTorque;
				_motorImpulse = MathUtils.Clamp(_motorImpulse + num4, 0f - num5, num5);
				num4 = _motorImpulse - motorImpulse;
				num -= invIA * num4;
				num2 += invIB * num4;
			}
			if (_enableLimit && _limitState != LimitState.Inactive && !flag)
			{
				FVector2 fVector = v2 + MathUtils.Cross(num2, m_rB) - v - MathUtils.Cross(num, m_rA);
				float z = num2 - num;
				FVector3 b = new FVector3(fVector.X, fVector.Y, z);
				FVector3 fVector2 = -m_mass.Solve33(b);
				if (_limitState == LimitState.Equal)
				{
					_impulse += fVector2;
				}
				else if (_limitState == LimitState.AtLower)
				{
					float num6 = _impulse.Z + fVector2.Z;
					if (num6 < 0f)
					{
						FVector2 b2 = -fVector + _impulse.Z * new FVector2(m_mass.ez.X, m_mass.ez.Y);
						FVector2 fVector3 = m_mass.Solve22(b2);
						fVector2.X = fVector3.X;
						fVector2.Y = fVector3.Y;
						fVector2.Z = 0f - _impulse.Z;
						_impulse.X += fVector3.X;
						_impulse.Y += fVector3.Y;
						_impulse.Z = 0f;
					}
					else
					{
						_impulse += fVector2;
					}
				}
				else if (_limitState == LimitState.AtUpper)
				{
					float num7 = _impulse.Z + fVector2.Z;
					if (num7 > 0f)
					{
						FVector2 b3 = -fVector + _impulse.Z * new FVector2(m_mass.ez.X, m_mass.ez.Y);
						FVector2 fVector4 = m_mass.Solve22(b3);
						fVector2.X = fVector4.X;
						fVector2.Y = fVector4.Y;
						fVector2.Z = 0f - _impulse.Z;
						_impulse.X += fVector4.X;
						_impulse.Y += fVector4.Y;
						_impulse.Z = 0f;
					}
					else
					{
						_impulse += fVector2;
					}
				}
				FVector2 fVector5 = new FVector2(fVector2.X, fVector2.Y);
				v -= invMassA * fVector5;
				num -= invIA * (MathUtils.Cross(m_rA, fVector5) + fVector2.Z);
				v2 += invMassB * fVector5;
				num2 += invIB * (MathUtils.Cross(m_rB, fVector5) + fVector2.Z);
			}
			else
			{
				FVector2 fVector6 = v2 + MathUtils.Cross(num2, m_rB) - v - MathUtils.Cross(num, m_rA);
				FVector2 fVector7 = m_mass.Solve22(-fVector6);
				_impulse.X += fVector7.X;
				_impulse.Y += fVector7.Y;
				v -= invMassA * fVector7;
				num -= invIA * MathUtils.Cross(m_rA, fVector7);
				v2 += invMassB * fVector7;
				num2 += invIB * MathUtils.Cross(m_rB, fVector7);
			}
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = num;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = num2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			FVector2 c = data.positions[m_indexA].c;
			float num = data.positions[m_indexA].a;
			FVector2 c2 = data.positions[m_indexB].c;
			float num2 = data.positions[m_indexB].a;
			Rot q = new Rot(num);
			Rot q2 = new Rot(num2);
			float num3 = 0f;
			bool flag = m_invIA + m_invIB == 0f;
			if (_enableLimit && _limitState != LimitState.Inactive && !flag)
			{
				float num4 = num2 - num - ReferenceAngle;
				float num5 = 0f;
				if (_limitState == LimitState.Equal)
				{
					float num6 = MathUtils.Clamp(num4 - _lowerAngle, -0.13962635f, 0.13962635f);
					num5 = (0f - m_motorMass) * num6;
					num3 = Math.Abs(num6);
				}
				else if (_limitState == LimitState.AtLower)
				{
					float num7 = num4 - _lowerAngle;
					num3 = 0f - num7;
					num7 = MathUtils.Clamp(num7 + 0.03490659f, -0.13962635f, 0f);
					num5 = (0f - m_motorMass) * num7;
				}
				else if (_limitState == LimitState.AtUpper)
				{
					float num8 = num4 - _upperAngle;
					num3 = num8;
					num8 = MathUtils.Clamp(num8 - 0.03490659f, 0f, 0.13962635f);
					num5 = (0f - m_motorMass) * num8;
				}
				num -= m_invIA * num5;
				num2 += m_invIB * num5;
			}
			q.Set(num);
			q2.Set(num2);
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			FVector2 b = c2 + fVector2 - c - fVector;
			float num9 = b.Length();
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			Mat22 mat = default(Mat22);
			mat.ex.X = invMassA + invMassB + invIA * fVector.Y * fVector.Y + invIB * fVector2.Y * fVector2.Y;
			mat.ex.Y = (0f - invIA) * fVector.X * fVector.Y - invIB * fVector2.X * fVector2.Y;
			mat.ey.X = mat.ex.Y;
			mat.ey.Y = invMassA + invMassB + invIA * fVector.X * fVector.X + invIB * fVector2.X * fVector2.X;
			FVector2 fVector3 = -mat.Solve(b);
			c -= invMassA * fVector3;
			num -= invIA * MathUtils.Cross(fVector, fVector3);
			c2 += invMassB * fVector3;
			num2 += invIB * MathUtils.Cross(fVector2, fVector3);
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = num;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = num2;
			return num9 <= 0.005f && num3 <= 0.03490659f;
		}
	}
}
