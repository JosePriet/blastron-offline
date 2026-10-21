using System;

namespace EB.Sequence
{
	public class VariableAttribute : Attribute
	{
		public Type ExpectedType;

		public Direction Direction;

		public bool Show = true;
	}
}
