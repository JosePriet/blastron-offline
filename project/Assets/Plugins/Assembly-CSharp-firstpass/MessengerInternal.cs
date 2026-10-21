using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

internal static class MessengerInternal
{
	public static Dictionary<KeyValuePair<GameObject, int>, Delegate> s_EventRegistry = new Dictionary<KeyValuePair<GameObject, int>, Delegate>();

	public static Dictionary<int, HashSet<Delegate>> s_BroadcastEventRegistry = new Dictionary<int, HashSet<Delegate>>();

	private static float LastTime = 0f;

	private static HashSet<int> OrphanedMessages = new HashSet<int>();

	public static void OnListenerAdding(KeyValuePair<GameObject, int> Key, Delegate Listener)
	{
		Delegate value = null;
		if (!s_EventRegistry.TryGetValue(Key, out value))
		{
			s_EventRegistry.Add(Key, Listener);
		}
		if (value != null && value.GetType() != Listener.GetType())
		{
			throw new Exception(string.Format("Attempting to add listener with inconsistent signature for event {0}.  Current listeners have type {1} and listener being added has type {2}.", Key.Value, value.GetType().Name, Listener.GetType().Name));
		}
		if (value != null)
		{
			s_EventRegistry[Key] = Delegate.Combine(value, Listener);
		}
		HashSet<Delegate> value2 = null;
		if (!s_BroadcastEventRegistry.TryGetValue(Key.Value, out value2))
		{
			value2 = new HashSet<Delegate>();
			s_BroadcastEventRegistry.Add(Key.Value, value2);
		}
		value2.Add(Listener);
	}

	public static void OnListenerRemoving(KeyValuePair<GameObject, int> Key, Delegate Listener)
	{
		Delegate value = null;
		if (s_EventRegistry.TryGetValue(Key, out value))
		{
			if (value == null)
			{
				throw new Exception(string.Format("Attempting to remove listener for event {0} but current listener is null.", Key.Value));
			}
			if (value.GetType() != Listener.GetType())
			{
				throw new Exception(string.Format("Attempting to remove listener with inconsistent signature for event {0}.  Current listeners have type {1} and listener being removed has type {2}.", Key.Value, value.GetType(), Listener.GetType()));
			}
			Delegate obj = Delegate.Remove(value, Listener);
			if (obj == null)
			{
				s_EventRegistry.Remove(Key);
			}
			else
			{
				s_EventRegistry[Key] = obj;
			}
			HashSet<Delegate> value2 = null;
			if (s_BroadcastEventRegistry.TryGetValue(Key.Value, out value2))
			{
				value2.Remove(Listener);
			}
			return;
		}
		throw new Exception(string.Format("Attempting to remove listener for unknown event {0}.", Key.Value));
	}

	public static void ShowDebugInfo(bool UseTime)
	{
		if (!(UnityEngine.Time.time - LastTime > 5f) && UseTime)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<int, HashSet<Delegate>> item in s_BroadcastEventRegistry)
		{
			num += item.Value.Count;
			foreach (Delegate item2 in item.Value)
			{
				MonoBehaviour monoBehaviour = item2.Target as MonoBehaviour;
				if (null == monoBehaviour)
				{
					EB.Debug.LogWarning("NULL THING:" + item.Key);
					OrphanedMessages.Add(item.Key);
				}
				else if (!UseTime)
				{
					EB.Debug.Log(string.Concat("Listener(", item2.Target, "):", item.Key));
				}
			}
		}
		EB.Debug.Log("MSGS:" + num);
		LastTime = UnityEngine.Time.time;
	}

	public static void OnBroadcastToAll(string EventName, ref ArrayList CallbacksToFire)
	{
		HashSet<Delegate> value = null;
		int hashCode = EventName.GetHashCode();
		if (!s_BroadcastEventRegistry.TryGetValue(hashCode, out value))
		{
			return;
		}
		CallbacksToFire = new ArrayList(value.Count);
		foreach (Delegate item in value)
		{
			MonoBehaviour monoBehaviour = item.Target as MonoBehaviour;
			if (null != monoBehaviour && monoBehaviour.enabled)
			{
				CallbacksToFire.Add(item);
			}
		}
		if (CallbacksToFire.Count == 0)
		{
			CallbacksToFire = null;
		}
	}
}
