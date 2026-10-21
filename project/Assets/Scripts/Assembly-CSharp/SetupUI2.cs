using System.Collections;
using UnityEngine;

public class SetupUI2 : MonoBehaviour
{
	private static SetupUI2 _this;

	public static SetupUI2 Instance
	{
		get
		{
			return _this;
		}
	}

	private void Awake()
	{
		_this = this;
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private IEnumerator Start()
	{
		base.transform.position = new Vector3(1000f, 1000f, 1000f);
		Object.DontDestroyOnLoad(base.gameObject);
		yield break;
	}
}
