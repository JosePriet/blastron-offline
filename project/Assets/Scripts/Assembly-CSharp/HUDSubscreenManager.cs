using System;
using EB;
using UnityEngine;

public class HUDSubscreenManager : MonoBehaviour
{
	public enum Subscreen
	{
		WeaponInventory = 0,
		BoostInventory = 1,
		Chat = 2,
		None = 3
	}

	private enum State
	{
		Starting = 0,
		Animating = 1,
		Finishing = 2,
		Finished = 3,
		Idle = 4,
		Dragging = 5
	}

	private const float animationDuration = 0.2f;

	[NonSerialized]
	public StoreWeaponInGameUI storeWeaponInGameUi;

	[NonSerialized]
	public StoreBoostInGameUI storeBoostInGameUi;

	[NonSerialized]
	public GameObject weaponUiContainer;

	[NonSerialized]
	public GameObject boostUiContainer;

	public bool _animating;

	private Subscreen currentSubscreen = Subscreen.None;

	private State state = State.Idle;

	private Vector2 moveControlOriginalPosition = Vector2.zero;

	private Vector2 moveControlOffscreenPosition = Vector2.zero;

	private Vector2 moveControlStartPosition = Vector2.zero;

	private Vector2 moveControlTargetPosition = Vector2.zero;

	private Vector2 aimControlOriginalPosition = Vector2.zero;

	private Vector2 aimControlOffscreenPosition = Vector2.zero;

	private Vector2 aimControlStartPosition = Vector2.zero;

	private Vector2 aimControlTargetPosition = Vector2.zero;

	private Vector2 bottomControlOriginalPosition = Vector2.zero;

	private Vector2 bottomControlHigherPosition = Vector2.zero;

	private Vector2 bottomControlStartPosition = Vector2.zero;

	private Vector2 bottomControlTargetPosition = Vector2.zero;

	private UIAnchor moveAnchor;

	private UIAnchor aimAnchor;

	private UIAnchor bottomAnchor;

	private UIPanel uiPanel;

	private GameObject closeSubscreenInput;

	private float animStartTime;

	private float animEndTime;

	public void ToggleSubScreen(Subscreen subscreen)
	{
		if (state != State.Idle)
		{
			return;
		}
		if ((currentSubscreen == Subscreen.WeaponInventory && subscreen == Subscreen.BoostInventory) || (currentSubscreen == Subscreen.BoostInventory && subscreen == Subscreen.WeaponInventory))
		{
			GetScreenBehaviour(currentSubscreen).SendMessage("Close");
			GetScreenContainer(currentSubscreen).SetActive(false);
			currentSubscreen = subscreen;
			GetScreenContainer(currentSubscreen).SetActive(true);
			GetScreenBehaviour(currentSubscreen).SendMessage("Open");
			return;
		}
		EB.Debug.Log("> HUDSubscreenManager > ToggleSubScreen() > subscreen: " + subscreen);
		if (subscreen == currentSubscreen)
		{
			subscreen = Subscreen.None;
			EB.Debug.Log("> HUDSubscreenManager > ToggleSubScreen() > toggled back to none.");
		}
		currentSubscreen = subscreen;
		if (currentSubscreen == Subscreen.None)
		{
			moveControlTargetPosition = moveControlOriginalPosition;
			aimControlTargetPosition = aimControlOriginalPosition;
			bottomControlTargetPosition = bottomControlOriginalPosition;
			closeSubscreenInput.SetActive(false);
		}
		else
		{
			moveControlTargetPosition = moveControlOffscreenPosition;
			aimControlTargetPosition = aimControlOffscreenPosition;
			bottomControlTargetPosition = ((currentSubscreen == Subscreen.Chat) ? bottomControlOriginalPosition : bottomControlHigherPosition);
			closeSubscreenInput.SetActive(true);
		}
		base.enabled = true;
	}

	private void Start()
	{
		base.enabled = false;
		EB.Debug.Log("> HUDSubscreenManager > Start()");
		GameObject objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Move");
		moveAnchor = Util.FindComponent<UIAnchor>(objectExactMatch);
		moveControlOriginalPosition = moveAnchor.relativeOffset;
		moveControlOffscreenPosition = moveControlOriginalPosition;
		moveControlOffscreenPosition.x *= -1f;
		moveControlOffscreenPosition.y += 0.2f;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "Aim");
		aimAnchor = Util.FindComponent<UIAnchor>(objectExactMatch);
		aimControlOriginalPosition = aimAnchor.relativeOffset;
		aimControlOffscreenPosition = aimControlOriginalPosition;
		aimControlOffscreenPosition.x *= -1f;
		aimControlOffscreenPosition.y += 0.2f;
		objectExactMatch = Util.GetObjectExactMatch(base.gameObject, "BottomScreenAnchor");
		bottomAnchor = Util.FindComponent<UIAnchor>(objectExactMatch);
		bottomControlOriginalPosition = bottomAnchor.relativeOffset;
		bottomControlHigherPosition = bottomControlOriginalPosition;
		bottomControlHigherPosition.y = 0.2f;
		uiPanel = Util.FindComponent<UIPanel>(base.gameObject);
		closeSubscreenInput = Util.GetObjectExactMatch(base.gameObject, "CloseSubscreenInput");
		UIEventListener uIEventListener = UIEventListener.Get(closeSubscreenInput);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnCloseSubscreen));
		storeWeaponInGameUi.Close();
		weaponUiContainer.SetActive(false);
		storeBoostInGameUi.Close();
		boostUiContainer.SetActive(false);
	}

	private void OnEnable()
	{
		EB.Debug.Log("> HUDSubscreenManager > OnEnable()");
		if (uiPanel != null)
		{
			uiPanel.widgetsAreStatic = false;
			state = State.Starting;
		}
	}

	private void OnDisable()
	{
		EB.Debug.Log("> HUDSubscreenManager > OnDisable()");
		if (uiPanel != null)
		{
			uiPanel.widgetsAreStatic = true;
			state = State.Idle;
		}
	}

	private void Update()
	{
		switch (state)
		{
		case State.Starting:
			StartAnimation();
			break;
		case State.Animating:
			UpdateAnimation();
			break;
		case State.Finishing:
			FinishAnimation();
			break;
		case State.Finished:
			base.enabled = false;
			break;
		case State.Idle:
			break;
		}
	}

	private void StartAnimation()
	{
		_animating = true;
		aimControlStartPosition = aimAnchor.relativeOffset;
		moveControlStartPosition = moveAnchor.relativeOffset;
		bottomControlStartPosition = bottomAnchor.relativeOffset;
		moveAnchor.disableWhenReady = false;
		moveAnchor.enabled = true;
		aimAnchor.disableWhenReady = false;
		aimAnchor.enabled = true;
		bottomAnchor.disableWhenReady = false;
		bottomAnchor.enabled = true;
		animStartTime = UnityEngine.Time.timeSinceLevelLoad;
		animEndTime = animStartTime + 0.2f;
		state = State.Animating;
		if (currentSubscreen == Subscreen.WeaponInventory)
		{
			weaponUiContainer.SetActive(true);
			storeWeaponInGameUi.Open();
			boostUiContainer.SetActive(false);
		}
		else if (currentSubscreen == Subscreen.BoostInventory)
		{
			boostUiContainer.SetActive(true);
			storeBoostInGameUi.Open();
			weaponUiContainer.SetActive(false);
		}
	}

	private void UpdateAnimation()
	{
		float num = Mathf.InverseLerp(animStartTime, animEndTime, UnityEngine.Time.timeSinceLevelLoad);
		UpdateOffsets(num);
		if (num >= 1f)
		{
			state = State.Finishing;
		}
	}

	private void FinishAnimation()
	{
		_animating = false;
		aimAnchor.relativeOffset = aimControlTargetPosition;
		moveAnchor.relativeOffset = moveControlTargetPosition;
		bottomAnchor.relativeOffset = bottomControlTargetPosition;
		moveAnchor.disableWhenReady = true;
		moveAnchor.enabled = false;
		aimAnchor.disableWhenReady = true;
		aimAnchor.enabled = false;
		bottomAnchor.disableWhenReady = true;
		bottomAnchor.enabled = false;
		state = State.Finished;
		if (currentSubscreen == Subscreen.None)
		{
			storeWeaponInGameUi.Close();
			weaponUiContainer.SetActive(false);
			storeBoostInGameUi.Close();
			boostUiContainer.SetActive(false);
		}
		else if (currentSubscreen == Subscreen.WeaponInventory)
		{
			storeWeaponInGameUi.MakeFaster();
		}
		else if (currentSubscreen == Subscreen.BoostInventory)
		{
			storeBoostInGameUi.MakeFaster();
		}
	}

	private void UpdateOffsets(float completionRatio)
	{
		completionRatio = Mathf.Clamp(completionRatio, 0f, 1f);
		aimAnchor.relativeOffset = Vector2.Lerp(aimControlStartPosition, aimControlTargetPosition, completionRatio);
		moveAnchor.relativeOffset = Vector2.Lerp(moveControlStartPosition, moveControlTargetPosition, completionRatio);
		bottomAnchor.relativeOffset = Vector2.Lerp(bottomControlStartPosition, bottomControlTargetPosition, completionRatio);
	}

	private void OnCloseSubscreen(GameObject button, bool pressed)
	{
		EB.Debug.Log("> HUDSubscreenManager > OnCloseSubscreen > pressed: " + pressed);
		if (pressed)
		{
			ToggleSubScreen(Subscreen.None);
		}
	}

	private MonoBehaviour GetScreenBehaviour(Subscreen s)
	{
		switch (s)
		{
		case Subscreen.BoostInventory:
			return storeBoostInGameUi;
		case Subscreen.WeaponInventory:
			return storeWeaponInGameUi;
		default:
			EB.Debug.LogError("HUDSubscreenManager > GetScreenBehaviour > missing case for type: " + s);
			return null;
		}
	}

	private GameObject GetScreenContainer(Subscreen s)
	{
		switch (s)
		{
		case Subscreen.BoostInventory:
			return boostUiContainer;
		case Subscreen.WeaponInventory:
			return weaponUiContainer;
		default:
			EB.Debug.LogError("HUDSubscreenManager > GetScreenContainer > missing case for type: " + s);
			return null;
		}
	}
}
