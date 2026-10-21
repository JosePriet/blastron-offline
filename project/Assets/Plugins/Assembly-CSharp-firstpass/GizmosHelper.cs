using UnityEngine;

public static class GizmosHelper
{
	private static Vector3 d0_0 = new Vector3(-5f, 5f, 0f);

	private static Vector3 d1_0 = new Vector3(5f, 5f, 0f);

	private static Vector3 d0_1 = new Vector3(-5f, 0f, 0f);

	private static Vector3 d1_1 = new Vector3(5f, 0f, 0f);

	private static Vector3 d0_2 = new Vector3(-5f, -5f, 0f);

	private static Vector3 d1_2 = new Vector3(5f, -5f, 0f);

	public static void DrawString(Vector3 position, string val)
	{
		DrawString(position, Vector3.one, val);
	}

	public static void DrawString(Vector3 position, Vector3 scale, string val)
	{
		Vector3 zero = Vector3.zero;
		for (int i = 0; i < val.Length; i++)
		{
			switch (val[i])
			{
			case '0':
				DrawChar_0(position + zero, scale);
				break;
			case '1':
				DrawChar_1(position + zero, scale);
				break;
			case '2':
				DrawChar_2(position + zero, scale);
				break;
			case '3':
				DrawChar_3(position + zero, scale);
				break;
			case '4':
				DrawChar_4(position + zero, scale);
				break;
			case '5':
				DrawChar_5(position + zero, scale);
				break;
			case '6':
				DrawChar_6(position + zero, scale);
				break;
			case '7':
				DrawChar_7(position + zero, scale);
				break;
			case '8':
				DrawChar_8(position + zero, scale);
				break;
			case '9':
				DrawChar_9(position + zero, scale);
				break;
			}
			zero.x += 0.3f * scale.x;
		}
	}

	private static void DrawChar_0(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[4, 2]
		{
			{ d0_0, d1_0 },
			{ d1_0, d1_2 },
			{ d1_2, d0_2 },
			{ d0_2, d0_0 }
		};
		DrawChar(position, ref points, 4, scale);
	}

	private static void DrawChar_1(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[2, 2]
		{
			{ d0_1, d1_0 },
			{ d1_0, d1_2 }
		};
		DrawChar(position, ref points, 2, scale);
	}

	private static void DrawChar_2(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[5, 2]
		{
			{ d0_0, d1_0 },
			{ d1_0, d1_1 },
			{ d1_1, d0_1 },
			{ d0_1, d0_2 },
			{ d0_2, d1_2 }
		};
		DrawChar(position, ref points, 5, scale);
	}

	private static void DrawChar_3(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[4, 2]
		{
			{ d0_0, d1_0 },
			{ d1_0, d1_2 },
			{ d0_1, d1_1 },
			{ d0_2, d1_2 }
		};
		DrawChar(position, ref points, 4, scale);
	}

	private static void DrawChar_4(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[3, 2]
		{
			{ d1_1, d0_1 },
			{ d0_1, d1_0 },
			{ d1_0, d1_2 }
		};
		DrawChar(position, ref points, 3, scale);
	}

	private static void DrawChar_5(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[5, 2]
		{
			{ d0_0, d1_0 },
			{ d0_0, d0_1 },
			{ d1_1, d0_1 },
			{ d1_1, d1_2 },
			{ d0_2, d1_2 }
		};
		DrawChar(position, ref points, 5, scale);
	}

	private static void DrawChar_6(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[6, 2]
		{
			{ d0_0, d1_0 },
			{ d0_0, d0_1 },
			{ d1_1, d0_1 },
			{ d1_1, d1_2 },
			{ d0_2, d1_2 },
			{ d0_2, d0_1 }
		};
		DrawChar(position, ref points, 6, scale);
	}

	private static void DrawChar_7(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[2, 2]
		{
			{ d0_0, d1_0 },
			{ d1_0, d1_2 }
		};
		DrawChar(position, ref points, 2, scale);
	}

	private static void DrawChar_8(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[7, 2]
		{
			{ d0_0, d1_0 },
			{ d0_1, d1_1 },
			{ d0_2, d1_2 },
			{ d0_0, d0_1 },
			{ d1_0, d1_1 },
			{ d0_1, d0_2 },
			{ d1_1, d1_2 }
		};
		DrawChar(position, ref points, 7, scale);
	}

	private static void DrawChar_9(Vector3 position, Vector3 scale)
	{
		Vector3[,] points = new Vector3[6, 2]
		{
			{ d0_0, d1_0 },
			{ d0_1, d1_1 },
			{ d0_2, d1_2 },
			{ d0_0, d0_1 },
			{ d1_0, d1_1 },
			{ d1_1, d1_2 }
		};
		DrawChar(position, ref points, 6, scale);
	}

	private static void DrawChar(Vector3 position, ref Vector3[,] points, int cols, Vector3 scale)
	{
		for (int i = 0; i < cols; i++)
		{
			points[i, 0].Scale(scale);
			points[i, 1].Scale(scale);
			Gizmos.DrawLine(position + points[i, 0] * 0.025f, position + points[i, 1] * 0.025f);
		}
	}
}
