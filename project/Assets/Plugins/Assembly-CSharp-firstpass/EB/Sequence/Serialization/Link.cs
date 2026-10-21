using System;

namespace EB.Sequence.Serialization
{
	[System.Serializable]
	public class Link
	{
		public int outId;

		public string outName = string.Empty;

		public int inId;

		public string inName = string.Empty;
	}
}
