using EB;
using UnityEngine;

public class HUD : MonoBehaviour
{
	private GameObject _hud;

	public GameObject HudObject
	{
		get
		{
			return _hud;
		}
	}

	private void OnNetworkLoadedLevel()
	{
		if ((bool)GameObject.Find("/Level"))
		{
			Object.Destroy(_hud);
			if (DebugSettings.SlideJumpLeft)
			{
				_hud = Object.Instantiate(Resources.Load("GameHUDTablet")) as GameObject;
				GameHUD component = _hud.GetComponent<GameHUD>();
				component.Initialize();
				bool flag = false;
				ScaleControlsForMobile();
			}
			else
			{
				_hud = Object.Instantiate(Resources.Load("GameHUDTabletMultiJump")) as GameObject;
			}
			GameObject gameObject = Util.GetObject(ScreenManager.Instance.gameObject, "Panel");
			Vector3 localPosition = _hud.transform.localPosition;
			Vector3 localScale = _hud.transform.localScale;
			_hud.transform.parent = gameObject.transform;
			_hud.transform.localPosition = localPosition;
			_hud.transform.localScale = localScale;
		}
	}

	public void OnLeaveGame()
	{
		if ((bool)_hud)
		{
			Object.Destroy(_hud);
			_hud = null;
		}
	}

	private void ScaleControlsForMobile()
	{
		Vector3 localScale = new Vector3(1.6f, 1.6f, 1.6f);
		GameObject objectExactMatch = Util.GetObjectExactMatch(_hud, "Aim");
		if (objectExactMatch != null)
		{
			objectExactMatch.transform.localScale = localScale;
			UIAnchor uIAnchor = Util.FindComponent<UIAnchor>(objectExactMatch);
			if (uIAnchor != null)
			{
				uIAnchor.relativeOffset = new Vector2(-0.14f, 0.22f);
			}
		}
		objectExactMatch = Util.GetObjectExactMatch(_hud, "Move");
		if (objectExactMatch != null)
		{
			objectExactMatch.transform.localScale = localScale;
			UIAnchor uIAnchor2 = Util.FindComponent<UIAnchor>(objectExactMatch);
			if (uIAnchor2 != null)
			{
				uIAnchor2.relativeOffset = new Vector2(0.14f, 0.22f);
			}
		}
		objectExactMatch = Util.GetObjectExactMatch(_hud, "TopButtons");
		if (objectExactMatch != null)
		{
			objectExactMatch.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
			UIAnchor uIAnchor3 = Util.FindComponent<UIAnchor>(objectExactMatch);
			if (uIAnchor3 != null)
			{
				uIAnchor3.relativeOffset = new Vector2(-0.01f, -0.08f);
			}
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 vector = new Vector3(-480f, -320f, 0f);
		Vector3 vector2 = new Vector3(480f, -320f, 0f);
		Vector3 vector3 = new Vector3(480f, 320f, 0f);
		Vector3 vector4 = new Vector3(-480f, 320f, 0f);
		Gizmos.DrawLine(vector, vector2);
		Gizmos.DrawLine(vector2, vector3);
		Gizmos.DrawLine(vector3, vector4);
		Gizmos.DrawLine(vector4, vector);
	}
}
