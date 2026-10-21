using System;

namespace EB.Sequence.Serialization
{
	[System.Serializable]
	public enum NodeType
	{
		None = 0,
		Event = 1,
		Action = 2,
		Condition = 3,
		Variable = 4
	}
}
