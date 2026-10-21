using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Controllers
{
	public class SimpleWindForce : AbstractForceController
	{
		public FVector2 Direction { get; set; }

		public float Divergence { get; set; }

		public bool IgnorePosition { get; set; }

		public override void ApplyForce(float dt, float strength)
		{
			foreach (Body body in World.BodyList)
			{
				float decayMultiplier = GetDecayMultiplier(body);
				if (decayMultiplier == 0f)
				{
					continue;
				}
				FVector2 fVector;
				if (ForceType == ForceTypes.Point)
				{
					fVector = body.Position - base.Position;
				}
				else
				{
					Direction.Normalize();
					fVector = Direction;
					if (fVector.Length() == 0f)
					{
						fVector = new FVector2(0f, 1f);
					}
				}
				if (base.Variation != 0f)
				{
					float num = (float)Randomize.NextDouble() * MathHelper.Clamp(base.Variation, 0f, 1f);
					fVector.Normalize();
					body.ApplyForce(fVector * strength * decayMultiplier * num);
				}
				else
				{
					fVector.Normalize();
					body.ApplyForce(fVector * strength * decayMultiplier);
				}
			}
		}
	}
}
