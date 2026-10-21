using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class RopeJoint : FarseerJoint
	{
		private float _impulse;

		private float _length;

		private int _indexA;

		private int _indexB;

		private FVector2 _localCenterA;

		private FVector2 _localCenterB;

		private float _invMassA;

		private float _invMassB;

		private float _invIA;

		private float _invIB;

		private float _mass;

		private FVector2 _rA;

		private FVector2 _rB;

		private LimitState _state;

		private FVector2 _u;

		public float MaxLength { get; set; }

		public LimitState State
		{
			get
			{
				return _state;
			}
		}

		public FVector2 LocalAnchorA { get; set; }

		public FVector2 LocalAnchorB { get; set; }

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

		internal RopeJoint()
		{
			base.JointType = JointType.Rope;
		}

		public RopeJoint(Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB)
			: base(bodyA, bodyB)
		{
			base.JointType = JointType.Rope;
			LocalAnchorA = localAnchorA;
			LocalAnchorB = localAnchorB;
			MaxLength = (WorldAnchorB - WorldAnchorA).Length();
		}

		public override FVector2 GetReactionForce(float invDt)
		{
			return invDt * _impulse * _u;
		}

		public override float GetReactionTorque(float invDt)
		{
			return 0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			_indexA = base.BodyA.IslandIndex;
			_indexB = base.BodyB.IslandIndex;
			_localCenterA = base.BodyA.Sweep.LocalCenter;
			_localCenterB = base.BodyB.Sweep.LocalCenter;
			_invMassA = base.BodyA.InvMass;
			_invMassB = base.BodyB.InvMass;
			_invIA = base.BodyA.InvI;
			_invIB = base.BodyB.InvI;
			FVector2 c = data.positions[_indexA].c;
			float a = data.positions[_indexA].a;
			FVector2 v = data.velocities[_indexA].v;
			float num = data.velocities[_indexA].w;
			FVector2 c2 = data.positions[_indexB].c;
			float a2 = data.positions[_indexB].a;
			FVector2 v2 = data.velocities[_indexB].v;
			float num2 = data.velocities[_indexB].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			_rA = MathUtils.Mul(q, LocalAnchorA - _localCenterA);
			_rB = MathUtils.Mul(q2, LocalAnchorB - _localCenterB);
			_u = c2 + _rB - c - _rA;
			_length = _u.Length();
			float num3 = _length - MaxLength;
			if (num3 > 0f)
			{
				_state = LimitState.AtUpper;
			}
			else
			{
				_state = LimitState.Inactive;
			}
			if (_length > 0.005f)
			{
				_u *= 1f / _length;
				float num4 = MathUtils.Cross(_rA, _u);
				float num5 = MathUtils.Cross(_rB, _u);
				float num6 = _invMassA + _invIA * num4 * num4 + _invMassB + _invIB * num5 * num5;
				_mass = ((num6 == 0f) ? 0f : (1f / num6));
				if (Settings.EnableWarmstarting)
				{
					_impulse *= data.step.dtRatio;
					FVector2 fVector = _impulse * _u;
					v -= _invMassA * fVector;
					num -= _invIA * MathUtils.Cross(_rA, fVector);
					v2 += _invMassB * fVector;
					num2 += _invIB * MathUtils.Cross(_rB, fVector);
				}
				else
				{
					_impulse = 0f;
				}
				data.velocities[_indexA].v = v;
				data.velocities[_indexA].w = num;
				data.velocities[_indexB].v = v2;
				data.velocities[_indexB].w = num2;
			}
			else
			{
				_u = FVector2.Zero;
				_mass = 0f;
				_impulse = 0f;
			}
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			FVector2 v = data.velocities[_indexA].v;
			float w = data.velocities[_indexA].w;
			FVector2 v2 = data.velocities[_indexB].v;
			float w2 = data.velocities[_indexB].w;
			FVector2 fVector = v + MathUtils.Cross(w, _rA);
			FVector2 fVector2 = v2 + MathUtils.Cross(w2, _rB);
			float num = _length - MaxLength;
			float num2 = FVector2.Dot(_u, fVector2 - fVector);
			if (num < 0f)
			{
				num2 += data.step.inv_dt * num;
			}
			float num3 = (0f - _mass) * num2;
			float impulse = _impulse;
			_impulse = Math.Min(0f, _impulse + num3);
			num3 = _impulse - impulse;
			FVector2 fVector3 = num3 * _u;
			v -= _invMassA * fVector3;
			w -= _invIA * MathUtils.Cross(_rA, fVector3);
			v2 += _invMassB * fVector3;
			w2 += _invIB * MathUtils.Cross(_rB, fVector3);
			data.velocities[_indexA].v = v;
			data.velocities[_indexA].w = w;
			data.velocities[_indexB].v = v2;
			data.velocities[_indexB].w = w2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			FVector2 c = data.positions[_indexA].c;
			float a = data.positions[_indexA].a;
			FVector2 c2 = data.positions[_indexB].c;
			float a2 = data.positions[_indexB].a;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			FVector2 fVector = MathUtils.Mul(q, LocalAnchorA - _localCenterA);
			FVector2 fVector2 = MathUtils.Mul(q2, LocalAnchorB - _localCenterB);
			FVector2 fVector3 = c2 + fVector2 - c - fVector;
			float num = fVector3.Length();
			fVector3.Normalize();
			float a3 = num - MaxLength;
			a3 = MathUtils.Clamp(a3, 0f, 0.2f);
			float num2 = (0f - _mass) * a3;
			FVector2 fVector4 = num2 * fVector3;
			c -= _invMassA * fVector4;
			a -= _invIA * MathUtils.Cross(fVector, fVector4);
			c2 += _invMassB * fVector4;
			a2 += _invIB * MathUtils.Cross(fVector2, fVector4);
			data.positions[_indexA].c = c;
			data.positions[_indexA].a = a;
			data.positions[_indexB].c = c2;
			data.positions[_indexB].a = a2;
			return num - MaxLength < 0.005f;
		}
	}
}
