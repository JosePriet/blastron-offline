using System.Collections;

namespace EB.Sparx
{
	public class Player
	{
		public Id UserId { get; private set; }

		public uint PlayerId { get; private set; }

		public Hashtable Attributes { get; private set; }

		public int Index { get; set; }

		public bool IsHost
		{
			get
			{
				return Index == 0;
			}
		}

		public Player(Id uid, uint connId)
		{
			UserId = uid;
			PlayerId = connId;
			Attributes = new Hashtable();
		}

		public Player(object obj)
		{
			Attributes = new Hashtable();
			UserId = new Id(Dot.Find("uid", obj));
			PlayerId = Dot.UInteger("id", obj, PlayerId);
			Update(obj);
		}

		public void Update(object obj)
		{
			Attributes = Dot.Object("attributes", obj, Attributes);
		}

		public override string ToString()
		{
			return Debug.Format("[Player: Id={0}, ConnId={1}, Index={2}, Attributes={3}]", UserId, PlayerId, Index, Attributes);
		}
	}
}
