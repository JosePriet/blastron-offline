using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Dynamics.Contacts
{
	public class ContactSolver
	{
		public static class WorldManifold
		{
			public static void Initialize(ref Manifold manifold, ref Transform xfA, float radiusA, ref Transform xfB, float radiusB, out FVector2 normal, out FixedArray2<FVector2> points)
			{
				normal = FVector2.Zero;
				points = default(FixedArray2<FVector2>);
				if (manifold.PointCount == 0)
				{
					return;
				}
				switch (manifold.Type)
				{
				case ManifoldType.Circles:
				{
					normal = new FVector2(1f, 0f);
					FVector2 fVector9 = MathUtils.Mul(ref xfA, manifold.LocalPoint);
					FVector2 fVector10 = MathUtils.Mul(ref xfB, manifold.Points[0].LocalPoint);
					if (FVector2.DistanceSquared(fVector9, fVector10) > 1.4210855E-14f)
					{
						normal = fVector10 - fVector9;
						normal.Normalize();
					}
					FVector2 fVector11 = fVector9 + radiusA * normal;
					FVector2 fVector12 = fVector10 - radiusB * normal;
					points[0] = 0.5f * (fVector11 + fVector12);
					break;
				}
				case ManifoldType.FaceA:
				{
					normal = MathUtils.Mul(xfA.q, manifold.LocalNormal);
					FVector2 fVector5 = MathUtils.Mul(ref xfA, manifold.LocalPoint);
					for (int j = 0; j < manifold.PointCount; j++)
					{
						FVector2 fVector6 = MathUtils.Mul(ref xfB, manifold.Points[j].LocalPoint);
						FVector2 fVector7 = fVector6 + (radiusA - FVector2.Dot(fVector6 - fVector5, normal)) * normal;
						FVector2 fVector8 = fVector6 - radiusB * normal;
						points[j] = 0.5f * (fVector7 + fVector8);
					}
					break;
				}
				case ManifoldType.FaceB:
				{
					normal = MathUtils.Mul(xfB.q, manifold.LocalNormal);
					FVector2 fVector = MathUtils.Mul(ref xfB, manifold.LocalPoint);
					for (int i = 0; i < manifold.PointCount; i++)
					{
						FVector2 fVector2 = MathUtils.Mul(ref xfA, manifold.Points[i].LocalPoint);
						FVector2 fVector3 = fVector2 + (radiusB - FVector2.Dot(fVector2 - fVector, normal)) * normal;
						FVector2 fVector4 = fVector2 - radiusA * normal;
						points[i] = 0.5f * (fVector4 + fVector3);
					}
					normal = -normal;
					break;
				}
				}
			}
		}

		private static class PositionSolverManifold
		{
			public static void Initialize(ContactPositionConstraint pc, Transform xfA, Transform xfB, int index, out FVector2 normal, out FVector2 point, out float separation)
			{
				switch (pc.type)
				{
				case ManifoldType.Circles:
				{
					FVector2 fVector5 = MathUtils.Mul(ref xfA, pc.localPoint);
					FVector2 fVector6 = MathUtils.Mul(ref xfB, pc.localPoints[0]);
					normal = fVector6 - fVector5;
					normal.Normalize();
					point = 0.5f * (fVector5 + fVector6);
					separation = FVector2.Dot(fVector6 - fVector5, normal) - pc.radiusA - pc.radiusB;
					break;
				}
				case ManifoldType.FaceA:
				{
					normal = MathUtils.Mul(xfA.q, pc.localNormal);
					FVector2 fVector3 = MathUtils.Mul(ref xfA, pc.localPoint);
					FVector2 fVector4 = MathUtils.Mul(ref xfB, pc.localPoints[index]);
					separation = FVector2.Dot(fVector4 - fVector3, normal) - pc.radiusA - pc.radiusB;
					point = fVector4;
					break;
				}
				case ManifoldType.FaceB:
				{
					normal = MathUtils.Mul(xfB.q, pc.localNormal);
					FVector2 fVector = MathUtils.Mul(ref xfB, pc.localPoint);
					FVector2 fVector2 = MathUtils.Mul(ref xfA, pc.localPoints[index]);
					separation = FVector2.Dot(fVector2 - fVector, normal) - pc.radiusA - pc.radiusB;
					point = fVector2;
					normal = -normal;
					break;
				}
				default:
					normal = FVector2.Zero;
					point = FVector2.Zero;
					separation = 0f;
					break;
				}
			}
		}

		public TimeStep _step;

		public Position[] _positions;

		public Velocity[] _velocities;

		public ContactPositionConstraint[] _positionConstraints;

		public ContactVelocityConstraint[] _velocityConstraints;

		public Contact[] _contacts;

		public int _count;

		public void Reset(TimeStep step, int count, Contact[] contacts, Position[] positions, Velocity[] velocities)
		{
			_step = step;
			_count = count;
			_positions = positions;
			_velocities = velocities;
			_contacts = contacts;
			if (_velocityConstraints == null || _velocityConstraints.Length < count)
			{
				_velocityConstraints = new ContactVelocityConstraint[count * 2];
				_positionConstraints = new ContactPositionConstraint[count * 2];
				for (int i = 0; i < _velocityConstraints.Length; i++)
				{
					_velocityConstraints[i] = new ContactVelocityConstraint();
				}
				for (int j = 0; j < _positionConstraints.Length; j++)
				{
					_positionConstraints[j] = new ContactPositionConstraint();
				}
			}
			for (int k = 0; k < _count; k++)
			{
				Contact contact = contacts[k];
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				Shape shape = fixtureA.Shape;
				Shape shape2 = fixtureB.Shape;
				float radius = shape.Radius;
				float radius2 = shape2.Radius;
				Body body = fixtureA.Body;
				Body body2 = fixtureB.Body;
				Manifold manifold = contact.Manifold;
				int pointCount = manifold.PointCount;
				ContactVelocityConstraint contactVelocityConstraint = _velocityConstraints[k];
				contactVelocityConstraint.friction = contact.Friction;
				contactVelocityConstraint.restitution = contact.Restitution;
				contactVelocityConstraint.tangentSpeed = contact.TangentSpeed;
				contactVelocityConstraint.indexA = body.IslandIndex;
				contactVelocityConstraint.indexB = body2.IslandIndex;
				contactVelocityConstraint.invMassA = body.InvMass;
				contactVelocityConstraint.invMassB = body2.InvMass;
				contactVelocityConstraint.invIA = body.InvI;
				contactVelocityConstraint.invIB = body2.InvI;
				contactVelocityConstraint.contactIndex = k;
				contactVelocityConstraint.pointCount = pointCount;
				contactVelocityConstraint.K.SetZero();
				contactVelocityConstraint.normalMass.SetZero();
				ContactPositionConstraint contactPositionConstraint = _positionConstraints[k];
				contactPositionConstraint.indexA = body.IslandIndex;
				contactPositionConstraint.indexB = body2.IslandIndex;
				contactPositionConstraint.invMassA = body.InvMass;
				contactPositionConstraint.invMassB = body2.InvMass;
				contactPositionConstraint.localCenterA = body.Sweep.LocalCenter;
				contactPositionConstraint.localCenterB = body2.Sweep.LocalCenter;
				contactPositionConstraint.invIA = body.InvI;
				contactPositionConstraint.invIB = body2.InvI;
				contactPositionConstraint.localNormal = manifold.LocalNormal;
				contactPositionConstraint.localPoint = manifold.LocalPoint;
				contactPositionConstraint.pointCount = pointCount;
				contactPositionConstraint.radiusA = radius;
				contactPositionConstraint.radiusB = radius2;
				contactPositionConstraint.type = manifold.Type;
				for (int l = 0; l < pointCount; l++)
				{
					ManifoldPoint manifoldPoint = manifold.Points[l];
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[l];
					if (Settings.EnableWarmstarting)
					{
						velocityConstraintPoint.normalImpulse = _step.dtRatio * manifoldPoint.NormalImpulse;
						velocityConstraintPoint.tangentImpulse = _step.dtRatio * manifoldPoint.TangentImpulse;
					}
					else
					{
						velocityConstraintPoint.normalImpulse = 0f;
						velocityConstraintPoint.tangentImpulse = 0f;
					}
					velocityConstraintPoint.rA = FVector2.Zero;
					velocityConstraintPoint.rB = FVector2.Zero;
					velocityConstraintPoint.normalMass = 0f;
					velocityConstraintPoint.tangentMass = 0f;
					velocityConstraintPoint.velocityBias = 0f;
					contactPositionConstraint.localPoints[l] = manifoldPoint.LocalPoint;
				}
			}
		}

		public void InitializeVelocityConstraints()
		{
			for (int i = 0; i < _count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = _velocityConstraints[i];
				ContactPositionConstraint contactPositionConstraint = _positionConstraints[i];
				float radiusA = contactPositionConstraint.radiusA;
				float radiusB = contactPositionConstraint.radiusB;
				Manifold manifold = _contacts[contactVelocityConstraint.contactIndex].Manifold;
				int indexA = contactVelocityConstraint.indexA;
				int indexB = contactVelocityConstraint.indexB;
				float invMassA = contactVelocityConstraint.invMassA;
				float invMassB = contactVelocityConstraint.invMassB;
				float invIA = contactVelocityConstraint.invIA;
				float invIB = contactVelocityConstraint.invIB;
				FVector2 localCenterA = contactPositionConstraint.localCenterA;
				FVector2 localCenterB = contactPositionConstraint.localCenterB;
				FVector2 c = _positions[indexA].c;
				float a = _positions[indexA].a;
				FVector2 v = _velocities[indexA].v;
				float w = _velocities[indexA].w;
				FVector2 c2 = _positions[indexB].c;
				float a2 = _positions[indexB].a;
				FVector2 v2 = _velocities[indexB].v;
				float w2 = _velocities[indexB].w;
				Transform xfA = default(Transform);
				Transform xfB = default(Transform);
				xfA.q.Set(a);
				xfB.q.Set(a2);
				xfA.p = c - MathUtils.Mul(xfA.q, localCenterA);
				xfB.p = c2 - MathUtils.Mul(xfB.q, localCenterB);
				FVector2 normal;
				FixedArray2<FVector2> points;
				WorldManifold.Initialize(ref manifold, ref xfA, radiusA, ref xfB, radiusB, out normal, out points);
				contactVelocityConstraint.normal = normal;
				int pointCount = contactVelocityConstraint.pointCount;
				for (int j = 0; j < pointCount; j++)
				{
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[j];
					velocityConstraintPoint.rA = points[j] - c;
					velocityConstraintPoint.rB = points[j] - c2;
					float num = MathUtils.Cross(velocityConstraintPoint.rA, contactVelocityConstraint.normal);
					float num2 = MathUtils.Cross(velocityConstraintPoint.rB, contactVelocityConstraint.normal);
					float num3 = invMassA + invMassB + invIA * num * num + invIB * num2 * num2;
					velocityConstraintPoint.normalMass = ((!(num3 > 0f)) ? 0f : (1f / num3));
					FVector2 b = MathUtils.Cross(contactVelocityConstraint.normal, 1f);
					float num4 = MathUtils.Cross(velocityConstraintPoint.rA, b);
					float num5 = MathUtils.Cross(velocityConstraintPoint.rB, b);
					float num6 = invMassA + invMassB + invIA * num4 * num4 + invIB * num5 * num5;
					velocityConstraintPoint.tangentMass = ((!(num6 > 0f)) ? 0f : (1f / num6));
					velocityConstraintPoint.velocityBias = 0f;
					float num7 = FVector2.Dot(contactVelocityConstraint.normal, v2 + MathUtils.Cross(w2, velocityConstraintPoint.rB) - v - MathUtils.Cross(w, velocityConstraintPoint.rA));
					if (num7 < -1f)
					{
						velocityConstraintPoint.velocityBias = (0f - contactVelocityConstraint.restitution) * num7;
					}
				}
				if (contactVelocityConstraint.pointCount == 2)
				{
					VelocityConstraintPoint velocityConstraintPoint2 = contactVelocityConstraint.points[0];
					VelocityConstraintPoint velocityConstraintPoint3 = contactVelocityConstraint.points[1];
					float num8 = MathUtils.Cross(velocityConstraintPoint2.rA, contactVelocityConstraint.normal);
					float num9 = MathUtils.Cross(velocityConstraintPoint2.rB, contactVelocityConstraint.normal);
					float num10 = MathUtils.Cross(velocityConstraintPoint3.rA, contactVelocityConstraint.normal);
					float num11 = MathUtils.Cross(velocityConstraintPoint3.rB, contactVelocityConstraint.normal);
					float num12 = invMassA + invMassB + invIA * num8 * num8 + invIB * num9 * num9;
					float num13 = invMassA + invMassB + invIA * num10 * num10 + invIB * num11 * num11;
					float num14 = invMassA + invMassB + invIA * num8 * num10 + invIB * num9 * num11;
					if (num12 * num12 < 1000f * (num12 * num13 - num14 * num14))
					{
						contactVelocityConstraint.K.ex = new FVector2(num12, num14);
						contactVelocityConstraint.K.ey = new FVector2(num14, num13);
						contactVelocityConstraint.normalMass = contactVelocityConstraint.K.Inverse;
					}
					else
					{
						contactVelocityConstraint.pointCount = 1;
					}
				}
			}
		}

		public void WarmStart()
		{
			for (int i = 0; i < _count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = _velocityConstraints[i];
				int indexA = contactVelocityConstraint.indexA;
				int indexB = contactVelocityConstraint.indexB;
				float invMassA = contactVelocityConstraint.invMassA;
				float invIA = contactVelocityConstraint.invIA;
				float invMassB = contactVelocityConstraint.invMassB;
				float invIB = contactVelocityConstraint.invIB;
				int pointCount = contactVelocityConstraint.pointCount;
				FVector2 v = _velocities[indexA].v;
				float num = _velocities[indexA].w;
				FVector2 v2 = _velocities[indexB].v;
				float num2 = _velocities[indexB].w;
				FVector2 normal = contactVelocityConstraint.normal;
				FVector2 fVector = MathUtils.Cross(normal, 1f);
				for (int j = 0; j < pointCount; j++)
				{
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[j];
					FVector2 fVector2 = velocityConstraintPoint.normalImpulse * normal + velocityConstraintPoint.tangentImpulse * fVector;
					num -= invIA * MathUtils.Cross(velocityConstraintPoint.rA, fVector2);
					v -= invMassA * fVector2;
					num2 += invIB * MathUtils.Cross(velocityConstraintPoint.rB, fVector2);
					v2 += invMassB * fVector2;
				}
				_velocities[indexA].v = v;
				_velocities[indexA].w = num;
				_velocities[indexB].v = v2;
				_velocities[indexB].w = num2;
			}
		}

		public void SolveVelocityConstraints()
		{
			for (int i = 0; i < _count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = _velocityConstraints[i];
				int indexA = contactVelocityConstraint.indexA;
				int indexB = contactVelocityConstraint.indexB;
				float invMassA = contactVelocityConstraint.invMassA;
				float invIA = contactVelocityConstraint.invIA;
				float invMassB = contactVelocityConstraint.invMassB;
				float invIB = contactVelocityConstraint.invIB;
				int pointCount = contactVelocityConstraint.pointCount;
				FVector2 v = _velocities[indexA].v;
				float num = _velocities[indexA].w;
				FVector2 v2 = _velocities[indexB].v;
				float num2 = _velocities[indexB].w;
				FVector2 normal = contactVelocityConstraint.normal;
				FVector2 fVector = MathUtils.Cross(normal, 1f);
				float friction = contactVelocityConstraint.friction;
				for (int j = 0; j < pointCount; j++)
				{
					VelocityConstraintPoint velocityConstraintPoint = contactVelocityConstraint.points[j];
					FVector2 value = v2 + MathUtils.Cross(num2, velocityConstraintPoint.rB) - v - MathUtils.Cross(num, velocityConstraintPoint.rA);
					float num3 = FVector2.Dot(value, fVector) - contactVelocityConstraint.tangentSpeed;
					float num4 = velocityConstraintPoint.tangentMass * (0f - num3);
					float num5 = friction * velocityConstraintPoint.normalImpulse;
					float num6 = MathUtils.Clamp(velocityConstraintPoint.tangentImpulse + num4, 0f - num5, num5);
					num4 = num6 - velocityConstraintPoint.tangentImpulse;
					velocityConstraintPoint.tangentImpulse = num6;
					FVector2 fVector2 = num4 * fVector;
					v -= invMassA * fVector2;
					num -= invIA * MathUtils.Cross(velocityConstraintPoint.rA, fVector2);
					v2 += invMassB * fVector2;
					num2 += invIB * MathUtils.Cross(velocityConstraintPoint.rB, fVector2);
				}
				if (contactVelocityConstraint.pointCount == 1)
				{
					VelocityConstraintPoint velocityConstraintPoint2 = contactVelocityConstraint.points[0];
					FVector2 value2 = v2 + MathUtils.Cross(num2, velocityConstraintPoint2.rB) - v - MathUtils.Cross(num, velocityConstraintPoint2.rA);
					float num7 = FVector2.Dot(value2, normal);
					float num8 = (0f - velocityConstraintPoint2.normalMass) * (num7 - velocityConstraintPoint2.velocityBias);
					float num9 = Math.Max(velocityConstraintPoint2.normalImpulse + num8, 0f);
					num8 = num9 - velocityConstraintPoint2.normalImpulse;
					velocityConstraintPoint2.normalImpulse = num9;
					FVector2 fVector3 = num8 * normal;
					v -= invMassA * fVector3;
					num -= invIA * MathUtils.Cross(velocityConstraintPoint2.rA, fVector3);
					v2 += invMassB * fVector3;
					num2 += invIB * MathUtils.Cross(velocityConstraintPoint2.rB, fVector3);
				}
				else
				{
					VelocityConstraintPoint velocityConstraintPoint3 = contactVelocityConstraint.points[0];
					VelocityConstraintPoint velocityConstraintPoint4 = contactVelocityConstraint.points[1];
					FVector2 fVector4 = new FVector2(velocityConstraintPoint3.normalImpulse, velocityConstraintPoint4.normalImpulse);
					FVector2 value3 = v2 + MathUtils.Cross(num2, velocityConstraintPoint3.rB) - v - MathUtils.Cross(num, velocityConstraintPoint3.rA);
					FVector2 value4 = v2 + MathUtils.Cross(num2, velocityConstraintPoint4.rB) - v - MathUtils.Cross(num, velocityConstraintPoint4.rA);
					float num10 = FVector2.Dot(value3, normal);
					float num11 = FVector2.Dot(value4, normal);
					FVector2 v3 = new FVector2
					{
						X = num10 - velocityConstraintPoint3.velocityBias,
						Y = num11 - velocityConstraintPoint4.velocityBias
					};
					v3 -= MathUtils.Mul(ref contactVelocityConstraint.K, fVector4);
					FVector2 fVector5 = -MathUtils.Mul(ref contactVelocityConstraint.normalMass, v3);
					if (fVector5.X >= 0f && fVector5.Y >= 0f)
					{
						FVector2 fVector6 = fVector5 - fVector4;
						FVector2 fVector7 = fVector6.X * normal;
						FVector2 fVector8 = fVector6.Y * normal;
						v -= invMassA * (fVector7 + fVector8);
						num -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, fVector7) + MathUtils.Cross(velocityConstraintPoint4.rA, fVector8));
						v2 += invMassB * (fVector7 + fVector8);
						num2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, fVector7) + MathUtils.Cross(velocityConstraintPoint4.rB, fVector8));
						velocityConstraintPoint3.normalImpulse = fVector5.X;
						velocityConstraintPoint4.normalImpulse = fVector5.Y;
					}
					else
					{
						fVector5.X = (0f - velocityConstraintPoint3.normalMass) * v3.X;
						fVector5.Y = 0f;
						num10 = 0f;
						num11 = contactVelocityConstraint.K.ex.Y * fVector5.X + v3.Y;
						if (fVector5.X >= 0f && num11 >= 0f)
						{
							FVector2 fVector9 = fVector5 - fVector4;
							FVector2 fVector10 = fVector9.X * normal;
							FVector2 fVector11 = fVector9.Y * normal;
							v -= invMassA * (fVector10 + fVector11);
							num -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, fVector10) + MathUtils.Cross(velocityConstraintPoint4.rA, fVector11));
							v2 += invMassB * (fVector10 + fVector11);
							num2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, fVector10) + MathUtils.Cross(velocityConstraintPoint4.rB, fVector11));
							velocityConstraintPoint3.normalImpulse = fVector5.X;
							velocityConstraintPoint4.normalImpulse = fVector5.Y;
						}
						else
						{
							fVector5.X = 0f;
							fVector5.Y = (0f - velocityConstraintPoint4.normalMass) * v3.Y;
							num10 = contactVelocityConstraint.K.ey.X * fVector5.Y + v3.X;
							num11 = 0f;
							if (fVector5.Y >= 0f && num10 >= 0f)
							{
								FVector2 fVector12 = fVector5 - fVector4;
								FVector2 fVector13 = fVector12.X * normal;
								FVector2 fVector14 = fVector12.Y * normal;
								v -= invMassA * (fVector13 + fVector14);
								num -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, fVector13) + MathUtils.Cross(velocityConstraintPoint4.rA, fVector14));
								v2 += invMassB * (fVector13 + fVector14);
								num2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, fVector13) + MathUtils.Cross(velocityConstraintPoint4.rB, fVector14));
								velocityConstraintPoint3.normalImpulse = fVector5.X;
								velocityConstraintPoint4.normalImpulse = fVector5.Y;
							}
							else
							{
								fVector5.X = 0f;
								fVector5.Y = 0f;
								num10 = v3.X;
								num11 = v3.Y;
								if (num10 >= 0f && num11 >= 0f)
								{
									FVector2 fVector15 = fVector5 - fVector4;
									FVector2 fVector16 = fVector15.X * normal;
									FVector2 fVector17 = fVector15.Y * normal;
									v -= invMassA * (fVector16 + fVector17);
									num -= invIA * (MathUtils.Cross(velocityConstraintPoint3.rA, fVector16) + MathUtils.Cross(velocityConstraintPoint4.rA, fVector17));
									v2 += invMassB * (fVector16 + fVector17);
									num2 += invIB * (MathUtils.Cross(velocityConstraintPoint3.rB, fVector16) + MathUtils.Cross(velocityConstraintPoint4.rB, fVector17));
									velocityConstraintPoint3.normalImpulse = fVector5.X;
									velocityConstraintPoint4.normalImpulse = fVector5.Y;
								}
							}
						}
					}
				}
				_velocities[indexA].v = v;
				_velocities[indexA].w = num;
				_velocities[indexB].v = v2;
				_velocities[indexB].w = num2;
			}
		}

		public void StoreImpulses()
		{
			for (int i = 0; i < _count; i++)
			{
				ContactVelocityConstraint contactVelocityConstraint = _velocityConstraints[i];
				Manifold manifold = _contacts[contactVelocityConstraint.contactIndex].Manifold;
				for (int j = 0; j < contactVelocityConstraint.pointCount; j++)
				{
					ManifoldPoint value = manifold.Points[j];
					value.NormalImpulse = contactVelocityConstraint.points[j].normalImpulse;
					value.TangentImpulse = contactVelocityConstraint.points[j].tangentImpulse;
					manifold.Points[j] = value;
				}
				_contacts[contactVelocityConstraint.contactIndex].Manifold = manifold;
			}
		}

		public bool SolvePositionConstraints()
		{
			float num = 0f;
			for (int i = 0; i < _count; i++)
			{
				ContactPositionConstraint contactPositionConstraint = _positionConstraints[i];
				int indexA = contactPositionConstraint.indexA;
				int indexB = contactPositionConstraint.indexB;
				FVector2 localCenterA = contactPositionConstraint.localCenterA;
				float invMassA = contactPositionConstraint.invMassA;
				float invIA = contactPositionConstraint.invIA;
				FVector2 localCenterB = contactPositionConstraint.localCenterB;
				float invMassB = contactPositionConstraint.invMassB;
				float invIB = contactPositionConstraint.invIB;
				int pointCount = contactPositionConstraint.pointCount;
				FVector2 c = _positions[indexA].c;
				float num2 = _positions[indexA].a;
				FVector2 c2 = _positions[indexB].c;
				float num3 = _positions[indexB].a;
				for (int j = 0; j < pointCount; j++)
				{
					Transform xfA = default(Transform);
					Transform xfB = default(Transform);
					xfA.q.Set(num2);
					xfB.q.Set(num3);
					xfA.p = c - MathUtils.Mul(xfA.q, localCenterA);
					xfB.p = c2 - MathUtils.Mul(xfB.q, localCenterB);
					FVector2 normal;
					FVector2 point;
					float separation;
					PositionSolverManifold.Initialize(contactPositionConstraint, xfA, xfB, j, out normal, out point, out separation);
					FVector2 a = point - c;
					FVector2 a2 = point - c2;
					num = Math.Min(num, separation);
					float num4 = MathUtils.Clamp(0.2f * (separation + 0.005f), -0.2f, 0f);
					float num5 = MathUtils.Cross(a, normal);
					float num6 = MathUtils.Cross(a2, normal);
					float num7 = invMassA + invMassB + invIA * num5 * num5 + invIB * num6 * num6;
					float num8 = ((!(num7 > 0f)) ? 0f : ((0f - num4) / num7));
					FVector2 fVector = num8 * normal;
					c -= invMassA * fVector;
					num2 -= invIA * MathUtils.Cross(a, fVector);
					c2 += invMassB * fVector;
					num3 += invIB * MathUtils.Cross(a2, fVector);
				}
				_positions[indexA].c = c;
				_positions[indexA].a = num2;
				_positions[indexB].c = c2;
				_positions[indexB].a = num3;
			}
			return num >= -0.015f;
		}

		public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
		{
			float num = 0f;
			for (int i = 0; i < _count; i++)
			{
				ContactPositionConstraint contactPositionConstraint = _positionConstraints[i];
				int indexA = contactPositionConstraint.indexA;
				int indexB = contactPositionConstraint.indexB;
				FVector2 localCenterA = contactPositionConstraint.localCenterA;
				FVector2 localCenterB = contactPositionConstraint.localCenterB;
				int pointCount = contactPositionConstraint.pointCount;
				float num2 = 0f;
				float num3 = 0f;
				if (indexA == toiIndexA || indexA == toiIndexB)
				{
					num2 = contactPositionConstraint.invMassA;
					num3 = contactPositionConstraint.invIA;
				}
				float invMassB = contactPositionConstraint.invMassB;
				float invIB = contactPositionConstraint.invIB;
				if (indexB == toiIndexA || indexB == toiIndexB)
				{
					invMassB = contactPositionConstraint.invMassB;
					invIB = contactPositionConstraint.invIB;
				}
				FVector2 c = _positions[indexA].c;
				float num4 = _positions[indexA].a;
				FVector2 c2 = _positions[indexB].c;
				float num5 = _positions[indexB].a;
				for (int j = 0; j < pointCount; j++)
				{
					Transform xfA = default(Transform);
					Transform xfB = default(Transform);
					xfA.q.Set(num4);
					xfB.q.Set(num5);
					xfA.p = c - MathUtils.Mul(xfA.q, localCenterA);
					xfB.p = c2 - MathUtils.Mul(xfB.q, localCenterB);
					FVector2 normal;
					FVector2 point;
					float separation;
					PositionSolverManifold.Initialize(contactPositionConstraint, xfA, xfB, j, out normal, out point, out separation);
					FVector2 a = point - c;
					FVector2 a2 = point - c2;
					num = Math.Min(num, separation);
					float num6 = MathUtils.Clamp(0.2f * (separation + 0.005f), -0.2f, 0f);
					float num7 = MathUtils.Cross(a, normal);
					float num8 = MathUtils.Cross(a2, normal);
					float num9 = num2 + invMassB + num3 * num7 * num7 + invIB * num8 * num8;
					float num10 = ((!(num9 > 0f)) ? 0f : ((0f - num6) / num9));
					FVector2 fVector = num10 * normal;
					c -= num2 * fVector;
					num4 -= num3 * MathUtils.Cross(a, fVector);
					c2 += invMassB * fVector;
					num5 += invIB * MathUtils.Cross(a2, fVector);
				}
				_positions[indexA].c = c;
				_positions[indexA].a = num4;
				_positions[indexB].c = c2;
				_positions[indexB].a = num5;
			}
			return num >= -0.0075f;
		}
	}
}
