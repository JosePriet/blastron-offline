using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers
{
	public sealed class BuoyancyController : Controller
	{
		public float AngularDragCoefficient;

		public float Density;

		public float LinearDragCoefficient;

		public FVector2 Velocity;

		private AABB _container;

		private FVector2 _gravity;

		private FVector2 _normal;

		private float _offset;

		private Dictionary<int, Body> _uniqueBodies = new Dictionary<int, Body>();

		public AABB Container
		{
			get
			{
				return _container;
			}
			set
			{
				_container = value;
				_offset = _container.UpperBound.Y;
			}
		}

		public BuoyancyController(AABB container, float density, float linearDragCoefficient, float rotationalDragCoefficient, FVector2 gravity)
			: base(ControllerType.BuoyancyController)
		{
			Container = container;
			_normal = new FVector2(0f, 1f);
			Density = density;
			LinearDragCoefficient = linearDragCoefficient;
			AngularDragCoefficient = rotationalDragCoefficient;
			_gravity = gravity;
		}

		public override void Update(float dt)
		{
			_uniqueBodies.Clear();
			World.QueryAABB((Fixture fixture2) =>
			{
				if (fixture2.Body.IsStatic || !fixture2.Body.Awake)
				{
					return true;
				}
				if (!_uniqueBodies.ContainsKey(fixture2.Body.BodyId))
				{
					_uniqueBodies.Add(fixture2.Body.BodyId, fixture2.Body);
				}
				return true;
			}, ref _container);
			foreach (KeyValuePair<int, Body> uniqueBody in _uniqueBodies)
			{
				Body value = uniqueBody.Value;
				FVector2 zero = FVector2.Zero;
				FVector2 zero2 = FVector2.Zero;
				float num = 0f;
				float num2 = 0f;
				for (int num3 = 0; num3 < value.FixtureList.Count; num3++)
				{
					Fixture fixture = value.FixtureList[num3];
					if (fixture.Shape.ShapeType == ShapeType.Polygon || fixture.Shape.ShapeType == ShapeType.Circle)
					{
						Shape shape = fixture.Shape;
						FVector2 sc;
						float num4 = shape.ComputeSubmergedArea(_normal, _offset, value.Xf, out sc);
						num += num4;
						zero.X += num4 * sc.X;
						zero.Y += num4 * sc.Y;
						num2 += num4 * shape.Density;
						zero2.X += num4 * sc.X * shape.Density;
						zero2.Y += num4 * sc.Y * shape.Density;
					}
				}
				zero.X /= num;
				zero.Y /= num;
				zero2.X /= num2;
				zero2.Y /= num2;
				if (!(num < 1.1920929E-07f))
				{
					FVector2 force = (0f - Density) * num * _gravity;
					value.ApplyForce(force, zero2);
					FVector2 force2 = value.GetLinearVelocityFromWorldPoint(zero) - Velocity;
					force2 *= (0f - LinearDragCoefficient) * num;
					value.ApplyForce(force2, zero);
					value.ApplyTorque((0f - value.Inertia) / value.Mass * num * value.AngularVelocity * AngularDragCoefficient);
				}
			}
		}
	}
}
