using System;

namespace EB.Sequence.Runtime
{
	public class Exception : System.Exception
	{
		public Node Node { get; set; }

		public Exception(Node node, string message)
			: base(message)
		{
			Node = node;
		}
	}
}
