using System.Collections.Generic;

namespace Poly2Tri.Triangulation.Sets
{
	public class ConstrainedPointSet : PointSet
	{
		private List<TriangulationPoint> _constrainedPointList;

		public int[] EdgeIndex { get; private set; }

		public override TriangulationMode TriangulationMode
		{
			get
			{
				return TriangulationMode.Constrained;
			}
		}

		public ConstrainedPointSet(List<TriangulationPoint> points, int[] index)
			: base(points)
		{
			EdgeIndex = index;
		}

		public ConstrainedPointSet(List<TriangulationPoint> points, IEnumerable<TriangulationPoint> constraints)
			: base(points)
		{
			_constrainedPointList = new List<TriangulationPoint>();
			_constrainedPointList.AddRange(constraints);
		}

		public override void PrepareTriangulation(TriangulationContext tcx)
		{
			base.PrepareTriangulation(tcx);
			if (_constrainedPointList != null)
			{
				List<TriangulationPoint>.Enumerator enumerator = _constrainedPointList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					TriangulationPoint current = enumerator.Current;
					enumerator.MoveNext();
					TriangulationPoint current2 = enumerator.Current;
					tcx.NewConstraint(current, current2);
				}
			}
			else
			{
				for (int i = 0; i < EdgeIndex.Length; i += 2)
				{
					tcx.NewConstraint(Points[EdgeIndex[i]], Points[EdgeIndex[i + 1]]);
				}
			}
		}

		public bool isValid()
		{
			return true;
		}
	}
}
