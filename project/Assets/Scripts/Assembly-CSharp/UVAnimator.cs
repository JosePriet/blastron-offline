using UnityEngine;

[ExecuteInEditMode]
public class UVAnimator : MonoBehaviour
{
	public int NumTilesU = 1;

	public int NumTilesV = 1;

	public float SpeedU = 1f;

	public float SpeedV = 1f;

	public string TextureName = "_MainTex";

	private float _tilingU = 1f;

	private float _tilingV = 1f;

	public bool bRandomize;

	private void Start()
	{
		_tilingU = 1f / (float)NumTilesU;
		_tilingV = 1f / (float)NumTilesV;
	}

	private void Update()
	{
		if (!base.renderer)
		{
			base.enabled = false;
			return;
		}
		Material sharedMaterial = base.renderer.sharedMaterial;
		if (sharedMaterial != null)
		{
			int num = NumTilesU * NumTilesV;
			int num2 = (int)(Time.timeSinceLevelLoad * SpeedU) % num;
			if (bRandomize)
			{
				num2 = Random.Range(0, num);
			}
			int num3 = num2 % NumTilesU;
			int num4 = num2 / NumTilesU;
			sharedMaterial.SetTextureScale(TextureName, new Vector2(_tilingU, _tilingV));
			sharedMaterial.SetTextureOffset(TextureName, new Vector2(_tilingU * (float)num3, _tilingV * (float)num4));
		}
	}
}
