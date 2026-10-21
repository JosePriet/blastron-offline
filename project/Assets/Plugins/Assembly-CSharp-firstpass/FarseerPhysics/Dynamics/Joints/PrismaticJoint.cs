using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class PrismaticJoint : FarseerJoint
	{
		public FVector2 LocalAnchorA;

		public FVector2 LocalAnchorB;

		private FVector2 _localXAxisA;

		private FVector2 _localYAxisA;

		private float m_referenceAngle;

		private FVector3 _impulse;

		private float _motorImpulse;

		private float _lowerTranslation;

		private float _upperTranslation;

		private float _maxMotorForce;

		private float _motorSpeed;

		private bool _enableLimit;

		private bool _enableMotor;

		private LimitState _limitState;

		private int m_indexA;

		private int m_indexB;

		private FVector2 m_localCenterA;

		private FVector2 m_localCenterB;

		private float m_invMassA;

		private float m_invMassB;

		private float m_invIA;

		private float m_invIB;

		private FVector2 m_axis;

		private FVector2 m_perp;

		private float m_s1;

		private float m_s2;

		private float m_a1;

		private float m_a2;

		private Mat33 m_K;

		private float m_motorMass;

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

		public float JointTranslation
		{
			get
			{
				FVector2 value = base.BodyB.GetWorldPoint(LocalAnchorB) - base.BodyA.GetWorldPoint(LocalAnchorA);
				FVector2 worldVector = base.BodyA.GetWorldVector(ref _localXAxisA);
				return FVector2.Dot(value, worldVector);
			}
		}

		public float JointSpeed
		{
			get
			{
				Transform transform;
				base.BodyA.GetTransform(out transform);
				Transform transform2;
				base.BodyB.GetTransform(out transform2);
				FVector2 fVector = MathUtils.Mul(ref transform.q, LocalAnchorA - base.BodyA.LocalCenter);
				FVector2 fVector2 = MathUtils.Mul(ref transform2.q, LocalAnchorB - base.BodyB.LocalCenter);
				FVector2 fVector3 = base.BodyA.Sweep.C + fVector;
				FVector2 fVector4 = base.BodyB.Sweep.C + fVector2;
				FVector2 value = fVector4 - fVector3;
				FVector2 worldVector = base.BodyA.GetWorldVector(ref _localXAxisA);
				FVector2 linearVelocityInternal = base.BodyA.LinearVelocityInternal;
				FVector2 linearVelocityInternal2 = base.BodyB.LinearVelocityInternal;
				float angularVelocityInternal = base.BodyA.AngularVelocityInternal;
				float angularVelocityInternal2 = base.BodyB.AngularVelocityInternal;
				return FVector2.Dot(value, MathUtils.Cross(angularVelocityInternal, worldVector)) + FVector2.Dot(worldVector, linearVelocityInternal2 + MathUtils.Cross(angularVelocityInternal2, fVector2) - linearVelocityInternal - MathUtils.Cross(angularVelocityInternal, fVector));
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
				if (value != _enableLimit)
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
				return _lowerTranslation;
			}
			set
			{
				if (value != _lowerTranslation)
				{
					WakeBodies();
					_lowerTranslation = value;
					_impulse.Z = 0f;
				}
			}
		}

		public float UpperLimit
		{
			get
			{
				return _upperTranslation;
			}
			set
			{
				if (value != _upperTranslation)
				{
					WakeBodies();
					_upperTranslation = value;
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

		public float MaxMotorForce
		{
			get
			{
				return _maxMotorForce;
			}
			set
			{
				WakeBodies();
				_maxMotorForce = value;
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
				_motorImpulse = value;
			}
		}

		public FVector2 LocalXAxisA
		{
			get
			{
				return _localXAxisA;
			}
			set
			{
				_localXAxisA = base.BodyA.GetLocalVector(value);
				_localYAxisA = MathUtils.Cross(1f, _localXAxisA);
			}
		}

		public float ReferenceAngle
		{
			get
			{
				return m_referenceAngle;
			}
			set
			{
				m_referenceAngle = value;
			}
		}

		internal PrismaticJoint()
		{
			base.JointType = JointType.Prismatic;
		}

		public PrismaticJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB, FVector2 axis)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Prismatic;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
			_localXAxisA = base.BodyA.GetLocalVector(axis);
			_localXAxisA.Normalize();
			_localYAxisA = MathUtils.Cross(1f, _localXAxisA);
			m_referenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
			_limitState = LimitState.Inactive;
		}

		public void SetLimits(float lower, float upper)
		{
			if (upper != _upperTranslation || lower != _lowerTranslation)
			{
				WakeBodies();
				_upperTranslation = upper;
				_lowerTranslation = lower;
				_impulse.Z = 0f;
			}
		}

		public float GetMotorForce(float inv_dt)
		{
			return inv_dt * _motorImpulse;
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			return inv_dt * (_impulse.X * m_perp + (_motorImpulse + _impulse.Z) * m_axis);
		}

		public override float GetReactionTorque(float inv_dt)
		{
			return inv_dt * _impulse.Y;
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
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - m_localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - m_localCenterB);
			FVector2 fVector3 = c2 - c + fVector2 - fVector;
			float invMassA = m_invMassA;
			float invMassB = m_invMassB;
			float invIA = m_invIA;
			float invIB = m_invIB;
			m_axis = MathUtils.Mul(q, LocalXAxisA);
			m_a1 = MathUtils.Cross(fVector3 + fVector, m_axis);
			m_a2 = MathUtils.Cross(fVector2, m_axis);
			m_motorMass = invMassA + invMassB + invIA * m_a1 * m_a1 + invIB * m_a2 * m_a2;
			if (m_motorMass > 0f)
			{
				m_motorMass = 1f / m_motorMass;
			}
			m_perp = MathUtils.Mul(q, _localYAxisA);
			m_s1 = MathUtils.Cross(fVector3 + fVector, m_perp);
			m_s2 = MathUtils.Cross(fVector2, m_perp);
			float x = invMassA + invMassB + invIA * m_s1 * m_s1 + invIB * m_s2 * m_s2;
			float num3 = invIA * m_s1 + invIB * m_s2;
			float num4 = invIA * m_s1 * m_a1 + invIB * m_s2 * m_a2;
			float num5 = invIA + invIB;
			if (num5 == 0f)
			{
				num5 = 1f;
			}
			float num6 = invIA * m_a1 + invIB * m_a2;
			float z = invMassA + invMassB + invIA * m_a1 * m_a1 + invIB * m_a2 * m_a2;
			m_K.ex = new FVector3(x, num3, num4);
			m_K.ey = new FVector3(num3, num5, num6);
			m_K.ez = new FVector3(num4, num6, z);
			if (_enableLimit)
			{
				float num7 = FVector2.Dot(m_axis, fVector3);
				if (Math.Abs(_upperTranslation - _lowerTranslation) < 0.01f)
				{
					_limitState = LimitState.Equal;
				}
				else if (num7 <= _lowerTranslation)
				{
					if (_limitState != LimitState.AtLower)
					{
						_limitState = LimitState.AtLower;
						_impulse.Z = 0f;
					}
				}
				else if (num7 >= _upperTranslation)
				{
					if (_limitState != LimitState.AtUpper)
					{
						_limitState = LimitState.AtUpper;
						_impulse.Z = 0f;
					}
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
			if (!_enableMotor)
			{
				_motorImpulse = 0f;
			}
			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.step.dtRatio;
				_motorImpulse *= data.step.dtRatio;
				FVector2 fVector4 = _impulse.X * m_perp + (_motorImpulse + _impulse.Z) * m_axis;
				float num8 = _impulse.X * m_s1 + _impulse.Y + (_motorImpulse + _impulse.Z) * m_a1;
				float num9 = _impulse.X * m_s2 + _impulse.Y + (_motorImpulse + _impulse.Z) * m_a2;
				v -= invMassA * fVector4;
				num -= invIA * num8;
				v2 += invMassB * fVector4;
				num2 += invIB * num9;
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
			if (_enableMotor && _limitState != LimitState.Equal)
			{
				float num3 = FVector2.Dot(m_axis, v2 - v) + m_a2 * num2 - m_a1 * num;
				float num4 = m_motorMass * (_motorSpeed - num3);
				float motorImpulse = _motorImpulse;
				float num5 = data.step.dt * _maxMotorForce;
				_motorImpulse = MathUtils.Clamp(_motorImpulse + num4, 0f - num5, num5);
				num4 = _motorImpulse - motorImpulse;
				FVector2 fVector = num4 * m_axis;
				float num6 = num4 * m_a1;
				float num7 = num4 * m_a2;
				v -= invMassA * fVector;
				num -= invIA * num6;
				v2 += invMassB * fVector;
				num2 += invIB * num7;
			}
			FVector2 fVector2 = new FVector2
			{
				X = FVector2.Dot(m_perp, v2 - v) + m_s2 * num2 - m_s1 * num,
				Y = num2 - num
			};
			if (_enableLimit && _limitState != LimitState.Inactive)
			{
				float z = FVector2.Dot(m_axis, v2 - v) + m_a2 * num2 - m_a1 * num;
				FVector3 fVector3 = new FVector3(fVector2.X, fVector2.Y, z);
				FVector3 impulse = _impulse;
				FVector3 fVector4 = m_K.Solve33(-fVector3);
				_impulse += fVector4;
				if (_limitState == LimitState.AtLower)
				{
					_impulse.Z = Math.Max(_impulse.Z, 0f);
				}
				else if (_limitState == LimitState.AtUpper)
				{
					_impulse.Z = Math.Min(_impulse.Z, 0f);
				}
				FVector2 b = -fVector2 - (_impulse.Z - impulse.Z) * new FVector2(m_K.ez.X, m_K.ez.Y);
				FVector2 fVector5 = m_K.Solve22(b) + new FVector2(impulse.X, impulse.Y);
				_impulse.X = fVector5.X;
				_impulse.Y = fVector5.Y;
				fVector4 = _impulse - impulse;
				FVector2 fVector6 = fVector4.X * m_perp + fVector4.Z * m_axis;
				float num8 = fVector4.X * m_s1 + fVector4.Y + fVector4.Z * m_a1;
				float num9 = fVector4.X * m_s2 + fVector4.Y + fVector4.Z * m_a2;
				v -= invMassA * fVector6;
				num -= invIA * num8;
				v2 += invMassB * fVector6;
				num2 += invIB * num9;
			}
			else
			{
				FVector2 fVector7 = m_K.Solve22(-fVector2);
				_impulse.X += fVector7.X;
				_impulse.Y += fVector7.Y;
				FVector2 fVector8 = fVector7.X * m_perp;
				float num10 = fVector7.X * m_s1 + fVector7.Y;
				float num11 = fVector7.X * m_s2 + fVector7.Y;
				v -= invMassA * fVector8;
				num -= invIA * num10;
				v2 += invMassB * fVector8;
				num2 += invIB * num11;
				fVector2.X = FVector2.Dot(m_perp, v2 - v) + m_s2 * num2 - m_s1 * num;
				fVector2.Y = num2 - num;
				if (Math.Abs(fVector2.X) > 0.01f || Math.Abs(fVector2.Y) > 0.01f)
				{
					fVector2.X = fVector2.X;
				}
			}
			data.velocities[m_indexA].v = v;
			data.velocities[m_indexA].w = num;
			data.velocities[m_indexB].v = v2;
			data.velocities[m_indexB].w = num2;
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
			FVector2 fVector3 = c2 + fVector2 - c - fVector;
			FVector2 fVector4 = MathUtils.Mul(q, LocalXAxisA);
			float num = MathUtils.Cross(fVector3 + fVector, fVector4);
			float num2 = MathUtils.Cross(fVector2, fVector4);
			FVector2 fVector5 = MathUtils.Mul(q, _localYAxisA);
			float num3 = MathUtils.Cross(fVector3 + fVector, fVector5);
			float num4 = MathUtils.Cross(fVector2, fVector5);
			FVector2 fVector6 = new FVector2
			{
				X = FVector2.Dot(fVector5, fVector3),
				Y = a2 - a - m_referenceAngle
			};
			float num5 = Math.Abs(fVector6.X);
			float num6 = Math.Abs(fVector6.Y);
			bool flag = false;
			float z = 0f;
			if (_enableLimit)
			{
				float num7 = FVector2.Dot(fVector4, fVector3);
				if (Math.Abs(_upperTranslation - _lowerTranslation) < 0.01f)
				{
					z = MathUtils.Clamp(num7, -0.2f, 0.2f);
					num5 = Math.Max(num5, Math.Abs(num7));
					flag = true;
				}
				else if (num7 <= _lowerTranslation)
				{
					z = MathUtils.Clamp(num7 - _lowerTranslation + 0.005f, -0.2f, 0f);
					num5 = Math.Max(num5, _lowerTranslation - num7);
					flag = true;
				}
				else if (num7 >= _upperTranslation)
				{
					z = MathUtils.Clamp(num7 - _upperTranslation - 0.005f, 0f, 0.2f);
					num5 = Math.Max(num5, num7 - _upperTranslation);
					flag = true;
				}
			}
			FVector3 fVector7;
			if (flag)
			{
				float x = invMassA + invMassB + invIA * num3 * num3 + invIB * num4 * num4;
				float num8 = invIA * num3 + invIB * num4;
				float num9 = invIA * num3 * num + invIB * num4 * num2;
				float num10 = invIA + invIB;
				if (num10 == 0f)
				{
					num10 = 1f;
				}
				float num11 = invIA * num + invIB * num2;
				float z2 = invMassA + invMassB + invIA * num * num + invIB * num2 * num2;
				fVector7 = new Mat33
				{
					ex = new FVector3(x, num8, num9),
					ey = new FVector3(num8, num10, num11),
					ez = new FVector3(num9, num11, z2)
				}.Solve33(-new FVector3
				{
					X = fVector6.X,
					Y = fVector6.Y,
					Z = z
				});
			}
			else
			{
				float x2 = invMassA + invMassB + invIA * num3 * num3 + invIB * num4 * num4;
				float num12 = invIA * num3 + invIB * num4;
				float num13 = invIA + invIB;
				if (num13 == 0f)
				{
					num13 = 1f;
				}
				FVector2 fVector8 = new Mat22
				{
					ex = new FVector2(x2, num12),
					ey = new FVector2(num12, num13)
				}.Solve(-fVector6);
				fVector7 = new FVector3
				{
					X = fVector8.X,
					Y = fVector8.Y,
					Z = 0f
				};
			}
			FVector2 fVector9 = fVector7.X * fVector5 + fVector7.Z * fVector4;
			float num14 = fVector7.X * num3 + fVector7.Y + fVector7.Z * num;
			float num15 = fVector7.X * num4 + fVector7.Y + fVector7.Z * num2;
			c -= invMassA * fVector9;
			a -= invIA * num14;
			c2 += invMassB * fVector9;
			a2 += invIB * num15;
			data.positions[m_indexA].c = c;
			data.positions[m_indexA].a = a;
			data.positions[m_indexB].c = c2;
			data.positions[m_indexB].a = a2;
			return num5 <= 0.005f && num6 <= 0.03490659f;
		}
	}
}
