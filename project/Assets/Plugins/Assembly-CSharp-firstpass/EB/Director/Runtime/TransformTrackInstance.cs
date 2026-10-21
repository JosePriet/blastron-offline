using EB.Director.Serialization;

namespace EB.Director.Runtime
{
	public class TransformTrackInstance : BlendableTrackInstance
	{
		private QuatPos _state;

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			if (_target != null)
			{
				QuatPos quatPosValue = from.QuatPosValue;
				QuatPos quatPosValue2 = to.QuatPosValue;
				QuatPos.Lerp(quatPosValue, quatPosValue2, blend).Apply(_target.transform, _track.space);
			}
		}

		public override void SaveState()
		{
			if (_target != null)
			{
				_state = QuatPos.FromTransform(_target.transform, _track.space);
			}
		}

		public override void RestoreState()
		{
			if (_target != null)
			{
				_state.Apply(_target.transform, _track.space);
			}
		}
	}
}
