using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
	private const float _cMaxPowerSegments = 13f;

	private const float _cMinPowerT = 0.2f;

	private static Mesh _moveRingMesh;

	private static Mesh _moveIndicatorMesh;

	private static Mesh _powerMesh;

	private static Mesh _aimMesh;

	private static Mesh _airstrikeAimMesh;

	private static Mesh _aimAssistBlobMesh;

	private static Material _powerMeterMaterial;

	private static Material _aimMaterial;

	private static Material _airstrikeAimMaterial;

	private static Material _moveRingMaterial;

	private static Material _moveIndicatorMaterial;

	private static Material _aimAssistBlobMaterial;

	private static int _guiLayer;

	private void Start()
	{
		_powerMeterMaterial = UnityEngine.Object.Instantiate(Resources.Load("Materials/powerMeterSegMaterial")) as Material;
		_powerMesh = Misc.MakeQuadMesh(0f, -0.5f, 1f, 0.5f);
		_aimMaterial = UnityEngine.Object.Instantiate(Resources.Load("Materials/aimDeviceMaterial")) as Material;
		_aimMesh = Misc.MakeQuadMesh(-0.5f, -0.5f, 0.5f, 0.5f);
		_airstrikeAimMaterial = UnityEngine.Object.Instantiate(Resources.Load("Materials/airstrikeMaterial")) as Material;
		_airstrikeAimMesh = Misc.MakeQuadMesh(-0.5f, -1f, 0.5f, 0f);
		_moveRingMaterial = Resources.Load("Materials/moveRingMaterial") as Material;
		_moveRingMesh = Misc.MakeQuadMesh(-0.5f, -0.5f, 0.5f, 0.5f);
		_moveIndicatorMaterial = Resources.Load("Materials/moveIndicatorMaterial") as Material;
		_moveIndicatorMesh = Misc.MakeQuadMesh(-0.5f, -0.5f, 0.5f, 0.5f);
		_aimAssistBlobMaterial = Resources.Load("Materials/aimAssistMaterial") as Material;
		_aimAssistBlobMesh = Misc.MakeQuadMesh(-0.5f, -0.5f, 0.5f, 0.5f);
		_airstrikeAimMaterial.renderQueue = 5000;
		_guiLayer = LayerMask.NameToLayer("GUI");
	}

	public static void DrawPowerMeter(Vector3 worldPos, Vector2 offset, float angle, float mag, float displayScale)
	{
		worldPos.z = -60f;
		Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
		worldPos += RotateOffsetYFlip(offset, angle);
		mag = mag * 0.8f + 0.2f;
		_powerMeterMaterial.SetFloat("_Power", mag);
		Graphics.DrawMesh(_powerMesh, Matrix4x4.TRS(worldPos, q, new Vector3(mag * displayScale, 2f, 2f)), _powerMeterMaterial, 0);
	}

	public static void DrawAimDevice(Vector3 worldPos, Vector2 offset, float angle)
	{
		worldPos.z = -60f;
		worldPos += AngleHeightRightToWorldOffset(offset, angle);
		Graphics.DrawMesh(_aimMesh, Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * 3f), _aimMaterial, 0);
	}

	public static void DrawAirstrikeAimDevice(Vector3 pos, float mag, Vector3 dir)
	{
		Vector3 position = Misc.TopOfScreenIntersectionVP(pos, dir);
		Camera currentCamera = UICamera.currentCamera;
		position = currentCamera.ViewportToWorldPoint(position);
		position.z = 1000f;
		Graphics.DrawMesh(_airstrikeAimMesh, Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * 0.2f), _airstrikeAimMaterial, _guiLayer, currentCamera);
	}

	public static Vector3 RotateOffsetYFlip(Vector2 offset, Quaternion rot)
	{
		Vector3 vector = rot * Vector3.right;
		Vector3 vector2 = rot * Vector3.up;
		float num = ((!(vector.x > 0f)) ? (-1f) : 1f);
		return vector * offset.x + vector2 * offset.y * num;
	}

	public static Vector3 RotateOffsetYFlip(Vector2 offset, float angle)
	{
		Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
		return RotateOffsetYFlip(offset, rot);
	}

	public static Vector3 AngleHeightRightToWorldOffset(Vector2 offset, float angle)
	{
		Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
		Vector3 vector = quaternion * Vector3.right;
		return vector * offset.x + Vector3.up * offset.y;
	}

	public static void DrawPowerArrowFINAL(Vector3 worldPos, Vector2 offset, float angle, float mag, float displayScale)
	{
		worldPos += RotateOffsetYFlip(offset, angle);
		worldPos.z = -60f;
		Quaternion quaternion = Quaternion.AngleAxis(angle, Vector3.forward);
		Vector3 vector = quaternion * Vector3.right * 0.7f;
		int num = (int)Mathf.Min(Mathf.Ceil(mag * 13f), 13f);
		float num2 = 1f;
		float y = num2 * Mathf.Pow(1.1f, 13f);
		float num3 = ((!(vector.x < 0f)) ? 1f : (-1f));
		float num4 = 1.2f;
		Vector4 vector2 = new Vector4(num2, y, 0f, 0f);
		_powerMeterMaterial.SetVector("_Scales", vector2);
		for (int i = 0; i < num; i++)
		{
			worldPos += vector;
			Graphics.DrawMesh(_powerMesh, Matrix4x4.TRS(worldPos, quaternion, new Vector3(num2, num2 * num3, num2)), _powerMeterMaterial, 0);
			num2 *= 1.1f;
			vector *= num4;
			num4 = Mathf.Max(1f, num4 * 0.97f);
		}
	}

	public static void DrawMoveRing(Vector3 worldPos, float radius)
	{
		radius *= 2f;
		worldPos.z = -60f;
		Graphics.DrawMesh(_moveRingMesh, Matrix4x4.TRS(worldPos, Quaternion.identity, new Vector3(radius, radius, 1f)), _moveRingMaterial, 0);
	}

	public static void DrawMoveIndicator(Vector3 worldPos, float radius, Vector3 dir)
	{
		float num = Mathf.Atan2(dir.y, dir.x) * 57.29578f;
		if (num < -90f)
		{
			num = 180f;
		}
		else if (num < 0f)
		{
			num = 0f;
		}
		num = Mathf.Max(Mathf.Min(num, 180f), 0f);
		Quaternion quaternion = Quaternion.AngleAxis(num, Vector3.forward);
		dir = quaternion * Vector3.right;
		worldPos += dir * radius;
		worldPos.z = -65f;
		Graphics.DrawMesh(_moveIndicatorMesh, Matrix4x4.TRS(worldPos, quaternion, new Vector3(2f, 2f, 1f)), _moveIndicatorMaterial, 0);
	}

	public static void DrawProjectileFlight(Vector3 pos, Vector2 offset, float angle, float mag, Weapon w, Body ignoreBody)
	{
		pos += RotateOffsetYFlip(offset, angle);
		float speed = w.ProjectileSpeed(mag);
		float dt = 0.2f;
		List<Vector3> segs = new List<Vector3>();
		AIHelp.EvalProjectileFlight(speed, FSWorldComponent.PhysicsWorld.Gravity.Y * w.GravityScale, angle * ((float)Math.PI / 180f), dt, (Vector3 p, float t) =>
		{
			segs.Add(p + pos);
			return !(t > 3f - mag);
		});
		if (segs.Count <= 1)
		{
			return;
		}
		float num = 0f;
		float num2 = 1.5f;
		Vector3 input = segs[0];
		Vector3 s = new Vector3(0f, 0f, 0f);
		float num3 = 0f;
		float b = 1.5f;
		for (int num4 = 0; num4 < segs.Count - 1; num4++)
		{
			Vector3 vector = segs[num4];
			Vector3 vector2 = segs[num4 + 1];
			float num5 = Vector3.Distance(vector, vector2);
			float num6 = num2 / num5;
			while (num < 1f)
			{
				Vector3 vector3 = Vector3.Lerp(vector, vector2, num);
				num += num6;
				FVector2 dir = FSHelper.Vector3ToFVector2(vector3) - FSHelper.Vector3ToFVector2(input);
				if (Misc.RayCast(FSHelper.Vector3ToFVector2(input), dir, ignoreBody))
				{
					return;
				}
				Graphics.DrawMesh(_aimAssistBlobMesh, Matrix4x4.TRS(vector3, Quaternion.identity, s), _aimAssistBlobMaterial, 0);
				input = vector3;
				num3 = (s.z = (s.y = (s.x = Mathf.Min(num3 + 0.1f, b))));
			}
			num--;
			vector = vector2;
		}
	}
}
