using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;
using UnityEngine;

namespace CatsintheSky.FarseerDebug
{
	public class CCDTest : Test
	{
		public CCDTest(Transform parent)
			: base(parent)
		{
			title = "Continuous Collision Detection";
		}

		public override void Start()
		{
			base.Start();
			Body body = BodyFactory.CreateBody(FSWorldComponent.PhysicsWorld, new FVector2(5f, -3.3333333f));
			body.BodyType = BodyType.Dynamic;
			body.IsBullet = true;
			Fixture fixture = FixtureFactory.AttachRectangle(3f, 0.3f, 4f, FVector2.Zero, body);
			fixture.Restitution = 1.4f;
			fixture = FixtureFactory.AttachRectangle(0.3f, 3f, 4f, new FVector2(-1.45f, 1.6833333f), body);
			fixture.Restitution = 1.4f;
			fixture = FixtureFactory.AttachRectangle(0.3f, 3f, 4f, new FVector2(1.45f, 1.6833333f), body);
			fixture.Restitution = 1.4f;
			for (int i = 0; i < 5; i++)
			{
				body = BodyFactory.CreateBody(FSWorldComponent.PhysicsWorld, new FVector2((Random.value * 300f + 250f) / 30f, (Random.value * -320f - 20f) / 30f));
				Fixture fixture2 = FixtureFactory.AttachCircle((Random.value * 10f + 5f) / 30f, 1f, body);
				fixture2.Friction = 0.3f;
				fixture2.Restitution = 1.1f;
				body.BodyType = BodyType.Dynamic;
				body.IsBullet = true;
			}
		}
	}
}
