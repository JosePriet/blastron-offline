using System;

namespace EB.Sequence.Runtime
{
	public class Node : IDisposable
	{
		public static string kAlphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public Component Parent { get; set; }

		public int Id { get; set; }

		public virtual void Init()
		{
		}

		public virtual void Dispose()
		{
		}

		public virtual void OnInputChanged()
		{
		}

		public virtual void GlobalValueChanged()
		{
		}

		public virtual void Serialize(BitStream bs)
		{
		}
	}
}
