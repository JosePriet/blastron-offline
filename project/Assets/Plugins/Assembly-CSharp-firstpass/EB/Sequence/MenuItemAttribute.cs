using System;

namespace EB.Sequence
{
	public class MenuItemAttribute : Attribute
	{
		public string Path = string.Empty;

		public Type VariableType;
	}
}
