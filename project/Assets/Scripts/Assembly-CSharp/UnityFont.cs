using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class UnityFont : UIFontBase
{
	public Font font;

	public int fontSize = 32;

	public float lineSpace = 2f;

	public float spaceWidth = 2f;

	public float yOffset;

	public FontStyle fontStyle;

	private List<Color> mColors = new List<Color>();

	private static List<UnityFont> _fonts = new List<UnityFont>();

	public override Material material
	{
		get
		{
			if (font != null)
			{
				return font.material;
			}
			return null;
		}
		set
		{
		}
	}

	public override float pixelSize
	{
		get
		{
			return 1f;
		}
		set
		{
		}
	}

	public override int size
	{
		get
		{
			return fontSize;
		}
	}

	private static void SetupCallback(Font font)
	{
		if (font.textureRebuildCallback != null)
		{
			return;
		}
		font.material.shader = Shader.Find("Unlit/Transparent Colored Font");
		font.textureRebuildCallback = () =>
		{
			foreach (UnityFont font2 in _fonts)
			{
				if ((bool)font2 && font2.font == font)
				{
					font2.MarkAsDirty();
				}
			}
		};
	}

	private void SetupCallback()
	{
		if (!_fonts.Contains(this))
		{
			_fonts.Add(this);
			SetupCallback(font);
		}
	}

	public override Vector2 CalculatePrintedSize(string text, bool encoding, SymbolStyle symbolStyle)
	{
		SetupCallback();
		Vector2 zero = Vector2.zero;
		if (font != null && text != null && text.Length > 0)
		{
			if (encoding)
			{
				text = NGUITools.StripSymbols(text);
			}
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			float num4 = ((float)size + lineSpace) / (float)fontSize;
			bool flag = true;
			int i = 0;
			for (int length = text.Length; i < length; i++)
			{
				char c = text[i];
				if (c == '\n')
				{
					if (num > num3)
					{
						num3 = num;
					}
					num = 0f;
					num2 += num4;
					continue;
				}
				if (encoding && c == '[')
				{
					int num5 = NGUITools.ParseSymbol(text, i, mColors);
					if (num5 > 0)
					{
						i += num5 - 1;
						continue;
					}
				}
				CharacterInfo info;
				if (c == ' ')
				{
					num += spaceWidth / (float)fontSize;
				}
				else if (font.GetCharacterInfo(c, out info, fontSize, fontStyle))
				{
					float num6 = info.vert.x / (float)info.size;
					float num7 = num6 + info.vert.width / (float)info.size;
					num += num7;
				}
				else if (font.HasCharacter(c))
				{
					font.RequestCharactersInTexture(text, fontSize, fontStyle);
					flag = false;
				}
			}
			if (!flag)
			{
				MarkAsDirty();
			}
			zero.x = ((!(num > num3)) ? num3 : num);
			zero.y = num2 + num4;
		}
		return zero;
	}

	private void Align(BetterList<Vector3> verts, int indexOffset, Alignment alignment, float x, float lineWidth)
	{
		if (alignment != Alignment.Left && fontSize > 0)
		{
			lineWidth /= (float)fontSize;
			float num = ((alignment != Alignment.Right) ? ((lineWidth - x) * 0.5f) : (lineWidth - x));
			if (num < 0f)
			{
				num = 0f;
			}
			for (int i = indexOffset; i < verts.size; i++)
			{
				Vector3 vector = verts.buffer[i];
				vector.x += num;
				verts.buffer[i] = vector;
			}
		}
	}

	public override void Print(string text, Color32 color, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, bool encoding, SymbolStyle symbolStyle, Alignment alignment, int lineWidth, bool premultiply)
	{
		if (!(font != null) || text == null || text.Length <= 0)
		{
			return;
		}
		mColors.Clear();
		mColors.Add(color);
		bool flag = true;
		int num = verts.size;
		float num2 = 0f;
		float num3 = yOffset;
		int i = 0;
		for (int length = text.Length; i < length; i++)
		{
			char c = text[i];
			if (c == '\n')
			{
				if (alignment != Alignment.Left)
				{
					Align(verts, num, alignment, num2, lineWidth);
					num = verts.size;
				}
				num2 = 0f;
				num3 -= ((float)fontSize + lineSpace) / (float)fontSize;
				continue;
			}
			if (encoding && c == '[')
			{
				int num4 = NGUITools.ParseSymbol(text, i, mColors);
				if (num4 > 0)
				{
					color = mColors[mColors.Count - 1];
					i += num4 - 1;
					continue;
				}
			}
			CharacterInfo info;
			if (c == ' ')
			{
				num2 += spaceWidth / (float)fontSize;
			}
			else if (font.GetCharacterInfo(c, out info, fontSize, fontStyle))
			{
				Rect uv = info.uv;
				float num5 = info.vert.y / (float)info.size;
				float num6 = num5 + info.vert.height / (float)info.size;
				float num7 = info.vert.x / (float)info.size;
				float num8 = info.vert.width / (float)info.size;
				float num9 = num7 + num8;
				Vector3 item = new Vector3(num2 + num7, num3 + num6, 0f);
				Vector2 item2 = new Vector2(uv.x, uv.y);
				Vector3 item3 = new Vector3(num2 + num7, num3 + num5, 0f);
				Vector2 item4 = new Vector2(uv.x, uv.y + uv.height);
				Vector3 item5 = new Vector3(num2 + num9, num3 + num6, 0f);
				Vector2 item6 = new Vector2(uv.x + uv.width, uv.y);
				Vector3 item7 = new Vector3(num2 + num9, num3 + num5, 0f);
				Vector2 item8 = new Vector2(uv.x + uv.width, uv.y + uv.height);
				verts.Add(item7);
				verts.Add(item5);
				verts.Add(item);
				verts.Add(item3);
				if (info.flipped)
				{
					uvs.Add(item8);
					uvs.Add(item4);
					uvs.Add(item2);
					uvs.Add(item6);
				}
				else
				{
					uvs.Add(item8);
					uvs.Add(item6);
					uvs.Add(item2);
					uvs.Add(item4);
				}
				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
				cols.Add(color);
				num2 += num9;
			}
			else if (font.HasCharacter(c))
			{
				flag = false;
			}
		}
		if (alignment != Alignment.Left && num < verts.size)
		{
			Align(verts, num, alignment, num2, lineWidth);
		}
		if (flag)
		{
		}
	}

	public override string WrapText(string text, float maxWidth, int maxLineCount, bool encoding, SymbolStyle symbolStyle)
	{
		int num = Mathf.RoundToInt(maxWidth * (float)size);
		if (num < 1)
		{
			return text;
		}
		StringBuilder s = new StringBuilder(text.Length * 2);
		int length = text.Length;
		int num2 = num;
		int num3 = 0;
		int i = 0;
		int j = 0;
		bool flag = true;
		bool flag2 = maxLineCount != 1;
		int num4 = 1;
		for (; j < length; j++)
		{
			char c = text[j];
			if (c == '\n')
			{
				if (!flag2 || num4 == maxLineCount)
				{
					break;
				}
				num2 = num;
				if (i < j)
				{
					s.Append(text.Substring(i, j - i + 1));
				}
				else
				{
					s.Append(c);
				}
				flag = true;
				num4++;
				i = j + 1;
				num3 = 0;
				continue;
			}
			if (c == ' ' && num3 != 32 && i < j)
			{
				s.Append(text.Substring(i, j - i + 1));
				flag = false;
				i = j + 1;
				num3 = c;
			}
			if (encoding && c == '[' && j + 2 < length)
			{
				if (text[j + 1] == '-' && text[j + 2] == ']')
				{
					j += 2;
					continue;
				}
				if (j + 7 < length && text[j + 7] == ']' && NGUITools.EncodeColor(NGUITools.ParseColor(text, j + 1)) == text.Substring(j + 1, 6).ToUpper())
				{
					j += 7;
					continue;
				}
			}
			int num5 = 0;
			CharacterInfo info;
			if (font.GetCharacterInfo(c, out info, fontSize, fontStyle))
			{
				num5 += (int)(info.vert.x + info.vert.width);
			}
			else
			{
				if (c != ' ')
				{
					continue;
				}
				num5 = (int)spaceWidth;
			}
			num2 -= num5;
			if (num2 < 0)
			{
				if (flag || !flag2 || num4 == maxLineCount)
				{
					s.Append(text.Substring(i, Mathf.Max(0, j - i)));
					if (!flag2 || num4 == maxLineCount)
					{
						i = j;
						break;
					}
					UIFont.EndLine(ref s);
					flag = true;
					num4++;
					if (c == ' ')
					{
						i = j + 1;
						num2 = num;
					}
					else
					{
						i = j;
						num2 = num - num5;
					}
					num3 = 0;
				}
				else
				{
					for (; i < length && text[i] == ' '; i++)
					{
					}
					flag = true;
					num2 = num;
					j = i - 1;
					num3 = 0;
					if (!flag2 || num4 == maxLineCount)
					{
						break;
					}
					num4++;
					UIFont.EndLine(ref s);
				}
			}
			else
			{
				num3 = c;
			}
		}
		if (i < j)
		{
			s.Append(text.Substring(i, j - i));
		}
		return s.ToString();
	}

	public override string GetEndOfLineThatFits(string text, float maxWidth, bool encoding, SymbolStyle symbolStyle)
	{
		return text;
	}
}
