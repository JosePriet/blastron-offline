using System;

namespace Microsoft.Xna.Framework
{
	public class Curve
	{
		private CurveKeyCollection keys;

		private CurveLoopType postLoop;

		private CurveLoopType preLoop;

		public bool IsConstant
		{
			get
			{
				return keys.Count <= 1;
			}
		}

		public CurveKeyCollection Keys
		{
			get
			{
				return keys;
			}
		}

		public CurveLoopType PostLoop
		{
			get
			{
				return postLoop;
			}
			set
			{
				postLoop = value;
			}
		}

		public CurveLoopType PreLoop
		{
			get
			{
				return preLoop;
			}
			set
			{
				preLoop = value;
			}
		}

		public Curve()
		{
			keys = new CurveKeyCollection();
		}

		public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
		{
			throw new NotImplementedException();
		}

		public void ComputeTangent(int keyIndex, CurveTangent tangentType)
		{
			ComputeTangent(keyIndex, tangentType, tangentType);
		}

		public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
		{
			throw new NotImplementedException();
		}

		public void ComputeTangents(CurveTangent tangentType)
		{
			ComputeTangents(tangentType, tangentType);
		}

		public Curve Clone()
		{
			Curve curve = new Curve();
			curve.keys = keys.Clone();
			curve.preLoop = preLoop;
			curve.postLoop = postLoop;
			return curve;
		}

		public float Evaluate(float position)
		{
			CurveKey curveKey = keys[0];
			CurveKey curveKey2 = keys[keys.Count - 1];
			if (position < curveKey.Position)
			{
				switch (PreLoop)
				{
				case CurveLoopType.Constant:
					return curveKey.Value;
				case CurveLoopType.Linear:
					return curveKey.Value - curveKey.TangentIn * (curveKey.Position - position);
				case CurveLoopType.Cycle:
				{
					int numberOfCycle = GetNumberOfCycle(position);
					float position2 = position - (float)numberOfCycle * (curveKey2.Position - curveKey.Position);
					return GetCurvePosition(position2);
				}
				case CurveLoopType.CycleOffset:
				{
					int numberOfCycle = GetNumberOfCycle(position);
					float position2 = position - (float)numberOfCycle * (curveKey2.Position - curveKey.Position);
					return GetCurvePosition(position2) + (float)numberOfCycle * (curveKey2.Value - curveKey.Value);
				}
				case CurveLoopType.Oscillate:
				{
					int numberOfCycle = GetNumberOfCycle(position);
					float position2 = (((float)numberOfCycle % 2f != 0f) ? (curveKey2.Position - position + curveKey.Position + (float)numberOfCycle * (curveKey2.Position - curveKey.Position)) : (position - (float)numberOfCycle * (curveKey2.Position - curveKey.Position)));
					return GetCurvePosition(position2);
				}
				}
			}
			else if (position > curveKey2.Position)
			{
				switch (PostLoop)
				{
				case CurveLoopType.Constant:
					return curveKey2.Value;
				case CurveLoopType.Linear:
					return curveKey2.Value + curveKey.TangentOut * (position - curveKey2.Position);
				case CurveLoopType.Cycle:
				{
					int numberOfCycle2 = GetNumberOfCycle(position);
					float num = position - (float)numberOfCycle2 * (curveKey2.Position - curveKey.Position);
					return GetCurvePosition(num);
				}
				case CurveLoopType.CycleOffset:
				{
					int numberOfCycle2 = GetNumberOfCycle(position);
					float num = position - (float)numberOfCycle2 * (curveKey2.Position - curveKey.Position);
					return GetCurvePosition(num) + (float)numberOfCycle2 * (curveKey2.Value - curveKey.Value);
				}
				case CurveLoopType.Oscillate:
				{
					int numberOfCycle2 = GetNumberOfCycle(position);
					float num = position - (float)numberOfCycle2 * (curveKey2.Position - curveKey.Position);
					num = (((float)numberOfCycle2 % 2f != 0f) ? (curveKey2.Position - position + curveKey.Position + (float)numberOfCycle2 * (curveKey2.Position - curveKey.Position)) : (position - (float)numberOfCycle2 * (curveKey2.Position - curveKey.Position)));
					return GetCurvePosition(num);
				}
				}
			}
			return GetCurvePosition(position);
		}

		private int GetNumberOfCycle(float position)
		{
			float num = (position - keys[0].Position) / (keys[keys.Count - 1].Position - keys[0].Position);
			if (num < 0f)
			{
				num--;
			}
			return (int)num;
		}

		private float GetCurvePosition(float position)
		{
			CurveKey curveKey = keys[0];
			for (int i = 1; i < keys.Count; i++)
			{
				CurveKey curveKey2 = Keys[i];
				if (curveKey2.Position >= position)
				{
					if (curveKey.Continuity == CurveContinuity.Step)
					{
						if (position >= 1f)
						{
							return curveKey2.Value;
						}
						return curveKey.Value;
					}
					float num = (position - curveKey.Position) / (curveKey2.Position - curveKey.Position);
					float num2 = num * num;
					float num3 = num2 * num;
					return (2f * num3 - 3f * num2 + 1f) * curveKey.Value + (num3 - 2f * num2 + num) * curveKey.TangentOut + (3f * num2 - 2f * num3) * curveKey2.Value + (num3 - num2) * curveKey2.TangentIn;
				}
				curveKey = curveKey2;
			}
			return 0f;
		}
	}
}
