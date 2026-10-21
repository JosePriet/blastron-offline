using UnityEngine;

public class RecipeInstance : MonoBehaviour
{
	private CharacterDB.Recipe _recipe;

	public CharacterDB.Recipe Recipe
	{
		get
		{
			return _recipe;
		}
		set
		{
			if (_recipe != null)
			{
				CharacterDB.DecRefCount(_recipe);
			}
			_recipe = value;
		}
	}

	private void OnDestroy()
	{
		Recipe = null;
	}
}
