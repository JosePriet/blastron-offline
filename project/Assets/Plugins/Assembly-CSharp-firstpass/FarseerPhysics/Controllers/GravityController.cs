using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers
{
	public class GravityController : Controller
	{
		public List<Body> Bodies = new List<Body>();

		public List<FVector2> Points = new List<FVector2>();

		public float MinRadius { get; set; }

		public float MaxRadius { get; set; }

		public float Strength { get; set; }

		public GravityType GravityType { get; set; }

		public GravityController(float strength)
			: base(ControllerType.GravityController)
		{
			Strength = strength;
			MaxRadius = float.MaxValue;
		}

		public GravityController(float strength, float maxRadius, float minRadius)
			: base(ControllerType.GravityController)
		{
			MinRadius = minRadius;
			MaxRadius = maxRadius;
			Strength = strength;
		}

		public override void Update(float dt)
		{
			FVector2 force = FVector2.Zero;
			foreach (Body body in World.BodyList)
			{
				if (!IsActiveOn(body))
				{
					continue;
				}
				foreach (Body body2 in Bodies)
				{
					if (body == body2 || (body.IsStatic && body2.IsStatic) || !body2.Enabled)
					{
						continue;
					}
					FVector2 fVector = body2.WorldCenter - body.WorldCenter;
					float num = fVector.LengthSquared();
					if (num < 1.1920929E-07f)
					{
						continue;
					}
					float num2 = fVector.Length();
					if (!(num2 >= MaxRadius) && !(num2 <= MinRadius))
					{
						switch (GravityType)
						{
						case GravityType.DistanceSquared:
							force = Strength / num / (float)Math.Sqrt(num) * body.Mass * body2.Mass * fVector;
							break;
						case GravityType.Linear:
							force = Strength / num * body.Mass * body2.Mass * fVector;
							break;
						}
						body.ApplyForce(ref force);
						FVector2.Negate(ref force, out force);
						body2.ApplyForce(ref force);
					}
				}
				foreach (FVector2 point in Points)
				{
					FVector2 fVector2 = point - body.Position;
					float num3 = fVector2.LengthSquared();
					if (num3 < 1.1920929E-07f)
					{
						continue;
					}
					float num4 = fVector2.Length();
					if (!(num4 >= MaxRadius) && !(num4 <= MinRadius))
					{
						switch (GravityType)
						{
						case GravityType.DistanceSquared:
							force = Strength / num3 / (float)Math.Sqrt(num3) * body.Mass * fVector2;
							break;
						case GravityType.Linear:
							force = Strength / num3 * body.Mass * fVector2;
							break;
						}
						body.ApplyForce(ref force);
					}
				}
			}
		}

		public void AddBody(Body body)
		{
			Bodies.Add(body);
		}

		public void AddPoint(FVector2 point)
		{
			Points.Add(point);
		}
	}
}
