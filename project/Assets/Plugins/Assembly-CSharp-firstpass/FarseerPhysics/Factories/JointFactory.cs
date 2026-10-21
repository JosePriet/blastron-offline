using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Factories
{
	public static class JointFactory
	{
		public static RevoluteJoint CreateRevoluteJoint(Body bodyA, Body bodyB, FVector2 localAnchorB)
		{
			FVector2 localPoint = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localAnchorB));
			return new RevoluteJoint(bodyA, bodyB, localPoint, localAnchorB);
		}

		public static RevoluteJoint CreateRevoluteJoint(World world, Body bodyA, Body bodyB, FVector2 anchor)
		{
			RevoluteJoint revoluteJoint = CreateRevoluteJoint(bodyA, bodyB, anchor);
			world.AddJoint(revoluteJoint);
			return revoluteJoint;
		}

		public static WeldJoint CreateWeldJoint(Body bodyA, Body bodyB, FVector2 worldAnchor)
		{
			return new WeldJoint(bodyA, bodyB, bodyA.GetLocalPoint(worldAnchor), bodyB.GetLocalPoint(worldAnchor));
		}

		public static WeldJoint CreateWeldJoint(World world, Body bodyA, Body bodyB, FVector2 worldAnchor)
		{
			WeldJoint weldJoint = CreateWeldJoint(bodyA, bodyB, worldAnchor);
			world.AddJoint(weldJoint);
			return weldJoint;
		}

		public static WeldJoint CreateWeldJoint(World world, Body bodyA, Body bodyB, FVector2 localAnchorA, FVector2 localAnchorB)
		{
			WeldJoint weldJoint = new WeldJoint(bodyA, bodyB, localAnchorA, localAnchorB);
			world.AddJoint(weldJoint);
			return weldJoint;
		}

		public static PrismaticJoint CreatePrismaticJoint(Body bodyA, Body bodyB, FVector2 localanchorB, FVector2 axis)
		{
			FVector2 localPoint = bodyA.GetLocalPoint(bodyB.GetWorldPoint(localanchorB));
			return new PrismaticJoint(bodyA, bodyB, localPoint, localanchorB, axis);
		}

		public static PrismaticJoint CreatePrismaticJoint(World world, Body bodyA, Body bodyB, FVector2 localanchorB, FVector2 axis)
		{
			PrismaticJoint prismaticJoint = CreatePrismaticJoint(bodyA, bodyB, localanchorB, axis);
			world.AddJoint(prismaticJoint);
			return prismaticJoint;
		}

		public static WheelJoint CreateWheelJoint(Body bodyA, Body bodyB, FVector2 anchor, FVector2 axis)
		{
			return new WheelJoint(bodyA, bodyB, anchor, axis);
		}

		public static WheelJoint CreateWheelJoint(World world, Body bodyA, Body bodyB, FVector2 localanchorB, FVector2 axis)
		{
			WheelJoint wheelJoint = CreateWheelJoint(bodyA, bodyB, localanchorB, axis);
			world.AddJoint(wheelJoint);
			return wheelJoint;
		}

		public static AngleJoint CreateAngleJoint(World world, Body bodyA, Body bodyB)
		{
			AngleJoint angleJoint = new AngleJoint(bodyA, bodyB);
			world.AddJoint(angleJoint);
			return angleJoint;
		}

		public static FixedAngleJoint CreateFixedAngleJoint(World world, Body body)
		{
			FixedAngleJoint fixedAngleJoint = new FixedAngleJoint(body);
			world.AddJoint(fixedAngleJoint);
			return fixedAngleJoint;
		}

		public static DistanceJoint CreateDistanceJoint(World world, Body bodyA, Body bodyB, FVector2 anchorA, FVector2 anchorB)
		{
			DistanceJoint distanceJoint = new DistanceJoint(bodyA, bodyB, anchorA, anchorB);
			world.AddJoint(distanceJoint);
			return distanceJoint;
		}

		public static FrictionJoint CreateFrictionJoint(World world, Body bodyA, Body bodyB, FVector2 anchorA, FVector2 anchorB)
		{
			FrictionJoint frictionJoint = new FrictionJoint(bodyA, bodyB, anchorA, anchorB);
			world.AddJoint(frictionJoint);
			return frictionJoint;
		}

		public static GearJoint CreateGearJoint(World world, FarseerJoint jointA, FarseerJoint jointB, float ratio)
		{
			GearJoint gearJoint = new GearJoint(jointA, jointB, ratio);
			world.AddJoint(gearJoint);
			return gearJoint;
		}

		public static PulleyJoint CreatePulleyJoint(World world, Body bodyA, Body bodyB, FVector2 groundAnchorA, FVector2 groundAnchorB, FVector2 anchorA, FVector2 anchorB, float ratio)
		{
			PulleyJoint pulleyJoint = new PulleyJoint(bodyA, bodyB, groundAnchorA, groundAnchorB, anchorA, anchorB, ratio);
			world.AddJoint(pulleyJoint);
			return pulleyJoint;
		}

		public static SliderJoint CreateSliderJoint(World world, Body bodyA, Body bodyB, FVector2 anchorA, FVector2 anchorB, float minLength, float maxLength)
		{
			SliderJoint sliderJoint = new SliderJoint(bodyA, bodyB, anchorA, anchorB, minLength, maxLength);
			world.AddJoint(sliderJoint);
			return sliderJoint;
		}

		public static FixedMouseJoint CreateFixedMouseJoint(World world, Body body, FVector2 target)
		{
			FixedMouseJoint fixedMouseJoint = new FixedMouseJoint(body, target);
			world.AddJoint(fixedMouseJoint);
			return fixedMouseJoint;
		}
	}
}
