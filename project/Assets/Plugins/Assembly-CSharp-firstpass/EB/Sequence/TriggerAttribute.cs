using System;

namespace EB.Sequence
{
	public class TriggerAttribute : Attribute
	{
		public string EditorName = string.Empty;

		public bool Show = true;
	}
}
