using System.Collections;
using EB;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
	public class Character
	{
		public CharacterDB.Recipe recipe;

		public Character(CharacterDB.Recipe rec)
		{
			recipe = rec;
		}
	}

	private static CharacterManager _this;

	private Character _player;

	public static CharacterManager Instance
	{
		get
		{
			return _this;
		}
	}

	public Character LocalPlayer
	{
		get
		{
			return _player;
		}
		set
		{
			_player = value;
		}
	}

	private void Awake()
	{
		_this = this;
		CreateLocalCharacter(null);
		Object.DontDestroyOnLoad(base.gameObject);
	}

	private void Start()
	{
	}

	public void CreateLocalCharacter(CharacterDB.Recipe recipe)
	{
		if (recipe == null)
		{
			recipe = new CharacterDB.Recipe();
		}
		_player = new Character(recipe);
	}

	public void UpdateLocalCharacter()
	{
		CacDisplay[] array = Object.FindObjectsOfType(typeof(CacDisplay)) as CacDisplay[];
		CacDisplay[] array2 = array;
		foreach (CacDisplay cacDisplay in array2)
		{
			cacDisplay.LocalPlayerUpdated();
		}
	}

	public void SerializeRecipe(Hashtable data)
	{
		SerializeRecipe(data, LocalPlayer.recipe);
	}

	public void SerializeRecipe(Hashtable data, CharacterDB.Recipe recipe)
	{
		data["recipe"] = recipe.ToHashtable();
	}

	public CharacterDB.Recipe DeserializeRecipe(Hashtable data)
	{
		return new CharacterDB.Recipe(data["recipe"]);
	}

	public void UpdateCharacter(GameObject playerObject, CharacterDB.Recipe recipe)
	{
		for (int i = 0; i < recipe.parts.Length; i++)
		{
			CharacterDB.ContentInfo content = CharacterDB.GetContent(recipe.parts[i]);
			if (content != null)
			{
				content.Apply(playerObject);
				continue;
			}
			EB.Debug.LogError("Missing content info for " + i + " hash: " + recipe.parts[i]);
		}
	}
}
