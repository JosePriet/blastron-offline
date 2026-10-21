using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director.Runtime
{
	public class OrthographicSizeTrackInstance : BlendableTrackInstance
	{
		private float _state;

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			if (_target != null && _target.camera != null)
			{
				float floatValue = from.FloatValue;
				float floatValue2 = to.FloatValue;
				float orthographicSize = Mathf.Lerp(floatValue, floatValue2, blend);
				_target.camera.orthographicSize = orthographicSize;
			}
		}

		public override void SaveState()
		{
			if (_target != null && _target.camera != null)
			{
				_state = _target.camera.orthographicSize;
			}
		}

		public override void RestoreState()
		{
			if (_target != null)
			{
				_target.camera.orthographicSize = _state;
			}
		}
	}
}
