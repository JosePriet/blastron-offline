using System;
using System.Collections;
using EB;
using UnityEngine;

public class BigArrow : MonoBehaviour
{
	public enum ArrowType
	{
		Unknown = 0,
		World = 1,
		UI = 2
	}

	public GameObject arrow;

	private Vector3 offset = Vector3.zero;

	private Vector3 animationOffset = Vector3.zero;

	private Vector3 originalScale = Vector3.zero;

	private float count;

	private float speed = 10f;

	private float amplitude = 0.1f;

	private float squish = 1f;

	private Vector3 bounds = Vector3.zero;

	private float angle;

	private GameObject uiParent;

	private EB.Action callback;

	private ArrowType arrowType;

	private Camera uiCamera;

	private IEnumerator Start()
	{
		GameObject prefab = null;
		yield return Assets.LoadAsync("UI/BigArrow", typeof(GameObject), (UnityEngine.Object asset) =>
		{
			prefab = (GameObject)asset;
		});
		arrow = (GameObject)UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
		if (arrowType == ArrowType.UI)
		{
			arrow.transform.parent = base.gameObject.transform;
		}
		else
		{
			GameObject uiPanel = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
			arrow.transform.parent = uiPanel.transform;
			uiCamera = NGUITools.FindCameraForLayer(uiPanel.layer);
		}
		arrow.transform.localPosition = Vector3.zero;
		if (arrowType == ArrowType.UI)
		{
			arrow.transform.localScale = Vector3.one * Tuning.Instance.bigarrow_uiScale;
		}
		else
		{
			arrow.transform.localScale = Vector3.one * Tuning.Instance.bigarrow_worldScale;
		}
		RefreshArrowProperties();
		UnityEngine.Object.DontDestroyOnLoad(arrow);
	}

	private void OnClicked(object context)
	{
		if (callback != null)
		{
			callback();
		}
	}

	public void SetCallback(EB.Action user)
	{
		callback = user;
	}

	public void SetUIParent(GameObject go)
	{
		uiParent = go;
	}

	private Vector3 WorldToScreen(Vector3 pos)
	{
		Vector3 position = Camera.main.WorldToViewportPoint(pos);
		return uiCamera.ViewportToWorldPoint(position);
	}

	private void WorldToScreen(Transform t, Vector3 pos)
	{
		t.position = WorldToScreen(pos);
		Vector3 localPosition = new Vector3((int)t.localPosition.x, (int)t.localPosition.y, 5f);
		t.localPosition = localPosition;
	}

	private void RefreshArrowProperties()
	{
		if (arrow != null)
		{
			amplitude = Tuning.Instance.bigarrow_uiAmplitude;
			speed = Tuning.Instance.bigarrow_uiSpeed;
			squish = Tuning.Instance.bigarrow_uiSquish;
			if (arrowType == ArrowType.World)
			{
				Vector3 position = uiParent.transform.position;
				WorldToScreen(arrow.transform, position + offset);
			}
			else if (arrowType == ArrowType.UI)
			{
				bounds.y -= arrow.transform.localPosition.y;
				Vector3 vector = new Vector3(0f, 0f, -50f);
				arrow.transform.localPosition += vector;
			}
			originalScale = arrow.transform.localScale;
		}
		Update();
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(arrow);
	}

	public void SetArrowType(ArrowType at)
	{
		arrowType = at;
	}

	public void SetOffset(float x, float y, float z)
	{
		offset = new Vector3(x, y, z);
	}

	public void SetAngle(float rot)
	{
		angle = rot;
	}

	public void Update()
	{
		if (arrow != null)
		{
			count = UnityEngine.Time.realtimeSinceStartup * speed;
			Vector3 zero = Vector3.zero;
			zero.x = (0f - Mathf.Sin(angle * ((float)Math.PI / 180f))) * 1f;
			zero.y = Mathf.Cos(angle * ((float)Math.PI / 180f)) * 1f;
			arrow.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
			float num = Mathf.Sin(count) * amplitude;
			float x = (1f - Mathf.Sin(count)) * squish;
			Vector3 zero2 = Vector3.zero;
			zero2.x = x;
			arrow.transform.localScale = originalScale + zero2;
			animationOffset.x = zero.x * num;
			animationOffset.y = zero.y * num;
			if (uiParent != null && arrowType == ArrowType.World)
			{
				Vector3 position = uiParent.transform.position;
				WorldToScreen(arrow.transform, position + offset);
				arrow.transform.localPosition = arrow.transform.localPosition + animationOffset;
			}
			else
			{
				arrow.transform.localPosition = offset + bounds + animationOffset;
			}
		}
	}

	private void OnDrawGizmos()
	{
		if (arrow != null)
		{
			Gizmos.DrawSphere(arrow.transform.position, 0.1f);
		}
	}
}
