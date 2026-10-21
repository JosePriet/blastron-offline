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
	public class TheoJansenTest : Test
	{
		private float tScale;

		private FVector2 m_offset = new FVector2(0f, 0f);

		private Body m_chassis;

		private Body m_wheel;

		private RevoluteJoint m_motorJoint;

		private bool m_motorOn = true;

		private float m_motorSpeed;

		public TheoJansenTest(UnityEngine.Transform parent)
			: base(parent)
		{
			title = "Theo Jansen Walker";
		}

		public override void Start()
		{
			base.Start();
			tScale = 60f;
			m_offset = new FVector2(6f, -6.6666665f);
			m_motorSpeed = 2f;
			m_motorOn = true;
			FVector2 fVector = new FVector2(0f, 24f / tScale);
			for (int i = 0; i < 50; i++)
			{
				Body body = BodyFactory.CreateCircle(FSWorldComponent.PhysicsWorld, 0.125f, 1f, new FVector2((UnityEngine.Random.value * 620f + 10f) / 30f, -11.333333f));
				body.BodyType = BodyType.Dynamic;
			}
			m_chassis = BodyFactory.CreateBody(FSWorldComponent.PhysicsWorld, FVector2.Add(fVector, m_offset));
			m_chassis.BodyType = BodyType.Dynamic;
			Fixture fixture = FixtureFactory.AttachRectangle(150f / tScale, 60f / tScale, 1f, FVector2.Zero, m_chassis);
			fixture.CollisionCategories = Category.Cat10;
			fixture.CollidesWith = Category.Cat1;
			m_wheel = BodyFactory.CreateBody(FSWorldComponent.PhysicsWorld, FVector2.Add(fVector, m_offset));
			m_wheel.BodyType = BodyType.Dynamic;
			Fixture fixture2 = FixtureFactory.AttachCircle(48f / tScale, 1f, m_wheel);
			fixture2.CollisionCategories = Category.Cat10;
			fixture2.CollidesWith = Category.Cat1;
			m_motorJoint = JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, m_wheel, m_chassis, FVector2.Zero);
			m_motorJoint.MotorSpeed = m_motorSpeed;
			m_motorJoint.MaxMotorTorque = 400f;
			m_motorJoint.CollideConnected = false;
			m_motorJoint.MotorEnabled = m_motorOn;
			FVector2 wheelAnchor = new FVector2(0f, -24f / tScale) + fVector;
			CreateLeg(-1f, wheelAnchor);
			CreateLeg(1f, wheelAnchor);
			m_wheel.Rotation = (float)Math.PI * 2f / 3f;
			CreateLeg(-1f, wheelAnchor);
			CreateLeg(1f, wheelAnchor);
			m_wheel.Rotation = (float)Math.PI * -2f / 3f;
			CreateLeg(-1f, wheelAnchor);
			CreateLeg(1f, wheelAnchor);
		}

		private void CreateLeg(float s, FVector2 wheelAnchor)
		{
			FVector2 item = new FVector2(162f * s / tScale, -183f / tScale);
			FVector2 fVector = new FVector2(216f * s / tScale, -36f / tScale);
			FVector2 fVector2 = new FVector2(129f * s / tScale, -57f / tScale);
			FVector2 fVector3 = new FVector2(93f * s / tScale, 24f / tScale);
			FVector2 value = new FVector2(180f * s / tScale, 45f / tScale);
			FVector2 value2 = new FVector2(75f * s / tScale, 111f / tScale);
			Body body = BodyFactory.CreateBody(FSWorldComponent.PhysicsWorld, m_offset);
			body.BodyType = BodyType.Dynamic;
			Body body2 = BodyFactory.CreateBody(FSWorldComponent.PhysicsWorld, FVector2.Add(fVector3, m_offset));
			body2.BodyType = BodyType.Dynamic;
			List<Vertices> list = new List<Vertices>();
			List<Vertices> list2 = new List<Vertices>();
			list.Add(new Vertices());
			list2.Add(new Vertices());
			if (s > 0f)
			{
				list[0].Add(item);
				list[0].Add(fVector);
				list[0].Add(fVector2);
				list2[0].Add(FVector2.Zero);
				list2[0].Add(FVector2.Subtract(value, fVector3));
				list2[0].Add(FVector2.Subtract(value2, fVector3));
			}
			else
			{
				list[0].Add(item);
				list[0].Add(fVector2);
				list[0].Add(fVector);
				list2[0].Add(FVector2.Zero);
				list2[0].Add(FVector2.Subtract(value2, fVector3));
				list2[0].Add(FVector2.Subtract(value, fVector3));
			}
			Fixture fixture = FixtureFactory.AttachCompoundPolygon(list, 1f, body)[0];
			Fixture fixture2 = FixtureFactory.AttachCompoundPolygon(list2, 1f, body2)[0];
			fixture.CollisionCategories = Category.Cat10;
			fixture.CollidesWith = Category.Cat1;
			fixture2.CollisionCategories = Category.Cat10;
			fixture2.CollidesWith = Category.Cat1;
			body.AngularDamping = 10f;
			body2.AngularDamping = 10f;
			float dampingRatio = 0.5f;
			float frequency = 10f;
			DistanceJoint distanceJoint = JointFactory.CreateDistanceJoint(FSWorldComponent.PhysicsWorld, body, body2, body.GetLocalPoint(FVector2.Add(fVector, m_offset)), body2.GetLocalPoint(FVector2.Add(value, m_offset)));
			distanceJoint.DampingRatio = dampingRatio;
			distanceJoint.Frequency = frequency;
			distanceJoint = JointFactory.CreateDistanceJoint(FSWorldComponent.PhysicsWorld, body, body2, body.GetLocalPoint(FVector2.Add(fVector2, m_offset)), body2.GetLocalPoint(FVector2.Add(fVector3, m_offset)));
			distanceJoint.DampingRatio = dampingRatio;
			distanceJoint.Frequency = frequency;
			distanceJoint = JointFactory.CreateDistanceJoint(FSWorldComponent.PhysicsWorld, body, m_wheel, body.GetLocalPoint(FVector2.Add(fVector2, m_offset)), m_wheel.GetLocalPoint(FVector2.Add(wheelAnchor, m_offset)));
			distanceJoint.DampingRatio = dampingRatio;
			distanceJoint.Frequency = frequency;
			distanceJoint = JointFactory.CreateDistanceJoint(FSWorldComponent.PhysicsWorld, body2, m_wheel, body2.GetLocalPoint(FVector2.Add(value2, m_offset)), m_wheel.GetLocalPoint(FVector2.Add(wheelAnchor, m_offset)));
			distanceJoint.DampingRatio = dampingRatio;
			distanceJoint.Frequency = frequency;
			JointFactory.CreateRevoluteJoint(FSWorldComponent.PhysicsWorld, body2, m_chassis, m_chassis.GetLocalPoint(FVector2.Add(fVector3, m_offset)));
		}

		public override void Update()
		{
			if (m_chassis.Position.X > 16.666666f && m_motorSpeed > 0f)
			{
				m_motorSpeed = -2f;
				m_motorJoint.MotorSpeed = m_motorSpeed;
			}
			if (m_chassis.Position.X < 4f && m_motorSpeed < 0f)
			{
				m_motorSpeed = 2f;
				m_motorJoint.MotorSpeed = m_motorSpeed;
			}
			base.Update();
		}
	}
}
