using System;
using System.Collections;
using UnityEngine;

namespace EB.Sequence.Runtime
{
	public abstract class Event : Operation
	{
		private static Hashtable _active = new Hashtable();

		[Property]
		public int Priority;

		[Property]
		public bool AllowPropagation = true;

		[Trigger]
		public Trigger Out = new Trigger();

		public virtual bool CheckActivate(GameObject instigator, object target)
		{
			return true;
		}

		public override bool Update()
		{
			Out.Invoke();
			return false;
		}

		public override void Init()
		{
			base.Init();
			EventList eventList = (EventList)_active[GetType()];
			if (eventList == null)
			{
				eventList = new EventList();
				_active[GetType()] = eventList;
			}
			eventList.Add(this);
		}

		public override void Dispose()
		{
			EventList eventList = (EventList)_active[GetType()];
			eventList.Remove(this);
			base.Dispose();
		}

		public static void Activate(GameObject instigator, object target, Type eventType)
		{
			EventList eventList = (EventList)_active[eventType];
			if (eventList == null)
			{
				return;
			}
			eventList.Check();
			foreach (Event item in eventList)
			{
				if (item.CheckActivate(instigator, target))
				{
					item.Activate();
					if (!item.AllowPropagation)
					{
						break;
					}
				}
			}
		}
	}
}
