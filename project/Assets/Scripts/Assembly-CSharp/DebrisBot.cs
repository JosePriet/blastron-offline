using System.Collections;
using SmoothMoves;
using UnityEngine;

public class DebrisBot : MonoBehaviour
{
	public enum MainExpression
	{
		kGeneric = 0,
		kBored = 1,
		kCeleb2 = 2,
		kEmbarassed = 3,
		kPuking = 4,
		kWoohoo = 5,
		kStunned = 6
	}

	private const int _idleAnimCount = 5;

	public bool _head;

	public bool _dead;

	public bool _randomStart = true;

	public bool _randomUpdate = true;

	public float _blinkIntervalMin;

	public float _blinkIntervalMax = 1f;

	public MainExpression _main;

	private Vector3 _vel;

	private Quaternion _rot;

	private readonly string _expressionObjectName = "Expression";

	private string _expression;

	private BoneAnimation _expressionAnim;

	private string _idleAnim;

	private void Start()
	{
		if (_head)
		{
			if (!_dead)
			{
				Transform transform = base.transform.FindChild(_expressionObjectName);
				_expressionAnim = transform.GetComponent<BoneAnimation>();
				_idleAnim = "idle_" + Random.Range(0, 5);
				_expressionAnim.Play(_idleAnim);
			}
			else
			{
				_expressionAnim.Play("dead");
			}
		}
	}

	private void Update()
	{
		base.transform.position += _vel * Time.smoothDeltaTime;
		base.transform.localRotation *= _rot;
		if (_expressionAnim != null)
		{
			_expressionAnim.Play(_idleAnim, PlayMode.StopSameLayer);
		}
		Vector3 vector = SinglePlayerUI.WorldCam.WorldToViewportPoint(base.transform.position);
		vector.x = (2f * vector.x - 1f) * 1f;
		vector.y = (2f * vector.y - 1f) * 1f;
		vector.z = 0f;
		Color color = base.renderer.material.color;
		float a = (color.a = Mathf.Clamp01(vector.magnitude));
		base.renderer.material.color = color;
		color = Color.white;
		color.a = a;
		_expressionAnim.SetMeshColor(color);
	}

	public void InitMovement()
	{
		_vel.x = Random.Range(-0.2f, 0.2f);
		_vel.y = Random.Range(-0.2f, 0.2f);
		_rot = Quaternion.Euler(0f, 0f, Random.value * 0.5f + 0.5f);
	}

	private IEnumerator Blink()
	{
		while (true)
		{
			yield return new WaitForSeconds(Random.Range(_blinkIntervalMin, _blinkIntervalMax));
			if (_expressionAnim != null)
			{
				_expressionAnim.Play("blink", PlayMode.StopSameLayer);
			}
		}
	}

	private void OnEnable()
	{
		if (_head && !_dead)
		{
			if (_expressionAnim != null)
			{
				_idleAnim = "idle_" + Random.Range(0, 5);
			}
			StartCoroutine(Blink());
		}
	}
}
