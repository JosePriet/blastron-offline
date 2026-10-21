using SmoothMoves;
using UnityEngine;

public class Knight : MonoBehaviour
{
	private float _blinkTimeLeft;

	public BoneAnimation knight;

	public AudioSource swishSound;

	public AudioSource hitSound;

	public float minBlinkTime;

	public float maxBlinkTime;

	public Material oldMaterial;

	public Material newMaterial;

	public float speed;

	public Transform knightShadow;

	public BoneAnimation sparks;

	private void Start()
	{
		knight.SwapMaterial(oldMaterial, newMaterial);
		knight.RegisterColliderTriggerDelegate(ColliderTrigger);
		knight.RegisterUserTriggerDelegate(UserTrigger);
		knight.SwapBoneTexture("Weapon", "Weapons", "sword", "Weapons", "axe");
		knight.SwapTexture("Weapons", "sword_swish", "Weapons", "axe");
		knightShadow.parent = knight.GetBoneTransform("Root");
		knightShadow.localPosition = Vector3.zero;
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.LeftArrow))
		{
			knight.CrossFade("Stand");
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				knight.CrossFade("Stand");
				return;
			}
			if (!knight.IsPlaying("Walk"))
			{
				knight["Walk"].speed = 1f;
				knight.CrossFade("Walk");
			}
			knight.mLocalTransform.position += new Vector3(speed * Time.deltaTime, 0f, 0f);
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			if (Input.GetKey(KeyCode.RightArrow))
			{
				knight.CrossFade("Stand");
				return;
			}
			if (!knight.IsPlaying("Walk"))
			{
				knight["Walk"].speed = -1f;
				knight.CrossFade("Walk");
			}
			knight.mLocalTransform.position -= new Vector3(speed * Time.deltaTime, 0f, 0f);
		}
		if (Input.GetKeyDown(KeyCode.A))
		{
			knight.CrossFade("Attack");
		}
		if (Input.GetKeyDown(KeyCode.X))
		{
			knight.SwapBoneTexture("Weapon", "Weapons", "sword", "Weapons", "axe");
			knight.SwapBoneTexture("Weapon", "Weapons", "sword_swish", "Weapons", "axe");
		}
		if (Input.GetKeyDown(KeyCode.M))
		{
			knight.SwapBoneTexture("Weapon", "Weapons", "sword", "Weapons", "mace");
			knight.SwapBoneTexture("Weapon", "Weapons", "sword_swish", "Weapons", "mace");
		}
		if (Input.GetKeyDown(KeyCode.S))
		{
			knight.RestoreBoneTexture("Weapon");
		}
		if (Input.GetKeyDown(KeyCode.H))
		{
			knight.HideBone("Weapon", !knight.IsBoneHidden("Weapon"));
		}
		_blinkTimeLeft -= Time.deltaTime;
		if (_blinkTimeLeft <= 0f)
		{
			knight.Play("Blink");
			_blinkTimeLeft = Random.Range(minBlinkTime, maxBlinkTime);
		}
	}

	public void ColliderTrigger(ColliderTriggerEvent triggerEvent)
	{
		if (triggerEvent.boneName == "Weapon" && triggerEvent.triggerType == ColliderTriggerEvent.TRIGGER_TYPE.Enter)
		{
			hitSound.Play();
			sparks.mLocalTransform.position = triggerEvent.otherColliderClosestPointToBone;
			sparks.Play("Hit");
		}
	}

	public void UserTrigger(UserTriggerEvent triggerEvent)
	{
		if (triggerEvent.boneName == "Weapon")
		{
			swishSound.Play();
		}
	}
}
