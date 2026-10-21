using System.Collections;
using EB;
using SmoothMoves;
using UnityEngine;

public class CacDisplay : MonoBehaviour
{
	public delegate void SpawnPlayerComplete();

	public string[] _idleAnims = new string[2] { "bot_bored_polish_head_idle", "bot_yoga_poses_idle" };

	public float minIdleTime = 10f;

	public float maxIdleTime = 20f;

	private BoneAnimation _smCharacter;

	private float timer;

	public bool AutoSpawn = true;

	public bool PlayBoredAnimations = true;

	public bool FixQueue;

	public CharacterDB.Recipe Recipe;

	private CharacterDB.Recipe _spawned;

	private GameObject _instance;

	private bool _swapFace;

	private float faceTimer;

	public float faceDuration = 5f;

	private bool _hasExpressions;

	private bool _inSpawn;

	private Shader _replacement;

	private bool expressionSwapped;

	public bool SwapFace
	{
		get
		{
			return _swapFace;
		}
		set
		{
			if (!value && _hasExpressions && (bool)_smCharacter)
			{
				_smCharacter.RestoreBoneTexture("Facial expression");
			}
			_swapFace = value;
		}
	}

	public void UpdatePlayer()
	{
		CharacterManager.Instance.UpdateLocalCharacter();
	}

	public void LocalPlayerUpdated()
	{
		if (Recipe == null)
		{
			SpawnPlayer();
		}
	}

	private void Start()
	{
		timer = Random.Range(minIdleTime, maxIdleTime);
		faceTimer = faceDuration;
		if (AutoSpawn)
		{
			SpawnPlayer();
		}
	}

	private void ReturnToPool()
	{
		if (_instance != null)
		{
			CharacterDB.ReturnToPool(_instance);
			_instance = null;
			_smCharacter = null;
		}
	}

	private void OnDestroy()
	{
		ReturnToPool();
	}

	private bool HasExpressions()
	{
		if (_smCharacter != null)
		{
			Material[] mMaterials = _smCharacter.mMaterials;
			Material[] array = mMaterials;
			foreach (Material material in array)
			{
				if (material.name == "empty_expressions")
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void OnOpenPopup()
	{
		if (_spawned != null)
		{
			SpawnPlayer();
		}
	}

	private void OnHover(bool active)
	{
		if (!active)
		{
			ReturnToPool();
		}
	}

	private void OnClosePopup()
	{
		ReturnToPool();
	}

	public void SpawnPlayer()
	{
		SpawnPlayer(null);
	}

	public void SpawnPlayer(SpawnPlayerComplete fnSpawnComplete)
	{
		CharacterDB.Recipe recipe = Recipe ?? CharacterManager.Instance.LocalPlayer.recipe;
		bool play = false;
		if (_instance == null && !_inSpawn)
		{
			play = true;
		}
		else if (_spawned != null && _spawned.Equals(recipe))
		{
			return;
		}
		_spawned = recipe.Clone();
		_inSpawn = true;
		CharacterDB.Update(_instance, recipe, false, SafeAction.Wrap(this, (GameObject player) =>
		{
			_inSpawn = false;
			if (player != null)
			{
				if (player != _instance)
				{
					ReturnToPool();
				}
				_instance = player;
				player.transform.parent = base.transform;
				_smCharacter = player.GetComponent<BoneAnimation>();
				_smCharacter.HideBone("weapon", true);
				player.transform.localPosition = Vector3.zero;
				player.transform.localScale = Vector3.one;
				player.transform.localRotation = Quaternion.identity;
				int layer = base.transform.gameObject.layer;
				Util.SetLayerRecursive(player, layer);
				player.SetActive(true);
				if (play)
				{
					StartCoroutine(PlayCacAnimation());
				}
				_hasExpressions = HasExpressions();
				if (fnSpawnComplete != null)
				{
					fnSpawnComplete();
				}
			}
		}));
	}

	private void SetQueue()
	{
		if (_instance == null)
		{
			return;
		}
		if (_replacement == null)
		{
			_replacement = Shader.Find("Unlit/Transparent Colored");
		}
		Renderer renderer = _instance.renderer;
		if (!(renderer != null))
		{
			return;
		}
		Material[] materials = renderer.materials;
		foreach (Material material in materials)
		{
			if ((bool)material)
			{
				material.shader = _replacement;
			}
		}
	}

	private void LateUpdate()
	{
		if (FixQueue)
		{
			SetQueue();
		}
	}

	private IEnumerator PlayCacAnimation()
	{
		yield return new WaitForFixedUpdate();
		if (_smCharacter != null)
		{
			_smCharacter.Play("bot_cac_idle", PlayMode.StopAll);
		}
	}

	private void Update()
	{
		if (_smCharacter == null)
		{
			return;
		}
		timer -= UnityEngine.Time.deltaTime;
		if (timer <= 0f && PlayBoredAnimations)
		{
			_smCharacter.RestoreBoneTexture("Facial expression");
			expressionSwapped = false;
			timer = Random.Range(minIdleTime, maxIdleTime);
			_smCharacter.Play(Misc.RandomString(_idleAnims), PlayMode.StopSameLayer);
		}
		if (!_smCharacter.IsPlaying("bot_cac_idle") || !SwapFace || !_hasExpressions)
		{
			return;
		}
		faceTimer -= UnityEngine.Time.deltaTime;
		if (faceTimer <= 0f)
		{
			if (expressionSwapped)
			{
				_smCharacter.RestoreBoneTexture("Facial expression");
			}
			else
			{
				UnityEngine.Debug.LogError("DOING BORED");
				_smCharacter.SwapBoneTexture("Facial expression", "robot_expressions", "generic_expression", "robot_expressions", "bored_expression");
			}
			expressionSwapped = !expressionSwapped;
			faceTimer = faceDuration;
		}
	}
}
