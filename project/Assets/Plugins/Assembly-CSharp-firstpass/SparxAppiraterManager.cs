using EB.Sparx;
using UnityEngine;

public class SparxAppiraterManager : MonoBehaviour
{
	public AppiraterListener Listener { get; set; }

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void OnDisplay(string i)
	{
		if (Listener != null)
		{
			Listener.OnDisplay();
		}
	}

	private void OnDeclined(string i)
	{
		if (Listener != null)
		{
			Listener.OnDeclined();
		}
	}

	private void OnRated(string i)
	{
		if (Listener != null)
		{
			Listener.OnRated();
		}
	}

	private void OnRemind(string i)
	{
		if (Listener != null)
		{
			Listener.OnRemind();
		}
	}
}
