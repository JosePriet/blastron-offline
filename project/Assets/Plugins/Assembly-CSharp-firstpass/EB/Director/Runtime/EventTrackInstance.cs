using EB.Director.Serialization;

namespace EB.Director.Runtime
{
	public class EventTrackInstance : TrackInstance
	{
		private int _lastFrame;

		private int _lastIndex;

		public override void Update(float frameTime)
		{
			int num = (int)frameTime;
			while (_lastFrame <= num)
			{
				for (int i = _lastIndex; i < _track.frames.Count; i++)
				{
					KeyFrame keyFrame = _track.frames[i];
					if (keyFrame.frame == _lastFrame)
					{
						_parent.FireEvent(keyFrame.StringValue);
						_lastIndex = i + 1;
					}
					else if (keyFrame.frame > _lastFrame)
					{
						break;
					}
				}
				_lastFrame++;
			}
		}
	}
}
