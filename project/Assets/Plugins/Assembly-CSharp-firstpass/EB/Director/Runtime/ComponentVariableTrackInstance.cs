using System.Reflection;
using EB.Director.Serialization;

namespace EB.Director.Runtime
{
	public class ComponentVariableTrackInstance : ComponentTrackInstance
	{
		private FieldInfo _field;

		private VariableType _type;

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			Utils.ApplyVariable(_type, _field, _component, blend, from, to);
		}

		public override void SaveState()
		{
			base.SaveState();
			if (_component != null)
			{
				_field = _component.GetType().GetField(_track.target);
				_type = Utils.GetVariableType(_field);
			}
		}
	}
}
