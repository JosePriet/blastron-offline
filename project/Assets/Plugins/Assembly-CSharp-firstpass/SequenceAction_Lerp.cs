using EB;
using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/Lerp")]
public class SequenceAction_Lerp : EB.Sequence.Runtime.Action
{
	[Variable(ExpectedType = typeof(GameObject))]
	public Variable From = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable To = Variable.Null;

	[Variable(ExpectedType = typeof(GameObject))]
	public Variable Target = Variable.Null;

	[Property]
	public float Duration = 1f;

	[Property]
	public bool useScale;

	[Property]
	public float scaleTarget = 1f;

	private float _t;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Start()
	{
		_t = 0f;
		Duration = Mathf.Max(Duration, 0.01f);
		Activate();
	}

	public override bool Update()
	{
		_t += UnityEngine.Time.deltaTime;
		float num = Mathf.Clamp01(_t / Duration);
		Lerp(num);
		if (num > 0.99f)
		{
			Finished.Invoke();
			return false;
		}
		return true;
	}

	private void Lerp(float t)
	{
		GameObject gameObject = (GameObject)From.Value;
		GameObject gameObject2 = (GameObject)To.Value;
		GameObject gameObject3 = (GameObject)Target.Value;
		if (gameObject3 != null && gameObject != null && gameObject2 != null)
		{
			gameObject3.transform.position = Vector3.Lerp(gameObject.transform.position, gameObject2.transform.position, t);
			gameObject3.transform.rotation = Quaternion.Slerp(gameObject.transform.rotation, gameObject2.transform.rotation, t);
		}
		if (useScale)
		{
			if (gameObject3 != null && gameObject != null)
			{
				gameObject3.transform.localScale = Vector3.Lerp(gameObject.transform.localScale, new Vector3(scaleTarget, scaleTarget, scaleTarget), t);
			}
			else
			{
				EB.Debug.Log("SequenceAction_Lerp - scale cannot be used - target or from missing");
			}
		}
	}

	public override void Serialize(BitStream bs)
	{
		bs.Serialize(ref _t);
	}
}
