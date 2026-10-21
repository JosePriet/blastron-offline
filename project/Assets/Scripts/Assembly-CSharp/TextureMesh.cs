using UnityEngine;

public class TextureMesh : MonoBehaviour
{
	public Texture2D texture;

	public Vector2 size;

	public Material material;

	private void Start()
	{
		MeshFilter meshFilter = base.gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		Mesh mesh = new Mesh();
		Vector2 vector = size * -0.5f;
		mesh.vertices = new Vector3[4]
		{
			new Vector3(vector.x, vector.y, 0f),
			new Vector3(size.x + vector.x, vector.y, 0f),
			new Vector3(size.x + vector.x, size.y + vector.y, 0f),
			new Vector3(vector.x, size.y + vector.y, 0f)
		};
		mesh.uv = new Vector2[4]
		{
			new Vector2(0f, 0f),
			new Vector2(1f, 0f),
			new Vector2(1f, 1f),
			new Vector2(0f, 1f)
		};
		mesh.triangles = new int[6] { 0, 3, 2, 2, 1, 0 };
		meshFilter.sharedMesh = mesh;
		Material material = new Material(this.material);
		material.mainTexture = texture;
		meshRenderer.sharedMaterial = material;
	}
}
