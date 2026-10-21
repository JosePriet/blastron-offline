using System.Collections.Generic;
using EB.Director.Runtime;
using EB.Director.Serialization;
using EB.Sequence.Runtime;
using UnityEngine;

namespace EB.Director
{
	public class Component : MonoBehaviour
	{
		public const int kFPS = 15;

		public float Length = 10f;

		public int _nextId = 1;

		public List<Group> Groups = new List<Group>();

		private bool _playing;

		private bool _paused;

		private bool _inUpdate;

		private bool _doStop;

		private float _playSpeed = 1f;

		private float _playTime;

		private float _playRealTime;

		private SequenceAction_Director _action;

		private List<GroupInstance> _instances = new List<GroupInstance>();

		private Dictionary<string, Trigger> _events = new Dictionary<string, Trigger>();

		public List<GroupInstance> Instances
		{
			get
			{
				return _instances;
			}
		}

		public bool IsPlaying
		{
			get
			{
				return _playing;
			}
		}

		public float PlayTime
		{
			get
			{
				return _playTime;
			}
		}

		public virtual bool IsPaused
		{
			get
			{
				return _paused;
			}
		}

		public GameObject DirectorCamera { get; set; }

		public string[] GetEvents()
		{
			List<string> list = new List<string>();
			Group groupByType = GetGroupByType(GroupType.Event);
			if (groupByType != null)
			{
				foreach (Track track in groupByType.tracks)
				{
					if (track.type != TrackType.Event)
					{
						continue;
					}
					foreach (KeyFrame frame in track.frames)
					{
						string stringValue = frame.StringValue;
						if (!list.Contains(stringValue))
						{
							list.Add(stringValue);
						}
					}
				}
			}
			return list.ToArray();
		}

		public Group GetGroupByType(GroupType type)
		{
			foreach (Group group in Groups)
			{
				if (group.type == type)
				{
					return group;
				}
			}
			return null;
		}

		public GroupInstance GetGroupInstanceById(int id)
		{
			foreach (GroupInstance instance in _instances)
			{
				if (instance._group.id == id)
				{
					return instance;
				}
			}
			return null;
		}

		public Group GetGroupById(int id)
		{
			foreach (Group group in Groups)
			{
				if (group.id == id)
				{
					return group;
				}
			}
			return null;
		}

		public bool AddGroup(Group group)
		{
			if (!Utils.HasGroupInput(group.type) && GetGroupByType(group.type) != null)
			{
				return false;
			}
			group.id = _nextId++;
			Groups.Add(group);
			Groups.Sort((Group obj, Group obj2) => (obj.type == obj2.type) ? (obj.id - obj2.id) : (obj.type - obj2.type));
			return true;
		}

		public void Pause()
		{
			_paused = true;
		}

		public void Resume()
		{
			_paused = false;
		}

		public void FireEvent(string eventName)
		{
			Trigger value;
			if (_action != null && _events.TryGetValue(eventName, out value))
			{
				value.Invoke();
			}
		}

		public void Bind(SequenceAction_Director action)
		{
			_action = action;
			_events.Clear();
			string[] events = GetEvents();
			action.Events = new Trigger[events.Length];
			for (int i = 0; i < events.Length; i++)
			{
				action.Events[i] = new Trigger();
				_events[events[i]] = action.Events[i];
			}
		}

		public void Play()
		{
			if (_playing)
			{
				return;
			}
			_instances.Clear();
			int num = 0;
			foreach (Group group in Groups)
			{
				Variable variable = Variable.Null;
				if (Utils.HasGroupInput(group.type) && _action != null)
				{
					if (num < _action.Groups.Length)
					{
						variable = _action.Groups[num];
					}
					num++;
				}
				GroupInstance item = new GroupInstance(this, group, variable);
				_instances.Add(item);
			}
			if (_action != null)
			{
				_action.Started.Invoke();
			}
			_playTime = 0f;
			_playRealTime = Time.realtimeSinceStartup;
			_playing = true;
			_doStop = false;
			_inUpdate = false;
			OnPlay();
			UpdateTracks(0f);
		}

		public void Stop()
		{
			Stop(true);
		}

		public void Stop(bool notify)
		{
			if (_inUpdate)
			{
				_doStop = true;
			}
			else
			{
				if (!_playing)
				{
					return;
				}
				foreach (GroupInstance instance in _instances)
				{
					instance.Restore();
				}
				_instances.Clear();
				_playing = false;
				_doStop = false;
				OnStop();
				if (_action != null && notify)
				{
					_action.Stopped.Invoke();
				}
			}
		}

		public virtual void OnPlay()
		{
		}

		public virtual void OnStop()
		{
		}

		public void LateUpdate()
		{
			float a = Time.realtimeSinceStartup - _playRealTime;
			_playRealTime = Time.realtimeSinceStartup;
			a = Mathf.Min(a, 1f);
			UpdateTracks(a);
		}

		public void UpdateTracks(float dT)
		{
			if (!_playing)
			{
				return;
			}
			_playTime += ((!IsPaused) ? (dT * _playSpeed) : 0f);
			bool flag = false;
			if (_playTime > Length)
			{
				_playTime = Length;
				flag = true;
			}
			float frameTime = _playTime * 15f;
			_inUpdate = true;
			foreach (GroupInstance instance in _instances)
			{
				instance.Update(frameTime);
			}
			_inUpdate = false;
			if (flag || _doStop)
			{
				Stop();
			}
		}
	}
}
