namespace EB
{
	public delegate void Action();
	public delegate void Action<T>(T obj);
	public delegate void Action<T, U>(T obj, U obj1);
	public delegate void Action<T, U, V>(T obj, U obj1, V obj2);
	public delegate void Action<T, U, V, X>(T obj, U obj1, V obj2, X obj3);
	public delegate void Action<T, U, V, X, Y>(T obj, U obj1, V obj2, X obj3, Y obj4);
	public delegate void Action<T, U, V, X, Y, Z>(T obj, U obj1, V obj2, X obj3, Y obj4, Z obj5);
}
