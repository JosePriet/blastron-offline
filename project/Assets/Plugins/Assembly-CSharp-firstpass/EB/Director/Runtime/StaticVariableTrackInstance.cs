using System;
using System.Reflection;
using EB.Director.Serialization;

namespace EB.Director.Runtime
{
	public class StaticVariableTrackInstance : BlendableTrackInstance
	{
		private FieldInfo _field;

		private VariableType _type;

		private object _default;

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			Utils.ApplyVariable(_type, _field, null, blend, from, to);
		}

		public override void SaveState()
		{
			base.SaveState();
			Type type = Utils.GetType(_group.targetName);
			if (type != null)
			{
				_field = type.GetField(_track.target);
				_type = Utils.GetVariableType(_field);
				if (_field != null)
				{
					_default = _field.GetValue(null);
				}
			}
		}

		public override void RestoreState()
		{
			if (_default != null)
			{
				_field.SetValue(null, _default);
			}
			base.RestoreState();
		}
	}
}
