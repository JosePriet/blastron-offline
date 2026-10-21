using System;
using EB;
using UnityEngine;

public class WeaponSelectHudButton : MonoBehaviour
{
	private Player player;

	private bool isDragging;

	private void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(base.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(HandleClick));
	}

	private void Start()
	{
		player = LevelData.Instance.GetPlayer();
	}

	private void HandleClick(GameObject caller)
	{
		bool flag = false;
		CameraController component = Camera.main.GetComponent<CameraController>();
		if (component != null)
		{
			flag = component.IntroRunning;
		}
		if (!isDragging && !ScreenManager.Instance.PopupOpen && !flag && !player.CurrentMove.Moving)
		{
			EB.Debug.Log("WeaponSelectHudButton > HandleClick ");
			GameHUD.Instance.ToggleSubscreen(HUDSubscreenManager.Subscreen.WeaponInventory);
		}
	}

	private void Update()
	{
		if (BlastronGamepad.Instance.SwitchWeaponPressed())
		{
			HandleClick(base.gameObject);
		}
	}
}
