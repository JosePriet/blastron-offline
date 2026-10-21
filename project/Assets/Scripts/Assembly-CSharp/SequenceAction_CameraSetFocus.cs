using EB.Sequence;
using EB.Sequence.Runtime;
using UnityEngine;

[MenuItem(Path = "Actions/CameraSetFocus")]
public class SequenceAction_CameraSetFocus : Action
{
	public enum TargetType
	{
		Locator = 0,
		Player = 1,
		AI = 2
	}

	[Property(MapTo = typeof(TargetType))]
	public int type;

	[Property]
	public string target;

	[Property]
	public Vector3 targetOffset;

	[Property]
	public float Camera_OrthoFracCatchup;

	[Property]
	public float zoom;

	private CameraController cc;

	[Trigger]
	public Trigger Finished = new Trigger();

	[Entry]
	public void Process()
	{
		cc = Camera.mainCamera.GetComponent<CameraController>();
		if (cc != null)
		{
			GameObject gameObject = null;
			switch (type)
			{
			case 0:
				gameObject = GameObject.Find(target);
				break;
			case 1:
				gameObject = LevelData.Instance.GetPlayerObject();
				break;
			case 2:
			{
				Player aiPlayer = LevelData.Instance.GetAiPlayer(target);
				if (aiPlayer != null)
				{
					gameObject = aiPlayer.gameObject;
				}
				else
				{
					Finished.Invoke();
				}
				break;
			}
			default:
				Debug.LogError("Invalid type");
				break;
			}
			if (gameObject != null)
			{
				CameraController.SequenceCameraSettings sequenceCameraSettings = new CameraController.SequenceCameraSettings();
				sequenceCameraSettings.focusPoint = gameObject.transform.position + targetOffset;
				sequenceCameraSettings.orthoFracCatchup = Camera_OrthoFracCatchup;
				sequenceCameraSettings.zoom = zoom;
				cc.SendMessage("Sequence_CameraOverride", sequenceCameraSettings);
				Activate();
			}
			else
			{
				Debug.LogError("No camera target!");
				Finished.Invoke();
			}
		}
		else
		{
			Debug.LogError("Unable to find camera controller!");
			Finished.Invoke();
		}
	}

	public override bool Update()
	{
		if (cc != null && !cc.IsCameraMoving())
		{
			Finished.Invoke();
			return false;
		}
		return true;
	}
}
