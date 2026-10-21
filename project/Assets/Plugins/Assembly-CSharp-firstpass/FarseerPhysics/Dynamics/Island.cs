using System;
using System.Diagnostics;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics
{
	public class Island
	{
		private const float LinTolSqr = 0.0001f;

		private const float AngTolSqr = 0.0012184699f;

		public Body[] Bodies;

		public int BodyCount;

		public int ContactCount;

		public int JointCount;

		public Velocity[] _velocities;

		public Position[] _positions;

		public int BodyCapacity;

		public int ContactCapacity;

		private ContactManager _contactManager;

		private ContactSolver _contactSolver = new ContactSolver();

		private Contact[] _contacts;

		public int JointCapacity;

		private FarseerJoint[] _joints;

		public float JointUpdateTime;

		private Stopwatch _watch = new Stopwatch();

		private float _tmpTime;

		public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
		{
			BodyCapacity = bodyCapacity;
			ContactCapacity = contactCapacity;
			JointCapacity = jointCapacity;
			BodyCount = 0;
			ContactCount = 0;
			JointCount = 0;
			_contactManager = contactManager;
			if (Bodies == null || Bodies.Length < bodyCapacity)
			{
				Bodies = new Body[bodyCapacity];
				_velocities = new Velocity[bodyCapacity];
				_positions = new Position[bodyCapacity];
			}
			if (_contacts == null || _contacts.Length < contactCapacity)
			{
				_contacts = new Contact[contactCapacity * 2];
			}
			if (_joints == null || _joints.Length < jointCapacity)
			{
				_joints = new FarseerJoint[jointCapacity * 2];
			}
		}

		public void Clear()
		{
			BodyCount = 0;
			ContactCount = 0;
			JointCount = 0;
		}

		public void Solve(ref TimeStep step, ref FVector2 gravity)
		{
			float dt = step.dt;
			for (int i = 0; i < BodyCount; i++)
			{
				Body body = Bodies[i];
				FVector2 c = body.Sweep.C;
				float a = body.Sweep.A;
				FVector2 linearVelocity = body.LinearVelocity;
				float num = body.AngularVelocity;
				body.Sweep.C0 = body.Sweep.C;
				body.Sweep.A0 = body.Sweep.A;
				if (body.BodyType == BodyType.Dynamic)
				{
					linearVelocity += dt * (body.GravityScale * gravity + body.InvMass * body.Force);
					num += dt * body.InvI * body.Torque;
					linearVelocity *= MathUtils.Clamp(1f - dt * body.LinearDamping, 0f, 1f);
					num *= MathUtils.Clamp(1f - dt * body.AngularDamping, 0f, 1f);
				}
				_positions[i].c = c;
				_positions[i].a = a;
				_velocities[i].v = linearVelocity;
				_velocities[i].w = num;
			}
			SolverData data = new SolverData
			{
				step = step,
				positions = _positions,
				velocities = _velocities
			};
			_contactSolver.Reset(step, ContactCount, _contacts, _positions, _velocities);
			_contactSolver.InitializeVelocityConstraints();
			if (Settings.EnableWarmstarting)
			{
				_contactSolver.WarmStart();
			}
			if (Settings.EnableDiagnostics)
			{
				_watch.Start();
				_tmpTime = 0f;
			}
			for (int j = 0; j < JointCount; j++)
			{
				if (_joints[j].Enabled)
				{
					_joints[j].InitVelocityConstraints(ref data);
				}
			}
			if (Settings.EnableDiagnostics)
			{
				_tmpTime += _watch.ElapsedTicks;
			}
			for (int k = 0; k < Settings.VelocityIterations; k++)
			{
				if (Settings.EnableDiagnostics)
				{
					_watch.Start();
				}
				for (int l = 0; l < JointCount; l++)
				{
					FarseerJoint farseerJoint = _joints[l];
					if (farseerJoint.Enabled)
					{
						farseerJoint.SolveVelocityConstraints(ref data);
						farseerJoint.Validate(step.inv_dt);
					}
				}
				if (Settings.EnableDiagnostics)
				{
					_watch.Stop();
					_tmpTime += _watch.ElapsedTicks;
					_watch.Reset();
				}
				_contactSolver.SolveVelocityConstraints();
			}
			_contactSolver.StoreImpulses();
			for (int m = 0; m < BodyCount; m++)
			{
				FVector2 c2 = _positions[m].c;
				float a2 = _positions[m].a;
				FVector2 v = _velocities[m].v;
				float num2 = _velocities[m].w;
				FVector2 fVector = dt * v;
				if (FVector2.Dot(fVector, fVector) > 4f)
				{
					float num3 = 2f / fVector.Length();
					v *= num3;
				}
				float num4 = dt * num2;
				if (num4 * num4 > 2.4674013f)
				{
					float num5 = (float)Math.PI / 2f / Math.Abs(num4);
					num2 *= num5;
				}
				c2 += dt * v;
				a2 += dt * num2;
				_positions[m].c = c2;
				_positions[m].a = a2;
				_velocities[m].v = v;
				_velocities[m].w = num2;
			}
			bool flag = false;
			for (int n = 0; n < Settings.PositionIterations; n++)
			{
				bool flag2 = _contactSolver.SolvePositionConstraints();
				bool flag3 = true;
				if (Settings.EnableDiagnostics)
				{
					_watch.Start();
				}
				for (int num6 = 0; num6 < JointCount; num6++)
				{
					FarseerJoint farseerJoint2 = _joints[num6];
					if (farseerJoint2.Enabled)
					{
						bool flag4 = farseerJoint2.SolvePositionConstraints(ref data);
						flag3 = flag3 && flag4;
					}
				}
				if (Settings.EnableDiagnostics)
				{
					_watch.Stop();
					_tmpTime += _watch.ElapsedTicks;
					_watch.Reset();
				}
				if (flag2 && flag3)
				{
					flag = true;
					break;
				}
			}
			if (Settings.EnableDiagnostics)
			{
				JointUpdateTime = _tmpTime;
			}
			for (int num7 = 0; num7 < BodyCount; num7++)
			{
				Body body2 = Bodies[num7];
				body2.Sweep.C = _positions[num7].c;
				body2.Sweep.A = _positions[num7].a;
				body2.LinearVelocity = _velocities[num7].v;
				body2.AngularVelocity = _velocities[num7].w;
				body2.SynchronizeTransform();
			}
			Report(_contactSolver._velocityConstraints);
			if (!Settings.AllowSleep)
			{
				return;
			}
			float num8 = float.MaxValue;
			for (int num9 = 0; num9 < BodyCount; num9++)
			{
				Body body3 = Bodies[num9];
				if (body3.BodyType != BodyType.Static)
				{
					if ((body3.Flags & BodyFlags.AutoSleep) == 0 || body3.AngularVelocityInternal * body3.AngularVelocityInternal > 0.0012184699f || FVector2.Dot(body3.LinearVelocityInternal, body3.LinearVelocityInternal) > 0.0001f)
					{
						body3.SleepTime = 0f;
						num8 = 0f;
					}
					else
					{
						body3.SleepTime += dt;
						num8 = Math.Min(num8, body3.SleepTime);
					}
				}
			}
			if (num8 >= 0.5f && flag)
			{
				for (int num10 = 0; num10 < BodyCount; num10++)
				{
					Body body4 = Bodies[num10];
					body4.Awake = false;
				}
			}
		}

		internal void SolveTOI(ref TimeStep subStep, int toiIndexA, int toiIndexB)
		{
			for (int i = 0; i < BodyCount; i++)
			{
				Body body = Bodies[i];
				_positions[i].c = body.Sweep.C;
				_positions[i].a = body.Sweep.A;
				_velocities[i].v = body.LinearVelocity;
				_velocities[i].w = body.AngularVelocity;
			}
			_contactSolver.Reset(subStep, ContactCount, _contacts, _positions, _velocities);
			for (int j = 0; j < Settings.TOIPositionIterations; j++)
			{
				if (_contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB))
				{
					break;
				}
			}
			Bodies[toiIndexA].Sweep.C0 = _positions[toiIndexA].c;
			Bodies[toiIndexA].Sweep.A0 = _positions[toiIndexA].a;
			Bodies[toiIndexB].Sweep.C0 = _positions[toiIndexB].c;
			Bodies[toiIndexB].Sweep.A0 = _positions[toiIndexB].a;
			_contactSolver.InitializeVelocityConstraints();
			for (int k = 0; k < Settings.TOIVelocityIterations; k++)
			{
				_contactSolver.SolveVelocityConstraints();
			}
			float dt = subStep.dt;
			for (int l = 0; l < BodyCount; l++)
			{
				FVector2 c = _positions[l].c;
				float a = _positions[l].a;
				FVector2 v = _velocities[l].v;
				float num = _velocities[l].w;
				FVector2 fVector = dt * v;
				if (FVector2.Dot(fVector, fVector) > 4f)
				{
					float num2 = 2f / fVector.Length();
					v *= num2;
				}
				float num3 = dt * num;
				if (num3 * num3 > 2.4674013f)
				{
					float num4 = (float)Math.PI / 2f / Math.Abs(num3);
					num *= num4;
				}
				c += dt * v;
				a += dt * num;
				_positions[l].c = c;
				_positions[l].a = a;
				_velocities[l].v = v;
				_velocities[l].w = num;
				Body body2 = Bodies[l];
				body2.Sweep.C = c;
				body2.Sweep.A = a;
				body2.LinearVelocity = v;
				body2.AngularVelocity = num;
				body2.SynchronizeTransform();
			}
			Report(_contactSolver._velocityConstraints);
		}

		public void Add(Body body)
		{
			body.IslandIndex = BodyCount;
			Bodies[BodyCount] = body;
			BodyCount++;
		}

		public void Add(Contact contact)
		{
			_contacts[ContactCount++] = contact;
		}

		public void Add(FarseerJoint joint)
		{
			_joints[JointCount++] = joint;
		}

		private void Report(ContactVelocityConstraint[] constraints)
		{
			if (_contactManager == null)
			{
				return;
			}
			for (int i = 0; i < ContactCount; i++)
			{
				Contact contact = _contacts[i];
				if (contact.FixtureA.AfterCollision != null)
				{
					contact.FixtureA.AfterCollision(contact.FixtureA, contact.FixtureB, contact);
				}
				if (contact.FixtureB.AfterCollision != null)
				{
					contact.FixtureB.AfterCollision(contact.FixtureB, contact.FixtureA, contact);
				}
				if (_contactManager.PostSolve != null)
				{
					ContactVelocityConstraint impulse = constraints[i];
					_contactManager.PostSolve(contact, impulse);
				}
			}
		}
	}
}
