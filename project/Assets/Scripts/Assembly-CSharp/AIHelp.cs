using System;
using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Terrain;
using UnityEngine;

public static class AIHelp
{
	public struct PathMoveInstruction
	{
		public bool left;

		public bool right;

		public bool jump;

		public bool fall;

		public override string ToString()
		{
			return string.Format("PMI: left->{0} right->{1} jump->{2} fall->{3}", left, right, jump, fall);
		}
	}

	public enum PathMoveStatus
	{
		kOK = 0,
		kPathFinished = 1,
		kPathLost = 2
	}

	public delegate bool CurveEvalCallback(Vector3 pos, float t);

	private const float _lostR = 4f;

	private const float _lostR2 = 16f;

	private const int _firingSolutionSegs = 16;

	public static TerrainBody GroundTerrainBody(FVector2 start, FVector2 dir, out FVector2 hit, out FVector2 normal, out Fixture hitFixture, Body ignoreBody)
	{
		if (Misc.RayCastForClosest(start, dir, out hit, out normal, out hitFixture, ignoreBody))
		{
			Body body = hitFixture.Body;
			if (body.UserData != null)
			{
				return body.UserData as TerrainBody;
			}
		}
		return null;
	}

	public static float Range2To(FVector2 p, Navigation.Node n)
	{
		return FVector2.DistanceSquared(p, n.Position);
	}

	public static int PathTrend(Navigation.Node curr)
	{
		int i = 0;
		Navigation.Node next = curr.Next;
		int num = 0;
		for (; i < 3; i++)
		{
			if (object.ReferenceEquals(next, null))
			{
				break;
			}
			num += next._y - curr._y;
		}
		return num;
	}

	public static void PathProcess(Navigation.Node path)
	{
		Navigation.Node node = null;
		int num = 0;
		while (!object.ReferenceEquals(path, null))
		{
			Navigation.Node next = path.Next;
			if (!object.ReferenceEquals(next, null))
			{
				if (next._y == path._y - 1)
				{
					if (next._x == path._x + 1)
					{
						if (object.ReferenceEquals(node, null))
						{
							node = path;
							num = 1;
						}
						else if (num == -1)
						{
							node.Append(new Navigation.Node(node._x - 1, node._y, 0, false, true));
							node.Append(2, new Navigation.Node(node._x - 1, node._y - 2, 0, false, true));
							num = 0;
							node = null;
						}
						else
						{
							num = 0;
							node = null;
						}
					}
					else if (next._x == path._x - 1)
					{
						if (object.ReferenceEquals(node, null))
						{
							node = path;
							num = -1;
						}
						else if (num == 1)
						{
							node.Append(new Navigation.Node(node._x + 1, node._y, 0, false, true));
							node.Append(2, new Navigation.Node(node._x + 1, node._y - 2, 0, false, true));
							num = 0;
							node = null;
						}
						else
						{
							num = 0;
							node = null;
						}
					}
					else
					{
						num = 0;
						node = null;
					}
				}
				else
				{
					num = 0;
					node = null;
				}
			}
			path = next;
		}
	}

	public static Navigation.Node PathMove(FVector2 pos, Navigation.Node curr, out PathMoveInstruction pmi, bool grounded, out PathMoveStatus moveStatus)
	{
		pmi = default(PathMoveInstruction);
		Navigation.Node next = curr.Next;
		if (object.ReferenceEquals(next, null))
		{
			moveStatus = PathMoveStatus.kPathFinished;
			return null;
		}
		if (Range2To(pos, next) < Range2To(pos, curr) && grounded)
		{
			curr = curr.Next;
			next = curr.Next;
		}
		if (!object.ReferenceEquals(next, null))
		{
			if (next._jump)
			{
				pmi.jump = true;
				if (next._y <= curr._y - 3 || !grounded)
				{
					pmi.jump = false;
				}
			}
			int x;
			int y;
			LevelData.Instance.NavWorld.ToCell(pos, out x, out y);
			if (next._y < curr._y && LevelData.Instance.NavWorld.IsCellClearChecked(x, y - 1))
			{
				pmi.fall = true;
				pmi.jump = false;
			}
			if (next._x > curr._x)
			{
				pmi.right = true;
			}
			else if (next._x < curr._x)
			{
				pmi.left = true;
			}
		}
		if (Range2To(pos, curr) > 16f && grounded)
		{
			moveStatus = PathMoveStatus.kPathLost;
			return curr;
		}
		moveStatus = PathMoveStatus.kOK;
		return curr;
	}

	private static bool LookaheadFail(Navigation.Node lookaheadFrom, int lookaheadRange)
	{
		Navigation.Node next = lookaheadFrom.Next;
		int num = 0;
		int num2 = 0;
		while (!object.ReferenceEquals(next, null) && num < 4 && num2 < lookaheadRange)
		{
			num = ((next._x == lookaheadFrom._x && next._y > lookaheadFrom._y) ? (num + 1) : 0);
			next = next.Next;
			lookaheadFrom = lookaheadFrom.Next;
			num2++;
		}
		if (num == 4)
		{
			return true;
		}
		return false;
	}

	public static void MeleeSolution(out float theta, FVector2 fromPos, FVector2 toPos)
	{
		FVector2 fVector = toPos - fromPos;
		theta = Mathf.Atan2(fVector.Y, fVector.X);
	}

	public static bool AirstrikeSolution(out float theta, FVector2 fromPos, FVector2 toPos, Body ignoreBody)
	{
		Vector3 vector = Camera.mainCamera.WorldToViewportPoint(new Vector3(fromPos.X, fromPos.Y, 0f));
		FVector2 fVector = new FVector2(Camera.mainCamera.WorldToViewportPoint(new Vector3(toPos.X, toPos.Y, 0f)).x, 1f) - new FVector2(vector.x, vector.y);
		theta = Mathf.Atan2(fVector.Y, fVector.X);
		if (!Misc.RayCast(toPos, new FVector2(0f, 1000f), ignoreBody))
		{
			return true;
		}
		return false;
	}

	public static bool LaserSolution(out float theta, FVector2 fromPos, FVector2 toPos)
	{
		FVector2 fVector = toPos - fromPos;
		theta = Mathf.Atan2(fVector.Y, fVector.X);
		return true;
	}

	public static bool ProjectileFiringSolution(float speed, float g, out float theta, FVector2 fromPos, FVector2 toPos, Body ignoreBody, bool checkForPlayer)
	{
		theta = 0f;
		g = 0f - g;
		FVector2 fVector = toPos - fromPos;
		bool flag = fVector.X > 0f;
		if (g == 0f)
		{
			if (!Misc.RayCast(fromPos, fVector, ignoreBody))
			{
				theta = Mathf.Atan2(fVector.Y, fVector.X);
				return true;
			}
			return false;
		}
		fVector.X = Mathf.Abs(fVector.X);
		float num = fVector.X * fVector.X;
		float num2 = speed * speed;
		float num3 = num2 * num2;
		float num4 = num3 - g * (g * num + 2f * fVector.Y * num2);
		if (num4 < 0f)
		{
			return false;
		}
		float num5 = Mathf.Sqrt(num4);
		float num6 = g * fVector.X;
		float num7 = Mathf.Atan((num2 + num5) / num6);
		float num8 = Mathf.Atan((num2 - num5) / num6);
		bool flag2 = ProjectileFiringSolution(fVector, toPos.Y, speed, g, num8, flag, 8, fromPos, ignoreBody, checkForPlayer);
		bool flag3 = false;
		if (!flag2)
		{
			flag3 = ProjectileFiringSolution(fVector, toPos.Y, speed, g, num7, flag, 16, fromPos, ignoreBody, checkForPlayer);
		}
		if (!flag)
		{
			num7 = (float)Math.PI - num7;
			num8 = (float)Math.PI - num8;
		}
		if (flag3)
		{
			theta = num7;
			return true;
		}
		if (flag2)
		{
			theta = num8;
			return true;
		}
		return false;
	}

	private static bool ProjectileFiringSolution(FVector2 toTarget, float targetY, float speed, float g, float theta, bool l2r, int segs, FVector2 fromPos, Body ignoreBody, bool checkForPlayer)
	{
		float num = speed * Mathf.Cos(theta);
		float num2 = speed * Mathf.Sin(theta);
		float num3 = toTarget.X / num;
		float num4 = num3 / (float)(segs - 1);
		float num5 = 0f;
		float num6 = ((!l2r) ? (-1f) : 1f);
		List<FVector2> list = new List<FVector2>();
		for (int i = 0; i < segs; i++)
		{
			float y = num2 * num5 - g * num5 * num5 / 2f;
			float x = num * num5 * num6;
			list.Add(new FVector2(x, y) + fromPos);
			num5 += num4;
		}
		for (int j = 0; j < list.Count - 1; j++)
		{
			FVector2 fVector = list[j];
			FVector2 fVector2 = list[j + 1];
			FVector2 hitPoint;
			FVector2 hitNormal;
			Fixture hitFixture;
			if (j == list.Count - 2)
			{
				if (Misc.RayCastForClosest(fVector, fVector2 - fVector, out hitPoint, out hitNormal, out hitFixture, ignoreBody) && (hitFixture.Body.UserTag == "Player" || !checkForPlayer))
				{
					return true;
				}
				return false;
			}
			if (Misc.RayCast(fVector, fVector2 - fVector, out hitPoint, out hitNormal, out hitFixture, ignoreBody))
			{
				return false;
			}
		}
		return true;
	}

	public static void EvalProjectileFlight(float speed, float g, float theta, float dt, CurveEvalCallback fn)
	{
		g = 0f - g;
		float num = speed * Mathf.Cos(theta);
		float num2 = speed * Mathf.Sin(theta);
		float num3 = 0f;
		while (true)
		{
			float y = num2 * num3 - g * num3 * num3 / 2f;
			float x = num * num3;
			if (!fn(new Vector3(x, y, 0f), num3))
			{
				break;
			}
			num3 += dt;
		}
	}
}
