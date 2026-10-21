using System.Collections.Generic;
using EB.Director.Serialization;
using EB.Sequence.Runtime;
using UnityEngine;

namespace EB.Director.Runtime
{
	public class GroupInstance
	{
		public readonly Group _group;

		public readonly GameObject _target;

		private List<TrackInstance> _tracks = new List<TrackInstance>();

		public GroupInstance(Component parent, Group group, Variable variable)
		{
			_group = group;
			GroupType type = _group.type;
			if (type == GroupType.ImageEffect)
			{
				_target = parent.DirectorCamera;
				if (Application.platform == RuntimePlatform.OSXWebPlayer)
				{
					return;
				}
			}
			else
			{
				object value = variable.Value;
				if (value != null && value is GameObject)
				{
					_target = (GameObject)value;
				}
				else
				{
					_target = GameObject.Find(group.targetName);
					if (_target == null)
					{
						Debug.Log("GroupInstance Cant find " + group.targetName);
					}
				}
			}
			foreach (Track track in _group.tracks)
			{
				TrackInstance trackInstance = null;
				switch (track.type)
				{
				case TrackType.Transform:
					trackInstance = new TransformTrackInstance();
					break;
				case TrackType.FOV:
					trackInstance = new FOVTrackInstance();
					break;
				case TrackType.OrthographicSize:
					trackInstance = new OrthographicSizeTrackInstance();
					break;
				case TrackType.Director:
					trackInstance = new DirectorTrackInstance();
					break;
				case TrackType.Event:
					trackInstance = new EventTrackInstance();
					break;
				case TrackType.TimeScale:
					trackInstance = new TimeScaleTrackInstance();
					break;
				case TrackType.EnableComponent:
					trackInstance = new EnableComponentTrackInstance();
					break;
				case TrackType.Variable:
					trackInstance = new ComponentVariableTrackInstance();
					break;
				}
				if (trackInstance != null)
				{
					trackInstance._track = track;
					trackInstance._group = _group;
					trackInstance._target = _target;
					trackInstance._parent = parent;
					trackInstance.SaveState();
					_tracks.Add(trackInstance);
				}
			}
		}

		public void Update(float frameTime)
		{
			foreach (TrackInstance track in _tracks)
			{
				track.Update(frameTime);
			}
		}

		public void Restore()
		{
			foreach (TrackInstance track in _tracks)
			{
				if (track._track.restoreState || !Application.isPlaying)
				{
					track.RestoreState();
				}
			}
		}
	}
}
