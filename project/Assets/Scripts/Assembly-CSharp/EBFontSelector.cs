using System;
using EB;
using UnityEngine;

public class EBFontSelector : UIFontBase
{
	[System.Serializable]
	public class FontOption
	{
		public Language language;

		public string fontPath = string.Empty;
	}

	private UIFontBase _selected;

	private Language _selectedLanguage;

	public FontOption[] Fonts = new FontOption[0];

	private UIFontBase _font
	{
		get
		{
			SelectFont();
			return _selected;
		}
	}

	public override Material material
	{
		get
		{
			if ((bool)_font)
			{
				return _font.material;
			}
			return null;
		}
		set
		{
			if ((bool)_font)
			{
				_font.material = material;
			}
		}
	}

	public override float pixelSize
	{
		get
		{
			if ((bool)_font)
			{
				return _font.pixelSize;
			}
			return 1f;
		}
		set
		{
			if ((bool)_font)
			{
				_font.pixelSize = value;
			}
		}
	}

	public override int size
	{
		get
		{
			if ((bool)_font)
			{
				return _font.size;
			}
			return 1;
		}
	}

	private void SelectFont()
	{
		Language language = ((!Application.isPlaying) ? Language.English : Localizer.Current);
		if ((bool)_selected && language == _selectedLanguage)
		{
			return;
		}
		_selectedLanguage = language;
		FontOption[] fonts = Fonts;
		foreach (FontOption fontOption in fonts)
		{
			if (fontOption.language == language)
			{
				UnityEngine.Debug.Log("Loading: " + fontOption.fontPath);
				_selected = Assets.Load<UIFontBase>(fontOption.fontPath, false);
				return;
			}
		}
		FontOption[] fonts2 = Fonts;
		foreach (FontOption fontOption2 in fonts2)
		{
			if (fontOption2.language == Language.Unknown)
			{
				UnityEngine.Debug.Log("Loading Default: " + fontOption2.fontPath);
				_selected = Assets.Load<UIFontBase>(fontOption2.fontPath, false);
				break;
			}
		}
	}

	public override bool References(UIFontBase font)
	{
		if (font == _font)
		{
			return true;
		}
		return base.References(font);
	}

	public override Vector2 CalculatePrintedSize(string text, bool encoding, SymbolStyle symbolStyle)
	{
		if ((bool)_font)
		{
			return _font.CalculatePrintedSize(text, encoding, symbolStyle);
		}
		return Vector2.zero;
	}

	public override void Print(string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply)
	{
		if ((bool)_font)
		{
			_font.Print(text, color, verts, uvs, cols, encoding, symbolStyle, alignment, lineWidth, premultiply);
		}
	}

	public override string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle)
	{
		if ((bool)_font)
		{
			return _font.WrapText(text, maxWidth, maxLineCount, encoding, symbolStyle);
		}
		return text;
	}

	public override string GetEndOfLineThatFits(string text, float maxWidth, bool encoding, SymbolStyle symbolStyle)
	{
		if ((bool)_font)
		{
			return _font.GetEndOfLineThatFits(text, maxWidth, encoding, symbolStyle);
		}
		return string.Empty;
	}
}
