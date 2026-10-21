using System;
using UnityEngine;

[Serializable]
public class EBUI_TransitionInfo : MonoBehaviour
{
	public EBUI_Transition.TransitionPropertyInfo transition_intro = new EBUI_Transition.TransitionPropertyInfo();

	public EBUI_Transition.TransitionPropertyInfo transition_outro = new EBUI_Transition.TransitionPropertyInfo();

	private void Awake()
	{
		float z = base.gameObject.transform.localPosition.z;
		base.gameObject.transform.localPosition = new Vector3(Screen.width * 2, Screen.height * 2, z);
	}
}
