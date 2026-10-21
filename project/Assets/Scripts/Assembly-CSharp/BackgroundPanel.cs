using EB;
using UnityEngine;

public class BackgroundPanel : MonoBehaviour
{
	public GameObject selectedState;

	public GameObject unselectedState;

	public void Select()
	{
		Util.SetActiveRecursive(selectedState, true);
		Util.SetActiveRecursive(unselectedState, false);
	}

	public void Unselect()
	{
		Util.SetActiveRecursive(selectedState, false);
		Util.SetActiveRecursive(unselectedState, true);
	}
}
