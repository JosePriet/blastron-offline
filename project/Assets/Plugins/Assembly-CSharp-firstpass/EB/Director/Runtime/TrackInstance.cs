using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director.Runtime
{
	public abstract class TrackInstance
	{
		public Track _track;

		public Group _group;

		public Component _parent;

		public GameObject _target;

		public abstract void Update(float frameTime);

		public virtual void SaveState()
		{
		}

		public virtual void RestoreState()
		{
		}
	}
}
