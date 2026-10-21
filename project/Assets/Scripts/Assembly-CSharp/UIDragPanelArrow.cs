using UnityEngine;

public class UIDragPanelArrow : MonoBehaviour
{
	public enum Direction
	{
		Left = 0,
		Right = 1,
		Up = 2,
		Down = 3
	}

	public UIDraggablePanelSnap panel;

	public Direction direction;

	private void OnClick()
	{
		if (panel != null)
		{
			switch (direction)
			{
			case Direction.Left:
				panel.SwipeLeft();
				break;
			case Direction.Right:
				panel.SwipeRight();
				break;
			}
		}
	}
}
