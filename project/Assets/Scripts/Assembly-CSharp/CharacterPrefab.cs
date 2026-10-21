using System.Collections;
using EB;
using UnityEngine;

[ExecuteInEditMode]
public class CharacterPrefab : MonoBehaviour
{
	public enum aiType
	{
		Simple = 0,
		Boss = 1
	}

	public string Name = string.Empty;

	public string[] Parts = new string[0];

	public int health;

	public float damageMultiplier = 1f;

	public float sizeMult = 1f;

	public float animSpeed = 1f;

	public bool cameraShake;

	public bool useBossAnims;

	public aiType AIType;

	public void CopyTo(CharacterDB.Recipe recipe)
	{
		for (int i = 0; i < Parts.Length && i < recipe.parts.Length; i++)
		{
			int result = 0;
			if (!int.TryParse(Parts[i], out result))
			{
				result = Hash.StringHash(Parts[i]);
			}
			if (result != 0)
			{
				recipe.parts[i] = result;
			}
		}
	}

	public void FromRecipe(CharacterDB.Recipe recipe)
	{
		Parts = new string[recipe.parts.Length];
		for (int i = 0; i < Parts.Length; i++)
		{
			Parts[i] = recipe.parts[i].ToString();
			CharacterDB.ContentInfo content = CharacterDB.GetContent(recipe.parts[i]);
			if (content != null)
			{
				Parts[i] = content.AssetName;
			}
		}
	}

	public CharacterDB.Recipe ToRecipe()
	{
		CharacterDB.Recipe recipe = new CharacterDB.Recipe();
		for (int i = 0; i < Parts.Length && i < recipe.parts.Length; i++)
		{
			int result = 0;
			if (int.TryParse(Parts[i], out result))
			{
				recipe.parts[i] = result;
			}
			else if (!string.IsNullOrEmpty(Parts[i]))
			{
				recipe.parts[i] = Hash.StringHash(Parts[i]);
			}
			else
			{
				recipe.parts[i] = CharacterDB.GetRandomPart((CharacterDB.Category)i);
			}
		}
		return recipe;
	}

	public Hashtable CreatePlayerAttributes(string spawnPointName)
	{
		Hashtable hashtable = new Hashtable();
		hashtable["recipe"] = ToRecipe().ToHashtable();
		hashtable["name"] = Name;
		hashtable["ai"] = true;
		hashtable["aiType"] = AIType;
		hashtable["bossAnims"] = useBossAnims;
		hashtable["health"] = health;
		hashtable["damageMultiplier"] = damageMultiplier;
		hashtable["ai_sp"] = spawnPointName;
		hashtable["sizem"] = sizeMult;
		hashtable["animsp"] = animSpeed;
		hashtable["camsh"] = cameraShake;
		return hashtable;
	}
}
