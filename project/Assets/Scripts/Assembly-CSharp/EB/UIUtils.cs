using UnityEngine;

namespace EB
{
	public static class UIUtils
	{
		private const string validRegularTextChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";

		public static void ApplyTints(GameObject container, string identifier, Color tint)
		{
			GameObject[] objects = Util.GetObjects(container, identifier);
			foreach (GameObject gameObject in objects)
			{
				UIWidget component = gameObject.GetComponent<UIWidget>();
				if (component != null)
				{
					component.color = tint;
				}
			}
		}

		public static void SetAllAlphaValues(GameObject container, float alpha)
		{
			UIWidget[] array = Util.FindAllComponents<UIWidget>(container);
			foreach (UIWidget uIWidget in array)
			{
				Color color = uIWidget.color;
				color.a = alpha;
				uIWidget.color = color;
			}
		}

		public static void AssignTextAndResize(UILabel l, string text, Vector3 defaultSize)
		{
			if (!(l == null))
			{
				l.transform.localScale = defaultSize;
				l.text = text;
				ResizeLabelForText(l);
			}
		}

		public static void TruncateWithEllipsis(UILabel l, string s)
		{
			if (l == null)
			{
				return;
			}
			if (l.lineWidth <= 0 || l.maxLineCount != 1)
			{
				Debug.LogWarning("UILabel on game object '" + GetFullName(l.gameObject) + "' is not valid for TruncateWithEllipsis call.");
				return;
			}
			string text = s;
			while (true)
			{
				l.text = text;
				Vector2 labelTextSize = GetLabelTextSize(l);
				if ((float)l.lineWidth < labelTextSize.x)
				{
					int length = text.Length;
					text = ((!text.EndsWith("...")) ? text.Substring(0, length - 1) : text.Substring(0, length - 4));
					text += "...";
					continue;
				}
				break;
			}
		}

		public static void ResizeLabelForText(UILabel l)
		{
			if (l == null)
			{
				return;
			}
			if (l.lineWidth <= 0 || l.maxLineCount != 1)
			{
				Debug.LogWarning("UILabel on game object '" + GetFullName(l.gameObject) + "' is not valid for resizing.");
				return;
			}
			Vector3 localScale = l.transform.localScale;
			float num = 1f;
			float num2 = 1f;
			if (!Mathf.Approximately(localScale.x, localScale.y))
			{
				float num3 = Mathf.Max(localScale.x, localScale.y);
				num = localScale.x / num3;
				num2 = localScale.y / num3;
			}
			while (true)
			{
				Vector2 labelTextSize = GetLabelTextSize(l);
				if ((float)l.lineWidth < labelTextSize.x)
				{
					localScale.x -= num;
					localScale.y -= num2;
					l.transform.localScale = localScale;
					continue;
				}
				break;
			}
		}

		public static void SetLabelContents(GameObject obj, object labelText)
		{
			if (!(obj != null))
			{
				return;
			}
			UILabel uILabel = Util.FindComponent<UILabel>(obj);
			if (!(uILabel != null))
			{
				return;
			}
			string text = string.Empty;
			if (labelText != null)
			{
				text = labelText.ToString();
				if (text.StartsWith("ID_"))
				{
					text = Localizer.GetString(text);
				}
			}
			uILabel.text = text;
		}

		public static void SetLabelContents(GameObject container, string labelName, object labelText)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(container, labelName);
			SetLabelContents(objectExactMatch, labelText);
		}

		public static string GetLabelContents(GameObject container, string labelName)
		{
			string result = string.Empty;
			GameObject objectExactMatch = Util.GetObjectExactMatch(container, labelName);
			if (objectExactMatch != null)
			{
				UILabel uILabel = Util.FindComponent<UILabel>(objectExactMatch);
				if (uILabel != null)
				{
					result = uILabel.text;
				}
			}
			return result;
		}

		public static Vector2 GetLabelTextSize(UILabel label)
		{
			Vector2 zero = Vector2.zero;
			if (label != null)
			{
				Vector3 vector = label.font.CalculatePrintedSize(label.text, label.supportEncoding, label.symbolStyle);
				int size = label.font.size;
				float x = label.transform.localScale.x;
				float num = 1f / (float)size;
				num *= (float)size / x;
				zero.x = vector.x / num;
				zero.y = vector.y / num;
			}
			return zero;
		}

		public static string GetFullName(GameObject obj)
		{
			if (obj == null)
			{
				return "<null>";
			}
			string text = obj.name;
			while (obj != null)
			{
				Transform parent = obj.transform.parent;
				if (parent != null)
				{
					obj = parent.gameObject;
					text = obj.name + "/" + text;
				}
				else
				{
					obj = null;
				}
			}
			return text;
		}

		public static void SetTexture(GameObject container, Texture2D uiTexture, bool resizeContainerToFit = true)
		{
			UITexture uITexture = Util.FindComponent<UITexture>(container);
			if (uITexture == null)
			{
				Debug.LogWarning("SetTexture: Nowhere to put texture!", container);
				return;
			}
			if (uiTexture != null)
			{
				uITexture.mainTexture = uiTexture;
			}
			if (resizeContainerToFit)
			{
				Vector3 localScale = new Vector3(uITexture.mainTexture.width, uITexture.mainTexture.height, 0f);
				uITexture.transform.localScale = localScale;
			}
		}

		public static Vector2 GetLabelTextSize(GameObject container, string labelName)
		{
			Vector2 result = Vector2.zero;
			GameObject objectExactMatch = Util.GetObjectExactMatch(container, labelName);
			if (objectExactMatch != null)
			{
				UILabel label = Util.FindComponent<UILabel>(objectExactMatch);
				result = GetLabelTextSize(label);
			}
			return result;
		}

		public static void CenterSpriteAndLabel(GameObject button, string iconName, string labelName)
		{
			GameObject objectExactMatch = Util.GetObjectExactMatch(button, iconName);
			GameObject objectExactMatch2 = Util.GetObjectExactMatch(button, labelName);
			float x = GetLabelTextSize(objectExactMatch2.GetComponent<UILabel>()).x;
			float x2 = objectExactMatch.transform.localScale.x;
			float num = objectExactMatch2.transform.localPosition.x - objectExactMatch.transform.localPosition.x - x2;
			float num2 = x2 + num + x;
			Vector3 localPosition = objectExactMatch.transform.localPosition;
			localPosition.x = 0f - num2 / 2f;
			objectExactMatch.transform.localPosition = localPosition;
			Vector3 localPosition2 = objectExactMatch2.transform.localPosition;
			localPosition2.x = localPosition.x + x2 + num;
			objectExactMatch2.transform.localPosition = localPosition2;
		}

		public static T GetOrAssign<T>(ref T result, GameObject container, string childName = "") where T : Component
		{
			if (result != null)
			{
				return result;
			}
			if (childName != string.Empty)
			{
				container = Util.GetObjectExactMatch(container, childName);
			}
			result = Util.FindComponent<T>(container);
			return result;
		}

		public static char TextValidatorForName(string prevStr, char nextChar)
		{
			if (prevStr.Length >= 12)
			{
				return '\0';
			}
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".Contains(nextChar.ToString()))
			{
				return nextChar;
			}
			return '\0';
		}

		public static char TextValidatorForEmail(string prevStr, char nextChar)
		{
			if (prevStr.Length >= 45)
			{
				return '\0';
			}
			if ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890".Contains(nextChar.ToString()))
			{
				return nextChar;
			}
			if ("!#$%&'*+-/=?^_`{|}~\"(),:;<>@[\\]. ".Contains(nextChar.ToString()))
			{
				return nextChar;
			}
			return '\0';
		}

		public static char TextValidatorForPassword(string prevStr, char nextChar)
		{
			if (prevStr.Length >= 25)
			{
				return '\0';
			}
			if (char.IsLetterOrDigit(nextChar) || char.IsPunctuation(nextChar) || char.IsWhiteSpace(nextChar))
			{
				return nextChar;
			}
			return '\0';
		}
	}
}
