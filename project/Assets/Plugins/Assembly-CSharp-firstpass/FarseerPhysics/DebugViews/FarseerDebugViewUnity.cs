using System;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace FarseerPhysics.DebugViews
{
	public class FarseerDebugViewUnity : DebugView, IDisposable
	{
		private struct ContactPoint
		{
			public FVector2 Normal;

			public FVector2 Position;

			public PointState State;
		}

		private struct StringData
		{
			public object[] Args;

			public Color Color;

			public string S;

			public int X;

			public int Y;

			public StringData(int x, int y, string s, object[] args, Color color)
			{
				X = x;
				Y = y;
				S = s;
				Args = args;
				Color = color;
			}
		}

		private const int MaxContactPoints = 2048;

		public const int CircleSegments = 32;

		private List<FDVertex> lineListBatch;

		private List<FDVertex> triangleListBatch;

		private FVector2[] _tempVertices = new FVector2[Settings.MaxPolygonVertices];

		private List<StringData> _stringData;

		private FMatrix _localProjection;

		private FMatrix _localView;

		public Color DefaultShapeColor = new Color(0.9f, 0.7f, 0.7f);

		public Color InactiveShapeColor = new Color(0.5f, 0.5f, 0.3f);

		public Color KinematicShapeColor = new Color(0.5f, 0.5f, 0.9f);

		public Color SleepingShapeColor = new Color(0.6f, 0.6f, 0.6f);

		public Color StaticShapeColor = new Color(0.5f, 0.9f, 0.5f);

		public Color TextColor = Color.white;

		private int _pointCount;

		private ContactPoint[] _points = new ContactPoint[2048];

		public FVector2 DebugPanelPosition = new FVector2(400f, 300f);

		private int _max;

		private int _avg;

		private int _min;

		public bool AdaptiveLimits = true;

		public int ValuesToGraph = 500;

		public int MinimumValue;

		public int MaximumValue = 1000;

		private List<float> _graphValues = new List<float>();

		public Rect PerformancePanelBounds = new Rect(35f, 13f, 8f, 4f);

		private FVector2[] _background = new FVector2[4];

		public bool Enabled = true;

		public FarseerDebugViewUnity(World world)
			: base(world)
		{
			lineListBatch = new List<FDVertex>();
			triangleListBatch = new List<FDVertex>();
			ContactManager contactManager = world.ContactManager;
			contactManager.PreSolve = (PreSolveDelegate)Delegate.Combine(contactManager.PreSolve, new PreSolveDelegate(PreSolve));
			AppendFlags(DebugViewFlags.Shape);
			AppendFlags(DebugViewFlags.Controllers);
			AppendFlags(DebugViewFlags.Joint);
		}

		public void BeginCustomDraw(ref FMatrix projection, ref FMatrix view)
		{
		}

		public void EndCustomDraw()
		{
		}

		public void Dispose()
		{
			ContactManager contactManager = base.World.ContactManager;
			contactManager.PreSolve = (PreSolveDelegate)Delegate.Remove(contactManager.PreSolve, new PreSolveDelegate(PreSolve));
		}

		private void PreSolve(Contact contact, ref Manifold oldManifold)
		{
			if ((base.Flags & DebugViewFlags.ContactPoints) != DebugViewFlags.ContactPoints)
			{
				return;
			}
			Manifold manifold = contact.Manifold;
			if (manifold.PointCount == 0)
			{
				return;
			}
			Fixture fixtureA = contact.FixtureA;
			FixedArray2<PointState> state;
			FixedArray2<PointState> state2;
			FarseerPhysics.Collision.Collision.GetPointStates(out state, out state2, ref oldManifold, ref manifold);
			FVector2 normal;
			FixedArray2<FVector2> points;
			contact.GetWorldManifold(out normal, out points);
			for (int i = 0; i < manifold.PointCount; i++)
			{
				if (_pointCount >= 2048)
				{
					break;
				}
				if (fixtureA == null)
				{
					_points[i] = default(ContactPoint);
				}
				ContactPoint contactPoint = _points[_pointCount];
				contactPoint.Position = points[i];
				contactPoint.Normal = normal;
				contactPoint.State = state2[i];
				_points[_pointCount] = contactPoint;
				_pointCount++;
			}
		}

		private void DrawDebugData()
		{
			if ((base.Flags & DebugViewFlags.Shape) == DebugViewFlags.Shape)
			{
				foreach (Body body in base.World.BodyList)
				{
					FarseerPhysics.Common.Transform transform;
					body.GetTransform(out transform);
					foreach (Fixture fixture in body.FixtureList)
					{
						if (!body.Enabled)
						{
							DrawShape(fixture, transform, InactiveShapeColor);
						}
						else if (body.BodyType == BodyType.Static)
						{
							DrawShape(fixture, transform, StaticShapeColor);
						}
						else if (body.BodyType == BodyType.Kinematic)
						{
							DrawShape(fixture, transform, KinematicShapeColor);
						}
						else if (!body.Awake)
						{
							DrawShape(fixture, transform, SleepingShapeColor);
						}
						else
						{
							DrawShape(fixture, transform, DefaultShapeColor);
						}
					}
				}
			}
			if ((base.Flags & DebugViewFlags.ContactPoints) == DebugViewFlags.ContactPoints)
			{
				for (int i = 0; i < _pointCount; i++)
				{
					ContactPoint contactPoint = _points[i];
					if (contactPoint.State == PointState.Add)
					{
						DrawPoint(contactPoint.Position, 0.1f, new Color(0.3f, 0.95f, 0.3f));
					}
					else if (contactPoint.State == PointState.Persist)
					{
						DrawPoint(contactPoint.Position, 0.1f, new Color(0.3f, 0.3f, 0.95f));
					}
					if ((base.Flags & DebugViewFlags.ContactNormals) == DebugViewFlags.ContactNormals)
					{
						FVector2 position = contactPoint.Position;
						FVector2 end = position + 0.3f * contactPoint.Normal;
						DrawSegment(position, end, new Color(0.4f, 0.9f, 0.4f));
					}
				}
				_pointCount = 0;
			}
			if ((base.Flags & DebugViewFlags.PolygonPoints) == DebugViewFlags.PolygonPoints)
			{
				foreach (Body body2 in base.World.BodyList)
				{
					foreach (Fixture fixture2 in body2.FixtureList)
					{
						PolygonShape polygonShape = fixture2.Shape as PolygonShape;
						if (polygonShape != null)
						{
							FarseerPhysics.Common.Transform transform2;
							body2.GetTransform(out transform2);
							for (int j = 0; j < polygonShape.Vertices.Count; j++)
							{
								FVector2 p = MathUtils.Mul(ref transform2, polygonShape.Vertices[j]);
								DrawPoint(p, 0.1f, Color.red);
							}
						}
					}
				}
			}
			if ((base.Flags & DebugViewFlags.Joint) == DebugViewFlags.Joint)
			{
				foreach (FarseerJoint joint in base.World.JointList)
				{
					DrawJoint(joint);
				}
			}
			if ((base.Flags & DebugViewFlags.Pair) == DebugViewFlags.Pair)
			{
				Color color = new Color(0.3f, 0.9f, 0.9f);
				for (int k = 0; k < base.World.ContactManager.ContactList.Count; k++)
				{
					Contact contact = base.World.ContactManager.ContactList[k];
					Fixture fixtureA = contact.FixtureA;
					Fixture fixtureB = contact.FixtureB;
					AABB aabb;
					fixtureA.GetAABB(out aabb, 0);
					AABB aabb2;
					fixtureB.GetAABB(out aabb2, 0);
					FVector2 center = aabb.Center;
					FVector2 center2 = aabb2.Center;
					DrawSegment(center, center2, color);
				}
			}
			if ((base.Flags & DebugViewFlags.AABB) == DebugViewFlags.AABB)
			{
				Color color2 = new Color(0.9f, 0.3f, 0.9f);
				IBroadPhase broadPhase = base.World.ContactManager.BroadPhase;
				foreach (Body body3 in base.World.BodyList)
				{
					if (!body3.Enabled)
					{
						continue;
					}
					foreach (Fixture fixture3 in body3.FixtureList)
					{
						for (int l = 0; l < fixture3.ProxyCount; l++)
						{
							FixtureProxy fixtureProxy = fixture3.Proxies[l];
							AABB aabb3;
							broadPhase.GetFatAABB(fixtureProxy.ProxyId, out aabb3);
							DrawAABB(ref aabb3, color2);
						}
					}
				}
			}
			if ((base.Flags & DebugViewFlags.CenterOfMass) == DebugViewFlags.CenterOfMass)
			{
				foreach (Body body4 in base.World.BodyList)
				{
					FarseerPhysics.Common.Transform transform3;
					body4.GetTransform(out transform3);
					transform3.p = body4.WorldCenter;
					DrawTransform(ref transform3);
				}
			}
			if ((base.Flags & DebugViewFlags.Controllers) != DebugViewFlags.Controllers)
			{
				return;
			}
			for (int m = 0; m < base.World.ControllerList.Count; m++)
			{
				Controller controller = base.World.ControllerList[m];
				BuoyancyController buoyancyController = controller as BuoyancyController;
				if (buoyancyController != null)
				{
					AABB aabb4 = buoyancyController.Container;
					DrawAABB(ref aabb4, new Color(0.3f, 0.5f, 1f));
				}
			}
		}

		public void OnGUI(Camera camera)
		{
			if ((base.Flags & DebugViewFlags.DebugPanel) == DebugViewFlags.DebugPanel)
			{
				DrawDebugPanel(camera);
			}
		}

		private void DrawPerformanceGraph()
		{
			_graphValues.Add(base.World.UpdateTime);
			if (_graphValues.Count > ValuesToGraph + 1)
			{
				_graphValues.RemoveAt(0);
			}
			float num = PerformancePanelBounds.x;
			float num2 = PerformancePanelBounds.width / (float)ValuesToGraph;
			float num3 = PerformancePanelBounds.yMax - PerformancePanelBounds.yMin;
			if (_graphValues.Count > 2)
			{
				_max = (int)_graphValues.Max();
				_avg = (int)_graphValues.Average();
				_min = (int)_graphValues.Min();
				if (AdaptiveLimits)
				{
					MaximumValue = _max;
					MinimumValue = 0;
				}
				for (int num4 = _graphValues.Count - 1; num4 > 0; num4--)
				{
					float value = PerformancePanelBounds.yMax - _graphValues[num4] / (float)(MaximumValue - MinimumValue) * num3;
					float value2 = PerformancePanelBounds.yMax - _graphValues[num4 - 1] / (float)(MaximumValue - MinimumValue) * num3;
					FVector2 start = new FVector2(MathHelper.Clamp(num, PerformancePanelBounds.xMin, PerformancePanelBounds.xMax), MathHelper.Clamp(value, PerformancePanelBounds.yMin, PerformancePanelBounds.yMax));
					FVector2 end = new FVector2(MathHelper.Clamp(num + num2, PerformancePanelBounds.xMin, PerformancePanelBounds.xMax), MathHelper.Clamp(value2, PerformancePanelBounds.yMin, PerformancePanelBounds.yMax));
					DrawSegment(start, end, new Color(0.5f, 1f, 0.3f));
					num += num2;
				}
			}
			DrawString((int)PerformancePanelBounds.xMax + 10, (int)PerformancePanelBounds.yMin, "Max: " + _max);
			DrawString((int)PerformancePanelBounds.xMax + 10, (int)PerformancePanelBounds.yMin + (int)PerformancePanelBounds.height / 2 - 7, "Avg: " + _avg);
			DrawString((int)PerformancePanelBounds.xMax + 10, (int)PerformancePanelBounds.yMax - 15, "Min: " + _min);
			_background[0] = new FVector2(PerformancePanelBounds.x, PerformancePanelBounds.y);
			_background[1] = new FVector2(PerformancePanelBounds.x, PerformancePanelBounds.y + PerformancePanelBounds.height);
			_background[2] = new FVector2(PerformancePanelBounds.x + PerformancePanelBounds.width, PerformancePanelBounds.y + PerformancePanelBounds.height);
			_background[3] = new FVector2(PerformancePanelBounds.x + PerformancePanelBounds.width, PerformancePanelBounds.y);
			DrawSolidPolygon(_background, 4, new Color(0.3f, 0.3f, 0.3f), true);
		}

		private void DrawDebugPanel(Camera camera)
		{
			int num = 0;
			for (int i = 0; i < base.World.BodyList.Count; i++)
			{
				num += base.World.BodyList[i].FixtureList.Count;
			}
			int num2 = 0;
			int y = 0;
			Vector3 vector = camera.ScreenToWorldPoint(new Vector3(camera.pixelWidth - 210f, camera.pixelHeight - 120f, 0f));
			GUI.BeginGroup(new Rect(vector.x, vector.y, 210f, 120f), GUI.skin.box);
			DrawString(num2, y, "Objects:\n- Bodies: " + base.World.BodyList.Count + "\n- Fixtures: " + num + "\n- Contacts: " + base.World.ContactList.Count + "\n- Joints: " + base.World.JointList.Count + "\n- Controllers: " + base.World.ControllerList.Count + "\n- Proxies: " + base.World.ProxyCount);
			DrawString(num2 + 110, y, "Update time:\n- Body: " + base.World.SolveUpdateTime + "\n- Contact: " + base.World.ContactsUpdateTime + "\n- CCD: " + base.World.ContinuousPhysicsTime + "\n- Joint: " + base.World.Island.JointUpdateTime + "\n- Controller: " + base.World.ControllersUpdateTime + "\n- Total: " + base.World.UpdateTime);
			GUI.EndGroup();
		}

		public void DrawAABB(ref AABB aabb, Color color)
		{
			DrawPolygon(new FVector2[4]
			{
				new FVector2(aabb.LowerBound.X, aabb.LowerBound.Y),
				new FVector2(aabb.UpperBound.X, aabb.LowerBound.Y),
				new FVector2(aabb.UpperBound.X, aabb.UpperBound.Y),
				new FVector2(aabb.LowerBound.X, aabb.UpperBound.Y)
			}, 4, color);
		}

		private void DrawJoint(FarseerJoint joint)
		{
			if (joint.Enabled)
			{
				Body bodyA = joint.BodyA;
				Body bodyB = joint.BodyB;
				FarseerPhysics.Common.Transform transform;
				bodyA.GetTransform(out transform);
				FVector2 fVector = FVector2.Zero;
				if (!joint.IsFixedType())
				{
					FarseerPhysics.Common.Transform transform2;
					bodyB.GetTransform(out transform2);
					fVector = transform2.p;
				}
				FVector2 worldAnchorA = joint.WorldAnchorA;
				FVector2 worldAnchorB = joint.WorldAnchorB;
				FVector2 p = transform.p;
				Color color = new Color(0.5f, 0.8f, 0.8f);
				switch (joint.JointType)
				{
				case JointType.Distance:
					DrawSegment(worldAnchorA, worldAnchorB, color);
					break;
				case JointType.Pulley:
				{
					PulleyJoint pulleyJoint = (PulleyJoint)joint;
					FVector2 groundAnchorA = pulleyJoint.GroundAnchorA;
					FVector2 groundAnchorB = pulleyJoint.GroundAnchorB;
					DrawSegment(groundAnchorA, worldAnchorA, color);
					DrawSegment(groundAnchorB, worldAnchorB, color);
					DrawSegment(groundAnchorA, groundAnchorB, color);
					break;
				}
				case JointType.FixedMouse:
					DrawPoint(worldAnchorA, 0.5f, new Color(0f, 1f, 0f));
					DrawSegment(worldAnchorA, worldAnchorB, new Color(0.8f, 0.8f, 0.8f));
					break;
				case JointType.Revolute:
					DrawSegment(worldAnchorB, worldAnchorA, color);
					DrawSolidCircle(worldAnchorB, 0.1f, FVector2.Zero, Color.red);
					DrawSolidCircle(worldAnchorA, 0.1f, FVector2.Zero, Color.blue);
					break;
				case JointType.FixedAngle:
					break;
				case JointType.FixedRevolute:
					DrawSegment(p, worldAnchorA, color);
					DrawSolidCircle(worldAnchorA, 0.1f, FVector2.Zero, new Color(1f, 0.5f, 1f));
					break;
				case JointType.FixedLine:
					DrawSegment(p, worldAnchorA, color);
					DrawSegment(worldAnchorA, worldAnchorB, color);
					break;
				case JointType.FixedDistance:
					DrawSegment(p, worldAnchorA, color);
					DrawSegment(worldAnchorA, worldAnchorB, color);
					break;
				case JointType.FixedPrismatic:
					DrawSegment(p, worldAnchorA, color);
					DrawSegment(worldAnchorA, worldAnchorB, color);
					break;
				case JointType.Gear:
					DrawSegment(p, fVector, color);
					break;
				default:
					DrawSegment(p, worldAnchorA, color);
					DrawSegment(worldAnchorA, worldAnchorB, color);
					DrawSegment(fVector, worldAnchorB, color);
					break;
				}
			}
		}

		public void DrawShape(Fixture fixture, FarseerPhysics.Common.Transform xf, Color color)
		{
			switch (fixture.ShapeType)
			{
			case ShapeType.Circle:
			{
				CircleShape circleShape = (CircleShape)fixture.Shape;
				FVector2 center = MathUtils.Mul(ref xf, circleShape.Position);
				float radius = circleShape.Radius;
				FVector2 axis = MathUtils.Mul(xf.q, new FVector2(1f, 0f));
				DrawSolidCircle(center, radius, axis, color);
				break;
			}
			case ShapeType.Polygon:
			{
				PolygonShape polygonShape = (PolygonShape)fixture.Shape;
				int count2 = polygonShape.Vertices.Count;
				for (int j = 0; j < count2; j++)
				{
					_tempVertices[j] = MathUtils.Mul(ref xf, polygonShape.Vertices[j]);
				}
				DrawSolidPolygon(_tempVertices, count2, color);
				break;
			}
			case ShapeType.Edge:
			{
				EdgeShape edgeShape = (EdgeShape)fixture.Shape;
				FVector2 start = MathUtils.Mul(ref xf, edgeShape.Vertex1);
				FVector2 end = MathUtils.Mul(ref xf, edgeShape.Vertex2);
				DrawSegment(start, end, color);
				break;
			}
			case ShapeType.Chain:
			{
				ChainShape chainShape = (ChainShape)fixture.Shape;
				int count = chainShape.Vertices.Count;
				FVector2 fVector = MathUtils.Mul(ref xf, chainShape.Vertices[count - 1]);
				DrawCircle(fVector, 0.05f, color);
				for (int i = 0; i < count; i++)
				{
					FVector2 fVector2 = MathUtils.Mul(ref xf, chainShape.Vertices[i]);
					DrawSegment(fVector, fVector2, color);
					fVector = fVector2;
				}
				break;
			}
			}
		}

		public override void DrawPolygon(FVector2[] vertices, int count, float red, float green, float blue)
		{
			DrawPolygon(vertices, count, new Color(red, green, blue));
		}

		public void DrawPolygon(FVector2[] vertices, int count, Color color)
		{
			for (int i = 0; i < count - 1; i++)
			{
				lineListBatch.Add(FDVertex.FromLine(vertices[i], vertices[i + 1], color));
			}
			lineListBatch.Add(FDVertex.FromLine(vertices[count - 1], vertices[0], color));
		}

		public override void DrawSolidPolygon(FVector2[] vertices, int count, float red, float green, float blue)
		{
			DrawSolidPolygon(vertices, count, new Color(red, green, blue), true);
		}

		public void DrawSolidPolygon(FVector2[] vertices, int count, Color color)
		{
			DrawSolidPolygon(vertices, count, color, true);
		}

		public void DrawSolidPolygon(FVector2[] vertices, int count, Color color, bool outline)
		{
			if (count == 2)
			{
				DrawPolygon(vertices, count, color);
				return;
			}
			Color color2 = color * ((!outline) ? 1f : 0.5f);
			for (int i = 1; i < count - 1; i++)
			{
				triangleListBatch.Add(FDVertex.FromTriangleList(vertices[0], vertices[i], vertices[i + 1], color2));
			}
			if (outline)
			{
				DrawPolygon(vertices, count, color);
			}
		}

		public override void DrawCircle(FVector2 center, float radius, float red, float green, float blue)
		{
			DrawCircle(center, radius, new Color(red, green, blue));
		}

		public void DrawCircle(FVector2 center, float radius, Color color)
		{
			double num = 0.0;
			for (int i = 0; i < 32; i++)
			{
				FVector2 begin = center + radius * new FVector2((float)Math.Cos(num), (float)Math.Sin(num));
				FVector2 end = center + radius * new FVector2((float)Math.Cos(num + Math.PI / 16.0), (float)Math.Sin(num + Math.PI / 16.0));
				lineListBatch.Add(FDVertex.FromLine(begin, end, color));
				num += Math.PI / 16.0;
			}
		}

		public override void DrawSolidCircle(FVector2 center, float radius, FVector2 axis, float red, float green, float blue)
		{
			DrawSolidCircle(center, radius, axis, new Color(red, green, blue));
		}

		public void DrawSolidCircle(FVector2 center, float radius, FVector2 axis, Color color)
		{
			double num = 0.0;
			Color color2 = color * 0.5f;
			FVector2 p = center + radius * new FVector2((float)Math.Cos(num), (float)Math.Sin(num));
			num += Math.PI / 16.0;
			for (int i = 1; i < 31; i++)
			{
				FVector2 p2 = center + radius * new FVector2((float)Math.Cos(num), (float)Math.Sin(num));
				FVector2 p3 = center + radius * new FVector2((float)Math.Cos(num + Math.PI / 16.0), (float)Math.Sin(num + Math.PI / 16.0));
				triangleListBatch.Add(FDVertex.FromTriangleList(p, p2, p3, color2));
				num += Math.PI / 16.0;
			}
			DrawCircle(center, radius, color);
			DrawSegment(center, center + axis * radius, color);
		}

		public override void DrawSegment(FVector2 start, FVector2 end, float red, float green, float blue)
		{
			DrawSegment(start, end, new Color(red, green, blue));
		}

		public void DrawSegment(FVector2 start, FVector2 end, Color color)
		{
			lineListBatch.Add(FDVertex.FromLine(start, end, color));
		}

		public override void DrawTransform(ref FarseerPhysics.Common.Transform transform)
		{
			FVector2 p = transform.p;
			FVector2 end = p + 0.4f * transform.q.GetXAxis();
			DrawSegment(p, end, Color.red);
			end = p + 0.4f * transform.q.GetYAxis();
			DrawSegment(p, end, Color.green);
		}

		public void DrawPoint(FVector2 p, float size, Color color)
		{
			FVector2[] array = new FVector2[4];
			float num = size / 2f;
			array[0] = p + new FVector2(0f - num, 0f - num);
			array[1] = p + new FVector2(num, 0f - num);
			array[2] = p + new FVector2(num, num);
			array[3] = p + new FVector2(0f - num, num);
			DrawSolidPolygon(array, 4, color, true);
		}

		public void DrawString(int x, int y, string s, params object[] args)
		{
			GUI.Label(new Rect(x, y, (float)s.Length * 15f, 120f), s);
		}

		public void DrawArrow(FVector2 start, FVector2 end, float length, float width, bool drawStartIndicator, Color color)
		{
			DrawSegment(start, end, color);
			float num = width / 2f;
			FVector2 fVector = start - end;
			fVector.Normalize();
			float radians = (float)Math.Atan2(fVector.X, 0f - fVector.Y);
			FMatrix matrix = FMatrix.CreateRotationZ(radians);
			FMatrix matrix2 = FMatrix.CreateTranslation(end.X, end.Y, 0f);
			FVector2[] array = new FVector2[3]
			{
				new FVector2(0f, 0f),
				new FVector2(0f - num, 0f - length),
				new FVector2(num, 0f - length)
			};
			FVector2.Transform(array, ref matrix, array);
			FVector2.Transform(array, ref matrix2, array);
			DrawSolidPolygon(array, 3, color, false);
			if (drawStartIndicator)
			{
				FMatrix matrix3 = FMatrix.CreateTranslation(start.X, start.Y, 0f);
				FVector2[] array2 = new FVector2[4]
				{
					new FVector2(0f - num, length / 4f),
					new FVector2(num, length / 4f),
					new FVector2(num, 0f),
					new FVector2(0f - num, 0f)
				};
				FVector2.Transform(array2, ref matrix, array2);
				FVector2.Transform(array2, ref matrix3, array2);
				DrawSolidPolygon(array2, 4, color, false);
			}
		}

		public void RenderDebugData(ref Matrix4x4 projection, ref Matrix4x4 view, Material mat)
		{
			if (Enabled && base.Flags != 0)
			{
				lineListBatch.Clear();
				triangleListBatch.Clear();
				DrawDebugData();
				if ((base.Flags & DebugViewFlags.PerformanceGraph) == DebugViewFlags.PerformanceGraph)
				{
					DrawPerformanceGraph();
				}
				GL.PushMatrix();
				mat.SetPass(0);
				GL.Begin(1);
				int count = lineListBatch.Count;
				for (int i = 0; i < count; i++)
				{
					GL.Color(lineListBatch[i].Color);
					GL.Vertex(lineListBatch[i].Position[0]);
					GL.Vertex(lineListBatch[i].Position[1]);
				}
				GL.End();
				GL.Begin(4);
				count = triangleListBatch.Count;
				for (int j = 0; j < count; j++)
				{
					GL.Color(triangleListBatch[j].Color);
					GL.Vertex(triangleListBatch[j].Position[0]);
					GL.Vertex(triangleListBatch[j].Position[1]);
					GL.Vertex(triangleListBatch[j].Position[2]);
				}
				GL.End();
				GL.PopMatrix();
			}
		}

		public void RenderDebugData(ref Matrix4x4 projection, Material mat)
		{
			if (Enabled)
			{
				Matrix4x4 view = Matrix4x4.identity;
				RenderDebugData(ref projection, ref view, mat);
			}
		}
	}
}
