using System;

namespace EB.Sequence
{
	public class PropertyAttribute : Attribute
	{
		public string Description = string.Empty;

		public Type MapTo;

		public string Hint = string.Empty;

		public bool NonEditable;
	}
}
