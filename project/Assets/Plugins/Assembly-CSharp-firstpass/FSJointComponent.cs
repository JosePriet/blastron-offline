using System.Collections;
using UnityEngine;

public class FSJointComponent : MonoBehaviour
{
	public FSBodyComponent BodyA;

	public FSBodyComponent BodyB;

	public bool CollideConnected;

	private IEnumerator Start()
	{
		yield return new WaitForEndOfFrame();
		InitJoint();
	}

	public virtual void InitJoint()
	{
	}

	public virtual void Update()
	{
	}

	public virtual void OnDrawGizmos()
	{
	}
}
