using UnityEngine;

namespace EB.Director
{
	public struct CameraData
	{
		public Matrix4x4 projection;

		public Vector3 position;

		public Quaternion rotation;

		public bool orthographic;

		public float orthographicSize;

		public CameraData(Camera cam)
		{
			position = cam.transform.position;
			rotation = cam.transform.rotation;
			orthographic = cam.orthographic;
			orthographicSize = cam.orthographicSize;
			if (cam.orthographic)
			{
				projection = cam.projectionMatrix;
			}
			else
			{
				projection = Matrix4x4.Perspective(cam.fov, cam.aspect, cam.near, cam.far);
			}
		}

		public static Matrix4x4 Lerp(Matrix4x4 a, Matrix4x4 b, float t)
		{
			Matrix4x4 result = default(Matrix4x4);
			result.SetRow(0, Vector4.Lerp(a.GetRow(0), b.GetRow(0), t));
			result.SetRow(1, Vector4.Lerp(a.GetRow(1), b.GetRow(1), t));
			result.SetRow(2, Vector4.Lerp(a.GetRow(2), b.GetRow(2), t));
			result.SetRow(3, Vector4.Lerp(a.GetRow(3), b.GetRow(3), t));
			return result;
		}

		public static CameraData Lerp(CameraData from, CameraData to, float t)
		{
			CameraData result = default(CameraData);
			result.projection = Lerp(from.projection, to.projection, t);
			result.position = Vector3.Lerp(from.position, to.position, t);
			result.rotation = Quaternion.Lerp(from.rotation, to.rotation, t);
			result.orthographic = to.orthographic;
			result.orthographicSize = Mathf.Lerp(from.orthographicSize, to.orthographicSize, t);
			return result;
		}

		public void Apply(Camera camera)
		{
			camera.orthographic = orthographic;
			camera.orthographicSize = orthographicSize;
			camera.projectionMatrix = projection;
			camera.transform.position = position;
			camera.transform.rotation = rotation;
		}

		public static implicit operator CameraData(Camera cam)
		{
			return new CameraData(cam);
		}
	}
}
