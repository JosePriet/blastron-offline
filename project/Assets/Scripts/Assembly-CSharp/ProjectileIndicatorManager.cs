using System.Collections.Generic;
using UnityEngine;

public class ProjectileIndicatorManager : MonoBehaviour
{
	private class ProjectileTracker
	{
		public RangeWeapon projectile;

		public ProjectileTracker(RangeWeapon proj)
		{
			projectile = proj;
		}
	}

	private static ProjectileIndicatorManager _this;

	private List<ProjectileTracker> activeProjectiles;

	private Mesh _indicatorMeshLeft;

	private Material _indicatorMaterial;

	private Vector3 left = new Vector3(0f, 0f, 180f);

	private Vector3 leftUp = new Vector3(0f, 0f, 135f);

	private Vector3 up = new Vector3(0f, 0f, 90f);

	private Vector3 rightUp = new Vector3(0f, 0f, 45f);

	private Vector3 right = Vector3.zero;

	public static ProjectileIndicatorManager Instance
	{
		get
		{
			return _this;
		}
	}

	private void Awake()
	{
		_this = this;
		activeProjectiles = new List<ProjectileTracker>();
		MakeMeshes();
	}

	public void RegisterProjectile(RangeWeapon projectile)
	{
		activeProjectiles.Add(new ProjectileTracker(projectile));
	}

	public void UnregisterProjectile(RangeWeapon projectile)
	{
		foreach (ProjectileTracker activeProjectile in activeProjectiles)
		{
			if (activeProjectile.projectile == projectile)
			{
				activeProjectiles.Remove(activeProjectile);
				break;
			}
		}
	}

	public bool DelayEndOfGame()
	{
		foreach (ProjectileTracker activeProjectile in activeProjectiles)
		{
			if (activeProjectile.projectile != null && activeProjectile.projectile.WillDelayEndOfGame())
			{
				return true;
			}
		}
		return false;
	}

	private void Update()
	{
		foreach (ProjectileTracker activeProjectile in activeProjectiles)
		{
			Draw(activeProjectile.projectile);
		}
	}

	public void CleanUp()
	{
		activeProjectiles.Clear();
	}

	private void MakeMeshes()
	{
		_indicatorMaterial = Object.Instantiate(Resources.Load("Materials/offscreenIndicatorMaterial")) as Material;
		_indicatorMeshLeft = Misc.MakeQuadMesh(-1f, -0.5f, 0f, 0.5f);
		_indicatorMaterial.renderQueue = 5000;
	}

	private void Draw(RangeWeapon projectile)
	{
		int layer = LayerMask.NameToLayer("GUI");
		Camera currentCamera = UICamera.currentCamera;
		float num = currentCamera.orthographicSize * 0.15f;
		Vector3 s = new Vector3(num, num, num);
		Vector3 vector = Camera.main.WorldToViewportPoint(projectile.transform.position);
		vector.z = -5f;
		Vector3 position = vector;
		Vector3 euler = right;
		if (!new Rect(0f, 0f, 1f, 1f).Contains(vector))
		{
			if (vector.y > 1f && vector.x < 0f)
			{
				position.y = 1f;
				position.x = 0f;
				euler = leftUp;
			}
			else if (vector.y > 1f && vector.x > 1f)
			{
				position.y = 1f;
				position.x = 1f;
				euler = rightUp;
			}
			else if (vector.y > 1f)
			{
				position.y = 1f;
				euler = up;
			}
			else if (vector.x > 1f)
			{
				position.x = 1f;
			}
			else if (vector.x < 0f)
			{
				position.x = 0f;
				euler = left;
			}
			Graphics.DrawMesh(_indicatorMeshLeft, Matrix4x4.TRS(currentCamera.ViewportToWorldPoint(position), Quaternion.Euler(euler), s), _indicatorMaterial, layer, currentCamera);
		}
	}
}
