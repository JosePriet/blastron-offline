using FarseerPhysics.Collision;

public class Element<T>
{
	public QuadTree<T> Parent;

	public AABB Span;

	public T Value;

	public Element(T value, AABB span)
	{
		Span = span;
		Value = value;
		Parent = null;
	}
}
