using System;
using UnityEngine;

[AddComponentMenu("NGUI/Internal/Draw Call")]
[ExecuteInEditMode]
public class UIDrawCall : MonoBehaviour
{
	public enum Clipping
	{
		None = 0,
		HardClip = 1,
		AlphaClip = 2,
		SoftClip = 3
	}

	private Transform mTrans;

	private Material mSharedMat;

	private Mesh mMesh0;

	private Mesh mMesh1;

	private MeshFilter mFilter;

	private MeshRenderer mRen;

	private Clipping mClipping;

	private Vector4 mClipRange;

	private Vector2 mClipSoft;

	private Material mClippedMat;

	private Material mDepthMat;

	private int[] mIndices;

	private Vector3[] mVerts;

	private Vector2[] mUVs;

	private Color[] mColors;

	private Vector3[] mNormals;

	private Vector4[] mTans;

	private bool mDepthPass;

	private bool mReset = true;

	private bool mEven = true;

	public bool depthPass
	{
		get
		{
			return mDepthPass;
		}
		set
		{
			if (mDepthPass != value)
			{
				mDepthPass = value;
				mReset = true;
			}
		}
	}

	public Transform cachedTransform
	{
		get
		{
			if (mTrans == null)
			{
				mTrans = base.transform;
			}
			return mTrans;
		}
	}

	public Material material
	{
		get
		{
			return mSharedMat;
		}
		set
		{
			mSharedMat = value;
		}
	}

	public int triangles
	{
		get
		{
			Mesh mesh = ((!mEven) ? mMesh1 : mMesh0);
			return (mesh != null) ? (mesh.vertexCount >> 1) : 0;
		}
	}

	public Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mClipping = value;
				mReset = true;
			}
		}
	}

	public Vector4 clipRange
	{
		get
		{
			return mClipRange;
		}
		set
		{
			mClipRange = value;
		}
	}

	public Vector2 clipSoftness
	{
		get
		{
			return mClipSoft;
		}
		set
		{
			mClipSoft = value;
		}
	}

	private Mesh GetMesh(ref bool rebuildIndices, int vertexCount)
	{
		mEven = !mEven;
		if (mEven)
		{
			if (mMesh0 == null)
			{
				mMesh0 = new Mesh();
				mMesh0.hideFlags = HideFlags.DontSave;
				mMesh0.name = "Mesh0 for " + mSharedMat.name;
				rebuildIndices = true;
			}
			else if (rebuildIndices || mMesh0.vertexCount != vertexCount)
			{
				rebuildIndices = true;
				mMesh0.Clear();
			}
			return mMesh0;
		}
		if (mMesh1 == null)
		{
			mMesh1 = new Mesh();
			mMesh1.hideFlags = HideFlags.DontSave;
			mMesh1.name = "Mesh1 for " + mSharedMat.name;
			rebuildIndices = true;
		}
		else if (rebuildIndices || mMesh1.vertexCount != vertexCount)
		{
			rebuildIndices = true;
			mMesh1.Clear();
		}
		return mMesh1;
	}

	private void UpdateMaterials()
	{
		if (mClipping != Clipping.None)
		{
			Shader shader = null;
			if (mClipping != Clipping.None)
			{
				string text = mSharedMat.shader.name;
				text = text.Replace(" (HardClip)", string.Empty);
				text = text.Replace(" (AlphaClip)", string.Empty);
				text = text.Replace(" (SoftClip)", string.Empty);
				if (mClipping == Clipping.HardClip)
				{
					shader = Shader.Find(text + " (HardClip)");
				}
				else if (mClipping == Clipping.AlphaClip)
				{
					shader = Shader.Find(text + " (AlphaClip)");
				}
				else if (mClipping == Clipping.SoftClip)
				{
					shader = Shader.Find(text + " (SoftClip)");
				}
				if (shader == null)
				{
					mClipping = Clipping.None;
				}
			}
			if (shader != null)
			{
				mClippedMat = new Material(mSharedMat);
				mClippedMat.hideFlags = HideFlags.DontSave;
				mClippedMat.shader = shader;
			}
		}
		else if (mClippedMat != null)
		{
			NGUITools.Destroy(mClippedMat);
			mClippedMat = null;
		}
		if (mDepthPass)
		{
			if (mDepthMat == null)
			{
				Shader shader2 = Shader.Find("Unlit/Depth Cutout");
				mDepthMat = new Material(shader2);
				mDepthMat.hideFlags = HideFlags.DontSave;
				mDepthMat.mainTexture = mSharedMat.mainTexture;
			}
		}
		else if (mDepthMat != null)
		{
			NGUITools.Destroy(mDepthMat);
			mDepthMat = null;
		}
		Material material = ((!(mClippedMat != null)) ? mSharedMat : mClippedMat);
		if (mDepthMat != null)
		{
			if (mRen.sharedMaterials == null || mRen.sharedMaterials.Length != 2 || !(mRen.sharedMaterials[1] == material))
			{
				mRen.sharedMaterials = new Material[2] { mDepthMat, material };
			}
		}
		else if (mRen.sharedMaterial != material)
		{
			mRen.sharedMaterials = new Material[1] { material };
		}
	}

	public void Set(BetterList<Vector3> verts, BetterList<Vector3> norms, BetterList<Vector4> tans, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		int size = verts.size;
		if (size > 0 && size == uvs.size && size == cols.size && size % 4 == 0)
		{
			if (mFilter == null)
			{
				mFilter = base.gameObject.GetComponent<MeshFilter>();
			}
			if (mFilter == null)
			{
				mFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			if (mRen == null)
			{
				mRen = base.gameObject.GetComponent<MeshRenderer>();
			}
			if (mRen == null)
			{
				mRen = base.gameObject.AddComponent<MeshRenderer>();
				UpdateMaterials();
			}
			if (verts.size < 65000)
			{
				int num = (size >> 1) * 3;
				bool rebuildIndices = mIndices == null || mIndices.Length != num;
				if (mVerts == null || size != mVerts.Length)
				{
					mVerts = new Vector3[size];
					mColors = new Color[size];
					mUVs = new Vector2[size];
				}
				if (norms != null && (mNormals == null || norms.size != mNormals.Length))
				{
					mNormals = new Vector3[norms.size];
				}
				if (tans != null && (mTans == null || tans.size != mTans.Length))
				{
					mTans = new Vector4[tans.size];
				}
				if (rebuildIndices)
				{
					mIndices = new int[num];
					int num2 = 0;
					for (int i = 0; i < size; i += 4)
					{
						mIndices[num2++] = i;
						mIndices[num2++] = i + 1;
						mIndices[num2++] = i + 2;
						mIndices[num2++] = i + 2;
						mIndices[num2++] = i + 3;
						mIndices[num2++] = i;
					}
				}
				Mesh mesh = GetMesh(ref rebuildIndices, verts.size);
				Array.Copy(verts.buffer, 0, mVerts, 0, mVerts.Length);
				Array.Copy(uvs.buffer, 0, mUVs, 0, mUVs.Length);
				Array.Copy(cols.buffer, 0, mColors, 0, mColors.Length);
				mesh.vertices = mVerts;
				mesh.colors = mColors;
				mesh.uv = mUVs;
				if (norms != null)
				{
					Array.Copy(norms.buffer, 0, mNormals, 0, mNormals.Length);
					mesh.normals = mNormals;
				}
				if (tans != null)
				{
					Array.Copy(tans.buffer, 0, mTans, 0, mTans.Length);
					mesh.tangents = mTans;
				}
				if (rebuildIndices)
				{
					mesh.triangles = mIndices;
				}
				mesh.RecalculateBounds();
				mFilter.mesh = mesh;
			}
			else
			{
				if (mFilter.mesh != null)
				{
					mFilter.mesh.Clear();
				}
				Debug.LogError("Too many vertices on one panel: " + verts.size);
			}
		}
		else
		{
			if (mFilter.mesh != null)
			{
				mFilter.mesh.Clear();
			}
			Debug.LogError("UIWidgets must fill the buffer with 4 vertices per quad. Found " + size);
		}
	}

	private void OnWillRenderObject()
	{
		if (mReset)
		{
			mReset = false;
			UpdateMaterials();
		}
		if (mClippedMat != null)
		{
			mClippedMat.mainTextureOffset = new Vector2((0f - mClipRange.x) / mClipRange.z, (0f - mClipRange.y) / mClipRange.w);
			mClippedMat.mainTextureScale = new Vector2(1f / mClipRange.z, 1f / mClipRange.w);
			Vector2 vector = new Vector2(1000f, 1000f);
			if (mClipSoft.x > 0f)
			{
				vector.x = mClipRange.z / mClipSoft.x;
			}
			if (mClipSoft.y > 0f)
			{
				vector.y = mClipRange.w / mClipSoft.y;
			}
			mClippedMat.SetVector("_ClipSharpness", vector);
		}
	}

	private void OnDestroy()
	{
		NGUITools.DestroyImmediate(mMesh0);
		NGUITools.DestroyImmediate(mMesh1);
		NGUITools.DestroyImmediate(mClippedMat);
		NGUITools.DestroyImmediate(mDepthMat);
	}
}
