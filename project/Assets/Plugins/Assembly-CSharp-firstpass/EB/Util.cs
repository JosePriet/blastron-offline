using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EB
{
	public static class Util
	{
		public static Vector2 ScreenSize
		{
			get
			{
				return new Vector2(Screen.width, Screen.height);
			}
		}

		public static Vector2 ScreenCenter
		{
			get
			{
				return ScreenSize * 0.5f;
			}
		}

		public static Hashtable FromList(params object[] list)
		{
			if ((list.Length & 1) == 1)
			{
				throw new Exception("List must be even length!");
			}
			Hashtable hashtable = new Hashtable();
			int num = list.Length / 2;
			for (int i = 0; i < num; i++)
			{
				hashtable.Add(list[i * 2], list[i * 2 + 1]);
			}
			return hashtable;
		}

		public static Rect RectFromPointSize(Vector2 pt, Vector2 size)
		{
			Vector2 vector = size * 0.5f;
			return new Rect(pt.x - vector.x, pt.y - vector.y, size.x, size.y);
		}

		public static void SetActiveRecursive(GameObject obj, bool active)
		{
			if (obj != null)
			{
				obj.SetActive(active);
			}
		}

		public static void SetActiveRecursive(GameObject obj, string name, bool active)
		{
			SetActiveRecursive(GetObject(obj, name, true), active);
		}

		public static void SetLayerRecursive(GameObject obj, string name, int layer)
		{
			SetLayerRecursive(GetObject(obj, name), layer);
		}

		public static void SetLayerRecursive(GameObject obj, int layer)
		{
			if (!(obj != null))
			{
				return;
			}
			obj.layer = layer;
			foreach (Transform item in obj.transform)
			{
				SetLayerRecursive(item.gameObject, layer);
			}
		}

		public static void SetLayerRecursive(GameObject obj, int layer, int checkLayer)
		{
			if (!(obj != null) || obj.layer != checkLayer)
			{
				return;
			}
			obj.layer = layer;
			foreach (Transform item in obj.transform)
			{
				SetLayerRecursive(item.gameObject, layer, checkLayer);
			}
		}

		public static void SetLayerRecursiveCheckRoot(GameObject obj, string name, int layer)
		{
			if (obj != null && obj.layer != layer)
			{
				SetLayerRecursive(obj, name, layer);
			}
		}

		public static void SetLayerRecursiveCheckRoot(GameObject root, int layer)
		{
			if (root != null && root.layer != layer)
			{
				SetLayerRecursive(root, layer);
			}
		}

		public static T FindComponent<T>(GameObject obj) where T : Component
		{
			if (obj != null)
			{
				T component = obj.GetComponent<T>();
				if (component != null)
				{
					return component;
				}
				foreach (Transform item in obj.transform)
				{
					component = FindComponent<T>(item.gameObject);
					if (component != null)
					{
						return component;
					}
				}
			}
			return (T)null;
		}

		public static T FindComponentUpwards<T>(GameObject obj) where T : Component
		{
			if (obj != null)
			{
				T component = obj.GetComponent<T>();
				if (component != null)
				{
					return component;
				}
				Transform parent = obj.transform.parent;
				if (parent != null)
				{
					component = FindComponentUpwards<T>(obj.transform.parent.gameObject);
					if (component != null)
					{
						return component;
					}
				}
			}
			return (T)null;
		}

		public static T[] FindAllComponents<T>(GameObject obj) where T : Component
		{
			List<T> list = new List<T>();
			if (obj != null)
			{
				T[] components = obj.GetComponents<T>();
				if (components != null)
				{
					T[] array = components;
					foreach (T item in array)
					{
						list.Add(item);
					}
				}
				foreach (Transform item3 in obj.transform)
				{
					components = FindAllComponents<T>(item3.gameObject);
					if (components != null)
					{
						T[] array2 = components;
						foreach (T item2 in array2)
						{
							list.Add(item2);
						}
					}
				}
			}
			return list.ToArray();
		}

		public static void Parent(GameObject parent, string name, GameObject child)
		{
			Parent(GetObjectExactMatch(parent, name), child);
		}

		public static void Parent(Transform parent, Transform child)
		{
			Vector3 localPosition = child.localPosition;
			Quaternion localRotation = child.localRotation;
			Vector3 localScale = child.localScale;
			child.parent = parent;
			child.localPosition = localPosition;
			child.localRotation = localRotation;
			child.localScale = localScale;
		}

		public static void Parent(GameObject parent, GameObject child)
		{
			if (parent != null && child != null)
			{
				Parent(parent.transform, child.transform);
			}
		}

		public static GameObject[] GetObjects(GameObject obj, string name = null)
		{
			List<GameObject> list = new List<GameObject>();
			if (obj == null)
			{
				return list.ToArray();
			}
			if (name == null || obj.name.Contains(name))
			{
				list.Add(obj);
			}
			foreach (Transform item2 in obj.transform)
			{
				GameObject[] objects = GetObjects(item2.gameObject, name);
				GameObject[] array = objects;
				foreach (GameObject item in array)
				{
					list.Add(item);
				}
			}
			return list.ToArray();
		}

		public static GameObject GetObject(GameObject obj, string name)
		{
			return GetObject(obj, name, false);
		}

		public static GameObject GetObjectExactMatch(GameObject obj, string name)
		{
			return GetObject(obj, name, true);
		}

		public static GameObject GetObject(GameObject obj, string name, bool bExactMatch)
		{
			if (obj == null || string.IsNullOrEmpty(name))
			{
				return null;
			}
			if (!bExactMatch)
			{
				if (obj.name.Contains(name))
				{
					return obj;
				}
			}
			else if (obj.name == name)
			{
				return obj;
			}
			Transform transform = obj.transform;
			if ((bool)transform)
			{
				int childCount = transform.GetChildCount();
				for (int i = 0; i < childCount; i++)
				{
					Transform child = transform.GetChild(i);
					if ((bool)child)
					{
						GameObject gameObject = GetObject(child.gameObject, name, bExactMatch);
						if (gameObject != null)
						{
							return gameObject;
						}
					}
				}
			}
			return null;
		}

		public static Material[] GatherMaterials(GameObject obj, string name)
		{
			List<Material> list = new List<Material>();
			if (obj != null)
			{
				if (obj.renderer != null)
				{
					if (string.IsNullOrEmpty(name))
					{
						Material[] materials = obj.renderer.materials;
						foreach (Material item in materials)
						{
							list.Add(item);
						}
					}
					else
					{
						Material[] materials2 = obj.renderer.materials;
						foreach (Material material in materials2)
						{
							if (material.name.Contains(name))
							{
								list.Add(material);
							}
						}
					}
				}
				foreach (Transform item3 in obj.transform)
				{
					Material[] array = GatherMaterials(item3.gameObject, name);
					Material[] array2 = array;
					foreach (Material item2 in array2)
					{
						list.Add(item2);
					}
				}
			}
			return list.ToArray();
		}

		public static int[] GetIntArray(ArrayList data)
		{
			List<int> list = new List<int>();
			if (data != null)
			{
				foreach (object datum in data)
				{
					if (datum != null)
					{
						int result = 0;
						if (int.TryParse(datum.ToString(), out result))
						{
							list.Add(result);
						}
					}
				}
			}
			return list.ToArray();
		}

		public static void BroadcastMessage(string message)
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject = (GameObject)array[i];
				if ((bool)gameObject)
				{
					gameObject.SendMessage(message, SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		public static void BroadcastMessage(string message, object value)
		{
			UnityEngine.Object[] array = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject = (GameObject)array[i];
				if ((bool)gameObject)
				{
					gameObject.SendMessage(message, value, SendMessageOptions.DontRequireReceiver);
				}
			}
		}

		public static float[] GetFloatArray(ArrayList data)
		{
			List<float> list = new List<float>();
			if (data != null)
			{
				foreach (object datum in data)
				{
					if (datum != null)
					{
						float result = 0f;
						if (float.TryParse(datum.ToString(), out result))
						{
							list.Add(result);
						}
					}
				}
			}
			return list.ToArray();
		}

		public static Transform Ascend(string ancestorName, Transform child)
		{
			if (child.transform.parent != null)
			{
				if (child.transform.parent.name == ancestorName)
				{
					return child.transform.parent;
				}
				return Ascend(ancestorName, child.transform.parent);
			}
			return null;
		}
	}
}
