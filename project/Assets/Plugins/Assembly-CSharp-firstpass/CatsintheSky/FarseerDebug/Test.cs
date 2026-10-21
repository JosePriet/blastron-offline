using System;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace CatsintheSky.FarseerDebug
{
	public class Test
	{
		protected const float physScale = 30f;

		protected UnityEngine.Transform parent;

		protected FixedMouseJoint mouseJoint;

		protected string title = "Test";

		public static float MouseXWorldPhys;

		public static float MouseYWorldPhys;

		protected FVector2 mousePVec = default(FVector2);

		public Test(UnityEngine.Transform parent)
		{
			this.parent = parent;
		}

		public virtual void Start()
		{
			Body body = new Body(FSWorldComponent.PhysicsWorld);
			float num = 0f;
			float num2 = 640f;
			float num3 = 0f;
			float num4 = -360f;
			FVector2 start = new FVector2(num / 30f, num3 / 30f);
			FVector2 end = new FVector2(num2 / 30f, num3 / 30f);
			FixtureFactory.AttachEdge(start, end, body);
			start = new FVector2(num2 / 30f, num3 / 30f);
			end = new FVector2(num2 / 30f, num4 / 30f);
			FixtureFactory.AttachEdge(start, end, body);
			start = new FVector2(num / 30f, num4 / 30f);
			end = new FVector2(num2 / 30f, num4 / 30f);
			FixtureFactory.AttachEdge(start, end, body);
			start = new FVector2(num / 30f, num3 / 30f);
			end = new FVector2(num / 30f, num4 / 30f);
			FixtureFactory.AttachEdge(start, end, body);
		}

		public virtual void Update()
		{
			UpdateMouseWorld();
			MouseDrag();
		}

		public virtual void Stop()
		{
			FSWorldComponent.PhysicsWorld.Clear();
		}

		public virtual void UpdateMouseWorld()
		{
			Vector3 vector = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			MouseXWorldPhys = vector.x;
			MouseYWorldPhys = vector.y;
		}

		public virtual Body GetBodyAtMouse()
		{
			return GetBodyAtMouse(false);
		}

		public virtual Body GetBodyAtMouse(bool includeStatic)
		{
			mousePVec.X = MouseXWorldPhys;
			mousePVec.Y = MouseYWorldPhys;
			FVector2 min = new FVector2(MouseXWorldPhys - 0.001f, MouseYWorldPhys - 0.001f);
			FVector2 max = new FVector2(MouseXWorldPhys + 0.001f, MouseYWorldPhys + 0.001f);
			AABB aabb = new AABB(min, max);
			Body body = null;
			Func<Fixture, bool> callback = (Fixture fixture0) =>
			{
				Shape shape = fixture0.Shape;
				if (fixture0.Body.BodyType != BodyType.Static || includeStatic)
				{
					FarseerPhysics.Common.Transform transform;
					fixture0.Body.GetTransform(out transform);
					if (shape.TestPoint(ref transform, ref mousePVec))
					{
						body = fixture0.Body;
						return false;
					}
				}
				return true;
			};
			FSWorldComponent.PhysicsWorld.QueryAABB(callback, ref aabb);
			return body;
		}

		public virtual void MouseDrag()
		{
			if (Input.GetMouseButtonDown(0) && mouseJoint == null)
			{
				Body bodyAtMouse = GetBodyAtMouse();
				if (bodyAtMouse != null)
				{
					mouseJoint = JointFactory.CreateFixedMouseJoint(target: new FVector2(MouseXWorldPhys, MouseYWorldPhys), world: FSWorldComponent.PhysicsWorld, body: bodyAtMouse);
					mouseJoint.CollideConnected = true;
					mouseJoint.MaxForce = 300f * bodyAtMouse.Mass;
					bodyAtMouse.Awake = true;
				}
			}
			if (Input.GetMouseButtonUp(0) && mouseJoint != null)
			{
				FSWorldComponent.PhysicsWorld.RemoveJoint(mouseJoint);
				mouseJoint = null;
			}
			if (mouseJoint != null)
			{
				FVector2 worldAnchorB = new FVector2(MouseXWorldPhys, MouseYWorldPhys);
				mouseJoint.WorldAnchorB = worldAnchorB;
			}
		}

		public virtual void OnGUI()
		{
			GUI.BeginGroup(new Rect(400f, 10f, 240f, 120f), GUI.skin.box);
			GUI.Label(new Rect(10f, 5f, 220f, 20f), title);
			GUI.Label(new Rect(10f, 25f, 220f, 90f), "Left/Right Arrow: change test.\n\nClick & drag to interact.");
			GUI.EndGroup();
		}
	}
}
