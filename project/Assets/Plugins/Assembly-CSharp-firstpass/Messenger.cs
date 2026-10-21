using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Messenger
{
	public static void ShowDebugInfo()
	{
		MessengerInternal.ShowDebugInfo(false);
	}

	public static void AddListener(GameObject GameObject, string EventName, Callback Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerAdding(key, Handler);
	}

	public static void RemoveListener(GameObject GameObject, string EventName, Callback Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerRemoving(key, Handler);
	}

	public static bool Broadcast(GameObject GameObject, string EventName, bool IgnoreChildren)
	{
		bool result = false;
		if (null != GameObject)
		{
			KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
			Delegate value;
			if (MessengerInternal.s_EventRegistry.TryGetValue(key, out value))
			{
				Callback callback = value as Callback;
				if (callback == null)
				{
					throw new Exception(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", key.Value));
				}
				callback();
				result = true;
			}
			if (!IgnoreChildren)
			{
				foreach (Transform item in GameObject.transform)
				{
					Broadcast(item.gameObject, EventName);
				}
			}
		}
		return result;
	}

	public static bool Broadcast(GameObject GameObject, string EventName)
	{
		return Broadcast(GameObject, EventName, false);
	}

	public static bool BroadcastToAllListeners(string EventName)
	{
		bool result = false;
		ArrayList CallbacksToFire = null;
		MessengerInternal.OnBroadcastToAll(EventName, ref CallbacksToFire);
		if (CallbacksToFire != null)
		{
			foreach (Callback item in CallbacksToFire)
			{
				item();
			}
			result = true;
		}
		return result;
	}
}
public static class Messenger<Type1>
{
	public static void AddListener(GameObject GameObject, string EventName, Callback<Type1> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerAdding(key, Handler);
	}

	public static void RemoveListener(GameObject GameObject, string EventName, Callback<Type1> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerRemoving(key, Handler);
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, bool IgnoreChildren)
	{
		bool result = false;
		if (null != GameObject)
		{
			KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
			Delegate value;
			if (MessengerInternal.s_EventRegistry.TryGetValue(key, out value))
			{
				Callback<Type1> callback = value as Callback<Type1>;
				if (callback == null)
				{
					throw new Exception(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", key.Value));
				}
				callback(ref Argument1);
				result = true;
			}
			if (!IgnoreChildren)
			{
				foreach (Transform item in GameObject.transform)
				{
					Broadcast(item.gameObject, EventName, ref Argument1);
				}
			}
		}
		return result;
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1)
	{
		return Broadcast(GameObject, EventName, ref Argument1, false);
	}

	public static bool BroadcastToAllListeners(string EventName, ref Type1 Argument1)
	{
		bool result = false;
		ArrayList CallbacksToFire = null;
		MessengerInternal.OnBroadcastToAll(EventName, ref CallbacksToFire);
		if (CallbacksToFire != null)
		{
			foreach (Callback<Type1> item in CallbacksToFire)
			{
				item(ref Argument1);
			}
			result = true;
		}
		return result;
	}
}
public static class Messenger<Type1, Type2>
{
	public static void AddListener(GameObject GameObject, string EventName, Callback<Type1, Type2> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerAdding(key, Handler);
	}

	public static void RemoveListener(GameObject GameObject, string EventName, Callback<Type1, Type2> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerRemoving(key, Handler);
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, bool IgnoreChildren)
	{
		bool result = false;
		if (null != GameObject)
		{
			KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
			Delegate value;
			if (MessengerInternal.s_EventRegistry.TryGetValue(key, out value))
			{
				Callback<Type1, Type2> callback = value as Callback<Type1, Type2>;
				if (callback == null)
				{
					throw new Exception(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", key.Value));
				}
				callback(ref Argument1, ref Argument2);
				result = true;
			}
			if (!IgnoreChildren)
			{
				foreach (Transform item in GameObject.transform)
				{
					Broadcast(item.gameObject, EventName, ref Argument1, ref Argument2);
				}
			}
		}
		return result;
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2)
	{
		return Broadcast(GameObject, EventName, ref Argument1, ref Argument2, false);
	}

	public static bool BroadcastToAllListeners(string EventName, ref Type1 Argument1, ref Type2 Argument2)
	{
		bool result = false;
		ArrayList CallbacksToFire = null;
		MessengerInternal.OnBroadcastToAll(EventName, ref CallbacksToFire);
		if (CallbacksToFire != null)
		{
			foreach (Callback<Type1, Type2> item in CallbacksToFire)
			{
				item(ref Argument1, ref Argument2);
			}
			result = true;
		}
		return result;
	}
}
public static class Messenger<Type1, Type2, Type3>
{
	public static void AddListener(GameObject GameObject, string EventName, Callback<Type1, Type2, Type3> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerAdding(key, Handler);
	}

	public static void RemoveListener(GameObject GameObject, string EventName, Callback<Type1, Type2, Type3> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerRemoving(key, Handler);
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, bool IgnoreChildren)
	{
		bool result = false;
		if (null != GameObject)
		{
			KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
			Delegate value;
			if (MessengerInternal.s_EventRegistry.TryGetValue(key, out value))
			{
				Callback<Type1, Type2, Type3> callback = value as Callback<Type1, Type2, Type3>;
				if (callback == null)
				{
					throw new Exception(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", key.Value));
				}
				callback(ref Argument1, ref Argument2, ref Argument3);
				result = true;
			}
			if (!IgnoreChildren)
			{
				foreach (Transform item in GameObject.transform)
				{
					Broadcast(item.gameObject, EventName, ref Argument1, ref Argument2, ref Argument3);
				}
			}
		}
		return result;
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3)
	{
		return Broadcast(GameObject, EventName, ref Argument1, ref Argument2, ref Argument3, false);
	}

	public static bool BroadcastToAllListeners(string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3)
	{
		bool result = false;
		ArrayList CallbacksToFire = null;
		MessengerInternal.OnBroadcastToAll(EventName, ref CallbacksToFire);
		if (CallbacksToFire != null)
		{
			foreach (Callback<Type1, Type2, Type3> item in CallbacksToFire)
			{
				item(ref Argument1, ref Argument2, ref Argument3);
			}
			result = true;
		}
		return result;
	}
}
public static class Messenger<Type1, Type2, Type3, Type4>
{
	public static void AddListener(GameObject GameObject, string EventName, Callback<Type1, Type2, Type3, Type4> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerAdding(key, Handler);
	}

	public static void RemoveListener(GameObject GameObject, string EventName, Callback<Type1, Type2, Type3, Type4> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerRemoving(key, Handler);
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, ref Type4 Argument4, bool IgnoreChildren)
	{
		bool result = false;
		if (null != GameObject)
		{
			KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
			Delegate value;
			if (MessengerInternal.s_EventRegistry.TryGetValue(key, out value))
			{
				Callback<Type1, Type2, Type3, Type4> callback = value as Callback<Type1, Type2, Type3, Type4>;
				if (callback == null)
				{
					throw new Exception(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", key.Value));
				}
				callback(ref Argument1, ref Argument2, ref Argument3, ref Argument4);
				result = true;
			}
			if (!IgnoreChildren)
			{
				foreach (Transform item in GameObject.transform)
				{
					Broadcast(item.gameObject, EventName, ref Argument1, ref Argument2, ref Argument3, ref Argument4);
				}
			}
		}
		return result;
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, ref Type4 Argument4)
	{
		return Broadcast(GameObject, EventName, ref Argument1, ref Argument2, ref Argument3, ref Argument4, false);
	}

	public static bool BroadcastToAllListeners(string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, ref Type4 Argument4)
	{
		bool result = false;
		ArrayList CallbacksToFire = null;
		MessengerInternal.OnBroadcastToAll(EventName, ref CallbacksToFire);
		if (CallbacksToFire != null)
		{
			foreach (Callback<Type1, Type2, Type3, Type4> item in CallbacksToFire)
			{
				item(ref Argument1, ref Argument2, ref Argument3, ref Argument4);
			}
			result = true;
		}
		return result;
	}
}
public static class Messenger<Type1, Type2, Type3, Type4, Type5>
{
	public static void AddListener(GameObject GameObject, string EventName, Callback<Type1, Type2, Type3, Type4, Type5> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerAdding(key, Handler);
	}

	public static void RemoveListener(GameObject GameObject, string EventName, Callback<Type1, Type2, Type3, Type4, Type5> Handler)
	{
		KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
		MessengerInternal.OnListenerRemoving(key, Handler);
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, ref Type4 Argument4, ref Type5 Argument5, bool IgnoreChildren)
	{
		bool result = false;
		if (null != GameObject)
		{
			KeyValuePair<GameObject, int> key = new KeyValuePair<GameObject, int>(GameObject, EventName.GetHashCode());
			Delegate value;
			if (MessengerInternal.s_EventRegistry.TryGetValue(key, out value))
			{
				Callback<Type1, Type2, Type3, Type4, Type5> callback = value as Callback<Type1, Type2, Type3, Type4, Type5>;
				if (callback == null)
				{
					throw new Exception(string.Format("Broadcasting message {0} but listeners have a different signature than the broadcaster.", key.Value));
				}
				callback(ref Argument1, ref Argument2, ref Argument3, ref Argument4, ref Argument5);
				result = true;
			}
			if (!IgnoreChildren)
			{
				foreach (Transform item in GameObject.transform)
				{
					Broadcast(item.gameObject, EventName, ref Argument1, ref Argument2, ref Argument3, ref Argument4, ref Argument5);
				}
			}
		}
		return result;
	}

	public static bool Broadcast(GameObject GameObject, string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, ref Type4 Argument4, ref Type5 Argument5)
	{
		return Broadcast(GameObject, EventName, ref Argument1, ref Argument2, ref Argument3, ref Argument4, ref Argument5, false);
	}

	public static bool BroadcastToAllListeners(string EventName, ref Type1 Argument1, ref Type2 Argument2, ref Type3 Argument3, ref Type4 Argument4, ref Type5 Argument5)
	{
		bool result = false;
		ArrayList CallbacksToFire = null;
		MessengerInternal.OnBroadcastToAll(EventName, ref CallbacksToFire);
		if (CallbacksToFire != null)
		{
			foreach (Callback<Type1, Type2, Type3, Type4, Type5> item in CallbacksToFire)
			{
				item(ref Argument1, ref Argument2, ref Argument3, ref Argument4, ref Argument5);
			}
			result = true;
		}
		return result;
	}
}
