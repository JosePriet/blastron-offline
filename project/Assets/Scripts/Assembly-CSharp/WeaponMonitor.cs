using System.Collections;
using EB;
using Microsoft.Xna.Framework;
using UnityEngine;

public class WeaponMonitor : MonoBehaviour
{
	private class GraemesQuickieGradient
	{
		private Color _c0;

		private Color _c1;

		public GraemesQuickieGradient(Color c0, Color c1)
		{
			_c0 = c0;
			_c1 = c1;
		}

		public Color Evaluate(float t)
		{
			return Color.Lerp(_c0, _c1, t);
		}
	}

	private struct VisEntry
	{
		public enum Type
		{
			kFadeMesh = 0
		}

		private delegate void UpdateFn();

		private Material _mat;

		private Mesh _mesh;

		private float _radius;

		private GraemesQuickieGradient _colourGradient;

		private AnimationCurve _scaleCurve;

		private float _t;

		private Vector3 _pos;

		private float _duration;

		private Type _eType;

		private Matrix4x4 _mtx44;

		public static int GUI_Layer { get; set; }

		public bool Running { get; private set; }

		public void Init(Type eType, Vector3 pos, float radius, Material mat, Mesh mesh, GraemesQuickieGradient colour, AnimationCurve scale, float duration)
		{
			_pos = pos;
			_mesh = mesh;
			_mat = mat;
			_colourGradient = colour;
			_scaleCurve = scale;
			_radius = radius;
			_pos = pos;
			_mat.color = _colourGradient.Evaluate(0f);
			_t = (_duration = duration);
			_eType = eType;
			Running = true;
			Vector3 s = Vector3.one * scale.Evaluate(0f) * 2f * radius;
			_mtx44 = Matrix4x4.TRS(pos, Quaternion.identity, s);
		}

		public void Update()
		{
			if (Running && _eType == Type.kFadeMesh)
			{
				FadeMesh();
			}
		}

		private void FadeMesh()
		{
			_t -= UnityEngine.Time.smoothDeltaTime;
			if (_t < 0f)
			{
				Running = false;
				_mat.color = _colourGradient.Evaluate(1f);
				return;
			}
			float num = 1f - _t / _duration;
			_mat.color = _colourGradient.Evaluate(num);
			Vector3 s = Vector3.one * _scaleCurve.Evaluate(num) * 2f * _radius;
			_mtx44 = Matrix4x4.TRS(_pos, Quaternion.identity, s);
			Graphics.DrawMesh(_mesh, _mtx44, _mat, 0);
		}
	}

	private delegate void UpdateFn();

	private const float OriginalPointsPerSec = 250f;

	private const int _visCount = 20;

	private const float _visDuration = 5f;

	public Mesh _visRingMesh;

	public Mesh _visDiscMesh;

	public Material _visMaterial;

	public AnimationCurve _blastScale;

	public AnimationCurve _concussionScale;

	private Gradient g;

	public Color _blastDiscColour0;

	public Color _blastDiscColour1;

	public Color _blastRingColour0;

	public Color _blastRingColour1;

	public Color _concussionDiscColour0;

	public Color _concussionDiscColour1;

	public Color _concussionRingColour0;

	public Color _concussionRingColour1;

	private GraemesQuickieGradient _blastRingColour;

	private GraemesQuickieGradient _concussionRingColour;

	private GraemesQuickieGradient _blastDiscColour;

	private GraemesQuickieGradient _concussionDiscColour;

	private static WeaponMonitor _instance;

	private bool _visEnable;

	private float pointsPerSec = 250f;

	private VisEntry[] _visEntries;

	private GameObject _root;

	private int _points;

	private int _points1;

	public static WeaponMonitor Instance
	{
		get
		{
			if (_instance == null)
			{
				EB.Debug.LogError("WEAPONMONITOR IS NOT READY YET!");
			}
			return _instance;
		}
		private set
		{
			_instance = value;
		}
	}

	public GameObject Root
	{
		get
		{
			if (_root == null)
			{
				_root = new GameObject("WeaponsInFlight");
			}
			return _root;
		}
	}

	private bool _VisEnable
	{
		get
		{
			return _visEnable;
		}
		set
		{
			_visEnable = value;
			GameHUD.Instance.PointsAccumulatorEnable = _visEnable;
			if (_visEnable)
			{
				StopAllCoroutines();
				StartCoroutine(MineSweeper());
				SetPointsAccumulator(0);
			}
		}
	}

	private void Awake()
	{
		Instance = this;
		VisEntry.GUI_Layer = LayerMask.NameToLayer("GUI");
		_visEntries = new VisEntry[48];
		_blastRingColour = new GraemesQuickieGradient(_blastRingColour0, _blastRingColour1);
		_concussionRingColour = new GraemesQuickieGradient(_concussionRingColour0, _concussionRingColour1);
		_blastDiscColour = new GraemesQuickieGradient(_blastDiscColour0, _blastDiscColour1);
		_concussionDiscColour = new GraemesQuickieGradient(_concussionDiscColour0, _concussionDiscColour1);
	}

	public void Add(MonoBehaviour weapon)
	{
		weapon.transform.parent = Root.transform;
	}

	public void CleanUp()
	{
		Object.Destroy(_root);
	}

	public static void VisEnable(bool bEnable)
	{
		Instance._VisEnable = bEnable;
	}

	public void WeaponRangeQuery(Misc.FixtureRangeQueryCallback callback, FVector2 pos, float range)
	{
		foreach (Transform item in _root.transform)
		{
			RangeWeapon component = item.GetComponent<RangeWeapon>();
			if (component != null && component.MyBody != null)
			{
				callback(component.MyBody.FixtureList[0]);
			}
		}
	}

	private void Update()
	{
		if (_VisEnable)
		{
			for (int i = 0; i < _visEntries.Length; i++)
			{
				_visEntries[i].Update();
			}
			int points = _points;
			float num = pointsPerSec * UnityEngine.Time.smoothDeltaTime;
			_points = Mathf.Min(_points + (int)num, _points1);
			if (points != _points)
			{
				SetPointsAccumulator(_points);
				pointsPerSec *= 1.01f;
			}
			else if (pointsPerSec != 250f)
			{
				pointsPerSec = 250f;
			}
		}
	}

	private void SetPointsAccumulator(int points)
	{
		GameHUD.Instance.SetPointsAccumulator(Localizer.Format("ID_GAME_SCORING_POINTS", points));
	}

	public void AddPoints(int points)
	{
		_points1 += points;
	}

	public void Fire()
	{
		_points = (_points1 = 0);
		SetPointsAccumulator(_points);
	}

	public void Blast(Vector3 pos, float concussionRadius, float holeRadius)
	{
		Add(pos, concussionRadius, _visRingMesh, _concussionRingColour, _concussionScale);
		Add(pos, concussionRadius, _visDiscMesh, _concussionDiscColour, _concussionScale);
		pos.z -= 10f;
		Add(pos, holeRadius, _visRingMesh, _blastRingColour, _blastScale);
		Add(pos, holeRadius, _visDiscMesh, _blastDiscColour, _blastScale);
	}

	private void Add(Vector3 pos, float radius, Mesh mesh, GraemesQuickieGradient c, AnimationCurve scale)
	{
		int num = FirstFree();
		if (num >= 0)
		{
			_visEntries[num].Init(VisEntry.Type.kFadeMesh, pos, radius, new Material(_visMaterial), mesh, c, scale, 5f);
		}
	}

	private int FirstFree()
	{
		for (int i = 0; i < _visEntries.Length; i++)
		{
			if (!_visEntries[i].Running)
			{
				return i;
			}
		}
		return -1;
	}

	private IEnumerator MineSweeper()
	{
		while (true)
		{
			yield return new WaitForSeconds(10f);
			int count = 0;
			int candidateCount = 0;
			GameObject[] choppingBlock = new GameObject[5];
			for (int i = 0; i < Root.transform.childCount; i++)
			{
				Transform child = Root.transform.GetChild(i);
				RangeWeapon r = child.GetComponent<RangeWeapon>();
				if (r != null && (r._detonateOnPlayerContact || r._detonateWithinRange > 0f))
				{
					if (candidateCount < 5)
					{
						choppingBlock[candidateCount++] = r.gameObject;
					}
					count++;
				}
			}
			int toDestroy = Mathf.Min(count - candidateCount, 5);
			for (int j = 0; j < toDestroy; j++)
			{
				choppingBlock[j].transform.parent = null;
				Object.Destroy(choppingBlock[j]);
			}
		}
	}
}
