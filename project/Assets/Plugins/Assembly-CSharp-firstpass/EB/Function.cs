namespace EB
{
	public delegate R Function<R>();
	public delegate R Function<R, A1>(A1 a1);
	public delegate R Function<R, A1, A2>(A1 a1, A2 a2);
	public delegate R Function<R, A1, A2, A3>(A1 a1, A2 a2, A3 a3);
}
