using EB.Director.Serialization;

namespace EB.Director.Runtime
{
	public class EnableComponentTrackInstance : ComponentTrackInstance
	{
		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			if (_component != null)
			{
				bool boolValue = from.BoolValue;
				if (_component.enabled != boolValue)
				{
					_component.enabled = boolValue;
				}
			}
		}
	}
}
