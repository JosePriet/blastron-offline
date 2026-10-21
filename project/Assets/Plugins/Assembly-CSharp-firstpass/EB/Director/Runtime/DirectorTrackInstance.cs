using System.Collections.Generic;
using EB.Director.Serialization;
using UnityEngine;

namespace EB.Director.Runtime
{
	public class DirectorTrackInstance : BlendableTrackInstance
	{
		private Camera _camera;

		private Camera _main;

		private Dictionary<int, Camera> _cameraMap = new Dictionary<int, Camera>();

		private static int _mainCullingMask;

		private CameraData GetCamera(int id)
		{
			Camera value;
			if (_cameraMap.TryGetValue(id, out value))
			{
				return new CameraData(value);
			}
			return new CameraData(_camera);
		}

		public override void Apply(float blend, KeyFrame from, KeyFrame to)
		{
			CameraData camera = GetCamera(from.ByteValue);
			CameraData camera2 = GetCamera(to.ByteValue);
			CameraData.Lerp(camera, camera2, blend).Apply(_camera);
		}

		public override void SaveState()
		{
			_camera = new GameObject("director_camera for " + _parent.name, typeof(Camera)).GetComponent<Camera>();
			Object.DontDestroyOnLoad(_camera.gameObject);
			_main = Camera.mainCamera;
			float aspect = (float)Screen.width / (float)Screen.height;
			if (!Application.isPlaying)
			{
				aspect = 1.3333334f;
			}
			if (_main != null)
			{
				if (_main.cullingMask != 0)
				{
					_mainCullingMask = _main.cullingMask;
				}
				_camera.cullingMask = _mainCullingMask;
				_camera.depth = 0f;
				_camera.clearFlags = _main.clearFlags;
				_camera.backgroundColor = _main.backgroundColor;
				_camera.layerCullDistances = _main.layerCullDistances;
				_camera.depthTextureMode = _main.depthTextureMode;
				_camera.renderingPath = _main.renderingPath;
				_main.enabled = false;
				_main.tag = string.Empty;
				_cameraMap[0] = _main;
			}
			else
			{
				Debug.LogError("NO MAIN CAMERA!");
			}
			_camera.aspect = aspect;
			_camera.tag = "MainCamera";
			foreach (GroupInstance instance in _parent.Instances)
			{
				if (instance._group.type != GroupType.Camera)
				{
					continue;
				}
				GameObject target = instance._target;
				if (target != null)
				{
					Camera camera = target.camera;
					if (camera != null)
					{
						_cameraMap[instance._group.id] = camera;
						camera.ResetProjectionMatrix();
						camera.ResetWorldToCameraMatrix();
						camera.ResetAspect();
						camera.aspect = aspect;
					}
				}
			}
			_parent.DirectorCamera = _camera.gameObject;
		}

		public override void RestoreState()
		{
			if (_camera != null)
			{
				Object.DestroyImmediate(_camera.gameObject);
			}
			if (_main != null)
			{
				_main.enabled = true;
				_main.tag = "MainCamera";
				_main.ResetAspect();
				_main.ResetProjectionMatrix();
				_main.ResetWorldToCameraMatrix();
			}
			_parent.DirectorCamera = null;
		}
	}
}
