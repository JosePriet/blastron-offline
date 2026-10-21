using System;
using System.Collections.Generic;

namespace EB.Director.Serialization
{
	[System.Serializable]
	public class Track
	{
		public bool restoreState = true;

		public TrackType type;

		public SpaceType space;

		public List<KeyFrame> frames = new List<KeyFrame>();

		public string target = string.Empty;

		public string name
		{
			get
			{
				return type.ToString();
			}
		}

		public void Add(KeyFrame kf)
		{
			frames.RemoveAll((KeyFrame keyFrame) => keyFrame.frame == kf.frame);
			frames.Add(kf);
			Sort();
		}

		public void Rmv(KeyFrame kf)
		{
			frames.Remove(kf);
		}

		public void Sort()
		{
			frames.Sort((KeyFrame kf1, KeyFrame fk2) => kf1.frame - fk2.frame);
		}
	}
}
