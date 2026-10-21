using UnityEngine;

public abstract class UIFontBase : MonoBehaviour
{
	public enum Alignment
	{
		Left = 0,
		Center = 1,
		Right = 2
	}

	public enum SymbolStyle
	{
		None = 0,
		Uncolored = 1,
		Colored = 2
	}

	public abstract Material material { get; set; }

	public abstract float pixelSize { get; set; }

	public abstract int size { get; }

	public abstract Vector2 CalculatePrintedSize(string text, bool encoding, SymbolStyle symbolStyle);

	public abstract string GetEndOfLineThatFits(string text, float maxWidth, bool encoding, SymbolStyle symbolStyle);

	public abstract void Print(string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply);

	public abstract string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle);

	public static bool CheckIfRelated(UIFontBase a, UIFontBase b)
	{
		if (a == null || b == null)
		{
			return false;
		}
		return a == b || a.References(b) || b.References(a);
	}

	public virtual bool References(UIFontBase font)
	{
		if (font == null)
		{
			return false;
		}
		if (font == this)
		{
			return true;
		}
		if (font.material == material)
		{
			return true;
		}
		return false;
	}

	public virtual void MarkAsDirty()
	{
		UILabel[] array = NGUITools.FindActive<UILabel>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UILabel uILabel = array[i];
			if (uILabel.enabled && NGUITools.GetActive(uILabel.gameObject) && CheckIfRelated(this, uILabel.font))
			{
				uILabel.MarkAsChanged();
			}
		}
	}
}
