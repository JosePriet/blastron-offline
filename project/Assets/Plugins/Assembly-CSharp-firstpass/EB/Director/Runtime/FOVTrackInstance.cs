using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director.Runtime
{
	public class FOVTrackInstance : BlendableTrackInstance
	{
		private float _state;

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			if (_target != null && _target.camera != null)
			{
				float floatValue = from.FloatValue;
				float floatValue2 = to.FloatValue;
				float fov = Mathf.Lerp(floatValue, floatValue2, blend);
				_target.camera.fov = fov;
			}
		}

		public override void SaveState()
		{
			if (_target != null && _target.camera != null)
			{
				_state = _target.camera.fov;
			}
		}

		public override void RestoreState()
		{
			if (_target != null)
			{
				_target.camera.fov = _state;
			}
		}
	}
}
