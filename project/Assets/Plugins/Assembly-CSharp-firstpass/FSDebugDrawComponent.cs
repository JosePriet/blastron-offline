using System.Collections;
using FarseerPhysics;
using FarseerPhysics.DebugViews;
using UnityEngine;

[AddComponentMenu("FarseerUnity/Camera/GL DebugDraw Component")]
public class FSDebugDrawComponent : MonoBehaviour
{
	public Material GLMaterial;

	protected FarseerDebugViewUnity debugView;

	protected Camera cam;

	protected Matrix4x4 projection;

	private IEnumerator Start()
	{
		cam = GetComponent<Camera>();
		if (cam == null)
		{
			cam = Camera.mainCamera;
		}
		while (!FSWorldComponent.IsValid)
		{
			yield return 1;
		}
		projection = cam.projectionMatrix;
		debugView = new FarseerDebugViewUnity(FSWorldComponent.PhysicsWorld);
		debugView.AppendFlags(DebugViewFlags.Shape);
		debugView.AppendFlags(DebugViewFlags.ContactPoints);
		debugView.AppendFlags(DebugViewFlags.CenterOfMass);
		debugView.AppendFlags(DebugViewFlags.PerformanceGraph);
		debugView.AppendFlags(DebugViewFlags.DebugPanel);
		if (GLMaterial == null)
		{
			GLMaterial = Resources.Load("Materials/GLM") as Material;
		}
	}

	private void OnPostRender()
	{
		if (debugView != null)
		{
			projection = cam.projectionMatrix;
			debugView.RenderDebugData(ref projection, GLMaterial);
		}
	}

	private void OnGUI()
	{
		if (debugView != null)
		{
			debugView.OnGUI(cam);
		}
	}
}
