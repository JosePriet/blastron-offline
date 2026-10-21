using System;
using System.Collections;
using System.Collections.Generic;
using EB;
using Microsoft.Xna.Framework;
using UnityEngine;

public class AISimpleStates : MonoBehaviour
{
	public AISimple _aiScript;

	public int _currentWeaponIndex;

	public void CreateStates()
	{
		_aiScript.AddState("StateAttackMove", "DoAttackMove", this);
		_aiScript.AddState("StateHideMove", "DoHideMove", this);
		_aiScript.AddState("StateHappyJumpBot", "DoHappyJumpBot", this);
		_aiScript.AddState("StateProjectileFiringSolution", "DoProjectileFiringSolution", this);
		_aiScript.AddState("StateThink", "DoThink", this);
		_aiScript.AddState("StateFire", "DoFire", this);
		_aiScript.AddState("StateMelee", "DoMelee", this);
		_aiScript.AddState("StateTeleport", "DoTeleport", this);
		_aiScript.AddState("StatePretendThink", "DoPretendThink", this);
		_aiScript.AddState("StateRandomFireFallback", "DoRandomFireFallback", this);
		_aiScript.AddState("StateCarpetBombFiringSoulution", "DoAirstrike", this);
		_aiScript.AddState("StateLaserSolution", "DoLaserFiringSolution", this);
		_aiScript.AddState("StateGoAfterCrate", "DoGoAfterCrate", this);
		_aiScript.AddState("StateShootDestructableObject", "DoDestructableObjectSolution", this);
		_aiScript.AddState("StateBotOnTopOfMe", "DoBotOnTopOfMe", this);
		_aiScript.AddState("StateNothing", "DoNothing", this);
	}

	public IEnumerator DoPretendThink()
	{
		yield return new WaitForSeconds(3f);
		_aiScript.SetState("StateThink");
	}

	public IEnumerator DoThink()
	{
		_aiScript._targetPlayer = null;
		if (_aiScript.BotOnTopOfMe())
		{
			_aiScript.SetState("StateBotOnTopOfMe");
			yield break;
		}
		if ((_aiScript._p.CurrentWeapon._eType != Weapon.Type.kMelee && !_aiScript.MeleeCanAtack()) || (!_aiScript._firedThisTurn && _aiScript._weaponSwitchesThisTurn <= 2) || _aiScript._p.CurrentWeapon.FriendlyName == "Teleport Grenade")
		{
			_aiScript._weaponSwitchesThisTurn++;
			StartCoroutine(_aiScript.GetNewWeaponFromArsenal());
		}
		else if (_aiScript.MeleeCanAtack() && !_aiScript._firedThisTurn)
		{
			_aiScript.SetState("StateMelee");
			yield break;
		}
		yield return new WaitForSeconds(0.5f);
		if (_aiScript.GetNearestCrate(out _aiScript._CrateTargetPos) && _aiScript._GoAfterCrateAttempts != 5)
		{
			_aiScript._GoAfterCrateAttempts++;
			_aiScript.SetState("StateGoAfterCrate");
		}
		else if (UnityEngine.Random.Range(0, 2) == 0 && !_aiScript._firedThisTurn && AISimple._destructablePhysicsObjects.Count > 0 && _aiScript._p.CurrentWeapon.FriendlyName != "Teleport Grenade")
		{
			_aiScript.SetState("StateShootDestructableObject");
		}
		else if (_aiScript.CanFire() && _aiScript._p.CurrentWeapon.FriendlyName != "Teleport Grenade" && !_aiScript._firedThisTurn)
		{
			_aiScript.SetState(_aiScript._currentWeaponFiringState());
		}
		else
		{
			_aiScript.SetState("StateHideMove");
		}
	}

	public IEnumerator FollowPath()
	{
		while (true)
		{
			bool currLeft = _aiScript._goLeft;
			bool currRight = _aiScript._goRight;
			_aiScript._goLeft = false;
			_aiScript._goRight = false;
			_aiScript._jump = false;
			_aiScript._stuckTimer.Update();
			int residentCellY = (int)(_aiScript._p.Position().Y / Navigation.Node.CellSize);
			if (!_aiScript.Paused && (!_aiScript.TooClose() || _aiScript.IsCurrent("StateHideMove")) && !object.ReferenceEquals(_aiScript._path, null))
			{
				AIHelp.PathMoveInstruction pmi;
				AIHelp.PathMoveStatus ePathStatus;
				_aiScript.NavPath = AIHelp.PathMove(_aiScript._p.Position(), _aiScript.NavPath, out pmi, _aiScript._p.Grounded, out ePathStatus);
				if (!object.ReferenceEquals(_aiScript._path, null))
				{
					Navigation.Node next = _aiScript._path.Next;
					switch (ePathStatus)
					{
					case AIHelp.PathMoveStatus.kOK:
						if (pmi.jump && !object.ReferenceEquals(_aiScript._path.Next, null) && Mathf.Abs(_aiScript._myPositionF2.X - _aiScript._path.Next.Position.X) < Navigation.JumpHorizontalMax)
						{
							_aiScript._jump = pmi.jump;
						}
						if (pmi.fall)
						{
							if (_aiScript._hitR)
							{
								if (pmi.right)
								{
									pmi.right = false;
								}
							}
							else
							{
								pmi.right |= currRight;
							}
							if (_aiScript._hitL)
							{
								if (pmi.left)
								{
									pmi.left = false;
								}
							}
							else
							{
								pmi.left |= currLeft;
							}
						}
						if (((pmi.right && _aiScript._hitR) || (pmi.left && _aiScript._hitL)) && !object.ReferenceEquals(next, null) && next._y > residentCellY)
						{
							_aiScript._jump = true;
						}
						_aiScript._goLeft = pmi.left;
						_aiScript._goRight = pmi.right;
						break;
					case AIHelp.PathMoveStatus.kPathLost:
						if (!_aiScript.TooClose())
						{
							Navigation.PathingResult eRes;
							_aiScript.NavPath = LevelData.Instance.Path(_aiScript._p.Position(), _aiScript._targetPositionF2, out eRes);
							AIHelp.PathProcess(_aiScript.NavPath);
						}
						break;
					}
				}
			}
			yield return new WaitForFixedUpdate();
		}
	}

	public IEnumerator DoProjectileFiringSolution()
	{
		_aiScript._aiming = true;
		float power = 0f;
		float theta;
		_aiScript._targetPlayer = _aiScript.BestFiringTarget(_aiScript._myPositionF2, out theta, out power);
		if (_aiScript._targetPlayer != null && !_aiScript._targetPlayer.IsDead())
		{
			theta = _aiScript.AddInnacuracyForShot(theta);
			_aiScript._firingDir.x = Mathf.Cos(theta);
			_aiScript._firingDir.y = Mathf.Sin(theta);
			_aiScript.FiringMag = power;
			_aiScript.SetState("StateFire");
		}
		else
		{
			_aiScript.SetState("StateAttackMove");
			_aiScript._aiming = false;
		}
		yield break;
	}

	public IEnumerator DoMelee()
	{
		_aiScript._aiming = true;
		_aiScript._targetPlayer = _aiScript.ClosestTarget();
		if (_aiScript.MeleeCanAtack())
		{
			float theta;
			AIHelp.MeleeSolution(out theta, _aiScript._myPositionF2, _aiScript.TargetingPosition(_aiScript._targetPlayer));
			_aiScript._firingDir.x = Mathf.Cos(theta);
			_aiScript._firingDir.y = Mathf.Sin(theta);
			_aiScript.NavPath = null;
			_aiScript.SetState("StateFire");
		}
		else
		{
			_aiScript.SetState("StateAttackMove");
			_aiScript._aiming = false;
		}
		yield break;
	}

	public IEnumerator DoAirstrike()
	{
		_aiScript._targetPlayer = _aiScript.RandomTarget();
		_aiScript._aiming = true;
		float theta;
		if (AIHelp.AirstrikeSolution(out theta, _aiScript._myPositionF2, _aiScript.TargetingPosition(_aiScript._targetPlayer), _aiScript._p.MyBody))
		{
			theta = _aiScript.AddInnacuracyForShot(theta);
			_aiScript._firingDir.x = Mathf.Cos(theta);
			_aiScript._firingDir.y = Mathf.Sin(theta);
			_aiScript.SetState("StateFire");
		}
		else
		{
			_aiScript.SetState("StateAttackMove");
			_aiScript._aiming = false;
		}
		yield break;
	}

	public IEnumerator DoLaserFiringSolution()
	{
		_aiScript._targetPlayer = _aiScript.RandomTarget();
		_aiScript._aiming = true;
		float theta;
		if (AIHelp.LaserSolution(out theta, _aiScript._myPositionF2, _aiScript.TargetingPosition(_aiScript._targetPlayer)))
		{
			theta = _aiScript.AddInnacuracyForShot(theta);
			_aiScript._firingDir.x = Mathf.Cos(theta);
			_aiScript._firingDir.y = Mathf.Sin(theta);
			_aiScript._jump = false;
			_aiScript.SetState("StateFire");
		}
		yield break;
	}

	public IEnumerator DoDestructableObjectSolution()
	{
		_aiScript._aiming = true;
		float theta = 0f;
		float power = 0f;
		if (_aiScript.CanShootDestructableObject(out theta, out power))
		{
			_aiScript._firingDir.x = Mathf.Cos(theta);
			_aiScript._firingDir.y = Mathf.Sin(theta);
			_aiScript.FiringMag = power;
			_aiScript.SetState("StateFire");
		}
		else
		{
			_aiScript.SetState("StateThink");
		}
		yield break;
	}

	public IEnumerator DoHappyJumpBot()
	{
		while (true)
		{
			_aiScript._jump = true;
			yield return new WaitForSeconds(2f);
		}
	}

	public IEnumerator DoNothing()
	{
		while (true)
		{
			yield return new WaitForSeconds(1f);
		}
	}

	public IEnumerator DoAttackMove()
	{
		_aiScript.NavPath = null;
		if (!_aiScript.TooClose() && _aiScript._p.Grounded)
		{
			Player p = _aiScript.BestMoveTarget(_aiScript._myPositionF2);
			if (p != null)
			{
				_aiScript._targetPositionF2 = _aiScript.TargetingPosition(p);
			}
			Navigation.PathingResult eRes;
			_aiScript.NavPath = LevelData.Instance.Path(_aiScript._p.Position(), _aiScript._targetPositionF2, out eRes);
			if (!object.ReferenceEquals(_aiScript.NavPath, null))
			{
				AIHelp.PathProcess(_aiScript.NavPath);
				yield return new WaitForSeconds(6f);
				_aiScript.SetState("StateThink");
				yield break;
			}
		}
		_aiScript.SetState("StateHideMove");
	}

	public IEnumerator DoHideMove()
	{
		_aiScript.NavPath = null;
		FVector2 hidePos;
		if (_aiScript.DetermineHidingPlace(out hidePos))
		{
			yield return new WaitForSeconds(0.5f);
			Navigation.PathingResult eRes;
			_aiScript.NavPath = LevelData.Instance.Path(_aiScript._p.Position(), hidePos, out eRes);
			if (!object.ReferenceEquals(_aiScript.NavPath, null))
			{
				int secondsPassed = 0;
				Navigation.Node.LimitTo(_aiScript.NavPath, 10);
				AIHelp.PathProcess(_aiScript.NavPath);
				while ((float)secondsPassed < 6f)
				{
					yield return new WaitForSeconds(1f);
					secondsPassed++;
					if (object.ReferenceEquals(_aiScript.NavPath, null))
					{
						break;
					}
				}
				_aiScript.SetState("StateThink");
				yield break;
			}
		}
		yield return new WaitForSeconds(1f);
		_aiScript.SetState("StateTeleport");
	}

	public IEnumerator DoGoAfterCrate()
	{
		int crates = CrateManager.Instance._cratesInScene.Count;
		int secondsPassed = 0;
		Navigation.PathingResult eRes;
		_aiScript.NavPath = LevelData.Instance.Path(_aiScript._p.Position(), _aiScript._CrateTargetPos, out eRes);
		if (!object.ReferenceEquals(_aiScript.NavPath, null))
		{
			AIHelp.PathProcess(_aiScript.NavPath);
			while (crates == CrateManager.Instance._cratesInScene.Count && !object.ReferenceEquals(_aiScript.NavPath, null) && secondsPassed != 6)
			{
				yield return new WaitForSeconds(1f);
				secondsPassed++;
			}
			_aiScript.SetState("StateThink");
		}
		else
		{
			_aiScript.SetState("StateHideMove");
		}
	}

	public IEnumerator DoFire()
	{
		yield return new WaitForSeconds(0.5f);
		_aiScript.Fire = true;
		_aiScript._firedThisTurn = true;
		if (_aiScript._p.CurrentWeapon._eType == Weapon.Type.kBeam)
		{
			yield return new WaitForSeconds(0.2f);
			string effectName = Arsenal.WeaponAt(_currentWeaponIndex).spriteName + "(Clone)";
			GameObject effect = GameObject.Find(effectName);
			while (effect != null)
			{
				_aiScript.NavPath = null;
				yield return new WaitForEndOfFrame();
			}
		}
		else if (_aiScript._p.CurrentWeapon._eType == Weapon.Type.kMelee)
		{
			yield return new WaitForSeconds(0.2f);
			string[] meleeAnims = new string[4] { "bot_hack_fast", "bot_stab", "bot_hack", "bot_chainsaw" };
			while (_aiScript._p.IsPlayingOneOfTheseAnims(meleeAnims))
			{
				_aiScript.NavPath = null;
				yield return new WaitForEndOfFrame();
			}
		}
		List<int> ammo;
		List<int> list = (ammo = _aiScript._myArsenal[_aiScript._curWeaponType]._ammo);
		int curWeaponIndexPos;
		int index = (curWeaponIndexPos = _aiScript._curWeaponIndexPos);
		curWeaponIndexPos = ammo[curWeaponIndexPos];
		list[index] = curWeaponIndexPos - 1;
		_aiScript.RemoveEmptyWeapons();
		_aiScript._aiming = false;
		yield return new WaitForSeconds(1f);
		_aiScript.SetState("StateThink");
	}

	public bool TeleportSolution(FVector2 pos, out Vector3 dir)
	{
		Vector3 to = new Vector3(Mathf.Sin((float)Math.PI / 2f), Mathf.Cos((float)Math.PI / 2f), 0f);
		Vector3 vector = new Vector3(Mathf.Sin(0.09f), Mathf.Cos(0.09f), 0f);
		if (pos.X > LevelData.Instance.GameplayWidth * 0.5f)
		{
			to.x = 0f - to.x;
			vector.x = 0f - vector.x;
		}
		float num = 0.2f;
		float num2 = 0f;
		for (int i = 0; i < 6; i++)
		{
			Vector3 vector2 = Vector3.Slerp(vector, to, num2);
			FVector2 fVector = new FVector2(vector2.x, vector2.y);
			if (!Misc.RayCast(pos, fVector * 25f, _aiScript._p.MyBody))
			{
				dir = FSHelper.FVector2ToVector3(fVector);
				return true;
			}
			num2 += num;
		}
		dir = Vector3.up;
		return false;
	}

	public void RandomFiringSolution(FVector2 pos, out Vector3 dir)
	{
		Vector3 to = new Vector3(Mathf.Sin((float)Math.PI / 2f), Mathf.Cos((float)Math.PI / 2f), 0f);
		Vector3 vector = new Vector3(Mathf.Sin(0.09f), Mathf.Cos(0.09f), 0f);
		if (pos.X > LevelData.Instance.GameplayWidth * 0.5f)
		{
			to.x = 0f - to.x;
			vector.x = 0f - vector.x;
		}
		float num = 0.2f;
		float num2 = 0f;
		dir = Vector3.up;
		for (int i = 0; i < 6; i++)
		{
			Vector3 vector2 = Vector3.Slerp(vector, to, num2);
			FVector2 fVector = new FVector2(vector2.x, vector2.y);
			if (!Misc.RayCast(pos, fVector * 5f, _aiScript._p.MyBody))
			{
				dir = FSHelper.FVector2ToVector3(fVector);
				break;
			}
			num2 += num;
		}
	}

	public IEnumerator DoTeleport()
	{
		bool localTeleport = false;
		if (_aiScript.PickTeleport() && TeleportSolution(_aiScript._myPositionF2, out _aiScript._firingDir))
		{
			_aiScript._aiming = true;
			yield return new WaitForSeconds(0.5f);
			_aiScript.FiringMag = UnityEngine.Random.Range(0f, 1f) * 0.100000024f + 0.7f;
			_aiScript.Fire = true;
			_aiScript.Teleported = true;
			yield return new WaitForSeconds(2f);
			localTeleport = true;
		}
		if (!localTeleport && !_aiScript._firedThisTurn)
		{
			_aiScript.SetState("StateRandomFireFallback");
		}
		else
		{
			_aiScript.SetState("StateThink");
		}
	}

	public IEnumerator DoRandomFireFallback()
	{
		_aiScript.PickAWeapon(Weapon.Type.kProjectile);
		yield return new WaitForSeconds(0.5f);
		RandomFiringSolution(_aiScript._myPositionF2, out _aiScript._firingDir);
		_aiScript.FiringMag = UnityEngine.Random.Range(0f, 1f) * 0.19999999f + 0.8f;
		_aiScript.Fire = true;
		_aiScript._firedThisTurn = true;
		EB.Debug.Log("++++ Firing randomly");
		yield return new WaitForSeconds(1f);
		_aiScript.SetState("StateThink");
	}

	public IEnumerator DoBotOnTopOfMe()
	{
		if (_aiScript._p.CurrentWeapon._eType != Weapon.Type.kMelee)
		{
			if (_aiScript._myArsenal[3]._weaponIndices.Count > 0 && !_aiScript._firedThisTurn)
			{
				UnityEngine.Debug.LogWarning("MENSAJE ERALMENTE LARGO Y EN MAYUSCULAS PARA SI LO VEA CUANDO SALGA:  2qrop@fsdio");
				_aiScript.PickAWeapon(Weapon.Type.kMelee);
				yield return new WaitForSeconds(0.3f);
				_aiScript.SetState("StateMelee");
				yield break;
			}
			if (_aiScript._p.CurrentWeapon._eType != Weapon.Type.kCarpetBomb)
			{
				if (_aiScript._triedToshakePlayerOff && !_aiScript._firedThisTurn)
				{
					_aiScript._aiming = true;
					float theta = 0f;
					float power = 0f;
					AIHelp.MeleeSolution(out theta, _aiScript._myPositionF2, _aiScript.TargetingPosition(_aiScript._targetPlayer));
					_aiScript._firingDir.x = Mathf.Cos(theta);
					_aiScript._firingDir.y = Mathf.Sin(theta);
					_aiScript.FiringMag = power;
					_aiScript.SetState("StateFire");
				}
				else
				{
					_aiScript._triedToshakePlayerOff = true;
					_aiScript.SetState("StateHideMove");
				}
				yield break;
			}
		}
		else if (_aiScript._p.CurrentWeapon._eType != Weapon.Type.kCarpetBomb)
		{
			if (_aiScript._triedToshakePlayerOff && !_aiScript._firedThisTurn)
			{
				_aiScript._aiming = true;
				float theta2 = 0f;
				float power2 = 0f;
				AIHelp.MeleeSolution(out theta2, _aiScript._myPositionF2, _aiScript.TargetingPosition(_aiScript._targetPlayer));
				_aiScript._firingDir.x = Mathf.Cos(theta2);
				_aiScript._firingDir.y = Mathf.Sin(theta2);
				_aiScript.FiringMag = power2;
				_aiScript.SetState("StateFire");
			}
			yield break;
		}
		_aiScript.SetState("StateThink");
	}
}
