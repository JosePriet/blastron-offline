using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;

namespace FarseerPhysics.Dynamics
{
	public class ContactManager
	{
		public BeginContactDelegate BeginContact;

		public IBroadPhase BroadPhase;

		public CollisionFilterDelegate ContactFilter;

		public List<Contact> ContactList = new List<Contact>(128);

		public EndContactDelegate EndContact;

		public BroadphaseDelegate OnBroadphaseCollision;

		public PostSolveDelegate PostSolve;

		public PreSolveDelegate PreSolve;

		internal ContactManager(IBroadPhase broadPhase)
		{
			BroadPhase = broadPhase;
			OnBroadphaseCollision = AddPair;
		}

		private void AddPair(ref FixtureProxy proxyA, ref FixtureProxy proxyB)
		{
			Fixture fixture = proxyA.Fixture;
			Fixture fixture2 = proxyB.Fixture;
			int childIndex = proxyA.ChildIndex;
			int childIndex2 = proxyB.ChildIndex;
			Body body = fixture.Body;
			Body body2 = fixture2.Body;
			if (body == body2)
			{
				return;
			}
			for (ContactEdge contactEdge = body2.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
			{
				if (contactEdge.Other == body)
				{
					Fixture fixtureA = contactEdge.Contact.FixtureA;
					Fixture fixtureB = contactEdge.Contact.FixtureB;
					int childIndexA = contactEdge.Contact.ChildIndexA;
					int childIndexB = contactEdge.Contact.ChildIndexB;
					if ((fixtureA == fixture && fixtureB == fixture2 && childIndexA == childIndex && childIndexB == childIndex2) || (fixtureA == fixture2 && fixtureB == fixture && childIndexA == childIndex2 && childIndexB == childIndex))
					{
						return;
					}
				}
			}
			if (body2.ShouldCollide(body) && ShouldCollide(fixture, fixture2) && (ContactFilter == null || ContactFilter(fixture, fixture2)) && (fixture.BeforeCollision == null || fixture.BeforeCollision(fixture, fixture2)) && (fixture2.BeforeCollision == null || fixture2.BeforeCollision(fixture2, fixture)))
			{
				Contact contact = Contact.Create(fixture, childIndex, fixture2, childIndex2);
				fixture = contact.FixtureA;
				fixture2 = contact.FixtureB;
				body = fixture.Body;
				body2 = fixture2.Body;
				ContactList.Add(contact);
				contact.NodeA.Contact = contact;
				contact.NodeA.Other = body2;
				contact.NodeA.Prev = null;
				contact.NodeA.Next = body.ContactList;
				if (body.ContactList != null)
				{
					body.ContactList.Prev = contact.NodeA;
				}
				body.ContactList = contact.NodeA;
				contact.NodeB.Contact = contact;
				contact.NodeB.Other = body;
				contact.NodeB.Prev = null;
				contact.NodeB.Next = body2.ContactList;
				if (body2.ContactList != null)
				{
					body2.ContactList.Prev = contact.NodeB;
				}
				body2.ContactList = contact.NodeB;
			}
		}

		internal void FindNewContacts()
		{
			BroadPhase.UpdatePairs(OnBroadphaseCollision);
		}

		internal void Destroy(Contact contact)
		{
			Fixture fixtureA = contact.FixtureA;
			Fixture fixtureB = contact.FixtureB;
			Body body = fixtureA.Body;
			Body body2 = fixtureB.Body;
			if (EndContact != null && contact.IsTouching())
			{
				EndContact(contact);
			}
			ContactList.Remove(contact);
			if (contact.NodeA.Prev != null)
			{
				contact.NodeA.Prev.Next = contact.NodeA.Next;
			}
			if (contact.NodeA.Next != null)
			{
				contact.NodeA.Next.Prev = contact.NodeA.Prev;
			}
			if (contact.NodeA == body.ContactList)
			{
				body.ContactList = contact.NodeA.Next;
			}
			if (contact.NodeB.Prev != null)
			{
				contact.NodeB.Prev.Next = contact.NodeB.Next;
			}
			if (contact.NodeB.Next != null)
			{
				contact.NodeB.Next.Prev = contact.NodeB.Prev;
			}
			if (contact.NodeB == body2.ContactList)
			{
				body2.ContactList = contact.NodeB.Next;
			}
			contact.Destroy();
		}

		internal void Collide()
		{
			for (int i = 0; i < ContactList.Count; i++)
			{
				Contact contact = ContactList[i];
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				int childIndexA = contact.ChildIndexA;
				int childIndexB = contact.ChildIndexB;
				Body body = fixtureA.Body;
				Body body2 = fixtureB.Body;
				if (!body.Awake && !body2.Awake)
				{
					continue;
				}
				if ((contact.Flags & ContactFlags.Filter) == ContactFlags.Filter)
				{
					if (!body2.ShouldCollide(body))
					{
						Contact contact2 = contact;
						Destroy(contact2);
						continue;
					}
					if (!ShouldCollide(fixtureA, fixtureB))
					{
						Contact contact3 = contact;
						Destroy(contact3);
						continue;
					}
					if (ContactFilter != null && !ContactFilter(fixtureA, fixtureB))
					{
						Contact contact4 = contact;
						Destroy(contact4);
						continue;
					}
					contact.Flags &= ~ContactFlags.Filter;
				}
				int proxyId = fixtureA.Proxies[childIndexA].ProxyId;
				int proxyId2 = fixtureB.Proxies[childIndexB].ProxyId;
				if (!BroadPhase.TestOverlap(proxyId, proxyId2))
				{
					Contact contact5 = contact;
					Destroy(contact5);
				}
				else
				{
					contact.Update(this);
				}
			}
		}

		private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
		{
			if (Settings.UseFPECollisionCategories)
			{
				if (fixtureA.CollisionGroup == fixtureB.CollisionGroup && fixtureA.CollisionGroup != 0 && fixtureB.CollisionGroup != 0)
				{
					return false;
				}
				if (((fixtureA.CollisionCategories & fixtureB.CollidesWith) == 0) & ((fixtureB.CollisionCategories & fixtureA.CollidesWith) == 0))
				{
					return false;
				}
				if (fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA))
				{
					return false;
				}
				return true;
			}
			if (fixtureA.CollisionGroup == fixtureB.CollisionGroup && fixtureA.CollisionGroup != 0)
			{
				return fixtureA.CollisionGroup > 0;
			}
			bool flag = (fixtureA.CollidesWith & fixtureB.CollisionCategories) != 0 && (fixtureA.CollisionCategories & fixtureB.CollidesWith) != 0;
			if (flag && (fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA)))
			{
				return false;
			}
			return flag;
		}

		internal void UpdateContacts(ContactEdge contactEdge, bool value)
		{
		}
	}
}
