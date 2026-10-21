using System;
using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace CatsintheSky.FarseerDebug
{
	public class BridgeTest : Test
	{
		public BridgeTest(UnityEngine.Transform parent)
			: base(parent)
		{
			title = "Bridge";
		}

		public override void Start()
		{
			base.Start();
			int num = 10;
			Body bodyA = BodyFactory.CreateCircle(FSWorldComponent.PhysicsWorld, 0.1f, 20f, new FVector2(3.3333333f, -8.333333f));
			RevoluteJoint revoluteJoint;
			Body body;
			for (int i = 0; i < num; i++)
			{
				body = BodyFactory.CreateRectangle(FSWorldComponent.PhysicsWorld, 1.6f, 1f / 3f, 20f, new FVector2((122f + 44f * (float)i) / 30f, -8.333333f));
				body.BodyType = BodyType.Dynamic;
				revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, bodyA, body, new FVector2(-11f / 15f, 0f));
				revoluteJoint.LowerLimit = -(float)Math.PI / 12f;
				revoluteJoint.UpperLimit = (float)Math.PI / 12f;
				revoluteJoint.LimitEnabled = true;
				bodyA = body;
			}
			body = BodyFactory.CreateCircle(FSWorldComponent.PhysicsWorld, 0.1f, 20f, new FVector2((100f + 44f * (float)num) / 30f, -8.333333f));
			revoluteJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, bodyA, body, new FVector2(0f, 0f));
			revoluteJoint.LowerLimit = -(float)Math.PI / 12f;
			revoluteJoint.UpperLimit = (float)Math.PI / 12f;
			revoluteJoint.LimitEnabled = true;
			for (int j = 0; j < 5; j++)
			{
				body = BodyFactory.CreateRectangle(FSWorldComponent.PhysicsWorld, (UnityEngine.Random.value * 10f + 20f) / 30f, (UnityEngine.Random.value * 10f + 20f) / 30f, 1f, new FVector2((UnityEngine.Random.value * 400f + 120f) / 30f, (UnityEngine.Random.value * -150f - 50f) / 30f));
				body.Rotation = UnityEngine.Random.value * (float)Math.PI;
				body.BodyType = BodyType.Dynamic;
			}
			for (int k = 0; k < 5; k++)
			{
				body = BodyFactory.CreateCircle(FSWorldComponent.PhysicsWorld, (UnityEngine.Random.value * 20f + 10f) / 30f, 1f, new FVector2((UnityEngine.Random.value * 400f + 120f) / 30f, (UnityEngine.Random.value * -150f - 50f) / 30f));
				body.Rotation = UnityEngine.Random.value * (float)Math.PI;
				body.BodyType = BodyType.Dynamic;
			}
			for (int l = 0; l < 15; l++)
			{
				Vertices vertices = new Vertices();
				if (UnityEngine.Random.value > 0.66f)
				{
					vertices.Add(new FVector2((10f + UnityEngine.Random.value * 10f) / 30f, (-10f - UnityEngine.Random.value * 10f) / 30f));
					vertices.Add(new FVector2((5f + UnityEngine.Random.value * 10f) / 30f, (10f + UnityEngine.Random.value * 10f) / 30f));
					vertices.Add(new FVector2((-5f - UnityEngine.Random.value * 10f) / 30f, (10f + UnityEngine.Random.value * 10f) / 30f));
					vertices.Add(new FVector2((-10f - UnityEngine.Random.value * 10f) / 30f, (-10f - UnityEngine.Random.value * 10f) / 30f));
				}
				else if (UnityEngine.Random.value > 0.5f)
				{
					FVector2 item = new FVector2(0f, (-10f - UnityEngine.Random.value * 10f) / 30f);
					FVector2 item2 = new FVector2((-5f - UnityEngine.Random.value * 10f) / 30f, (10f + UnityEngine.Random.value * 10f) / 30f);
					FVector2 item3 = new FVector2((5f + UnityEngine.Random.value * 10f) / 30f, (10f + UnityEngine.Random.value * 10f) / 30f);
					FVector2 item4 = new FVector2(item.X + item2.X, item.Y + item2.Y);
					item4 *= UnityEngine.Random.value / 2f + 0.8f;
					FVector2 item5 = new FVector2(item3.X + item.X, item3.Y + item.Y);
					item5 *= UnityEngine.Random.value / 2f + 0.8f;
					vertices.Add(item5);
					vertices.Add(item3);
					vertices.Add(item2);
					vertices.Add(item4);
					vertices.Add(item);
				}
				else
				{
					vertices.Add(new FVector2((5f + UnityEngine.Random.value * 10f) / 30f, (10f + UnityEngine.Random.value * 10f) / 30f));
					vertices.Add(new FVector2((-5f - UnityEngine.Random.value * 10f) / 30f, (10f + UnityEngine.Random.value * 10f) / 30f));
					vertices.Add(new FVector2(0f, (-10f - UnityEngine.Random.value * 10f) / 30f));
				}
				body = BodyFactory.CreateCompoundPolygon(FSWorldComponent.PhysicsWorld, new List<Vertices> { vertices }, 1f, new FVector2((UnityEngine.Random.value * 400f + 120f) / 30f, (UnityEngine.Random.value * -150f - 50f) / 30f));
				body.Rotation = UnityEngine.Random.value * (float)Math.PI;
				body.BodyType = BodyType.Dynamic;
			}
		}
	}
}
