using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace FarseerPhysics.Common
{
	public class DetectedVertices : Vertices
	{
		private List<Vertices> _holes;

		public List<Vertices> Holes
		{
			get
			{
				return _holes;
			}
			set
			{
				_holes = value;
			}
		}

		public DetectedVertices()
		{
		}

		public DetectedVertices(Vertices vertices)
			: base(vertices)
		{
		}

		public void Transform(FMatrix transform)
		{
			for (int i = 0; i < Count; i++)
			{
				this[i] = FVector2.Transform(this[i], transform);
			}
			FVector2[] array = null;
			if (_holes != null && _holes.Count > 0)
			{
				for (int j = 0; j < _holes.Count; j++)
				{
					array = _holes[j].ToArray();
					FVector2.Transform(array, ref transform, array);
					_holes[j] = new Vertices(array);
				}
			}
		}
	}
}
