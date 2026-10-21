using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Joints
{
	public class FixedMouseJoint : FarseerJoint
	{
		private FVector2 _targetA;

		private float _frequency;

		private float _dampingRatio;

		private float _beta;

		private FVector2 _impulse;

		private float _maxForce;

		private float _gamma;

		private int _indexA;

		private int _indexB;

		private FVector2 _rB;

		private FVector2 _localCenterB;

		private float _invMassB;

		private float _invIB;

		private Mat22 _mass;

		private FVector2 _C;

		public FVector2 LocalAnchorB { get; set; }

		public override FVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.GetWorldPoint(LocalAnchorB);
			}
		}

		public override FVector2 WorldAnchorB
		{
			get
			{
				return _targetA;
			}
			set
			{
				base.BodyA.Awake = true;
				_targetA = value;
			}
		}

		public float MaxForce
		{
			get
			{
				return _maxForce;
			}
			set
			{
				_maxForce = value;
			}
		}

		public float Frequency
		{
			get
			{
				return _frequency;
			}
			set
			{
				_frequency = value;
			}
		}

		public float DampingRatio
		{
			get
			{
				return _dampingRatio;
			}
			set
			{
				_dampingRatio = value;
			}
		}

		public FixedMouseJoint(Body body, FVector2 worldAnchor)
			: base(body)
		{
			base.JointType = JointType.FixedMouse;
			Frequency = 5f;
			DampingRatio = 0.7f;
			MaxForce = 1000f * body.Mass;
			_targetA = worldAnchor;
			LocalAnchorB = MathUtils.MulT(base.BodyA.Xf, worldAnchor);
		}

		public override FVector2 GetReactionForce(float inv_dt)
		{
			return inv_dt * _impulse;
		}

		public override float GetReactionTorque(float inv_dt)
		{
			return inv_dt * 0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			_indexB = base.BodyA.IslandIndex;
			_localCenterB = base.BodyA.Sweep.LocalCenter;
			_invMassB = base.BodyA.InvMass;
			_invIB = 0f;
			FVector2 c = data.positions[_indexB].c;
			float a = data.positions[_indexB].a;
			FVector2 v = data.velocities[_indexB].v;
			float w = data.velocities[_indexB].w;
			Rot q = new Rot(a);
			float mass = base.BodyA.Mass;
			float num = (float)Math.PI * 2f * Frequency;
			float num2 = 2f * mass * DampingRatio * num;
			float num3 = mass * (num * num);
			float dt = data.step.dt;
			_gamma = dt * (num2 + dt * num3);
			if (_gamma != 0f)
			{
				_gamma = 1f / _gamma;
			}
			_beta = dt * num3 * _gamma;
			_rB = MathUtils.Mul(q, LocalAnchorB - _localCenterB);
			Mat22 mat = default(Mat22);
			mat.ex.X = _invMassB + _invIB * _rB.Y * _rB.Y + _gamma;
			mat.ex.Y = (0f - _invIB) * _rB.X * _rB.Y;
			mat.ey.X = mat.ex.Y;
			mat.ey.Y = _invMassB + _invIB * _rB.X * _rB.X + _gamma;
			_mass = mat.Inverse;
			_C = c + _rB - _targetA;
			_C *= _beta;
			w *= 0.98f;
			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.step.dtRatio;
				v += _invMassB * _impulse;
				w += _invIB * MathUtils.Cross(_rB, _impulse);
			}
			else
			{
				_impulse = FVector2.Zero;
			}
			data.velocities[_indexB].v = v;
			data.velocities[_indexB].w = w;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			FVector2 v = data.velocities[_indexB].v;
			float w = data.velocities[_indexB].w;
			FVector2 fVector = v + MathUtils.Cross(w, _rB);
			FVector2 fVector2 = MathUtils.Mul(ref _mass, -(fVector + _C + _gamma * _impulse));
			FVector2 impulse = _impulse;
			_impulse += fVector2;
			float num = data.step.dt * MaxForce;
			if (_impulse.LengthSquared() > num * num)
			{
				_impulse *= num / _impulse.Length();
			}
			fVector2 = _impulse - impulse;
			v += _invMassB * fVector2;
			w += _invIB * MathUtils.Cross(_rB, fVector2);
			data.velocities[_indexB].v = v;
			data.velocities[_indexB].w = w;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}
