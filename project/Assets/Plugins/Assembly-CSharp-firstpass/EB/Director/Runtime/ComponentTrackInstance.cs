using UnityEngine;

namespace EB.Director.Runtime
{
	public abstract class ComponentTrackInstance : BlendableTrackInstance
	{
		protected Behaviour _component;

		private bool _added;

		public override void SaveState()
		{
			if (_target != null)
			{
				string targetName = _group.targetName;
				_component = (Behaviour)_target.GetComponent(targetName);
				if (_component == null)
				{
					_component = (Behaviour)_target.AddComponent(targetName);
					_component.enabled = false;
					_added = true;
				}
			}
			else
			{
				Debug.LogError("Failed to get target for component enable");
			}
			base.SaveState();
		}

		public override void RestoreState()
		{
			base.RestoreState();
			if (_component != null && _added)
			{
				Object.Destroy(_component);
			}
		}
	}
}
