using System.Collections;
using System.Collections.Generic;
using EB;
using UnityEngine;

public class HUDLabels : MonoBehaviour
{
	private class Labels
	{
		private int _next;

		private GameObject[] _concreteLabels;

		private Camera _uiCamera;

		private Camera UICamera
		{
			get
			{
				if (_uiCamera == null)
				{
					_uiCamera = NGUITools.FindCameraForLayer(LayerMask.NameToLayer("GUI"));
				}
				return null;
			}
		}

		public Labels(GameObject template, int count, Transform parent)
		{
			_next = 0;
			_concreteLabels = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				GameObject gameObject = (GameObject)Object.Instantiate(template);
				gameObject.SetActive(false);
				Util.Parent(parent.gameObject, gameObject);
				gameObject.transform.localPosition = Vector3.zero;
				_concreteLabels[i] = gameObject;
			}
		}

		public GameObject Next()
		{
			GameObject gameObject = _concreteLabels[_next];
			gameObject.SetActive(true);
			_next = (_next + 1) % _concreteLabels.Length;
			return gameObject;
		}

		public void Destroy()
		{
			GameObject[] concreteLabels = _concreteLabels;
			foreach (GameObject obj in concreteLabels)
			{
				Object.Destroy(obj);
			}
		}
	}

	public delegate IEnumerator LabelUpdateFn(GameObject go, Transform relativeTo, Vector3 offset);

	private List<Labels> _labelSet = new List<Labels>();

	public int _next;

	public static HUDLabels Instance { get; private set; }

	private void Awake()
	{
		Instance = this;
	}

	public GameObject Next(int id, LabelUpdateFn updateFn, string text, Transform relativeTo, Vector3 offset)
	{
		if (id >= 0 && id < _labelSet.Count)
		{
			GameObject gameObject = _labelSet[id].Next();
			gameObject.GetComponent<UILabel>().text = text;
			StartCoroutine(updateFn(gameObject, relativeTo, offset));
			return gameObject;
		}
		return null;
	}

	public int Register(GameObject template, Transform parent, int count)
	{
		_labelSet.Add(new Labels(template, count, parent));
		return _labelSet.Count - 1;
	}

	public void Clear()
	{
		StopAllCoroutines();
		foreach (Labels item in _labelSet)
		{
			item.Destroy();
		}
		_labelSet.Clear();
	}
}
