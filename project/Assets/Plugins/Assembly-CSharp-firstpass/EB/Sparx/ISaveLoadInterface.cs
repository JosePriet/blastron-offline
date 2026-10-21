using System.Collections;

namespace EB.Sparx
{
	public interface ISaveLoadInterface
	{
		string Key { get; }

		bool PostLoadSaveRequired { get; }

		IEnumerator Save(Id userId, Hashtable data);

		IEnumerator Load(Id userId, Hashtable data);

		IEnumerator PostLoad(Id userId);
	}
}
