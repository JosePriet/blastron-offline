using System;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace CatsintheSky.FarseerDebug
{
	public class RagdollTest : Test
	{
		public RagdollTest(Transform parent)
			: base(parent)
		{
			title = "Ragdolls";
		}

		public override void Start()
		{
			base.Start();
			PolygonShape polygonShape;
			Fixture fixture;
			for (int i = 0; i < 2; i++)
			{
				float num = 70f + UnityEngine.Random.value * 20f + 480f * (float)i;
				float num2 = (20f + UnityEngine.Random.value * 50f) * -1f;
				Body body = new Body(FSWorldComponent.PhysicsWorld);
				CircleShape shape = new CircleShape(5f / 12f, 1f);
				fixture = new Fixture(body, shape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.3f;
				body.Position = new FVector2(num / 30f, num2 / 30f);
				body.ApplyLinearImpulse(new FVector2(UnityEngine.Random.value * 100f - 50f, UnityEngine.Random.value * 100f - 50f), body.Position);
				body.BodyType = BodyType.Dynamic;
				Body body2 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.5f, 1f / 3f);
				fixture = new Fixture(body2, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body2.Position = new FVector2(num / 30f, (num2 - 28f) / 30f);
				body2.BodyType = BodyType.Dynamic;
				Body body3 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.5f, 1f / 3f);
				fixture = new Fixture(body3, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body3.Position = new FVector2(num / 30f, (num2 - 43f) / 30f);
				body3.BodyType = BodyType.Dynamic;
				Body body4 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.5f, 1f / 3f);
				fixture = new Fixture(body4, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body4.Position = new FVector2(num / 30f, (num2 - 58f) / 30f);
				body4.BodyType = BodyType.Dynamic;
				Body body5 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.6f, 13f / 60f);
				fixture = new Fixture(body5, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body5.Position = new FVector2((num - 30f) / 30f, (num2 - 20f) / 30f);
				body5.BodyType = BodyType.Dynamic;
				Body body6 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.6f, 13f / 60f);
				fixture = new Fixture(body6, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body6.Position = new FVector2((num + 30f) / 30f, (num2 - 20f) / 30f);
				body6.BodyType = BodyType.Dynamic;
				Body body7 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(17f / 30f, 0.2f);
				fixture = new Fixture(body7, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body7.Position = new FVector2((num - 57f) / 30f, (num2 - 20f) / 30f);
				body7.BodyType = BodyType.Dynamic;
				Body body8 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(17f / 30f, 0.2f);
				fixture = new Fixture(body8, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body8.Position = new FVector2((num + 57f) / 30f, (num2 - 20f) / 30f);
				body8.BodyType = BodyType.Dynamic;
				Body body9 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.25f, 11f / 15f);
				fixture = new Fixture(body9, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body9.Position = new FVector2((num - 8f) / 30f, (num2 - 85f) / 30f);
				body9.BodyType = BodyType.Dynamic;
				Body body10 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.25f, 11f / 15f);
				fixture = new Fixture(body10, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body10.Position = new FVector2((num + 8f) / 30f, (num2 - 85f) / 30f);
				body10.BodyType = BodyType.Dynamic;
				Body body11 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.25f, 11f / 15f);
				fixture = new Fixture(body11, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body11.Position = new FVector2((num - 8f) / 30f, (num2 - 120f) / 30f);
				body11.BodyType = BodyType.Dynamic;
				Body body12 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(0.25f, 11f / 15f);
				fixture = new Fixture(body12, polygonShape);
				fixture.Friction = 0.4f;
				fixture.Restitution = 0.1f;
				body12.Position = new FVector2((num + 8f) / 30f, (num2 - 120f) / 30f);
				body12.BodyType = BodyType.Dynamic;
				RevoluteJoint revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body2, body, body.GetLocalPoint(new FVector2(num / 30f, (num2 - 15f) / 30f)));
				revoluteJoint.LowerLimit = -0.6981317f;
				revoluteJoint.UpperLimit = 0.6981317f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body2, body5, body5.GetLocalPoint(new FVector2((num - 18f) / 30f, (num2 - 20f) / 30f)));
				revoluteJoint.LowerLimit = -1.4835298f;
				revoluteJoint.UpperLimit = (float)Math.PI * 13f / 18f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body2, body6, body6.GetLocalPoint(new FVector2((num + 18f) / 30f, (num2 - 20f) / 30f)));
				revoluteJoint.LowerLimit = (float)Math.PI * -13f / 18f;
				revoluteJoint.UpperLimit = 1.4835298f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body5, body7, body7.GetLocalPoint(new FVector2((num - 45f) / 30f, (num2 - 20f) / 30f)));
				revoluteJoint.LowerLimit = (float)Math.PI * -13f / 18f;
				revoluteJoint.UpperLimit = 0.17453292f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body6, body8, body8.GetLocalPoint(new FVector2((num + 45f) / 30f, (num2 - 20f) / 30f)));
				revoluteJoint.LowerLimit = -0.17453292f;
				revoluteJoint.UpperLimit = (float)Math.PI * 13f / 18f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body2, body3, body3.GetLocalPoint(new FVector2(num / 30f, (num2 - 35f) / 30f)));
				revoluteJoint.LowerLimit = -(float)Math.PI / 12f;
				revoluteJoint.UpperLimit = (float)Math.PI / 12f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body3, body4, body4.GetLocalPoint(new FVector2(num / 30f, (num2 - 50f) / 30f)));
				revoluteJoint.LowerLimit = -(float)Math.PI / 12f;
				revoluteJoint.UpperLimit = (float)Math.PI / 12f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body4, body9, body9.GetLocalPoint(new FVector2((num - 8f) / 30f, (num2 - 72f) / 30f)));
				revoluteJoint.LowerLimit = -0.43633232f;
				revoluteJoint.UpperLimit = (float)Math.PI / 4f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body4, body10, body10.GetLocalPoint(new FVector2((num + 8f) / 30f, (num2 - 72f) / 30f)));
				revoluteJoint.LowerLimit = -(float)Math.PI / 4f;
				revoluteJoint.UpperLimit = 0.43633232f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body9, body11, body11.GetLocalPoint(new FVector2((num - 8f) / 30f, (num2 - 105f) / 30f)));
				revoluteJoint.LowerLimit = -0.43633232f;
				revoluteJoint.UpperLimit = (float)Math.PI * 23f / 36f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body10, body12, body12.GetLocalPoint(new FVector2((num + 8f) / 30f, (num2 - 105f) / 30f)));
				revoluteJoint.LowerLimit = (float)Math.PI * -23f / 36f;
				revoluteJoint.UpperLimit = 0.43633232f;
				revoluteJoint.LimitEnabled = true;
				revoluteJoint.CollideConnected = false;
			}
			for (int j = 1; j <= 10; j++)
			{
				Body body13 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(10f * (float)j / 30f, 1f / 3f);
				fixture = new Fixture(body13, polygonShape);
				body13.Position = new FVector2(10f * (float)j / 30f, (150f + 20f * (float)j) / 30f * -1f);
			}
			for (int k = 1; k <= 10; k++)
			{
				Body body14 = new Body(FSWorldComponent.PhysicsWorld);
				polygonShape = new PolygonShape(1f);
				polygonShape.SetAsBox(10f * (float)k / 30f, 1f / 3f);
				fixture = new Fixture(body14, polygonShape);
				body14.Position = new FVector2((640f - 10f * (float)k) / 30f, (150f + 20f * (float)k) / 30f * -1f);
			}
			Body body15 = new Body(FSWorldComponent.PhysicsWorld);
			polygonShape = new PolygonShape(1f);
			polygonShape.SetAsBox(1f, 1.3333334f);
			fixture = new Fixture(body15, polygonShape);
			body15.Position = new FVector2(10.666667f, -10.666667f);
		}
	}
}
