using System;
using System.Collections.Generic;

namespace EB.Director.Serialization
{
	[System.Serializable]
	public class Group
	{
		public int id = -1;

		public string name = string.Empty;

		public string targetName = string.Empty;

		public GroupType type;

		public List<Track> tracks = new List<Track>();
	}
}
