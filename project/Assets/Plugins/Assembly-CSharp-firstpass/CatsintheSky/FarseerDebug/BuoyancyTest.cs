using System;
using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace CatsintheSky.FarseerDebug
{
	public class BuoyancyTest : Test
	{
		private BuoyancyController controller;

		private List<Body> bodies;

		public BuoyancyTest(UnityEngine.Transform parent)
			: base(parent)
		{
			title = "Buoyancy";
		}

		public override void Start()
		{
			base.Start();
			AABB container = new AABB(new FVector2(0f, -12f), new FVector2(21.333334f, -6.6666665f));
			controller = new BuoyancyController(container, 2f, 5f, 2f, FSWorldComponent.PhysicsWorld.Gravity);
			FSWorldComponent.PhysicsWorld.AddController(controller);
			bodies = new List<Body>();
			Body body;
			for (int i = 0; i < 5; i++)
			{
				body = BodyFactory.CreateRectangle(FSWorldComponent.PhysicsWorld, (UnityEngine.Random.value * 5f + 10f) / 30f, (UnityEngine.Random.value * 5f + 10f) / 30f, 1f, new FVector2((UnityEngine.Random.value * 400f + 120f) / 30f, (UnityEngine.Random.value * -150f - 50f) / 30f));
				body.Rotation = UnityEngine.Random.value * (float)Math.PI;
				body.BodyType = BodyType.Dynamic;
				bodies.Add(body);
			}
			for (int j = 0; j < 5; j++)
			{
				body = BodyFactory.CreateCircle(FSWorldComponent.PhysicsWorld, (UnityEngine.Random.value * 5f + 10f) / 30f, 0.5f, new FVector2((UnityEngine.Random.value * 400f + 120f) / 30f, (UnityEngine.Random.value * -150f - 50f) / 30f));
				body.Rotation = UnityEngine.Random.value * (float)Math.PI;
				body.BodyType = BodyType.Dynamic;
				bodies.Add(body);
			}
			for (int k = 0; k < 15; k++)
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
				bodies.Add(body);
			}
			body = new Body(FSWorldComponent.PhysicsWorld);
			body.Position = new FVector2(1.6666666f, -10f);
			body.BodyType = BodyType.Dynamic;
			FixtureFactory.AttachRectangle(2.6666667f, 2f / 3f, 3f, FVector2.Zero, body);
			bodies.Add(body);
			body = new Body(FSWorldComponent.PhysicsWorld);
			body.Position = new FVector2(10f, -10f);
			body.BodyType = BodyType.Dynamic;
			FixtureFactory.AttachSolidArc(2f, (float)Math.PI * 2f, 8, 7f / 30f, new FVector2(1f, 0f), 0f, body);
			FixtureFactory.AttachSolidArc(2f, (float)Math.PI * 2f, 8, 7f / 30f, new FVector2(-1f, 0f), 0f, body);
			FixtureFactory.AttachRectangle(2f, 2f / 15f, 2f, FVector2.Zero, body);
			FixtureFactory.AttachSolidArc(2f, (float)Math.PI * 2f, 8, 7f / 30f, new FVector2(0f, 1f), 0f, body);
			FixtureFactory.AttachSolidArc(2f, (float)Math.PI * 2f, 8, 7f / 30f, new FVector2(0f, -1f), 0f, body);
			FixtureFactory.AttachRectangle(2f / 15f, 2f, 2f, FVector2.Zero, body);
		}
	}
}
