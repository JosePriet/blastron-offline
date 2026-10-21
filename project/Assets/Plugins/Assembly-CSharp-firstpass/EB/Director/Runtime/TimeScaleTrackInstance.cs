using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director.Runtime
{
	public class TimeScaleTrackInstance : BlendableTrackInstance
	{
		private float _state;

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			Time.timeScale = Mathf.Lerp(from.FloatValue, to.FloatValue, blend);
		}

		public override void SaveState()
		{
			_state = Time.timeScale;
		}

		public override void RestoreState()
		{
			Time.timeScale = _state;
		}
	}
}
